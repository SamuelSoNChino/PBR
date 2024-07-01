using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PeekManager : NetworkBehaviour
{
    [SerializeField] private PuzzleManager puzzleManager;
    [SerializeField] private TilesManagerMultiplayer tilesManager;
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
            targetClientIds.Add(targetClientId);

            int targetClientBackgroundSkin = backgroundManager.GetClientBackground(targetClientId);
            backgroundManager.SetClientBackgroundRpc(userClientId, targetClientBackgroundSkin);

            puzzleManager.DisableTileMovement(userClientId);
            puzzleManager.DeselectAllClientTilesRpc(userClientId);
            puzzleManager.SetOtherClientsPositions(userClientId, targetClientId);

            bool targetAlsoPeeking = userClientIds.Contains(targetClientId);
            if (targetAlsoPeeking)
            {
                Debug.Log($"[Server] Target Client {targetClientId} is also peeking.");

                puzzleManager.EnableTileMovement(userClientId);
                puzzleManager.ResetClientsSnappedTilesRpc(userClientId);
            }
            UpdatePeekIndicatorRpc(targetClientId, targetAlsoPeeking, true);

            bool userBeingPeeked = targetClientIds.Contains(userClientId);
            if (userBeingPeeked)
            {
                Debug.Log($"[Server] User is also being peeked");
                UpdatePeekIndicatorRpc(userClientId, true, true);

                int peekSessionIndex = targetClientIds.IndexOf(userClientId);
                ulong peekerClientId = userClientIds[peekSessionIndex];
                puzzleManager.EnableTileMovement(peekerClientId);
                puzzleManager.ResetClientsSnappedTilesRpc(peekerClientId);
            }

            StartPeekRoutineUserRpc(userClientId, targetClientId);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void StartPeekRoutineUserRpc(ulong userClientId, ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == userClientId)
        {
            Debug.Log($"[Client] Starting peek routine as user. Target: {targetClientId}");

            peekButton.gameObject.SetActive(false);
            stopButton.gameObject.SetActive(true);

            peekText.gameObject.SetActive(true);
            peekText.text = $"You are peeking at: {targetClientId}";
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
            bool targetPeeking = userClientIds.Contains(targetClientId);
            UpdatePeekIndicatorRpc(targetClientId, targetPeeking, false);

            bool userBeingPeeked = targetClientIds.Contains(userClientId);
            if (userBeingPeeked)
            {
                int userBeingPeekedSessionIndex = targetClientIds.IndexOf(userClientId);
                ulong peekerClientId = userClientIds[userBeingPeekedSessionIndex];
                puzzleManager.DisableTileMovement(peekerClientId);
                puzzleManager.DeselectAllClientTilesRpc(peekerClientId);
                UpdatePeekIndicatorRpc(userClientId, false, userBeingPeeked);
            }

            puzzleManager.SetOtherClientsPositions(userClientId, userClientId);
            puzzleManager.UpdateGridForAllTilesRpc(userClientId);
            puzzleManager.DeselectAllClientTilesRpc(userClientId);
            puzzleManager.EnableTileMovement(userClientId);
            puzzleManager.ResetClientsSnappedTilesRpc(userClientId);

            int originalBackground = backgroundManager.GetClientBackground(userClientId);
            backgroundManager.SetClientBackgroundRpc(userClientId, originalBackground);
            StopPeekRoutineUserRpc(userClientId);

            userClientIds.RemoveAt(peekSessionIndex);
            targetClientIds.RemoveAt(peekSessionIndex);
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