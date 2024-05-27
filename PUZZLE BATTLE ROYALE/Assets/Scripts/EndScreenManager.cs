using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles the end screen scene.
/// </summary>
public class EndScreenManager : MonoBehaviour
{
    /// <summary>
    /// Sets the end screen text to the loaded time values.
    /// </summary>
    void Start()
    {
        GameObject.Find("End Time").GetComponent<Text>().text += PlayerPrefs.GetInt("LastTime");
        GameObject.Find("Best Time").GetComponent<Text>().text += PlayerPrefs.GetInt("BestTime");
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