using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages the start screen and the countdown before the game begins.
/// WORKS ONLY WITH SINGLEPLAYER
/// </summary>
public class StartScreenManager : MonoBehaviour
{
    /// <summary>
    /// Text component to display the countdown.
    /// </summary>
    [SerializeField] private TextMeshProUGUI startScreenText;

    /// <summary>
    /// Base text for the countdown message.
    /// </summary>
    [SerializeField] private string countdownText = "Match starting in";

    /// <summary>
    /// Number of seconds to count down from.
    /// </summary>
    [SerializeField] private int countdownStart = 3;

    /// <summary>
    /// Interval between countdown updates, in seconds.
    /// </summary>
    [SerializeField] private float countdownInterval = 1f;

    /// <summary>
    /// Starts the countdown sequence.
    /// </summary>
    /// <param name="countdownFinished">Task completion source to signal when the countdown is finished.</param>
    /// <returns>Coroutine enumerator.</returns>
    public IEnumerator StartCountdown(TaskCompletionSource<bool> countdownFinished)
    {
        // Counts down from the countdown start
        for (int i = countdownStart; i >= 0; i--)
        {
            // Updates the start screen text
            startScreenText.text = $"{countdownText}: {i}";

            // Waits for the length of countdown interval
            yield return new WaitForSeconds(countdownInterval);
        }

        // Updates the start screen text and waits for the length of countdown interval
        startScreenText.text = "GO!";
        yield return new WaitForSeconds(countdownInterval);

        // Deactivates the start screen object, making it not visible
        gameObject.SetActive(false);

        // Sets the countdown task completion source to completed
        countdownFinished.SetResult(true);
    }
}