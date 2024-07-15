using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages the leaderboard for the game, including updating player rankings and displaying the leaderboard.
/// </summary>
public class LeaderboardManager : NetworkBehaviour
{
    /// <summary>
    /// Reference to the PlayerManager for managing player-related operations.
    /// </summary>
    [SerializeField] private PlayerManager playerManager;

    /// <summary>
    /// Reference to the PuzzleManager for managing puzzle-related operations.
    /// </summary>
    [SerializeField] private PuzzleManager puzzleManager;

    /// <summary>
    /// Reference to the TextMeshProUGUI component for displaying the leaderboard.
    /// </summary>
    [SerializeField] private TextMeshProUGUI leaderboardText;

    /// <summary>
    /// List of players sorted by their rank.
    /// </summary>
    private List<Player> ranking = new();

    /// <summary>
    /// Initializes the ranking list with all players and updates the leaderboard.
    /// </summary>
    public void InitializeRanking()
    {
        foreach (Player player in playerManager.GetAllPlayers())
        {
            ranking.Add(player);
        }

        List<string> playerNames = new();

        foreach (Player player2 in ranking)
        {
            playerNames.Add(player2.Name);
        }

        string serializedPlayerNames = string.Join(",", playerNames);

        foreach (Player player1 in playerManager.GetAllPlayers())
        {
            UpdatePlayerLeaderboardRpc(player1.ClientId, serializedPlayerNames, ranking.IndexOf(player1));
        }
    }

    /// <summary>
    /// Updates the ranking of a specific player after his progress has been changed.
    /// </summary>
    /// <param name="player">The player whose ranking needs to be updated.</param>
    public void UpdateRanking(Player player)
    {
        int currentRank = ranking.IndexOf(player);
        ranking.RemoveAt(currentRank);

        bool isLast = true;

        for (int i = 0; i < ranking.Count; i++)
        {
            Player comparedPlayer = ranking[i];
            if (player.Progress > comparedPlayer.Progress)
            {
                isLast = false;
                ranking.Insert(i, player);
                break;
            }
        }

        if (isLast)
        {
            ranking.Add(player);
        }

        List<string> playerNames = new();

        foreach (Player player2 in ranking)
        {
            playerNames.Add(player2.Name);
        }

        string serializedPlayerNames = string.Join(",", playerNames);

        foreach (Player player1 in playerManager.GetAllPlayers())
        {
            UpdatePlayerLeaderboardRpc(player1.ClientId, serializedPlayerNames, ranking.IndexOf(player1));
        }
    }

    /// <summary>
    /// Updates the leaderboard display for a specific player using RPC.
    /// </summary>
    /// <param name="clientId">The ID of the client to update.</param>
    /// <param name="serializedPlayerNames">The serialized list of player names.</param>
    /// <param name="playerRank">The rank of the player to update.</param>
    [Rpc(SendTo.ClientsAndHost)]
    public void UpdatePlayerLeaderboardRpc(ulong clientId, string serializedPlayerNames, int playerRank)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            string[] playerNames = serializedPlayerNames.Split(',');
            string newLeaderboardText = "";

            for (int i = 0; i < playerNames.Length; i++)
            {
                string leaderboardEntry = $"{i + 1}. {playerNames[i]}";

                if (i == playerRank)
                {
                    leaderboardEntry = $"<color=#ff0000>{leaderboardEntry}</color>";
                }

                newLeaderboardText += $"{leaderboardEntry}\n";
            }

            leaderboardText.text = newLeaderboardText;
        }
    }
}