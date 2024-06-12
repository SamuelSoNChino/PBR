using System.Collections;
using System.Collections.Generic;
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

    // Peek user fields
    private bool isPeeking = false;
    private Vector3[] originalPositions;
    private float timeRemaining;

    // Peek target fields
    private bool isBeingPeeked;

    // ServerFields
    private List<ulong> targetClientIds = new();
    private List<ulong> userClientIds = new();
    private List<Coroutine> runningPeekCoroutines = new();

    public void Peek()
    {
        Debug.Log($"[Client] Client {NetworkManager.Singleton.LocalClientId} requesting peek.");
        RequestPeekRpc(NetworkManager.Singleton.LocalClientId);
    }

    [Rpc(SendTo.Server)]
    private void RequestPeekRpc(ulong userClientId)
    {
        if (!userClientIds.Contains(userClientId))
        {
            ulong targetClientId = 0;

            // Temporary solution, later the player will be able to choose
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (clientId != userClientId)
                {
                    targetClientId = clientId;
                    break;
                }
            }

            Debug.Log($"[Server] Client: {userClientId} has requested to use Peek on {targetClientId}.");
            userClientIds.Add(userClientId);
            targetClientIds.Add(targetClientId);

            Coroutine coroutine = StartCoroutine(PeekServerRoutine(userClientId, targetClientId));
            runningPeekCoroutines.Add(coroutine);
        }

    }


    public void StopPeeking()
    {
        RequestStopPeekingRpc(NetworkManager.Singleton.LocalClientId);
    }

    [Rpc(SendTo.Server)]
    private void RequestStopPeekingRpc(ulong userClientId)
    {
        if (userClientIds.Contains(userClientId))
        {
            int peekSessionIndex = userClientIds.IndexOf(userClientId);

            ulong targetClientId = targetClientIds[peekSessionIndex];
            Coroutine peekCoroutine = runningPeekCoroutines[peekSessionIndex];

            StopCoroutine(peekCoroutine);
            StopPeekRoutineTargetRpc(targetClientId);
            StopPeekRoutineUserRpc(userClientId);

            runningPeekCoroutines.RemoveAt(peekSessionIndex);
            userClientIds.RemoveAt(peekSessionIndex);
            targetClientIds.RemoveAt(peekSessionIndex);
        }

    }


    private IEnumerator PeekServerRoutine(ulong userClientId, ulong targetClientId)
    {
        StartPeekRoutineUserRpc(userClientId);
        StartPeekRoutineTargetRpc(targetClientId);

        yield return new WaitForSeconds(peekLenght);
        Debug.Log("[Server] The peek timer has ended. Requesting to stop sending positions periodically and restore original puzzle.");

        StopPeekRoutineTargetRpc(targetClientId);
        StopPeekRoutineUserRpc(userClientId);

        int peekSessionIndex = userClientIds.IndexOf(userClientId);

        runningPeekCoroutines.RemoveAt(peekSessionIndex);
        userClientIds.RemoveAt(peekSessionIndex);
        targetClientIds.RemoveAt(peekSessionIndex);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void StartPeekRoutineUserRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            isPeeking = true;

            peekButton.gameObject.SetActive(false);
            stopButton.gameObject.SetActive(true);

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
            isPeeking = false;

            peekButton.gameObject.SetActive(true); // Implement cooldown instead
            stopButton.gameObject.SetActive(false);
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
            isBeingPeeked = true;

            Debug.Log("[Client] Starting to send positions periodically.");
            peekIndicator.SetActive(true);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void StopPeekRoutineTargetRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            isBeingPeeked = false;

            Debug.Log("[Client] Stopping to send positions periodically.");
            peekIndicator.SetActive(false);
        }
    }

    [Rpc(SendTo.Server)]
    private void SendPuzzleToServerRpc(ulong targetClientId, Vector3[] positions)
    {
        int peekSessionIndex = targetClientIds.IndexOf(targetClientId);
        ulong userClientId = userClientIds[peekSessionIndex];

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
        if (isBeingPeeked)
        {
            SendPuzzleToServerRpc(NetworkManager.Singleton.LocalClientId, tilesManager.GetAllPositions());
        }

        if (isPeeking)
        {
            timeRemaining -= Time.deltaTime;
            peekTimer.text = $"{(int)timeRemaining}";
        }
    }
}
