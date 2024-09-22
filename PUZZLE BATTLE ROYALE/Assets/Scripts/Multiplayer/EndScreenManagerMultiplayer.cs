using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the end screen for multiplayer matches, handling the display of winning or losing screens,
/// and managing rematch requests and responses.
/// </summary>
public class EndScreenManagerMultiplayer : NetworkBehaviour
{
    // -----------------------------------------------------------------------
    // End Screens
    // -----------------------------------------------------------------------

    /// <summary>
    /// GameObject representing the winning screen.
    /// </summary>
    [SerializeField] private GameObject winningScreen;

    /// <summary>
    /// GameObject representing the losing screen.
    /// </summary>
    [SerializeField] private GameObject losingScreen;

    /// <summary>
    /// Currently active end screen (either winning or losing screen).
    /// </summary>
    private GameObject currentEndScreen;

    /// <summary>
    /// Loads the winning screen on the client specified by clientId. If clientId is not specified, targets all clients.
    /// </summary>
    /// <param name="clientId">The ID of the client to display the winning screen.</param>
    [Rpc(SendTo.ClientsAndHost)]
    public void LoadWinningScreenRpc(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            currentEndScreen = winningScreen;
            currentEndScreen.SetActive(true);
        }
    }

    /// <summary>
    /// Loads the losing screen on the client specified by clientId. If clientId is not specified, targets all clients.
    /// </summary>
    /// <param name="clientId">The ID of the client to display the losing screen.</param>
    [Rpc(SendTo.ClientsAndHost)]
    public void LoadLosingScreenRpc(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            currentEndScreen = losingScreen;
            currentEndScreen.SetActive(true);
        }
    }

    /// <summary>
    /// Unloads the current end screen for all clients.
    /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    public void UnloadEndScreenRpc()
    {
        currentEndScreen.SetActive(false);
        currentEndScreen = null;
    }

    // -----------------------------------------------------------------------
    // Menu Navigation
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns to the main menu, shutting down the network manager.
    /// </summary>
    public void BackToMenu()
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("Menu");
    }
}