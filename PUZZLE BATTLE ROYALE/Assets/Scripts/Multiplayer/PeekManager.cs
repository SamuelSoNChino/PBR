using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PeekManager : NetworkBehaviour
{
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private PuzzleManager puzzleManager;
    [SerializeField] private BackgroundManagerMultiplayer backgroundManager;
    [SerializeField] private Button peekButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private TextMeshProUGUI peekText;
    [SerializeField] private GameObject peekIndicator;
    [SerializeField] private GameObject peekIndicatorDanger;



    private List<ulong> targetClientIds = new();
    private List<ulong> userClientIds = new();

    public void Peek()
    {
        Debug.Log($"[Client] Client {NetworkManager.Singleton.LocalClientId} requesting peek.");
        RequestPeekRpc(NetworkManager.Singleton.LocalClientId);
    }

    public void StopPeeking()
    {
        Debug.Log($"[Client] Client {NetworkManager.Singleton.LocalClientId} requesting to stop peeking.");
        RequestStopPeekingRpc(NetworkManager.Singleton.LocalClientId);
    }

    [Rpc(SendTo.Server)]
    private void RequestPeekRpc(ulong userClientId)
    {
        Debug.Log($"[Server] Received peek request from Client {userClientId}");

        if (!userClientIds.Contains(userClientId))
        {
            // Will be replaced with appropriate logic
            ulong targetClientId = 0;
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (clientId != userClientId)
                {
                    targetClientId = clientId;
                    break;
                }
            }

            Debug.Log($"[Server] Client {userClientId} is peeking at Client {targetClientId}.");
            userClientIds.Add(userClientId);
            targetClientIds.Add(targetClientId);

            puzzleManager.DisableTileMovement(userClientId);
            puzzleManager.SetOtherClientsPositions(userClientId, targetClientId);

            int targetClientBackgroundId = playerManager.FindPlayerByClientId(targetClientId).BackgroundSkinId;
            backgroundManager.SetClientBackgroundRpc(userClientId, targetClientBackgroundId);

            bool targetAlsoPeeking = userClientIds.Contains(targetClientId);
            UpdatePeekIndicatorRpc(targetClientId, targetAlsoPeeking, true);
            if (targetAlsoPeeking)
            {
                puzzleManager.EnableTileMovement(userClientId);
                puzzleManager.ResetPlayerSnappedTiles(userClientId);
            }

            bool userBeingPeeked = targetClientIds.Contains(userClientId);
            UpdatePeekIndicatorRpc(userClientId, true, userBeingPeeked);
            if (userBeingPeeked)
            {
                for (int i = 0; i < targetClientIds.Count; i++)
                {
                    if (targetClientIds[i] == userClientId)
                    {
                        ulong peekerClientId = userClientIds[i];
                        puzzleManager.EnableTileMovement(peekerClientId);
                        puzzleManager.ResetPlayerSnappedTiles(peekerClientId);
                    }
                }
            }

            string targetName = playerManager.FindPlayerByClientId(targetClientId).Name;
            StartPeekRoutineUserRpc(userClientId, targetName);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void StartPeekRoutineUserRpc(ulong userClientId, string targetName)
    {
        if (NetworkManager.Singleton.LocalClientId == userClientId)
        {
            peekButton.gameObject.SetActive(false);
            stopButton.gameObject.SetActive(true);

            peekText.gameObject.SetActive(true);
            peekText.text = $"You are peeking at: {targetName}";
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

            userClientIds.RemoveAt(peekSessionIndex);
            targetClientIds.RemoveAt(peekSessionIndex);

            bool targetPeeking = userClientIds.Contains(targetClientId);
            bool targetStillBeingPeeked = targetClientIds.Contains(targetClientId);
            UpdatePeekIndicatorRpc(targetClientId, targetPeeking, targetStillBeingPeeked);

            bool userBeingPeeked = targetClientIds.Contains(userClientId);
            UpdatePeekIndicatorRpc(userClientId, false, userBeingPeeked);
            if (userBeingPeeked)
            {
                for (int i = 0; i < targetClientIds.Count; i++)
                {
                    if (targetClientIds[i] == userClientId)
                    {
                        ulong peekerClientId = userClientIds[i];
                        puzzleManager.DisableTileMovement(peekerClientId);
                    }
                }

            }

            puzzleManager.SetOtherClientsPositions(userClientId, userClientId);
            puzzleManager.EnableTileMovement(userClientId);
            puzzleManager.ResetPlayerSnappedTiles(userClientId);

            int userOriginalBackground = playerManager.FindPlayerByClientId(userClientId).BackgroundSkinId;
            backgroundManager.SetClientBackgroundRpc(userClientId, userOriginalBackground);

            StopPeekRoutineUserRpc(userClientId);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void StopPeekRoutineUserRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            Debug.Log($"[Client] Stopping peek routine as user.");

            peekButton.gameObject.SetActive(true);
            stopButton.gameObject.SetActive(false);
            peekText.gameObject.SetActive(false);
        }
    }

    //----------------------------------------------------------------------------------------------------------------------------

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdatePeekIndicatorRpc(ulong clientId, bool isPeeking, bool isBeingPeeked)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
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
    }

    public bool IsClientPeeking(ulong clientId)
    {
        return userClientIds.Contains(clientId);
    }

    public ulong GetTargetOfPeekUser(ulong userClientId)
    {
        int peekSessionIndex = userClientIds.IndexOf(userClientId);
        return targetClientIds[peekSessionIndex];
    }

    private void Update()
    {
        foreach (ulong userClientId in userClientIds)
        {
            int peekSessionIndex = userClientIds.IndexOf(userClientId);
            ulong targetClientId = targetClientIds[peekSessionIndex];
            bool targetAlsoPeeking = userClientIds.Contains(targetClientId);
            if (!targetAlsoPeeking)
            {
                puzzleManager.SetOtherClientsPositions(userClientId, targetClientId);
            }
        }
    }
}