using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Handles the generation of puzzle images and tiles from a server.
/// </summary>
public class PuzzleGenerator : MonoBehaviour
{
    /// <summary>
    /// The texture of the puzzle image.
    /// </summary>
    private Texture2D puzzleImage;

    /// <summary>
    /// The texture of the grid image.
    /// </summary>
    private Texture2D gridImage;

    /// <summary>
    /// The number of pieces the puzzle is divided into.
    /// </summary>
    [SerializeField] private int pieces;

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
    /// Requests and downloads the puzzle image from the server.
    /// </summary>
    /// <param name="seed">The seed value used for image generation.</param>
    /// <returns>An IEnumerator for the coroutine.</returns>
    public IEnumerator RequestPuzzleImage(int seed)
    {
        // Sends a request to the server 
        string puzzleImageUrl = $"{serverUrl}/generate_image?image_size={puzzleSize}&pieces={pieces}&seed={seed}";
        UnityWebRequest puzzleImageRequest = UnityWebRequestTexture.GetTexture(puzzleImageUrl);
        yield return puzzleImageRequest.SendWebRequest();

        // If the request wasn't successful prints an error and ends the coroutine
        if (puzzleImageRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to download puzzle image: " + puzzleImageRequest.error);
            yield break;
        }

        // If the request was successful downloads the texture
        puzzleImage = DownloadHandlerTexture.GetContent(puzzleImageRequest);
    }

    /// <summary>
    /// Requests and downloads the grid image from the server.
    /// </summary>
    /// <returns>An IEnumerator for the coroutine.</returns>
    public IEnumerator RequestGridImage()
    {
        // Sends a request to the server 
        string gridImageUrl = $"{serverUrl}/generate_grid?image_size={puzzleSize}&pieces={pieces}";
        UnityWebRequest gridImageRequest = UnityWebRequestTexture.GetTexture(gridImageUrl);
        yield return gridImageRequest.SendWebRequest();

        // If the request wasn't successful prints an error and ends the coroutine
        if (gridImageRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to download grid image: " + gridImageRequest.error);
            yield break;
        }

        // If the request was successful downloads the texture
        gridImage = DownloadHandlerTexture.GetContent(gridImageRequest);
    }

    /// <summary>
    /// Generates the puzzle and grid tiles from the downloaded images.
    /// </summary>
    public void GenerateTiles()
    {
        // Initial Z position for the first puzzle tiles, will be incremented for each new tile
        int puzzleTileZPosition = 1;
        // Constant Z position of all grid tiles, located right behind the last puzzle tile
        int gridZPosition = pieces * pieces + 1;

        // Iterates trhough each tile needed to be created
        for (int i = 0; i < pieces; i++)
        {
            for (int j = 0; j < pieces; j++)
            {
                // x, y represent both the position of the tiles in the game and the position of the
                // tile in the image, from where it will be cropped out
                int x = puzzleSize / pieces * i;
                int y = puzzleSize / pieces * j;

                // Width and Height need to be calculated this way so that the the resulting puzzle
                // actually coresponds to puzzleSize
                int nextX = puzzleSize / pieces * (i + 1);
                int nextY = puzzleSize / pieces * (j + 1);
                int tileWidth = nextX - x;
                int tileHeight = nextY - y;

                // Creates a puzzle tile and increments the Z position
                CreatePuzzleTile(i, j, x, y, tileWidth, tileHeight, puzzleTileZPosition);
                puzzleTileZPosition++;

                // Creates a grid tile
                CreateGridTile(i, j, x, y, tileWidth, tileHeight, gridZPosition);
            }
        }
    }

    /// <summary>
    /// Creates a grid tile at the specified position.
    /// </summary>
    /// <param name="indexX">The X index of the grid tile.</param>
    /// <param name="indexY">The Y index of the grid tile.</param>
    /// <param name="x">The X coordinate of the tile in the image.</param>
    /// <param name="y">The Y coordinate of the tile in the image.</param>
    /// <param name="width">The width of the tile.</param>
    /// <param name="height">The height of the tile.</param>
    /// <param name="zPosition">The Z position of the tile.</param>
    private void CreateGridTile(int indexX, int indexY, int x, int y, int width, int height, int zPosition)
    {
        // Sets basic attributes and the Grid parent
        GameObject tile = new();
        tile.transform.position = new Vector3(x, y, zPosition);
        tile.name = $"Grid Tile-{indexX}-{indexY}";
        tile.transform.parent = grid.transform;

        // Attaches the GridTile class to the object and sets its indexes
        GridTile gridTile = tile.AddComponent<GridTile>();
        gridTile.SetIndexes(indexX, indexY);

        // Creates the sprite for the tile and attaches it to the tile
        Sprite gridSprite = CreateSprite(gridImage, x, y, width, height);
        SpriteRenderer spriteRenderer = tile.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = gridSprite;
    }

    /// <summary>
    /// Creates a puzzle tile at the specified position.
    /// </summary>
    /// <param name="indexX">The X index of the puzzle tile.</param>
    /// <param name="indexY">The Y index of the puzzle tile.</param>
    /// <param name="x">The X coordinate of the tile in the image.</param>
    /// <param name="y">The Y coordinate of the tile in the image.</param>
    /// <param name="width">The width of the tile.</param>
    /// <param name="height">The height of the tile.</param>
    /// <param name="zPosition">The Z position of the tile.</param>
    public void CreatePuzzleTile(int indexX, int indexY, int x, int y, int width, int height, int zPosition)
    {
        // Sets basic attributes and the Tiles parent
        GameObject tile = new();
        tile.transform.position = new Vector3(x, y, zPosition);
        tile.name = $"Puzzle Tile-{indexX}-{indexY}";
        tile.transform.parent = tiles.transform;

        // Attaches the PuzzleTile class to the object and sets its indexes
        PuzzleTile puzzleTile = tile.AddComponent<PuzzleTile>();
        puzzleTile.SetIndexes(indexX, indexY);

        // Creates the sprite for the tile and attaches it to the tile
        Sprite tileSprite = CreateSprite(puzzleImage, x, y, width, height);
        SpriteRenderer spriteRenderer = tile.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = tileSprite;

        // Creates a collider for tile tile, so it can be interacted with
        BoxCollider2D collider = tile.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(width, height);
    }

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
}