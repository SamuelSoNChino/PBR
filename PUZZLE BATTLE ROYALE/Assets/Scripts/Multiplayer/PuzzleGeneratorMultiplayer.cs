using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Handles the generation of puzzle images and tiles from a server in a multiplayer environment.
/// </summary>
public class PuzzleGeneratorMultiplayer : NetworkBehaviour
{
    /// <summary>
    /// The parent GameObject for puzzle tiles.
    /// </summary>
    [SerializeField] private GameObject tiles;

    /// <summary>
    /// The parent GameObject for grid tiles.
    /// </summary>
    [SerializeField] private GameObject grid;

    /// <summary>
    /// The size of the puzzle image.
    /// </summary>
    [SerializeField] private int puzzleSize;

    /// <summary>
    /// The URL of the server to request images from.
    /// </summary>
    [SerializeField] private string serverUrl;

    /// <summary>
    /// The PuzzleManager component that manages the puzzle.
    /// </summary>
    [SerializeField] private PuzzleManager puzzleManager;

    /// <summary>
    /// The PlayerManager component that manages the players.
    /// </summary>
    [SerializeField] private PlayerManager playerManager;

    /// <summary>
    /// The texture of the puzzle image.
    /// </summary>
    private Texture2D puzzleImage;

    /// <summary>
    /// The texture of the grid image.
    /// </summary>
    private Texture2D gridImage;

    /// <summary>
    /// The number of tiles the puzzle is divided into.
    /// </summary>
    private int numberOfTiles;

    /// <summary>
    /// Sets a new value for the number of tiles.
    /// </summary>
    /// <param name="newNumberOfTiles">New number of tiles.</param>
    public void SetNumberOfTiles(int newNumberOfTiles)
    {
        numberOfTiles = newNumberOfTiles;
    }

    // -----------------------------------------------------------------------
    // Image Requests
    // -----------------------------------------------------------------------

    /// <summary>
    /// Requests and downloads the puzzle image from the server.
    /// </summary>
    /// <param name="seed">The seed value used for image generation (default is 0).</param>
    /// <returns>An IEnumerator for the coroutine.</returns>
    public IEnumerator RequestPuzzleImage(int seed = 0)
    {
        string puzzleImageUrl = $"{serverUrl}/generate_image?image_size={puzzleSize}&number_of_tiles={numberOfTiles}&seed={seed}";
        UnityWebRequest puzzleImageRequest = UnityWebRequestTexture.GetTexture(puzzleImageUrl);
        yield return puzzleImageRequest.SendWebRequest();

        if (puzzleImageRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to download puzzle image: " + puzzleImageRequest.error);
            yield break;
        }

        puzzleImage = DownloadHandlerTexture.GetContent(puzzleImageRequest);
    }

    /// <summary>
    /// Requests and downloads the grid image from the server.
    /// </summary>
    /// <returns>An IEnumerator for the coroutine.</returns>
    public IEnumerator RequestGridImage()
    {
        string gridImageUrl = $"{serverUrl}/generate_grid?image_size={puzzleSize}&number_of_tiles={numberOfTiles}";
        UnityWebRequest gridImageRequest = UnityWebRequestTexture.GetTexture(gridImageUrl);
        yield return gridImageRequest.SendWebRequest();

        if (gridImageRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to download grid image: " + gridImageRequest.error);
            yield break;
        }

        gridImage = DownloadHandlerTexture.GetContent(gridImageRequest);
    }

    // -----------------------------------------------------------------------
    // Puzzle Tiles Generation
    // -----------------------------------------------------------------------

    /// <summary>
    /// Generates the puzzle tiles from the downloaded puzzle image.
    /// </summary>
    public void GeneratePuzzleTiles()
    {
        // Prepares a list of possible Z positions for the tiles, which will be distributed randomly later
        List<int> possibleZPositions = new();
        for (int i = 1; i <= numberOfTiles * numberOfTiles; i++)
        {
            possibleZPositions.Add(i);
        }

        List<int> tileIds = new(puzzleManager.GetPuzzleTileIds());
        List<int> zPositions = new();
        List<Texture2D> tileTextures = new();

        for (int i = 0; i < numberOfTiles; i++)
        {
            for (int j = 0; j < numberOfTiles; j++)
            {
                // x, y represent the position of the tile in the image, from where it will be cropped out
                int x = puzzleSize / numberOfTiles * i;
                int y = puzzleSize / numberOfTiles * j;

                // Width and Height need to be calculated this way so that the resulting puzzle
                // actually corresponds to puzzleSize
                int nextX = puzzleSize / numberOfTiles * (i + 1);
                int nextY = puzzleSize / numberOfTiles * (j + 1);
                int tileWidth = nextX - x;
                int tileHeight = nextY - y;

                Color[] puzzleTilePixels = puzzleImage.GetPixels(x, y, tileWidth, tileHeight);
                Texture2D puzzleTileTexture = new(tileWidth, tileHeight);
                puzzleTileTexture.SetPixels(puzzleTilePixels);
                puzzleTileTexture.Apply();
                tileTextures.Add(puzzleTileTexture);

                // Picks a random Z position from the list of possible ones
                int zPositionIndex = Random.Range(0, possibleZPositions.Count - 1);
                int zPosition = possibleZPositions[zPositionIndex];
                possibleZPositions.RemoveAt(zPositionIndex);
                zPositions.Add(zPosition);
            }
        }

        // Sends each tile to clients in a random order
        for (int i = 0; i < numberOfTiles * numberOfTiles; i++)
        {
            int randomTileIndex = Random.Range(0, tileIds.Count);

            int tileId = tileIds[randomTileIndex];
            int zPosition = zPositions[randomTileIndex];
            Texture2D tileTexture = tileTextures[randomTileIndex];
            byte[] tileTextureData = TextureToByteArray(tileTexture);

            tileIds.RemoveAt(randomTileIndex);
            zPositions.RemoveAt(randomTileIndex);
            tileTextures.RemoveAt(randomTileIndex);

            // Initializes information about individual players' puzzle tiles
            foreach (Player player in playerManager.GetAllPlayers())
            {
                player.AddPuzzleTilePosition(tileId, new Vector3(0, 0, zPosition));
                player.AddPuzzleTileSnappedGridTile(tileId);
            }

            CreatePuzzleTileRpc(tileId, zPosition, tileTextureData);
        }
    }

    /// <summary>
    /// Creates a puzzle tile on all clients.
    /// </summary>
    /// <param name="tileId">The ID of the tile.</param>
    /// <param name="zPosition">The Z position of the tile.</param>
    /// <param name="textureData">The texture data of the tile.</param>
    [Rpc(SendTo.ClientsAndHost)]
    public void CreatePuzzleTileRpc(int tileId, int zPosition, byte[] textureData)
    {
        GameObject tile = new();
        tile.transform.position = new Vector3(0, 0, zPosition);
        tile.name = $"Puzzle Tile-{tileId}";
        tile.transform.parent = tiles.transform;
        PuzzleTileMultiplayer puzzleTile = tile.AddComponent<PuzzleTileMultiplayer>();
        puzzleTile.TileId = tileId;

        Texture2D tileTexture = ReconstructTexture(textureData);
        Sprite tileSprite = CreateSprite(tileTexture, 0, 0, tileTexture.width, tileTexture.height);
        SpriteRenderer spriteRenderer = tile.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = tileSprite;

        BoxCollider2D collider = tile.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(tileSprite.texture.width, tileSprite.texture.height);
        collider.enabled = false;
    }

    // -----------------------------------------------------------------------
    // Grid Tiles Generation
    // -----------------------------------------------------------------------

    /// <summary>
    /// Generates the grid tiles from the downloaded grid image.
    /// </summary>
    public void GenerateGridTiles()
    {
        // Constant Z position of all grid tiles, located right behind the last puzzle tile
        int gridZPosition = numberOfTiles * numberOfTiles + 1;

        List<int> gridTileIds = new(puzzleManager.GetGridTileIds());
        int tileIdIndex = 0;

        for (int i = 0; i < numberOfTiles; i++)
        {
            for (int j = 0; j < numberOfTiles; j++)
            {
                // x, y represent both the position of the tiles in the game and the position of the
                // tile in the image, from where it will be cropped out
                int x = puzzleSize / numberOfTiles * i;
                int y = puzzleSize / numberOfTiles * j;

                // Width and Height need to be calculated this way so that the resulting puzzle
                // actually corresponds to puzzleSize
                int nextX = puzzleSize / numberOfTiles * (i + 1);
                int nextY = puzzleSize / numberOfTiles * (j + 1);
                int tileWidth = nextX - x;
                int tileHeight = nextY - y;

                Color[] gridTilePixels = gridImage.GetPixels(x, y, tileWidth, tileHeight);
                Texture2D gridTileTexture = new(tileWidth, tileHeight);
                gridTileTexture.SetPixels(gridTilePixels);
                gridTileTexture.Apply();
                byte[] gridTileTextureData = TextureToByteArray(gridTileTexture);

                int gridTileId = gridTileIds[tileIdIndex];
                tileIdIndex++;

                // Initializes information about individual players' grid tiles
                foreach (Player player in playerManager.GetAllPlayers())
                {
                    player.AddGridTileCorrectlyOccupied(gridTileId);
                }
                // Saves the fixed grid tile position to the server for later checking whether a puzzle tile is placed on it
                puzzleManager.AddGridTileByPosition(new Vector2(x, y), gridTileId);

                CreateGridTileRpc(gridTileId, x, y, gridZPosition, gridTileTextureData);
            }
        }
    }

    /// <summary>
    /// Creates a grid tile at the specified position.
    /// </summary>
    /// <param name="tileId">The ID of the tile.</param>
    /// <param name="x">The X coordinate of the tile in the image.</param>
    /// <param name="y">The Y coordinate of the tile in the image.</param>
    /// <param name="zPosition">The Z position of the tile.</param>
    /// <param name="textureData">The texture data of the tile.</param>
    [Rpc(SendTo.ClientsAndHost)]
    private void CreateGridTileRpc(int tileId, int x, int y, int zPosition, byte[] textureData)
    {
        GameObject tile = new();
        tile.transform.position = new Vector3(x, y, zPosition);
        tile.name = $"Grid Tile-{tileId}";
        tile.transform.parent = grid.transform;
        GridTileMultiplayer gridTile = tile.AddComponent<GridTileMultiplayer>();
        gridTile.TileId = tileId;

        Texture2D gridTileTexture = ReconstructTexture(textureData);
        Sprite gridTileSprite = CreateSprite(gridTileTexture, 0, 0, gridTileTexture.width, gridTileTexture.height);
        SpriteRenderer spriteRenderer = tile.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = gridTileSprite;
    }


    // -----------------------------------------------------------------------
    // Sprites and Textures
    // -----------------------------------------------------------------------

    /// <summary>
    /// Creates a sprite from the specified portion of the texture.
    /// </summary>
    /// <param name="originalTexture">The original texture.</param>
    /// <param name="x">The X coordinate of the top-left corner of the portion.</param>
    /// <param name="y">The Y coordinate of the top-left corner of the portion.</param>
    /// <param name="width">The width of the portion.</param>
    /// <param name="height">The height of the portion.</param>
    /// <returns>A sprite created from the specified portion of the texture.</returns>
    private Sprite CreateSprite(Texture2D originalTexture, int x, int y, int width, int height)
    {
        return Sprite.Create(originalTexture, new Rect(x, y, width, height), Vector2.zero, 1);
    }

    /// <summary>
    /// Converts a texture to a byte array.
    /// </summary>
    /// <param name="texture">The texture to convert.</param>
    /// <returns>A byte array representing the texture.</returns>
    public byte[] TextureToByteArray(Texture texture)
    {
        RenderTexture renderTexture = new(texture.width, texture.height, 32);
        Graphics.Blit(texture, renderTexture);

        RenderTexture previousRenderTexture = RenderTexture.active;
        RenderTexture.active = renderTexture;

        Texture2D newTexture = new Texture2D(texture.width, texture.height);
        newTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        newTexture.Apply();

        RenderTexture.active = previousRenderTexture;
        byte[] bytes = newTexture.EncodeToPNG();
        return bytes;
    }

    /// <summary>
    /// Reconstructs a texture from a byte array.
    /// </summary>
    /// <param name="textureData">The byte array representing the texture.</param>
    /// <returns>A Texture2D object created from the byte array.</returns>
    private Texture2D ReconstructTexture(byte[] textureData)
    {
        Texture2D texture = new(1, 1);
        texture.LoadImage(textureData);
        return texture;
    }
}
