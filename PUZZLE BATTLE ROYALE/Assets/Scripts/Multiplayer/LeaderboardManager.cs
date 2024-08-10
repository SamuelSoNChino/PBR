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

    [SerializeField] private List<Sprite> profilePicturesSprites;

    [SerializeField] GameObject playerContainer;
    [SerializeField] GameObject leaderboardEntryPrefab;

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

        List<int> profilePictureIds = new();

        foreach (Player player2 in ranking)
        {
            profilePictureIds.Add(player2.ProfilePictureId);
        }

        string serializedProfilePictureIds = string.Join(",", profilePictureIds);

        foreach (Player player1 in playerManager.GetAllPlayers())
        {
            UpdatePlayerLeaderboardRpc(player1.ClientId, serializedProfilePictureIds, ranking.IndexOf(player1));
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

        List<int> profilePictureIds = new();

        foreach (Player player2 in ranking)
        {
            profilePictureIds.Add(player2.ProfilePictureId);
        }

        string serializedProfilePictureIds = string.Join(",", profilePictureIds);

        foreach (Player player1 in playerManager.GetAllPlayers())
        {
            UpdatePlayerLeaderboardRpc(player1.ClientId, serializedProfilePictureIds, ranking.IndexOf(player1));
        }
    }


    [Rpc(SendTo.ClientsAndHost)]
    public void UpdatePlayerLeaderboardRpc(ulong clientId, string serializedProfilePictureIds, int playerRank)
    {
        Debug.Log($"RPC Called for ClientID: {clientId} with Rank: {playerRank}");

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            foreach (Transform child in playerContainer.transform)
            {
                Destroy(child.gameObject);
            }

            string[] profilePictureIds = serializedProfilePictureIds.Split(",");

            for (int i = 0; i < profilePictureIds.Length; i++)
            {
                int profilePictureId = int.Parse(profilePictureIds[i]);

                Debug.Log($"Creating entry for rank {i} with profile picture ID {profilePictureIds[i]}");
                GameObject leaderboardEntry = Instantiate(leaderboardEntryPrefab);
                leaderboardEntry.transform.parent = playerContainer.transform;

                TextMeshProUGUI leaderboardEntryText = leaderboardEntry.transform.Find("RankingText").GetComponent<TextMeshProUGUI>();
                if (i == playerRank)
                {
                    leaderboardEntryText.text = $"<color=#ff0000>{i + 1}.</color>";
                }
                else
                {
                    leaderboardEntryText.text = $"{i + 1}.";
                }


                UnityEngine.UI.Image leaderboardEntryImage = leaderboardEntry.transform.Find("ProfilePictureButton").GetComponent<UnityEngine.UI.Image>();
                leaderboardEntryImage.sprite = profilePicturesSprites[profilePictureId];
            }
        }
    }
}