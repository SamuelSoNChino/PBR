using UnityEngine;

public class PuzzleGenerator : MonoBehaviour
{
    public GameObject main_camera;
    public Texture2D skin;
    public Texture2D pad;
    public int pieces;
    public GameObject Tiles;
    public GameObject Grid;
    public SortingLayer TileLayer;
    public SortingLayer GridLayer;
    public int PuzzleWidth;
    public int PuzzleHeight;

    void Start()
    {
        if (PuzzleWidth != skin.width || PuzzleHeight != skin.height || PuzzleHeight != pad.height || PuzzleWidth != pad.width)
        {
            print("IMAGES HAVE WRONG DIMENSIONS");
        } else {
            pieces = PlayerPrefs.GetInt("Tiles");
            generateTiles();
            shuffleTiles();
        }
        
    }

    void generateTiles()
    {
        for (int i = 0; i < pieces; i++)
        {
            for (int j = 0; j < pieces; j++)
            { 
                int x = (PuzzleWidth * i) / pieces;
                int y = (PuzzleHeight * j) / pieces;
                int nextX = (PuzzleWidth * (i + 1)) / pieces;
                int nextY = (PuzzleHeight * (j + 1)) / pieces;
                int tileWidth = nextX - x;
                int tileHeight = nextY - y;
                GameObject tile = new GameObject();
                tile.transform.position = new Vector3(x, y, 0);
                tile.name = "Tile-" + i.ToString() + "-" + j.ToString();
                tile.transform.parent = Tiles.transform;

                PuzzleTile puzzleTileC = tile.AddComponent<PuzzleTile>();
                puzzleTileC.x = i;
                puzzleTileC.y = j;

                Texture2D tileSkin  = new Texture2D(tileWidth, tileHeight);
                tileSkin.SetPixels(skin.GetPixels(x, y, tileWidth, tileHeight));
                tileSkin.Apply();
                Sprite tileSprite = Sprite.Create(tileSkin, new Rect(0, 0, tileWidth, tileHeight), Vector2.zero, 1);
                SpriteRenderer spriteRenderer = tile.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = tileSprite;


                BoxCollider2D collider = tile.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(tileWidth, tileHeight);


                GameObject gridTile = new GameObject();
                gridTile.transform.position = new Vector3(x, y, pieces * pieces + 1);
                gridTile.name = "Grid-" + i.ToString() + "-" + j.ToString();
                gridTile.transform.parent = Grid.transform;

                GridTile gridTileC = gridTile.AddComponent<GridTile>();
                gridTileC.x = i;
                gridTileC.y = j;
                
                Texture2D gridSkin = new Texture2D(tileWidth, tileHeight);
                gridSkin.SetPixels(pad.GetPixels(x ,y, tileWidth, tileHeight));
                gridSkin.Apply();
                Sprite gridSprite = Sprite.Create(gridSkin, new Rect(0, 0, tileWidth, tileHeight), Vector2.zero, 1);
                spriteRenderer = gridTile.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = gridSprite;
            }
        }
    }
    
    void shuffleTiles()
    {
        int tileWidth = PuzzleWidth / pieces;
        int tileHeight = PuzzleHeight / pieces;
        float minX = 0;
        float maxX = PuzzleWidth - tileWidth;
        float minY = -PuzzleHeight;
        float maxY = 0 - tileHeight;
        float numberOfTiles = Tiles.transform.childCount;
        for (int i = 0; i < numberOfTiles; i++)
        {
            Transform tile = Tiles.transform.GetChild(i);
            tile.position = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), i + 1);
        }
    }
}
