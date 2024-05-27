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
    /// Loads default values if the player is playing for the first time.
    /// </summary>
    void Start()
    {
        if (!PlayerPrefs.HasKey("Tiles"))
        {
            PlayerPrefs.SetInt("Tiles", 5);
        }
        if (!PlayerPrefs.HasKey("BestTime"))
        {
            PlayerPrefs.SetInt("BestTime", -1);
        }
    }

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