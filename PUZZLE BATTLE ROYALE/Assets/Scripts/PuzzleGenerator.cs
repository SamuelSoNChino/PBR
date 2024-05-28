using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class PuzzleGenerator : MonoBehaviour
{
    private Texture2D puzzleImage;
    private Texture2D gridImage;
    [SerializeField] int pieces;
    [SerializeField] GameObject tiles;
    [SerializeField] GameObject grid;
    [SerializeField] int puzzleSize;
    [SerializeField] string serverUrl;

    public IEnumerator RequestPuzzleImage(int seed)
    {
        string puzzleImageUrl = $"{serverUrl}/generate_image?image_size={puzzleSize}&pieces={pieces}&seed={seed}";
        UnityWebRequest puzzleImageRequest = UnityWebRequestTexture.GetTexture(puzzleImageUrl);
        yield return puzzleImageRequest.SendWebRequest();

        if (puzzleImageRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to download puzzle image: " + puzzleImageRequest.error);
            yield break;
        }
        puzzleImage = DownloadHandlerTexture.GetContent(puzzleImageRequest);
    }
    public IEnumerator RequestGridImage()
    {
        string gridImageUrl = $"{serverUrl}/generate_grid?image_size={puzzleSize}&pieces={pieces}";
        UnityWebRequest gridImageRequest = UnityWebRequestTexture.GetTexture(gridImageUrl);
        yield return gridImageRequest.SendWebRequest();

        if (gridImageRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to download grid image: " + gridImageRequest.error);
            yield break;
        }
        gridImage = DownloadHandlerTexture.GetContent(gridImageRequest);
    }
    public void GenerateTiles()
    {
        int zValue = 1;
        for (int i = 0; i < pieces; i++)
        {
            for (int j = 0; j < pieces; j++)
            {
                int x = puzzleSize * i / pieces;
                int y = puzzleSize * j / pieces;
                int nextX = puzzleSize * (i + 1) / pieces;
                int nextY = puzzleSize * (j + 1) / pieces;
                int tileWidth = nextX - x;
                int tileHeight = nextY - y;
                GameObject puzzleTile = new();
                puzzleTile.transform.position = new Vector3(x, y, zValue);
                zValue++;
                puzzleTile.name = "Tile-" + i.ToString() + "-" + j.ToString();
                puzzleTile.transform.parent = tiles.transform;

                PuzzleTile puzzleTileC = puzzleTile.AddComponent<PuzzleTile>();
                puzzleTileC.SetIndexes(i, j); // Stores correct index

                Texture2D tilepuzzleImage = new Texture2D(tileWidth, tileHeight);
                tilepuzzleImage.SetPixels(puzzleImage.GetPixels(x, y, tileWidth, tileHeight));
                tilepuzzleImage.Apply();
                Sprite tileSprite = Sprite.Create(tilepuzzleImage, new Rect(0, 0, tileWidth, tileHeight), Vector2.zero, 1);
                SpriteRenderer spriteRenderer = puzzleTile.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = tileSprite;

                BoxCollider2D collider = puzzleTile.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(tileWidth, tileHeight);


                GameObject gridTile = new GameObject();
                gridTile.transform.position = new Vector3(x, y, pieces * pieces + 1);
                gridTile.name = "grid-" + i.ToString() + "-" + j.ToString();
                gridTile.transform.parent = grid.transform;

                GridTile gridTileC = gridTile.AddComponent<GridTile>();
                gridTileC.SetIndexes(i, j);

                Texture2D gridpuzzleImage = new Texture2D(tileWidth, tileHeight);
                gridpuzzleImage.SetPixels(gridImage.GetPixels(x, y, tileWidth, tileHeight));
                gridpuzzleImage.Apply();
                Sprite gridSprite = Sprite.Create(gridpuzzleImage, new Rect(0, 0, tileWidth, tileHeight), Vector2.zero, 1);
                spriteRenderer = gridTile.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = gridSprite;
            }
        }
    }
}
