using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages background skins in a multiplayer environment, holding all the skins and allowing them to be changed.
/// </summary>
public class BackgroundManagerMultiplayer : NetworkBehaviour
{
    /// <summary>
    /// GameObject representing the background. This GameObject has a sprite component 
    /// which can be changed using the methods in this script.
    /// </summary>
    [SerializeField] private GameObject background;

    /// <summary>
    /// Array of available background skins as sprites.
    /// </summary>
    [SerializeField] private Sprite[] backgroundSkins;

    [SerializeField] private PlayerManager playerManager;

    public void SetAllClientsDefaultBackgrounds()
    {
        foreach (Player player in playerManager.GetAllPlayers())
        {
            SetClientBackgroundRpc(player.ClientId, player.BackgroundSkinId);
        }
    }

    /// <summary>
    /// Sets the background skin for the specified client. This method is called via an RPC (Remote Procedure Call).
    /// </summary>
    /// <param name="clientId">The ID of the client whose background skin is to be set.</param>
    /// <param name="backgroundSkinId">The ID of the new background skin.</param>
    [Rpc(SendTo.ClientsAndHost)]
    public void SetClientBackgroundRpc(ulong clientId, int backgroundSkinId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            SetNewBackground(backgroundSkinId);
        }
    }

    /// <summary>
    /// Loads a new background skin for the client.
    /// </summary>
    /// <param name="newBackgroundId">The ID of the new background skin to load.</param>
    public void SetNewBackground(int newBackgroundId)
    {
        Sprite newBackground = backgroundSkins[newBackgroundId];
        SpriteRenderer backgroundSpriteRenderer = background.GetComponent<SpriteRenderer>();
        backgroundSpriteRenderer.sprite = newBackground;
    }
}