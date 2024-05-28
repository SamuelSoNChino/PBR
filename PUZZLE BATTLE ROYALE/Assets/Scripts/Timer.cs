using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the time counter during a single-player game.
/// </summary>
public class Timer : MonoBehaviour
{
    /// <summary>
    /// The elapsed time in seconds.
    /// </summary>
    private float time = 0;

    /// <summary>
    /// Indicates whether the timer is currently enabled.
    /// </summary>
    private bool timerEnabled = false;

    /// <summary>
    /// The UI text component that displays the timer.
    /// </summary>
    [SerializeField] private Text timerText;

    /// <summary>
    /// Enables the timer, allowing it to start counting.
    /// </summary>
    public void EnableTimer()
    {
        timerEnabled = true;
    }

    /// <summary>
    /// Disables the timer, stopping it from counting.
    /// </summary>
    public void DisableTimer()
    {
        timerEnabled = false;
    }

    /// <summary>
    /// Gets the current time in seconds.
    /// </summary>
    /// <returns>The current elapsed time in seconds.</returns>
    public int GetCurrentTime()
    {
        return (int)time;
    }

    /// <summary>
    /// Updates the timer each frame if the timer is enabled.
    /// </summary>
    void Update()
    {
        if (timerEnabled)
        {
            // Increment the time by the time passed since the last frame
            time += Time.deltaTime;
            // Update the timer text to show the elapsed time in seconds
            timerText.text = $"Time: {GetCurrentTime()}";
        }
    }
}