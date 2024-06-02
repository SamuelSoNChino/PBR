using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the menu scene.
/// </summary>
public class MenuManager : MonoBehaviour
{
    /// <summary>
    /// Loads the singleplayer puzzle scene.
    /// </summary>
    public void StartSingleplayer()
    {
        SceneManager.LoadScene("PuzzleSingleplayer");
    }

    /// <summary>
    /// Loads the multiplayer puzzle scene.
    /// </summary>
    public void StartMultiplayer()
    {
        SceneManager.LoadScene("PuzzleMultiplayer");
    }

    /// <summary>
    /// Loads the options scene.
    /// </summary>
    public void GoToOptions()
    {
        SceneManager.LoadScene("Options");
    }
}