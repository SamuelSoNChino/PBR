using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundManagerMultiplayer : NetworkBehaviour
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



    // Server methods
    [Rpc(SendTo.ClientsAndHost)]
    public void CollectPlayersBackgroundsRpc()
    {
        SentBackgroundToServerRpc(NetworkManager.Singleton.LocalClientId, PlayerPrefs.GetInt("backgroundSkin"));
    }

    [Rpc(SendTo.Server)]
    private void SentBackgroundToServerRpc(ulong clientId, int background)
    {
        if (!playerBackgrounds.ContainsKey(clientId))
        {
            playerBackgrounds.Add(clientId, background);
        }
    }

    public int GetPlayerBackground(ulong clientId)
    {
        foreach (var kvp in playerBackgrounds)
        {
            Debug.Log($"{kvp.Key}:{kvp.Value}");
        }
        return playerBackgrounds[clientId];
    }
}
