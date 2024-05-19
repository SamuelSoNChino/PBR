using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class PuzzleGenerator : MonoBehaviour
{
    public GameObject main_camera;
    Texture2D skin;
    Texture2D pad;
    public int pieces;
    public GameObject Tiles;
    public GameObject Grid;
    public SortingLayer TileLayer;
    public SortingLayer GridLayer;
    public int PuzzleSize;

    void Start()
    {
        pieces = PlayerPrefs.GetInt("Tiles");
        StartCoroutine(RequestImages());
        
    }
    IEnumerator RequestImages()
    {
        string url = "SamuelSoNChino.eu.pythonanywhere.com";
        string skinUrl = url + "/generate_image?image_size=" + PuzzleSize.ToString() + "&pieces=" + pieces.ToString();
        UnityWebRequest skinRequest = UnityWebRequestTexture.GetTexture(skinUrl);
        yield return skinRequest.SendWebRequest();

        if (skinRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to download skin image: " + skinRequest.error);
            yield break;
        }
        skin = DownloadHandlerTexture.GetContent(skinRequest);

        string padUrl = url + "/generate_grid?image_size=" + PuzzleSize.ToString() + "&pieces=" + pieces.ToString();
        UnityWebRequest padRequest = UnityWebRequestTexture.GetTexture(padUrl);
        yield return padRequest.SendWebRequest();

        if (padRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to download pad image: " + padRequest.error);
            yield break;
        }
        pad = DownloadHandlerTexture.GetContent(padRequest);
        

        if (PuzzleSize != skin.width || PuzzleSize != skin.height || PuzzleSize != pad.height || PuzzleSize != pad.width)
        {
            Debug.LogError("IMAGES HAVE WRONG DIMENSIONS");
            yield break;
        }
        generateTiles();
        shuffleTiles();
        
    }
    void generateTiles()
    {
        for (int i = 0; i < pieces; i++)
        {
            for (int j = 0; j < pieces; j++)
            { 
                int x = (PuzzleSize * i) / pieces;
                int y = (PuzzleSize * j) / pieces;
                int nextX = (PuzzleSize * (i + 1)) / pieces;
                int nextY = (PuzzleSize * (j + 1)) / pieces;
                int tileWidth = nextX - x;
                int tileHeight = nextY - y;
                GameObject tile = new GameObject();
                tile.transform.position = new Vector3(x, y, 0);
                tile.name = "Tile-" + i.ToString() + "-" + j.ToString();
                tile.transform.parent = Tiles.transform;

                PuzzleTile puzzleTileC = tile.AddComponent<PuzzleTile>();
                puzzleTileC.indexX = i; // Stores correct index
                puzzleTileC.indexY = j; // Stores correct index

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
                gridTileC.indexX = i; // Stores index
                gridTileC.indexY = j; // Stores index
                
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
        int tileSize = PuzzleSize / pieces;
        float minX = 0;
        float maxX = PuzzleSize - tileSize;
        float minY = -PuzzleSize;
        float maxY = 0 - tileSize;
        float numberOfTiles = Tiles.transform.childCount;
        for (int i = 0; i < numberOfTiles; i++)
        {
            Transform tile = Tiles.transform.GetChild(i);
            tile.position = new Vector3(UnityEngine.Random.Range(minX, maxX), UnityEngine.Random.Range(minY, maxY), i + 1);
        }
    }
}
