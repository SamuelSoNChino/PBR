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
        // 
        int puzzleTileZValue = 1;
        int gridZValue = pieces * pieces + 1;
        for (int i = 0; i < pieces; i++)
        {
            for (int j = 0; j < pieces; j++)
            {
                int x = puzzleSize * i / pieces;
                int y = puzzleSize * j / pieces;

                // Width and Height need to be calculated this way to make su
                int nextX = puzzleSize * (i + 1) / pieces;
                int nextY = puzzleSize * (j + 1) / pieces;
                int tileWidth = nextX - x;
                int tileHeight = nextY - y;

                CreatePuzzleTile(i, j, x, y, tileWidth, tileHeight, puzzleTileZValue);
                puzzleTileZValue++;

                CreateGridTile(i, j, x, y, tileWidth, tileHeight, gridZValue);
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
    /// <param name="zValue">The Z position of the tile.</param>
    private void CreateGridTile(int indexX, int indexY, int x, int y, int width, int height, int zValue)
    {
        GameObject tile = new();
        tile.transform.position = new Vector3(x, y, zValue);
        tile.name = $"Grid Tile-{indexX}-{indexY}";
        tile.transform.parent = grid.transform;

        GridTile gridTile = tile.AddComponent<GridTile>();
        gridTile.SetIndexes(indexX, indexY);

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
    /// <param name="zValue">The Z position of the tile.</param>
    public void CreatePuzzleTile(int indexX, int indexY, int x, int y, int width, int height, int zValue)
    {
        GameObject tile = new();
        tile.transform.position = new Vector3(0, 0, zValue);
        tile.name = $"Puzzle Tile-{indexX}-{indexY}";
        tile.transform.parent = tiles.transform;

        PuzzleTile puzzleTile = tile.AddComponent<PuzzleTile>();
        puzzleTile.SetIndexes(indexX, indexY); // Stores correct index

        Sprite tileSprite = CreateSprite(puzzleImage, x, y, width, height);
        SpriteRenderer spriteRenderer = tile.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = tileSprite;

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