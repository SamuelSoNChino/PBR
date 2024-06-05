using System.Collections;
using System.Threading.Tasks;
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
    /// The end screen manager component for managing start screen.
    /// </summary>
    [SerializeField] private StartScreenManager startScreenManager;

    /// <summary>
    /// A script that manages panning and zooming during the game.
    /// </summary>
    [SerializeField] private PanZoom panZoom;

    /// <summary>
    /// Starts the game by starting the StartGame coroutine.
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
        // Starts the countdown on the start screen and tracks whether it is finished 
        TaskCompletionSource<bool> countdownFinished = new();
        StartCoroutine(startScreenManager.StartCountdown(countdownFinished));

        // If the numberOfTiles hasn't been set in the options scene yet, sets it to te default value
        if (!PlayerPrefs.HasKey("numberOfTiles"))
        {
            PlayerPrefs.SetInt("numberOfTiles", 5);
        }

        // Sets the number of tiles in puzzle generator and requests both the puzzle and grid images
        puzzleGenerator.SetNumberOfTiles(PlayerPrefs.GetInt("numberOfTiles"));
        yield return StartCoroutine(puzzleGenerator.RequestPuzzleImage(0));
        yield return StartCoroutine(puzzleGenerator.RequestGridImage());

        // Generates both puzzle tiles and grid tiles
        puzzleGenerator.GenerateGridTiles();
        puzzleGenerator.GeneratePuzzleTiles();

        // Shuffles the puzzle tiles
        tilesManager.ShuffleAllTiles();

        // Waits for the countdown to finish
        yield return new WaitUntil(() => countdownFinished.Task.IsCompleted);

        // Enables touch input and manipulating with the tiles
        panZoom.EnableTouchInput();
        tilesManager.EnableAllColiders();

        // Starts the timer
        timer.EnableTimer();
    }

    /// <summary>
    /// Ends the game by stopping the timer, recording the final time, and loading the end screen scene.
    /// </summary>
    public void EndGame()
    {
        // Stops the timer and saves the the new time values
        timer.DisableTimer();
        int finalTime = timer.GetCurrentTime();
        UpdateTimeValues(finalTime);

        // Prevents the player from manipulating the game behind the end screen
        tilesManager.DisableAllColiders();
        panZoom.DisableTouchInput();

        // Loads the end screen
        endScreenManager.EnableEndScreen();
    }

    /// <summary>
    /// Updates the time newTime and bestTime values in PlayerPrefs based on the finla time.
    /// </summary>
    /// <param name="finalTime">The resulting time from the last game.</param>
    public void UpdateTimeValues(int finalTime)
    {
        // Loads the number of tiles for accessing the values for the correct game mode (number of tiles played with)
        string NumberOfTiles = PlayerPrefs.GetInt("numberOfTiles").ToString();

        // Saves the finalTime value to PlayerPrefs (finalTime = resluting time = newTime)
        PlayerPrefs.SetInt("newTime-" + NumberOfTiles, finalTime);

        // If the gamemode (number of tiles played with) was played for the first time or final time is better than current bestTime
        if (!PlayerPrefs.HasKey("bestTime-" + NumberOfTiles) || finalTime < PlayerPrefs.GetInt("bestTime-" + NumberOfTiles))
        {

            // Sets the finalTime as a new best time for the current game mode (number of tiles played with)
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