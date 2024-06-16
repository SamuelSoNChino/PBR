using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private TextMeshProUGUI peekText;
    [SerializeField] private GameObject peekIndicator;
    [SerializeField] private GameObject peekIndicatorDanger;


    private bool isPeeking = false;
    private bool targetPeeking = false;
    private bool isBeingPeeked = false;


    private List<ulong> targetClientIds = new();
    private List<ulong> userClientIds = new();
    private List<Vector3[]> originalPositions = new();

    public void Peek()
    {
        Debug.Log($"[Client] Client {NetworkManager.Singleton.LocalClientId} requesting peek.");
        RequestPeekRpc(NetworkManager.Singleton.LocalClientId, tilesManager.GetAllPositions());
    }

    public void StopPeeking()
    {
        Debug.Log($"[Client] Client {NetworkManager.Singleton.LocalClientId} requesting to stop peeking.");
        RequestStopPeekingRpc(NetworkManager.Singleton.LocalClientId);
    }

    public bool GetPeekingStatus()
    {
        return isPeeking;
    }


    [Rpc(SendTo.Server)]
    private void RequestPeekRpc(ulong userClientId, Vector3[] userOriginalPositions)
    {
        Debug.Log($"[Server] Received peek request from Client {userClientId}");

        if (!userClientIds.Contains(userClientId))
        {
            // Will be replaced with appropriate logic
            ulong targetClientId = 0;
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (clientId != userClientId)
                {
                    targetClientId = clientId;
                    break;
                }
            }

            Debug.Log($"[Server] Client {userClientId} is peeking at Client {targetClientId}.");
            userClientIds.Add(userClientId);
            originalPositions.Add(userOriginalPositions);
            targetClientIds.Add(targetClientId);


            StartPeekRoutineServer(userClientId, targetClientId);

            StartPeekRoutineUserRpc(userClientId, targetClientId);

            StartPeekRoutineTargetRpc(targetClientId);
        }
    }

    private void StartPeekRoutineServer(ulong userClientId, ulong targetClientId)
    {
        bool targetAlsoPeeking = userClientIds.Contains(targetClientId);
        if (targetAlsoPeeking)
        {
            Debug.Log($"[Server] Target Client {targetClientId} is also peeking.");
            int targetPeekSessionIndex = userClientIds.IndexOf(targetClientId);
            SendPuzzleToClientRpc(userClientId, originalPositions[targetPeekSessionIndex]);
            NotifyTargetIsPeekingRpc(userClientId);
        }
        else
        {
            NotifyTargetIsNotPeekingRpc(userClientId);
        }

        bool userBeingPeeked = targetClientIds.Contains(userClientId);
        if (userBeingPeeked)
        {
            Debug.Log($"[Server] User is also being peeked");
            int peekSessionIndex = targetClientIds.IndexOf(userClientId);
            ulong peekerClientId = userClientIds[peekSessionIndex];
            NotifyTargetIsPeekingRpc(peekerClientId);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void StartPeekRoutineUserRpc(ulong userClientId, ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == userClientId)
        {
            Debug.Log($"[Client] Starting peek routine as user. Target: {targetClientId}");

            isPeeking = true;

            peekButton.gameObject.SetActive(false);
            stopButton.gameObject.SetActive(true);

            peekText.gameObject.SetActive(true);
            peekText.text = $"You are peeking at: {targetClientId}";
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void StartPeekRoutineTargetRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            Debug.Log($"[Client] Starting peek routine as target.");
            isBeingPeeked = true;
        }
    }

    // ------------------------------------------------------------------------------------------------------------------------

    [Rpc(SendTo.Server)]
    private void RequestStopPeekingRpc(ulong userClientId)
    {
        Debug.Log($"[Server] Received stop peek request from Client {userClientId}");

        if (userClientIds.Contains(userClientId))
        {
            int peekSessionIndex = userClientIds.IndexOf(userClientId);
            ulong targetClientId = targetClientIds[peekSessionIndex];

            StopPeekRoutineServer(userClientId, targetClientId);

            StopPeekRoutineUserRpc(userClientId, originalPositions[peekSessionIndex]);

            StopPeekRoutineTargetRpc(targetClientId);

            userClientIds.RemoveAt(peekSessionIndex);
            targetClientIds.RemoveAt(peekSessionIndex);
            originalPositions.RemoveAt(peekSessionIndex);
        }
    }

    private void StopPeekRoutineServer(ulong userClientId, ulong targetClientId)
    {
        for (int i = 0; i < targetClientIds.Count; i++)
        {
            if (userClientId == targetClientIds[i])
            {
                Debug.Log($"[Server] Disabling colliders for Client {targetClientIds[i]}");
                NotifyTargetIsNotPeekingRpc(targetClientIds[i]);
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void StopPeekRoutineUserRpc(ulong clientId, Vector3[] userOriginalPositions)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            Debug.Log($"[Client] Stopping peek routine as user.");
            isPeeking = false;

            peekButton.gameObject.SetActive(true);
            stopButton.gameObject.SetActive(false);
            peekText.gameObject.SetActive(false);

            tilesManager.DeselectAllTiles();
            tilesManager.SetAllPositions(userOriginalPositions);
            tilesManager.EnableAllColliders();
            tilesManager.SnapAllToGrid();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void StopPeekRoutineTargetRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            Debug.Log($"[Client] Stopping peek routine as target.");
            isBeingPeeked = false;
        }
    }

    //----------------------------------------------------------------------------------------------------------------------------

    [Rpc(SendTo.ClientsAndHost)]
    private void NotifyTargetIsNotPeekingRpc(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log($"[Client] Disabling all colliders.");
            tilesManager.DisableAllColliders();
            tilesManager.DeselectAllTiles();
            targetPeeking = false;

        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void NotifyTargetIsPeekingRpc(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log($"[Client] Enabling all colliders.");
            tilesManager.UnsnapAllFromGrid();
            tilesManager.EnableAllColliders();
            tilesManager.DeselectAllTiles();
            targetPeeking = true;
        }
    }

    [Rpc(SendTo.Server)]
    private void SendPuzzleToServerRpc(ulong targetClientId, Vector3[] positions)
    {
        for (int i = 0; i < targetClientIds.Count; i++)
        {
            if (targetClientId == targetClientIds[i])
            {
                ulong userClientId = userClientIds[i];
                SendPuzzleToClientRpc(userClientId, positions);
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SendPuzzleToClientRpc(ulong userClientId, Vector3[] positions)
    {
        if (userClientId == NetworkManager.Singleton.LocalClientId)
        {
            tilesManager.SetAllPositions(positions);
        }
    }

    [Rpc(SendTo.Server)]
    private void UpdateOriginalPositionsServerRpc(ulong clientId, Vector3[] positions)
    {

        if (userClientIds.Contains(clientId))
        {
            int peekSessionIndex = userClientIds.IndexOf(clientId);
            ulong targetClientId = targetClientIds[peekSessionIndex];

            if (userClientIds.Contains(targetClientId))
            {
                int peekerSessionIndex = userClientIds.IndexOf(targetClientId);
                originalPositions[peekerSessionIndex] = positions;
            }
        }
    }

    private void UpdatePeekIndicator()
    {
        if (isBeingPeeked)
        {
            if (isPeeking)
            {
                peekIndicatorDanger.SetActive(true);
                peekIndicator.SetActive(false);
            }
            else
            {
                peekIndicator.SetActive(true);
                peekIndicatorDanger.SetActive(false);
            }
        }
        else
        {
            peekIndicatorDanger.SetActive(false);
            peekIndicator.SetActive(false);
        }
    }

    private void Update()
    {
        UpdatePeekIndicator();

        if (isBeingPeeked && !isPeeking)
        {
            SendPuzzleToServerRpc(NetworkManager.Singleton.LocalClientId, tilesManager.GetAllPositions());
        }
        else if (isPeeking && targetPeeking)
        {
            UpdateOriginalPositionsServerRpc(NetworkManager.Singleton.LocalClientId, tilesManager.GetAllPositions());
        }
    }
}