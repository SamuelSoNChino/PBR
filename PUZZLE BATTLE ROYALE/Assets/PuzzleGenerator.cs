using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleGenerator : MonoBehaviour
{
    public Texture2D skin;
    public Texture2D pad;
    public int pieces;
    public GameObject Tiles;
    public GameObject Grid;
    public SortingLayer TileLayer;
    public SortingLayer GridLayer;

    void Start()
    {
        generateGridTiles();
        shuffleTiles();
    }

    void generateGridTiles()
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
                tile.transform.position = new Vector3(i, j, 0);
                tile.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
                tile.name = "Tile-" + i.ToString() + "-" + j.ToString();
                tile.transform.parent = Tiles.transform;

                Texture2D tileSkin = new Texture2D(tileWidth, tileHeight);
                tileSkin.SetPixels(skin.GetPixels(startW, startH, tileWidth, tileHeight));
                tileSkin.Apply();
                Sprite tileSprite = Sprite.Create(tileSkin, new Rect(0, 0, tileWidth, tileHeight), Vector2.zero, 100);
                SpriteRenderer spriteRenderer = tile.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = tileSprite;
                spriteRenderer.sortingLayerName = "Tiles";


                GameObject gridTile = new GameObject();
                gridTile.transform.position = new Vector3(i, j, 0);
                gridTile.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
                gridTile.name = "Grid-" + i.ToString() + "-" + j.ToString();
                gridTile.transform.parent = Grid.transform;
                

                Texture2D gridSkin = new Texture2D(tileWidth, tileHeight);
                gridSkin.SetPixels(pad.GetPixels(startW, startH, tileWidth, tileHeight));
                gridSkin.Apply();
                Sprite gridSprite = Sprite.Create(gridSkin, new Rect(0, 0, tileWidth, tileHeight), Vector2.zero, 100);
                spriteRenderer = gridTile.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = gridSprite;
                spriteRenderer.sortingLayerName = "Grid";
            }
        }
    }
    void shuffleTiles()
    {
        int numberOfTiles = Tiles.transform.childCount;
        for (int i = 0; i < numberOfTiles; i++)
        {
            
            Transform tile = Tiles.transform.GetChild(i);
            
        }
    }
}
