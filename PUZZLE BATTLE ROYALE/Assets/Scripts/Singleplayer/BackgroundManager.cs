using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundManager : NetworkBehaviour
{

    [SerializeField] private GameObject background;
    [SerializeField] private Sprite[] backgroundSkins;

    private Dictionary<ulong, int> playerBackgrounds = new();
    private void Awake()
    {
        LoadOriginalBackground();
    }

    public void LoadOriginalBackground()
    {
        if (!PlayerPrefs.HasKey("backgroundSkin"))
        {
            PlayerPrefs.SetInt("backgroundSkin", 0);
        }

        Sprite chosenBackground = backgroundSkins[PlayerPrefs.GetInt("backgroundSkin")];
        SpriteRenderer backgroundSpriteRenderer = background.GetComponent<SpriteRenderer>();
        backgroundSpriteRenderer.sprite = chosenBackground;
    }

    public void LoadNewBackground(int newBackground)
    {
        Sprite chosenBackground = backgroundSkins[newBackground];
        SpriteRenderer backgroundSpriteRenderer = background.GetComponent<SpriteRenderer>();
        backgroundSpriteRenderer.sprite = chosenBackground;
    }
}
