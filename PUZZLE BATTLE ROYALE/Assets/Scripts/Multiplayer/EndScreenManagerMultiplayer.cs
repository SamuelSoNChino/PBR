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
    /// <param name="clientId">The ID of the client to display the winning screen. Defaults to all clients.</param>
    [Rpc(SendTo.ClientsAndHost)]
    public void LoadWinningScreenRpc(ulong clientId = 1234567890)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId || clientId == 1234567890)
        {
            currentEndScreen = winningScreen;
            DisableRematchText();
            currentEndScreen.SetActive(true);
        }
    }

    /// <summary>
    /// Loads the losing screen on the client specified by clientId. If clientId is not specified, targets all clients.
    /// </summary>
    /// <param name="clientId">The ID of the client to display the losing screen. Defaults to all clients.</param>
    [Rpc(SendTo.ClientsAndHost)]
    public void LoadLosingScreenRpc(ulong clientId = 1234567890)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId || clientId == 1234567890)
        {
            currentEndScreen = losingScreen;
            DisableRematchText();
            currentEndScreen.SetActive(true);
        }
    }

    /// <summary>
    /// Unloads the current end screen on the client specified by clientId. If clientId is not specified, targets all clients.
    /// </summary>
    /// <param name="clientId">The ID of the client to unload the end screen. Defaults to all clients.</param>
    [Rpc(SendTo.ClientsAndHost)]
    public void UnloadEndScreenRpc(ulong clientId = 1234567890)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId || clientId == 1234567890)
        {
            currentEndScreen.SetActive(false);
            currentEndScreen = null;
        }
    }

    // -----------------------------------------------------------------------
    // Rematch Management
    // -----------------------------------------------------------------------

    /// <summary>
    /// Reference to the MultiplayerManager script.
    /// </summary>
    [SerializeField] private MultiplayerManager multiplayerManager;

    /// <summary>
    /// Indicates if a rematch is currently pending.
    /// </summary>
    private bool rematchPending;

    /// <summary>
    /// Enables the rematch text on the current end screen.
    /// </summary>
    private void EnableRematchText()
    {
        currentEndScreen?.transform.Find("RematchText").gameObject.SetActive(true);
    }

    /// <summary>
    /// Disables the rematch text on the current end screen.
    /// </summary>
    private void DisableRematchText()
    {
        currentEndScreen?.transform.Find("RematchText").gameObject.SetActive(false);
    }

    /// <summary>
    /// Sets the rematch pending status to true and enables the rematch text.
    /// </summary>
    public void EnableRematchPending()
    {
        rematchPending = true;
        EnableRematchText();
    }

    /// <summary>
    /// Sets the rematch pending status to false and disables the rematch text.
    /// </summary>
    public void DisableRematchPending()
    {
        rematchPending = false;
        DisableRematchText();
    }

    /// <summary>
    /// Handles the rematch button functionality, either sending a rematch request or accepting a pending one.
    /// </summary>
    public void Rematch()
    {
        Debug.Log("Rematch pending status:" + rematchPending);

        if (rematchPending)
        {
            DisableRematchPending();
            multiplayerManager.AcceptRematchServerRpc();
        }
        else
        {
            multiplayerManager.RequestRematchServerRpc(NetworkManager.Singleton.LocalClientId);
        }
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