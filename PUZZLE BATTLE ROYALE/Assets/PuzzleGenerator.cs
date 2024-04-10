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
    public 

    void Start()
    {
        generateTiles();
        adjustCamera();
        shuffleTiles();
    }

    void generateTiles()
    {
        int tileWidth = skin.width / pieces;
        int tileHeight = skin.height / pieces;
        for (int i = 0; i < pieces; i++)
        {
            for (int j = 0; j < pieces; j++)
            {
                int startW = i * tileWidth;
                int startH = j * tileHeight;
                

                GameObject tile = new GameObject();
                tile.transform.position = new Vector3(startW, startH, 0);
                tile.name = "Tile-" + i.ToString() + "-" + j.ToString();
                tile.transform.parent = Tiles.transform;

                Texture2D tileSkin  = new Texture2D(tileWidth, tileHeight);
                tileSkin.SetPixels(skin.GetPixels(startW, startH, tileWidth, tileHeight));
                tileSkin.Apply();
                Sprite tileSprite = Sprite.Create(tileSkin, new Rect(0, 0, tileWidth, tileHeight), Vector2.zero, 1);
                SpriteRenderer spriteRenderer = tile.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = tileSprite;
                spriteRenderer.sortingLayerName = "Tiles";
                tile.AddComponent<PuzzleTile>();
                
                BoxCollider2D collider = tile.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(tileWidth, tileHeight);


                GameObject gridTile = new GameObject();
                gridTile.transform.position = new Vector3(startW, startH, 0);
                gridTile.name = "Grid-" + i.ToString() + "-" + j.ToString();
                gridTile.transform.parent = Grid.transform;
                

                Texture2D gridSkin = new Texture2D(tileWidth, tileHeight);
                gridSkin.SetPixels(pad.GetPixels(startW, startH, tileWidth, tileHeight));
                gridSkin.Apply();
                Sprite gridSprite = Sprite.Create(gridSkin, new Rect(0, 0, tileWidth, tileHeight), Vector2.zero, 1);
                spriteRenderer = gridTile.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = gridSprite;
                spriteRenderer.sortingLayerName = "Grid";
            }
        }
    }
    
    void adjustCamera()
    {
        Camera.main.orthographicSize = skin.height;
        Camera.main.transform.position = new Vector3(skin.width / 2, 0, -1);
    }
    
    void shuffleTiles()
    {
        int tileWidth = skin.width / pieces;
        float minX = 0;
        float maxX = skin.width - tileWidth;
        float minY = -skin.height;
        float maxY = 0 - tileWidth;
        float numberOfTiles = Tiles.transform.childCount;
        for (int i = 0; i < numberOfTiles; i++)
        {
            Transform tile = Tiles.transform.GetChild(i);
            tile.position = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), 1);
        }
    }
}
