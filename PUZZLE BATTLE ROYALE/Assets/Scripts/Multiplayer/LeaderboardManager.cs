using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the leaderboard, displaying player rankings, names, and profile pictures.
/// </summary>
public class LeaderboardManager : NetworkBehaviour
{
    // -----------------------------------------------------------------------
    // Server functionality
    // -----------------------------------------------------------------------

    /// <summary>
    /// Reference to the PlayerManager to access player data.
    /// </summary>
    [SerializeField] private PlayerManager playerManager;

    /// <summary>
    /// List that stores the current ranking of players.
    /// </summary>
    private List<Player> ranking = new();

    /// <summary>
    /// Initializes the ranking by populating the leaderboard with all players.
    /// </summary>
    public void InitializeRanking()
    {
        foreach (Player player in playerManager.GetAllPlayers())
        {
            ranking.Add(player);
        }

        List<int> profilePictureIds = new();
        List<ulong> clientIdsInOrder = new();
        List<string> playerNamesInOrder = new();
        List<ulong> unpeekableClientIds = new();

        foreach (Player player2 in ranking)
        {
            profilePictureIds.Add(player2.ProfilePictureId);
            clientIdsInOrder.Add(player2.ClientId);
            playerNamesInOrder.Add(player2.Name);
        }

        string serializedProfilePictureIds = string.Join(",", profilePictureIds);
        string serializedClientIdsInOrder = string.Join(",", clientIdsInOrder);
        string serializedplayerNamesInOrder = string.Join(",", playerNamesInOrder);
        string serializedUnpeekableClientIdsInOrder = string.Join(",", unpeekableClientIds);

        foreach (Player player1 in playerManager.GetAllPlayers())
        {
            UpdatePlayerLeaderboardRpc(player1.ClientId, serializedProfilePictureIds, serializedClientIdsInOrder, serializedplayerNamesInOrder, serializedUnpeekableClientIdsInOrder, ranking.IndexOf(player1));
        }
    }

    /// <summary>
    /// Updates the ranking of a player and refreshes the leaderboard accordingly.
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
        List<ulong> clientIdsInOrder = new();
        List<string> playerNamesInOrder = new();
        List<ulong> unpeekableClientIds = new();

        foreach (Player player2 in ranking)
        {
            profilePictureIds.Add(player2.ProfilePictureId);
            clientIdsInOrder.Add(player2.ClientId);
            playerNamesInOrder.Add(player2.Name);
        }

        string serializedProfilePictureIds = string.Join(",", profilePictureIds);
        string serializedClientIdsInOrder = string.Join(",", clientIdsInOrder);
        string serializedplayerNamesInOrder = string.Join(",", playerNamesInOrder);
        string serializedUnpeekableClientIdsInOrder = string.Join(",", unpeekableClientIds);

        foreach (Player player1 in playerManager.GetAllPlayers())
        {
            UpdatePlayerLeaderboardRpc(player1.ClientId, serializedProfilePictureIds, serializedClientIdsInOrder, serializedplayerNamesInOrder, serializedUnpeekableClientIdsInOrder, ranking.IndexOf(player1));
        }
    }

    // -----------------------------------------------------------------------
    // Client functionality
    // -----------------------------------------------------------------------

    /// <summary>
    /// Reference to the PeekManager to handle peek actions.
    /// </summary>
    [SerializeField] private PeekManager peekManager;

    /// <summary>
    /// List of available profile picture sprites to display in the leaderboard.
    /// </summary>
    [SerializeField] private List<Sprite> profilePicturesSprites;

    /// <summary>
    /// The parent container that holds the leaderboard entries.
    /// </summary>
    [SerializeField] private GameObject playerContainer;

    /// <summary>
    /// The prefab used to create each leaderboard entry.
    /// </summary>
    [SerializeField] private GameObject leaderboardEntryPrefab;

    /// <summary>
    /// The button to hide the detailed leaderboard.
    /// </summary>
    [SerializeField] private GameObject hideButtonPrefab;

    /// <summary>
    /// The button to show the detailed leaderboard.
    /// </summary>
    [SerializeField] private GameObject showButtonPrefab;

    /// <summary>
    /// Indicates whether the long-form leaderboard (detailed) is shown.
    /// </summary>
    private bool longFormLeaderboard = false;

    /// <summary>
    /// Array of profile picture IDs received from the server.
    /// </summary>
    private string[] profilePictureIdsFromServer;

    /// <summary>
    /// Array of client IDs received from the server.
    /// </summary>
    private string[] clientIdsFromServer;

    /// <summary>
    /// Array of player names received from the server.
    /// </summary>
    private string[] playerNamesFromSever;

    /// <summary>
    /// Array of client IDs that cannot be peeked, received from the server.
    /// </summary>
    private string[] unpeekableCliendIdsFromServer;

    /// <summary>
    /// Stores the current rank of the local player.
    /// </summary>
    private int myCurrentRank;

    /// <summary>
    /// Indicates whether all peek buttons should be disabled.
    /// </summary>
    private bool disableAllPeekButtons = false;

    /// <summary>
    /// Sets whether all peek buttons should be disabled.
    /// </summary>
    public bool DisableAllPeekButtons
    {
        set { disableAllPeekButtons = value; }
    }

    /// <summary>
    /// RPC method that updates the leaderboard for the specified client.
    /// </summary>
    /// <param name="clientId">The client ID of the player to update the leaderboard for.</param>
    /// <param name="serializedProfilePictureIds">Serialized string of profile picture IDs.</param>
    /// <param name="serializedClientIdsInOrder">Serialized string of client IDs.</param>
    /// <param name="serializedplayerNamesInOrder">Serialized string of player names.</param>
    /// <param name="serializedUnpeekableClientIdsInOrder">Serialized string of unpeekable client IDs.</param>
    /// <param name="playerRank">The rank of the player.</param>
    [Rpc(SendTo.ClientsAndHost)]
    public void UpdatePlayerLeaderboardRpc(ulong clientId, string serializedProfilePictureIds, string serializedClientIdsInOrder, string serializedplayerNamesInOrder, string serializedUnpeekableClientIdsInOrder, int playerRank)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            profilePictureIdsFromServer = serializedProfilePictureIds.Split(",");
            clientIdsFromServer = serializedClientIdsInOrder.Split(",");
            playerNamesFromSever = serializedplayerNamesInOrder.Split(",");
            unpeekableCliendIdsFromServer = serializedUnpeekableClientIdsInOrder.Split(",");
            myCurrentRank = playerRank;

            RefreshLeaderboard();
        }
    }

    /// <summary>
    /// Destroys all current leaderboard entries to prepare for a refresh.
    /// </summary>
    private void DestroyCurrentLeaderboard()
    {
        foreach (Transform child in playerContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Creates the long-form (detailed) leaderboard.
    /// </summary>
    private void CreateLongFormLeaderboard()
    {
        for (int i = 0; i < profilePictureIdsFromServer.Length; i++)
        {
            GameObject leaderboardEntry = Instantiate(leaderboardEntryPrefab);
            leaderboardEntry.transform.SetParent(playerContainer.transform);

            TextMeshProUGUI leaderboardEntryRankingText = leaderboardEntry.transform.Find("RankingText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI leaderboardEntryNameText = leaderboardEntry.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            if (i == myCurrentRank)
            {
                leaderboardEntryRankingText.text = $"<color=#ff0000>{i + 1}.</color>";
                leaderboardEntryNameText.text = $"<color=#ff0000>You</color>";
            }
            else
            {
                leaderboardEntryRankingText.text = $"{i + 1}.";
                leaderboardEntryNameText.text = playerNamesFromSever[i];
            }

            int profilePictureId = int.Parse(profilePictureIdsFromServer[i]);
            Image leaderboardEntryImage = leaderboardEntry.transform.Find("ProfilePictureButton").GetComponent<Image>();
            leaderboardEntryImage.sprite = profilePicturesSprites[profilePictureId];

            ulong clientId = ulong.Parse(clientIdsFromServer[i]);
            Button leaderboardEntryButton = leaderboardEntry.transform.Find("ProfilePictureButton").GetComponent<Button>();
            if (clientId != NetworkManager.Singleton.LocalClientId && !unpeekableCliendIdsFromServer.Contains(clientId.ToString()) && !disableAllPeekButtons)
            {
                leaderboardEntryButton.onClick.AddListener(() => peekManager.Peek(clientId));
            }
            else
            {
                leaderboardEntryButton.interactable = false;

                ColorBlock colorBlock = leaderboardEntryButton.colors;

                Color disabledColor = colorBlock.disabledColor;
                disabledColor.a = 1f;
                disabledColor.r = 1f;
                colorBlock.disabledColor = disabledColor;
                leaderboardEntryButton.colors = colorBlock;
            }
        }
        GameObject hideButton = Instantiate(hideButtonPrefab);
        hideButton.name = "HideButton";
        hideButton.transform.SetParent(playerContainer.transform);
        hideButton.GetComponent<Button>().onClick.AddListener(() => HideLongFormLeaderboard());
    }

    /// <summary>
    /// Creates the short-form (condensed) leaderboard, showing only the local player's ranking.
    /// </summary>
    private void CreateShortFormLeaderboard()
    {
        GameObject leaderboardEntry = Instantiate(leaderboardEntryPrefab);
        leaderboardEntry.transform.SetParent(playerContainer.transform);

        TextMeshProUGUI leaderboardEntryText = leaderboardEntry.transform.Find("RankingText").GetComponent<TextMeshProUGUI>();
        leaderboardEntryText.text = $"<color=#ff0000>{myCurrentRank + 1}.</color>";

        TextMeshProUGUI leaderboardEntryNameText = leaderboardEntry.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        leaderboardEntryNameText.text = $"<color=#ff0000>You</color>";

        int profilePictureId = int.Parse(profilePictureIdsFromServer[myCurrentRank]);
        Image leaderboardEntryImage = leaderboardEntry.transform.Find("ProfilePictureButton").GetComponent<Image>();
        leaderboardEntryImage.sprite = profilePicturesSprites[profilePictureId];

        Button leaderboardEntryButton = leaderboardEntry.transform.Find("ProfilePictureButton").GetComponent<Button>();
        leaderboardEntryButton.interactable = false;

        ColorBlock colorBlock = leaderboardEntryButton.colors;
        Color disabledColor = colorBlock.disabledColor;
        disabledColor.a = 1f;
        disabledColor.r = 1f;
        colorBlock.disabledColor = disabledColor;
        leaderboardEntryButton.colors = colorBlock;

        GameObject showButton = Instantiate(showButtonPrefab);
        showButton.name = "ShowButton";
        showButton.transform.SetParent(playerContainer.transform);
        showButton.GetComponent<Button>().onClick.AddListener(() => ShowLongFormLeaderboard());
    }

    /// <summary>
    /// Refreshes the leaderboard by destroying and recreating it based on the current leaderboard type (long or short form).
    /// </summary>
    public void RefreshLeaderboard()
    {
        DestroyCurrentLeaderboard();

        if (longFormLeaderboard)
        {
            CreateLongFormLeaderboard();
        }
        else
        {
            CreateShortFormLeaderboard();
        }
    }

    /// <summary>
    /// Shows the long-form leaderboard and updates the visibility of the show/hide buttons.
    /// </summary>
    public void ShowLongFormLeaderboard()
    {
        longFormLeaderboard = true;
        RefreshLeaderboard();
    }

    /// <summary>
    /// Hides the long-form leaderboard and updates the visibility of the show/hide buttons.
    /// </summary>
    public void HideLongFormLeaderboard()
    {
        longFormLeaderboard = false;
        RefreshLeaderboard();
    }
}