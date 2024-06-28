using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the start screen, the matchmaking text, and the countdown before the match.
/// Works only with multiplayer.
/// </summary>
public class StartScreenManagerMultiplayer : NetworkBehaviour
{
    [SerializeField] private GameObject startScreen;
    
    /// <summary>
    /// Text component to display the matchmaking and countdown messages.
    /// </summary>
    [SerializeField] private TextMeshProUGUI startScreenText;

    /// <summary>
    /// Interval between updating the matchmaking dots, in seconds.
    /// </summary>
    [SerializeField] private float cyclingInterval = 0.5f;

    /// <summary>
    /// Base text for the matchmaking message.
    /// </summary>
    [SerializeField] private string matchmakingText = "Looking for an opponent";

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
    /// The button for canceling matchmaking (functionality in MultiplayerManager).
    /// </summary>
    [SerializeField] private Button cancelButton;

    /// <summary>
    /// Number of dots in the matchmaking message.
    /// </summary>
    private int dotCount = 0;

    /// <summary>
    /// Flag to stop the matchmaking cycle.
    /// </summary>
    private bool stopCycling = false;

    /// <summary>
    /// Coroutine to cycle through the matchmaking message with dots.
    /// </summary>
    /// <returns>Coroutine enumerator.</returns>
    private IEnumerator MatchmakingCycle()
    {
        // Cycles until the StopMatchmakingCycle was called
        while (!stopCycling)
        {
            // dotCount rotate between 0 and 3
            dotCount = (dotCount + 1) % 4;

            // Updates the startScreen text with the new dot count
            startScreenText.text = matchmakingText + new string('.', dotCount);

            // Waits for the length of the cycling interval
            yield return new WaitForSeconds(cyclingInterval);
        }
    }

    /// <summary>
    /// Starts the matchmaking message cycle.
    /// </summary>
    public void StartMatchmakingCycle()
    {
        StartCoroutine(MatchmakingCycle());
    }

    /// <summary>
    /// Stops the matchmaking message cycle.
    /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    public void StopMatchmakingCycleRpc()
    {
        stopCycling = true;
    }

    /// <summary>
    /// Enables the cancel button, making it visible and interactable.
    /// </summary>
    public void EnableCancelButton()
    {
        cancelButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// Disables the cancel button, making it invisible and uninteractable.
    /// </summary>
    public void DisableCancelButton()
    {
        cancelButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Starts the countdown sequence on all clients and the host.
    /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    public void StartCountdownRpc()
    {
        StartCoroutine(StartCountdown());
    }

    /// <summary>
    /// Starts the countdown sequence.
    /// </summary>
    /// <returns>Coroutine enumerator.</returns>
    public IEnumerator StartCountdown()
    {
        // Activates the start screen object, making it visible (needed for a Rematch)
        startScreen.SetActive(true);

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
        startScreen.SetActive(false);
    }
}