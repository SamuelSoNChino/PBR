using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages background skins in a multiplayer environment, allowing players to use their selected background skins.
/// WORKS ONLY WITH MULTIPLAYER.
/// </summary>
public class BackgroundManagerMultiplayer : NetworkBehaviour
{
    /// |---------------------------------|
    /// |             SERVER              |
    /// |---------------------------------|

    /// <summary>
    /// Dictionary to store the background skins of players, mapped by their client IDs.
    /// </summary>
    private Dictionary<ulong, int> playerBackgrounds = new();

    /// <summary>
    /// Collects the background skin from each client and sends it to the server.
    /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    public void CollectPlayerBackgroundsRpc()
    {
        SentBackgroundToServerRpc(NetworkManager.Singleton.LocalClientId, PlayerPrefs.GetInt("backgroundSkin"));
    }

    /// <summary>
    /// Sends the client's background skin to the server.
    /// </summary>
    /// <param name="clientId">The ID of the client sending the background skin.</param>
    /// <param name="background">The background skin ID.</param>
    [Rpc(SendTo.Server)]
    private void SentBackgroundToServerRpc(ulong clientId, int background)
    {
        if (!playerBackgrounds.ContainsKey(clientId))
        {
            playerBackgrounds.Add(clientId, background);
        }
    }

    /// <summary>
    /// Retrieves the background skin ID for a specified client.
    /// </summary>
    /// <param name="clientId">The ID of the client.</param>
    /// <returns>The background skin ID.</returns>
    public int GetClientBackground(ulong clientId)
    {
        return playerBackgrounds[clientId];
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetClientBackgroundRpc(ulong clientId, int backgroundSkin)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            SetNewBackground(backgroundSkin);
        }
    }



    /// |---------------------------------|
    /// |             CLIENT              |
    /// |---------------------------------|

    /// <summary>
    /// GameObject representing the background.
    /// </summary>
    [SerializeField] private GameObject background;

    /// <summary>
    /// Array of available background skins as sprites.
    /// </summary>
    [SerializeField] private Sprite[] backgroundSkins;

    /// <summary>
    /// Initializes the background when the script is loaded.
    /// </summary>
    private void Awake()
    {
        if (!PlayerPrefs.HasKey("backgroundSkin"))
        {
            PlayerPrefs.SetInt("backgroundSkin", 0);
        }

        SetNewBackground(PlayerPrefs.GetInt("backgroundSkin"));
    }

    /// <summary>
    /// Loads a new background skin for the client.
    /// </summary>
    /// <param name="newBackground">The ID of the new background skin to load.</param>
    public void SetNewBackground(int newBackground)
    {
        // Loads a new background skin sprite to the background game object
        Sprite chosenBackground = backgroundSkins[newBackground];
        SpriteRenderer backgroundSpriteRenderer = background.GetComponent<SpriteRenderer>();
        backgroundSpriteRenderer.sprite = chosenBackground;
    }
}