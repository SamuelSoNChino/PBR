using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleGenerator : MonoBehaviour
{
    public Texture2D skin;
    public int pieces;

    void Start()
    {
        generateTiles();

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
                Texture2D tileSkin = new Texture2D(tileWidth, tileHeight);
                tileSkin.SetPixels(skin.GetPixels(startW, startH, tileWidth, tileHeight));
                tileSkin.Apply();
                Sprite tileSprite = Sprite.Create(tileSkin, new Rect(0, 0, tileWidth, tileHeight), Vector2.zero, 100);

                GameObject tile = new GameObject();
                SpriteRenderer spriteRenderer = tile.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = tileSprite;

                tile.transform.position = new Vector3(i, j, 0);
                tile.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
            }
        }
    }

    void Update()
    {
        
    }
}
