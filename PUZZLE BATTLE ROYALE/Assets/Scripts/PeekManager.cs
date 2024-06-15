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

    // Peek user fields
    private bool isPeeking = false;
    private bool canMove = false;

    // Peek target fields
    private bool isBeingPeeked = false;

    // ServerFields
    private List<ulong> targetClientIds = new();
    private List<ulong> userClientIds = new();
    private List<Vector3[]> originalPositions = new();

    public void Peek()
    {
        Debug.Log($"[Client] Client {NetworkManager.Singleton.LocalClientId} requesting peek.");
        RequestPeekRpc(NetworkManager.Singleton.LocalClientId, tilesManager.GetAllPositions());
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

            // User logic
            StartPeekRoutineUserRpc(userClientId, targetClientId);

            bool targetAlsoPeeking = userClientIds.Contains(targetClientId);
            if (targetAlsoPeeking)
            {
                Debug.Log($"[Server] Target Client {targetClientId} is also peeking.");
                int targetPeekSessionIndex = userClientIds.IndexOf(targetClientId);
                SendPuzzleToClientRpc(userClientId, originalPositions[targetPeekSessionIndex]);
                EnableAllCollidersRpc(userClientId);
            }
            else
            {
                DisableAllCollidersRpc(userClientId);
            }

            bool userBeingPeeked = targetClientIds.Contains(userClientId);
            if (userBeingPeeked)
            {
                Debug.Log($"[Server] User Client {userClientId} is also being peeked.");
                UpdatePeekIndicatorRpc(userClientId);
                for (int i = 0; i < targetClientIds.Count; i++)
                {
                    if (targetClientIds[1] == userClientId)
                    {
                        EnableAllCollidersRpc(userClientIds[i]);
                    }
                }
            }

            // Target logic
            StartPeekRoutineTargetRpc(targetClientId);
            UpdatePeekIndicatorRpc(targetClientId);
        }
    }

    public void StopPeeking()
    {
        Debug.Log($"[Client] Client {NetworkManager.Singleton.LocalClientId} requesting to stop peeking.");
        RequestStopPeekingRpc(NetworkManager.Singleton.LocalClientId);
    }

    [Rpc(SendTo.Server)]
    private void RequestStopPeekingRpc(ulong userClientId)
    {
        Debug.Log($"[Server] Received stop peek request from Client {userClientId}");

        if (userClientIds.Contains(userClientId))
        {
            int peekSessionIndex = userClientIds.IndexOf(userClientId);
            ulong targetClientId = targetClientIds[peekSessionIndex];



            // User logic
            StopPeekRoutineUserRpc(userClientId, originalPositions[peekSessionIndex]);
            EnableAllCollidersRpc(userClientId);

            userClientIds.RemoveAt(peekSessionIndex);
            targetClientIds.RemoveAt(peekSessionIndex);
            originalPositions.RemoveAt(peekSessionIndex);

            for (int i = 0; i < targetClientIds.Count; i++)
            {
                if (userClientId == targetClientIds[i])
                {
                    Debug.Log($"[Server] Disabling colliders for Client {targetClientIds[i]}");
                    DisableAllCollidersRpc(targetClientIds[i]);
                }
            }



            // Target logic
            if (!targetClientIds.Contains(targetClientId))
            {
                Debug.Log($"[Server] Stopping peek routine for Target Client {targetClientId}");
                StopPeekRoutineTargetRpc(targetClientId);
                UpdatePeekIndicatorRpc(targetClientId);
            }
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
    private void StopPeekRoutineUserRpc(ulong clientId, Vector3[] userOriginalPositions)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            Debug.Log($"[Client] Stopping peek routine as user.");
            isPeeking = false;

            peekButton.gameObject.SetActive(true);
            stopButton.gameObject.SetActive(false);
            peekText.gameObject.SetActive(false);

            tilesManager.SetAllPositions(userOriginalPositions);
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

    [Rpc(SendTo.ClientsAndHost)]
    private void StopPeekRoutineTargetRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            Debug.Log($"[Client] Stopping peek routine as target.");
            isBeingPeeked = false;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdatePeekIndicatorRpc(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log($"[Client] Updating peek indicator. Being peeked: {isBeingPeeked}, Is peeking: {isPeeking}");

            if (isBeingPeeked)
            {
                if (isPeeking)
                {
                    peekIndicatorDanger.SetActive(true);
                }
                else
                {
                    peekIndicator.SetActive(true);
                }
            }
            else
            {
                peekIndicatorDanger.SetActive(false);
                peekIndicator.SetActive(false);
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DisableAllCollidersRpc(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log($"[Client] Disabling all colliders.");
            tilesManager.DisableAllColliders();
            canMove = false;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void EnableAllCollidersRpc(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log($"[Client] Enabling all colliders.");
            tilesManager.EnableAllColliders();
            canMove = true;
        }
    }

    [Rpc(SendTo.Server)]
    private void SendPuzzleToServerRpc(ulong targetClientId, Vector3[] positions)
    {
        Debug.Log($"[Server] Sending puzzle positions to Server from Client {targetClientId}");

        for (int i = 0; i < targetClientIds.Count; i++)
        {
            if (targetClientId == targetClientIds[i])
            {
                ulong userClientId = userClientIds[i];
                Debug.Log($"[Server] Forwarding puzzle positions to Client {userClientId}");
                SendPuzzleToClientRpc(userClientId, positions);
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SendPuzzleToClientRpc(ulong userClientId, Vector3[] positions)
    {
        if (userClientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log($"[Client] Receiving puzzle positions.");
            tilesManager.SetAllPositions(positions);

        }
    }

    [Rpc(SendTo.Server)]
    private void UpdateOriginalPositionsServerRpc(ulong clientId, Vector3[] positions)
    {
        Debug.Log($"[Server] Updating original positions from Client {clientId}");

        if (userClientIds.Contains(clientId))
        {
            int peekSessionIndex = userClientIds.IndexOf(clientId);
            ulong targetClientId = targetClientIds[peekSessionIndex];

            if (userClientIds.Contains(targetClientId))
            {
                Debug.Log($"[Server] Forwarding updated original positions to Client {targetClientId}");
                originalPositions[peekSessionIndex] = positions;
            }
        }
    }

    private void Update()
    {
        if (isBeingPeeked && !isPeeking)
        {
            Debug.Log($"[Client] Sending puzzle positions to server. Being peeked and not peeking.");
            SendPuzzleToServerRpc(NetworkManager.Singleton.LocalClientId, tilesManager.GetAllPositions());
        }
        else if (isPeeking && canMove)
        {
            Debug.Log($"[Client] Updating original positions on server. Peeking and can move.");
            UpdateOriginalPositionsServerRpc(NetworkManager.Singleton.LocalClientId, tilesManager.GetAllPositions());
        }
    }
}