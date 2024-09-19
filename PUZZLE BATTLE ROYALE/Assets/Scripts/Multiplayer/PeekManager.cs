using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the peeking functionality.
/// </summary>
public class PeekManager : NetworkBehaviour
{
    /// <summary>
    /// Reference to the player manager.
    /// </summary>
    [SerializeField] private PlayerManager playerManager;

    /// <summary>
    /// Reference to the puzzle manager.
    /// </summary>
    [SerializeField] private PuzzleManager puzzleManager;

    /// <summary>
    /// Reference to the power manager.
    /// </summary>
    [SerializeField] private PowerManager powerManager;

    /// <summary>
    /// Reference to the background manager for multiplayer.
    /// </summary>
    [SerializeField] private BackgroundManagerMultiplayer backgroundManager;

    /// <summary>
    /// Button to stop peeking.
    /// </summary>
    [SerializeField] private Button stopButton;

    /// <summary>
    /// Text to display the peeking status.
    /// </summary>
    [SerializeField] private TextMeshProUGUI peekText;

    /// <summary>
    /// GameObject to indicate peeking.
    /// </summary>
    [SerializeField] private GameObject peekIndicator;

    /// <summary>
    /// GameObject to indicate danger while peeking.
    /// </summary>
    [SerializeField] private GameObject peekIndicatorDanger;

    /// <summary>
    /// Reference to the leaderboard manager.
    /// </summary>
    [SerializeField] private LeaderboardManager leaderboardManager;

    /// <summary>
    /// List of players who are peeking.
    /// </summary>
    private List<Player> targetPlayers = new();

    /// <summary>
    /// List of players being peeked at.
    /// </summary>
    private List<Player> userPlayers = new();

    /// <summary>
    /// Event triggered when a player's peeking status changes.
    /// </summary>
    public event Action<Player> OnPlayerPeekingStatusChanged;

    // -----------------------------------------------------------------------
    // Peek buttons functionality
    // -----------------------------------------------------------------------

    /// <summary>
    /// Initiates a peek request from the client.
    /// </summary>
    public void Peek(ulong targetClientId)
    {
        Debug.Log($"[Client] Client {NetworkManager.Singleton.LocalClientId} requesting peek.");
        RequestPeekRpc(NetworkManager.Singleton.LocalClientId, targetClientId);
    }

    /// <summary>
    /// Requests to stop peeking from the client.
    /// </summary>
    public void StopPeeking()
    {
        Debug.Log($"[Client] Client {NetworkManager.Singleton.LocalClientId} requesting to stop peeking.");
        RequestStopPeekingRpc(NetworkManager.Singleton.LocalClientId);
    }

    // -----------------------------------------------------------------------
    // Peek Requests
    // -----------------------------------------------------------------------

    /// <summary>
    /// RPC to request a peek on the server.
    /// </summary>
    /// <param name="userClientId">The client ID of the user requesting the peek.</param>
    [Rpc(SendTo.Server)]
    private void RequestPeekRpc(ulong userClientId, ulong targetClientId)
    {
        Player userPlayer = playerManager.FindPlayerByClientId(userClientId);

        if (!userPlayer.IsPeeking && !userPlayer.PeekUseOnCooldown && userClientId != targetClientId)
        {
            StartCoroutine(PutPlayerOnExitCooldown(userPlayer));

            Player targetPlayer = playerManager.FindPlayerByClientId(targetClientId);

            userPlayers.Add(userPlayer);
            targetPlayers.Add(targetPlayer);

            userPlayer.IsPeeking = true;
            userPlayer.TargetOfPeekPlayer = targetPlayer;
            OnPlayerPeekingStatusChanged.Invoke(userPlayer);

            puzzleManager.DisableTileMovement(userPlayer);
            puzzleManager.SetOtherClientsPositions(userPlayer, targetPlayer);

            int targetClientBackgroundId = targetPlayer.BackgroundSkinId;
            backgroundManager.SetClientBackgroundRpc(userPlayer.ClientId, targetClientBackgroundId);

            bool targetAlsoPeeking = userPlayers.Contains(targetPlayer);
            if (targetAlsoPeeking)
            {
                puzzleManager.EnableTileMovement(userPlayer);
            }

            bool userBeingPeeked = targetPlayers.Contains(userPlayer);
            if (userBeingPeeked)
            {
                foreach (Player peekerPlayer in GetPeekersOfTarget(userPlayer))
                {
                    puzzleManager.EnableTileMovement(peekerPlayer);
                }
            }



            UpdatePeekIndicator(userPlayer);
            if (!userPlayer.HasPower("Secret Peek"))
            {
                UpdatePeekIndicator(targetPlayer);
            }

            StartPeekRoutineUserRpc(userPlayer.ClientId, targetPlayer.Name);
        }
    }

    /// <summary>
    /// RPC to request stopping peeking on the server.
    /// </summary>
    /// <param name="userClientId">The client ID of the user requesting to stop peeking.</param>
    [Rpc(SendTo.Server)]
    private void RequestStopPeekingRpc(ulong userClientId)
    {
        Player userPlayer = playerManager.FindPlayerByClientId(userClientId);

        if (userPlayers.Contains(userPlayer) && !userPlayer.PeekExitOnCooldown)
        {
            StartCoroutine(PutPlayerOnUseCooldown(userPlayer));

            int peekSessionIndex = userPlayers.IndexOf(userPlayer);
            Player targetPlayer = targetPlayers[peekSessionIndex];

            userPlayers.RemoveAt(peekSessionIndex);
            targetPlayers.RemoveAt(peekSessionIndex);

            userPlayer.IsPeeking = false;
            userPlayer.TargetOfPeekPlayer = null;
            OnPlayerPeekingStatusChanged.Invoke(userPlayer);

            bool userBeingPeeked = targetPlayers.Contains(userPlayer);
            if (userBeingPeeked)
            {
                foreach (Player peekerPlayer in GetPeekersOfTarget(userPlayer))
                {
                    puzzleManager.DisableTileMovement(peekerPlayer);
                }
            }

            UpdatePeekIndicator(userPlayer);
            if (!userPlayer.HasPower("Secret Peek"))
            {
                UpdatePeekIndicator(targetPlayer);
            }
            

            puzzleManager.SetOtherClientsPositions(userPlayer, userPlayer);
            puzzleManager.EnableTileMovement(userPlayer);

            int userOriginalBackground = userPlayer.BackgroundSkinId;
            backgroundManager.SetClientBackgroundRpc(userPlayer.ClientId, userOriginalBackground);

            StopPeekRoutineUserRpc(userPlayer.ClientId);
        }
    }

    // -----------------------------------------------------------------------
    // Peek User Routines
    // -----------------------------------------------------------------------

    /// <summary>
    /// RPC to start the peek routine on the client.
    /// </summary>
    /// <param name="userClientId">The client ID of the user.</param>
    /// <param name="targetName">The name of the target player being peeked at.</param>
    [Rpc(SendTo.ClientsAndHost)]
    private void StartPeekRoutineUserRpc(ulong userClientId, string targetName)
    {
        if (NetworkManager.Singleton.LocalClientId == userClientId)
        {
            stopButton.gameObject.SetActive(false);

            peekText.gameObject.SetActive(true);
            peekText.text = $"You are peeking at: {targetName}";

            leaderboardManager.DisableAllPeekButtons = true;
            leaderboardManager.RefreshLeaderboard();
        }
    }


    /// <summary>
    /// RPC to stop the peek routine on the client.
    /// </summary>
    /// <param name="clientId">The client ID of the user.</param>
    [Rpc(SendTo.ClientsAndHost)]
    private void StopPeekRoutineUserRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            stopButton.gameObject.SetActive(false);
            peekText.gameObject.SetActive(false);
        }
    }

    // -----------------------------------------------------------------------
    // Cooldown functionality
    // -----------------------------------------------------------------------

    /// <summary>
    /// Coroutine to put a player on a usage cooldown.
    /// </summary>
    /// <param name="player">The player to be put on cooldown.</param>
    /// <returns>An enumerator for the coroutine.</returns>
    public IEnumerator PutPlayerOnUseCooldown(Player player)
    {
        float cooldownPeriod = 5; // Temporary fixed value

        player.PeekUseOnCooldown = true;
        yield return new WaitForSeconds(cooldownPeriod);
        player.PeekUseOnCooldown = false;
        EnablePeekRpc(player.ClientId);
    }

    /// <summary>
    /// RPC to enable the peek button on the client.
    /// </summary>
    /// <param name="clientId">The client ID of the user.</param>
    [Rpc(SendTo.ClientsAndHost)]
    public void EnablePeekRpc(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            leaderboardManager.DisableAllPeekButtons = false;
            leaderboardManager.RefreshLeaderboard();
        }
    }

    /// <summary>
    /// Coroutine to put a player on an exit cooldown.
    /// </summary>
    /// <param name="player">The player to be put on cooldown.</param>
    /// <returns>An enumerator for the coroutine.</returns>
    public IEnumerator PutPlayerOnExitCooldown(Player player)
    {
        float cooldownPeriod = 2; // Temporary fixed value

        player.PeekExitOnCooldown = true;
        yield return new WaitForSeconds(cooldownPeriod);
        player.PeekExitOnCooldown = false;
        EnablePeekStopRpc(player.ClientId);
    }

    /// <summary>
    /// RPC to enable the stop button on the client.
    /// </summary>
    /// <param name="clientId">The client ID of the user.</param>
    [Rpc(SendTo.ClientsAndHost)]
    public void EnablePeekStopRpc(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            stopButton.gameObject.SetActive(true);
        }
    }

    // -----------------------------------------------------------------------
    // Miscellaneous
    // -----------------------------------------------------------------------

    /// <summary>
    /// Updates the peek indicator for a given player.
    /// </summary>
    /// <param name="player">The player for whom the peek indicator is updated.</param>
    private void UpdatePeekIndicator(Player player)
    {
        bool playerPeeking = userPlayers.Contains(player);
        bool playerBeingVisiblyPeeked = false;

        foreach (Player peekingPlayer in GetPeekersOfTarget(player))
        {
            if (!peekingPlayer.HasPower("Secret Peek"))
            {
                playerBeingVisiblyPeeked = true;
                break;
            }
        }

        UpdatePeekIndicatorRpc(player.ClientId, playerPeeking, playerBeingVisiblyPeeked);
    }

    /// <summary>
    /// RPC to update the peek indicator on the client.
    /// </summary>
    /// <param name="clientId">The client ID of the user.</param>
    /// <param name="isPeeking">Indicates if the user is peeking.</param>
    /// <param name="isBeingPeeked">Indicates if the user is being peeked at.</param>
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

    /// <summary>
    /// Gets the list of players who are peeking at a specified target player.
    /// </summary>
    /// <param name="targetPlayer">The target player being peeked at.</param>
    /// <returns>A list of players peeking at the target player.</returns>
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

    public void InitializeUnpeekablePlayers()
    {
        List<Player> defaultlyUnpeekablePlayers = new();
        foreach (Player player in playerManager.GetAllPlayers())
        {
            if (player.HasPower("Solo Leveling"))
            {
                defaultlyUnpeekablePlayers.Add(player);
            }
        }

        foreach (Player player1 in playerManager.GetAllPlayers())
        {
            player1.AddUnpeekablePlayer(player1);
            foreach (Player unpeekablePlayer in defaultlyUnpeekablePlayers)
            {
                player1.AddUnpeekablePlayer(unpeekablePlayer);
            }
        }
    }

    /// <summary>
    /// Updates the positions of other clients for players who are peeking.
    /// </summary>
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

