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
        puzzleGenerator.SetNumberOfTiles(PlayerPrefs.GetInt("Tiles"));
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
        PlayerPrefs.SetInt("LastTime", finalTime);
        if (finalTime < PlayerPrefs.GetInt("BestTime") || PlayerPrefs.GetInt("BestTime") < 0)
        {
            PlayerPrefs.SetInt("BestTime", finalTime);
        }
        SceneManager.LoadScene("EndScreen");
    }

    /// <summary>
    /// Loads the main menu scene.
    /// </summary>
    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}