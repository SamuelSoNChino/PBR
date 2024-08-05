using System;
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



    private List<Player> targetPlayers = new();
    private List<Player> userPlayers = new();

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
        Player userPlayer = playerManager.FindPlayerByClientId(userClientId);

        if (!userPlayer.IsPeeking)
        {
            // Will be replaced with appropriate logic
            Player targetPlayer = null;
            foreach (Player player in playerManager.GetAllPlayers())
            {
                if (player != userPlayer)
                {
                    targetPlayer = player;
                    break;
                }
            }

            userPlayers.Add(userPlayer);
            targetPlayers.Add(targetPlayer);

            userPlayer.IsPeeking = true;
            userPlayer.TargetOfPeekPlayer = targetPlayer;

            puzzleManager.DisableTileMovement(userPlayer);
            puzzleManager.SetOtherClientsPositions(userPlayer, targetPlayer);

            int targetClientBackgroundId = playerManager.FindPlayerByClientId(targetPlayer.ClientId).BackgroundSkinId;
            backgroundManager.SetClientBackgroundRpc(userPlayer.ClientId, targetClientBackgroundId);

            bool targetAlsoPeeking = userPlayers.Contains(targetPlayer);
            UpdatePeekIndicatorRpc(targetPlayer.ClientId, targetAlsoPeeking, true);
            if (targetAlsoPeeking)
            {
                puzzleManager.EnableTileMovement(userPlayer);
            }

            bool userBeingPeeked = targetPlayers.Contains(userPlayer);
            UpdatePeekIndicatorRpc(userPlayer.ClientId, true, userBeingPeeked);
            if (userBeingPeeked)
            {
                foreach (Player peekerPlayer in GetPeekersOfTarget(userPlayer))
                {
                    puzzleManager.EnableTileMovement(peekerPlayer);
                }
            }

            StartPeekRoutineUserRpc(userPlayer.ClientId, targetPlayer.Name);
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
        Player userPlayer = playerManager.FindPlayerByClientId(userClientId);

        if (userPlayers.Contains(userPlayer))
        {
            int peekSessionIndex = userPlayers.IndexOf(userPlayer);
            Player targetPlayer = targetPlayers[peekSessionIndex];

            userPlayers.RemoveAt(peekSessionIndex);
            targetPlayers.RemoveAt(peekSessionIndex);

            userPlayer.IsPeeking = false;
            userPlayer.TargetOfPeekPlayer = null;

            bool targetPeeking = targetPlayer.IsPeeking;
            bool targetStillBeingPeeked = targetPlayers.Contains(targetPlayer);
            UpdatePeekIndicatorRpc(targetPlayer.ClientId, targetPeeking, targetStillBeingPeeked);

            bool userBeingPeeked = targetPlayers.Contains(userPlayer);
            UpdatePeekIndicatorRpc(userPlayer.ClientId, false, userBeingPeeked);
            if (userBeingPeeked)
            {
                foreach (Player peekerPlayer in GetPeekersOfTarget(userPlayer))
                {
                    puzzleManager.DisableTileMovement(peekerPlayer);
                }
            }

            puzzleManager.SetOtherClientsPositions(userPlayer, userPlayer);
            puzzleManager.EnableTileMovement(userPlayer);

            int userOriginalBackground = playerManager.FindPlayerByClientId(userPlayer.ClientId).BackgroundSkinId;
            backgroundManager.SetClientBackgroundRpc(userPlayer.ClientId, userOriginalBackground);

            StopPeekRoutineUserRpc(userPlayer.ClientId);
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

    public List<Player> GetPeekersOfTarget(Player targetPlayer)
    {
        List<Player> peekers = new();
        for (int i = 0; i < userPlayers.Count; i++)
        {
            if (targetPlayers[i] == targetPlayer)
            {
                peekers.Add(userPlayers[i]);
            }
        }
        return peekers;
    }

    private void Update()
    {
        foreach (Player userPlayer in userPlayers)
        {
            Player targetPlayer = userPlayer.TargetOfPeekPlayer;
            if (!targetPlayer.IsPeeking)
            {
                puzzleManager.SetOtherClientsPositions(userPlayer, targetPlayer);
            }
        }
    }
}