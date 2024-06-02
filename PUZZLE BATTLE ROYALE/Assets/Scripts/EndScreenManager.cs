using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages the end screen, displaying the final time and best time, and provides options to return to the menu or play again.
/// </summary>
public class EndScreenManager : MonoBehaviour
{
    /// <summary>
    /// The UI text component that displays the player's end time.
    /// </summary>
    [SerializeField] private Text endTimeText;

    /// <summary>
    /// The UI text component that displays the player's best time.
    /// </summary>
    [SerializeField] private Text bestTimeText;

    /// <summary>
    /// Initializes the end screen by setting the end time and best time text fields to the respective saved values.
    /// </summary>
    private void Start()
    {
        string numberOfTiles = PlayerPrefs.GetInt("numberOfTiles").ToString();
        endTimeText.text += PlayerPrefs.GetInt("newTime-" + numberOfTiles);
        bestTimeText.text += PlayerPrefs.GetInt("bestTime-" + numberOfTiles);
    }

    /// <summary>
    /// Enables the end screen game object, making it visible.
    /// </summary>
    public void EnableEndScreen()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Loads the menu scene.
    /// </summary>
    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    /// <summary>
    /// Loads the puzzle singleplayer scene to play again.
    /// </summary>
    public void PlayAgain()
    {
        SceneManager.LoadScene("PuzzleSingleplayer");
    }
}