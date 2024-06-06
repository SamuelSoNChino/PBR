using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the end screen for multiplayer matches, handling the display of winning or losing screens,
/// and managing rematch requests and responses.
/// WORKS ONLY WITH MULTIPLAYER.
/// </summary>
public class EndScreenManagerMultiplayer : MonoBehaviour
{
    /// <summary>
    /// GameObject representing the winning screen.
    /// </summary>
    [SerializeField] private GameObject winningScreen;

    /// <summary>
    /// GameObject representing the losing screen.
    /// </summary>
    [SerializeField] private GameObject losingScreen;

    /// <summary>
    /// Reference to the MultiplayerManager script.
    /// </summary>
    [SerializeField] private MultiplayerManager multiplayerManager;

    /// <summary>
    /// Currently active end screen (either winning or losing screen).
    /// </summary>
    private GameObject currentEndScreen;

    /// <summary>
    /// Indicates if a rematch is currently pending.
    /// </summary>
    private bool rematchPending;

    /// <summary>
    /// Loads and displays the winning screen.
    /// </summary>
    public void LoadWinningScreen()
    {
        currentEndScreen = winningScreen;
        DisableRematchText();
        currentEndScreen.SetActive(true);
    }

    /// <summary>
    /// Loads and displays the losing screen.
    /// </summary>
    public void LoadLosingScreen()
    {
        currentEndScreen = losingScreen;
        DisableRematchText();
        currentEndScreen.SetActive(true);
    }

    /// <summary>
    /// Unloads and hides the current end screen.
    /// </summary>
    public void UnloadEndScreen()
    {
        currentEndScreen.SetActive(false);
        currentEndScreen = null;
    }

    /// <summary>
    /// Returns to the main menu, shutting down the network manager.
    /// </summary>
    public void BackToMenu()
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("Menu");
    }

    /// <summary>
    /// Disables the rematch text on the current end screen.
    /// </summary>
    private void DisableRematchText()
    {
        currentEndScreen.transform.Find("RematchText").gameObject.SetActive(false);
    }

    /// <summary>
    /// Enables the rematch text on the current end screen.
    /// </summary>
    private void EnableRematchText()
    {
        currentEndScreen.transform.Find("RematchText").gameObject.SetActive(true);
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

        // If the rematch request is already pending from other device
        if (rematchPending)
        {
            // Disables the rematch text and pending status
            DisableRematchPending();

            // Informs the server that the pending rematch was accepted
            multiplayerManager.AcceptRematchServerRpc();
        }
        else
        {
            // Sends a request for a rematch to the server
            multiplayerManager.RequestRematchServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }
}