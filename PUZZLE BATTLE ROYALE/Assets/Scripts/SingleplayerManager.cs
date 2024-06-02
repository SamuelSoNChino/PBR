using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the singleplayer game flow.
/// </summary>
public class SingleplayerManager : MonoBehaviour
{
    /// <summary>
    /// The timer component for tracking game time.
    /// </summary>
    [SerializeField] private Timer timer;

    /// <summary>
    /// The puzzle generator component for generating puzzle tiles.
    /// </summary>
    [SerializeField] private PuzzleGenerator puzzleGenerator;

    /// <summary>
    /// The tiles manager component for managing puzzle tiles.
    /// </summary>
    [SerializeField] private TilesManager tilesManager;

    /// <summary>
    /// The end screen manager component for managing end screen.
    /// </summary>
    [SerializeField] private EndScreenManager endScreenManager;

    /// <summary>
    /// Starts the game by requesting puzzle and grid images, generating tiles, shuffling them, and starting the timer.
    /// </summary>
    void Start()
    {
        StartCoroutine(StartGame());
    }

    /// <summary>
    /// Coroutine to start the game by requesting puzzle and grid images, generating tiles, shuffling them, and starting the timer.
    /// </summary>
    /// <returns>An IEnumerator for the coroutine.</returns>
    IEnumerator StartGame()
    {
        // If the numberOfTiles wasn't set yet in the options scene, sets it to te default value
        if (!PlayerPrefs.HasKey("numberOfTiles"))
        {
            PlayerPrefs.SetInt("numberOfTiles", 5);
        }

        puzzleGenerator.SetNumberOfTiles(PlayerPrefs.GetInt("numberOfTiles"));
        yield return StartCoroutine(puzzleGenerator.RequestPuzzleImage(0));
        yield return StartCoroutine(puzzleGenerator.RequestGridImage());
        puzzleGenerator.GenerateTiles();
        tilesManager.ShuffleAllTiles();
        timer.EnableTimer();
    }

    /// <summary>
    /// Ends the game by stopping the timer, recording the final time, and loading the end screen scene.
    /// </summary>
    public void EndGame()
    {
        timer.DisableTimer();
        int finalTime = timer.GetCurrentTime();
        UpdateTimeValues(finalTime);
        endScreenManager.EnableEndScreen();
    }

    /// <summary>
    /// Updates the time newTime and bestTime values in PlayerPrefs based on the finla time.
    /// </summary>
    /// <param name="finalTime">The resulting time from the last game.</param>
    public void UpdateTimeValues(int finalTime)
    {
        // Loads the number of tiles for accessing the values for the correct game mode
        string NumberOfTiles = PlayerPrefs.GetInt("numberOfTiles").ToString();
        PlayerPrefs.SetInt("newTime-" + NumberOfTiles, finalTime);
        // If the gamemode was player for the first time or final time is better than the current bestTime stores the finalTime value
        if (!PlayerPrefs.HasKey("bestTime-" + NumberOfTiles) || finalTime < PlayerPrefs.GetInt("bestTime-" + NumberOfTiles))
        {
            PlayerPrefs.SetInt("bestTime-" + NumberOfTiles, finalTime);
        }
    }

    /// <summary>
    /// Loads the main menu scene.
    /// </summary>
    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}