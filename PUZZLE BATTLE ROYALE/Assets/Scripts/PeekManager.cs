using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.UI;

public class PeekManager : NetworkBehaviour
{
    [SerializeField] private TilesManager tilesManager;
    [SerializeField] private Button peekButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private TextMeshProUGUI peekTimer;
    [SerializeField] private GameObject peekIndicator;
    [SerializeField] private float peekLenght;

    private Vector3[] originalPositions;
    private float timeRemaining;
    private bool timerRunning;


    private bool sendingPuzzle;


    private ulong targetClientId = 0;
    private ulong userClientId = 0;

    public void Peek()
    {
        Debug.Log($"[Client] Client {NetworkManager.Singleton.LocalClientId} requesting peek.");
        RequestPeekRpc(NetworkManager.Singleton.LocalClientId);
    }

    public void StopPeeking()
    {
        RequestStopPeekingRpc(NetworkManager.Singleton.LocalClientId);
    }

    [Rpc(SendTo.Server)]
    private void RequestStopPeekingRpc(ulong sentUserClientId)
    {
        if (sentUserClientId == userClientId)
        {
            StopCoroutine(PeekServerRoutine());

            StopPeekRoutineTargetRpc(targetClientId);
            StopPeekRoutineUserRpc(userClientId);

            userClientId = 0;
            targetClientId = 0;
        }

    }

    [Rpc(SendTo.Server)]
    private void RequestPeekRpc(ulong sentUserClientId)
    {
        userClientId = sentUserClientId;
        Debug.Log($"[Server] Connected clients: {string.Join(", ", NetworkManager.Singleton.ConnectedClientsIds)}");

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientId != userClientId)
            {
                targetClientId = clientId;
                break;
            }
        }

        Debug.Log($"[Server] Client: {userClientId} has requested to use Peek on {targetClientId}.");
        StartCoroutine(PeekServerRoutine());
    }

    private IEnumerator PeekServerRoutine()
    {
        StartPeekRoutineUserRpc(userClientId);
        StartPeekRoutineTargetRpc(targetClientId);

        yield return new WaitForSeconds(peekLenght);
        Debug.Log("[Server] The peek timer has ended. Requesting to stop sending positions periodically and restore original puzzle.");

        StopPeekRoutineTargetRpc(targetClientId);
        StopPeekRoutineUserRpc(userClientId);

        userClientId = 0;
        targetClientId = 0;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void StartPeekRoutineUserRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            peekButton.gameObject.SetActive(false);
            stopButton.gameObject.SetActive(true);

            timerRunning = true;
            timeRemaining = peekLenght;
            peekTimer.gameObject.SetActive(true);

            originalPositions = tilesManager.GetAllPositions();
            tilesManager.DisableAllColliders();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void StopPeekRoutineUserRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            peekButton.gameObject.SetActive(true); // Implement cooldown instead
            stopButton.gameObject.SetActive(false);

            timerRunning = false;
            peekTimer.gameObject.SetActive(false);

            Debug.Log("[Client] Restoring original puzzle.");
            tilesManager.SetAllPositions(originalPositions);
            originalPositions = null;
            tilesManager.EnableAllColliders();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void StartPeekRoutineTargetRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            Debug.Log("[Client] Starting to send positions periodically.");
            sendingPuzzle = true;
            peekIndicator.SetActive(true);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void StopPeekRoutineTargetRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            Debug.Log("[Client] Stopping to send positions periodically.");
            sendingPuzzle = false;
            peekIndicator.SetActive(false);
        }
    }

    [Rpc(SendTo.Server)]
    private void SendPuzzleToServerRpc(Vector3[] positions)
    {
        SendPuzzleToClientRpc(userClientId, positions);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SendPuzzleToClientRpc(ulong clientId, Vector3[] positions)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            // To avoid sending the position after it has stopped
            if (originalPositions != null)
            {
                tilesManager.SetAllPositions(positions);
            }
        }
    }

    private void Update()
    {
        if (sendingPuzzle)
        {
            SendPuzzleToServerRpc(tilesManager.GetAllPositions());
        }
        if (timerRunning)
        {
            timeRemaining -= Time.deltaTime;
            peekTimer.text = $"{(int)timeRemaining}";
        }
    }
}