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
    /// Reference to the PlayerManager which handles all player-related operations.
    /// </summary>
    [SerializeField] private PlayerManager playerManager;

    /// <summary>
    /// List that stores the current ranking of players based on their progress.
    /// </summary>
    private List<Player> rankedPlayers = new();

    /// <summary>
    /// Keeps track of the number of unranked players.
    /// </summary>
    private int numberOfUnrankedPlayers = 0;

    /// <summary>
    /// Initializes the ranking by adding players to the leaderboard.
    /// Players with "Solo Leveling" power are handled differently.
    /// </summary>
    public void InitializeRanking()
    {
        foreach (Player player in playerManager.GetAllPlayers())
        {
            if (player.HasPower("Solo Leveling"))
            {
                rankedPlayers.Add(player);
                numberOfUnrankedPlayers++;
            }
            else
            {
                rankedPlayers.Insert(rankedPlayers.Count - numberOfUnrankedPlayers, player);
            }
        }

        foreach (Player player in playerManager.GetAllPlayers())
        {
            var (profilePictureIds, clientIds, playerNames) = SerializePlayerDataForPlayer(player);

            UpdatePlayerLeaderboardRpc(
                player.ClientId,
                profilePictureIds,
                clientIds,
                playerNames,
                rankedPlayers.IndexOf(player),
                numberOfUnrankedPlayers
            );
        }
    }

    /// <summary>
    /// Updates the ranking of a specific player by comparing their progress with other players.
    /// </summary>
    /// <param name="player">The player whose rank needs to be updated.</param>
    public void UpdateRanking(Player player)
    {
        int currentRank = rankedPlayers.IndexOf(player);
        rankedPlayers.RemoveAt(currentRank);

        bool placedAtEnd = true;

        // Insert player at the correct rank based on their progress
        for (int i = 0; i < rankedPlayers.Count - numberOfUnrankedPlayers; i++)
        {
            Player comparedPlayer = rankedPlayers[i];
            if (player.Progress > comparedPlayer.Progress)
            {
                placedAtEnd = false;
                rankedPlayers.Insert(i, player);
                break;
            }
        }

        if (placedAtEnd)
        {
            rankedPlayers.Insert(rankedPlayers.Count - numberOfUnrankedPlayers, player);
        }

        foreach (Player otherPlayer in playerManager.GetAllPlayers())
        {
            var (profilePictureIds, clientIds, playerNames) = SerializePlayerDataForPlayer(otherPlayer);
            UpdatePlayerLeaderboardRpc(
                otherPlayer.ClientId,
                profilePictureIds,
                clientIds,
                playerNames,
                rankedPlayers.IndexOf(otherPlayer),
                numberOfUnrankedPlayers
            );
        }
    }

    /// <summary>
    /// Serializes the necessary player data for the leaderboard display.
    /// This includes profile pictures, player names, and client IDs.
    /// </summary>
    /// <param name="targetPlayer">The player requesting the data.</param>
    /// <returns>Serialized strings of profile pictures, client IDs, player names.</returns>
    private (string profilePictureIds, string clientIds, string playerNames)
        SerializePlayerDataForPlayer(Player targetPlayer)
    {
        List<int> profilePictureIds = new();
        List<ulong> clientIds = new();
        List<string> playerNames = new();

        foreach (Player player in rankedPlayers)
        {
            profilePictureIds.Add(player.ProfilePictureId);
            clientIds.Add(player.ClientId);
            playerNames.Add(player.Name);
        }

        return (
            string.Join(",", profilePictureIds),
            string.Join(",", clientIds),
            string.Join(",", playerNames)
        );
    }

    // -----------------------------------------------------------------------
    // Client functionality
    // -----------------------------------------------------------------------

    /// <summary>
    /// Manages the peeking functionality between players on the leaderboard.
    /// </summary>
    [SerializeField] private PeekManager peekManager;

    /// <summary>
    /// List of available profile picture sprites that can be displayed on the leaderboard.
    /// </summary>
    [SerializeField] private List<Sprite> profilePictureSprites;

    /// <summary>
    /// Container that holds the leaderboard entries in the UI.
    /// </summary>
    [SerializeField] private GameObject leaderboardEntryContainer;

    /// <summary>
    /// Prefab for creating individual leaderboard entries in the UI.
    /// </summary>
    [SerializeField] private GameObject leaderboardEntryPrefab;

    /// <summary>
    /// Prefab for the button used to hide the long-form leaderboard.
    /// </summary>
    [SerializeField] private GameObject hideLeaderboardButtonPrefab;

    /// <summary>
    /// Prefab for the button used to show the long-form leaderboard.
    /// </summary>
    [SerializeField] private GameObject showLeaderboardButtonPrefab;

    /// <summary>
    /// Boolean flag to indicate whether the long-form leaderboard is shown.
    /// </summary>
    private bool isLongFormLeaderboardShown = false;

    /// <summary>
    /// Array of profile picture IDs received from the server.
    /// </summary>
    private string[] receivedProfilePictureIds;

    /// <summary>
    /// Array of client IDs received from the server.
    /// </summary>
    private string[] receivedClientIds;

    /// <summary>
    /// Array of player names received from the server.
    /// </summary>
    private string[] receivedPlayerNames;

    /// <summary>
    /// The local player's rank in the leaderboard.
    /// </summary>
    private int localPlayerRank;

    /// <summary>
    /// The number of unranked players in the received data.
    /// </summary>
    private int receivedUnrankedPlayerCount;

    /// <summary>
    /// Boolean flag to disable all peek buttons on the leaderboard.
    /// </summary>
    private bool disableAllPeekButtons = false;

    /// <summary>
    /// Property to set the disable state of all peek buttons.
    /// </summary>
    public bool DisableAllPeekButtons
    {
        set { disableAllPeekButtons = value; }
    }

    /// <summary>
    /// Array of client IDs that are unpeekable by the local player.
    /// </summary>
    private string[] unpeekableClientIds;

    /// <summary>
    /// Updates the list of unpeekable players and refreshes the leaderboard if initialized.
    /// </summary>
    /// <param name="newUnpeekableClientIds">Array of client IDs that are unpeekable.</param>

    public void UpdateUnpeekablePlayers(string[] newUnpeekableClientIds)
    {
        unpeekableClientIds = newUnpeekableClientIds;

        // To avoid refreshing before initializing the leaderboard
        if (receivedClientIds != null)
        {
            RefreshLeaderboard();
        }
    }

    /// <summary>
    /// RPC method to update the leaderboard for the specified client.
    /// This method is sent from the server to the client.
    /// </summary>
    /// <param name="clientId">The client ID of the target player.</param>
    /// <param name="profilePictureIds">Serialized profile picture IDs.</param>
    /// <param name="clientIds">Serialized client IDs.</param>
    /// <param name="playerNames">Serialized player names.</param>
    /// <param name="playerRank">The player's rank on the leaderboard.</param>
    /// <param name="unrankedPlayerCount">The number of unranked players.</param>

    [Rpc(SendTo.ClientsAndHost)]
    public void UpdatePlayerLeaderboardRpc(
        ulong clientId,
        string profilePictureIds,
        string clientIds,
        string playerNames,
        int playerRank,
        int unrankedPlayerCount)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            receivedProfilePictureIds = profilePictureIds.Split(",");
            receivedClientIds = clientIds.Split(",");
            receivedPlayerNames = playerNames.Split(",");
            localPlayerRank = playerRank;
            receivedUnrankedPlayerCount = unrankedPlayerCount;

            RefreshLeaderboard();
        }
    }

    /// <summary>
    /// Destroys all current leaderboard entries in the UI.
    /// </summary>
    private void DestroyLeaderboardEntries()
    {
        foreach (Transform child in leaderboardEntryContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Creates the detailed (long-form) leaderboard, displaying all players' rankings.
    /// </summary>
    private void CreateLongFormLeaderboard()
    {
        for (int i = 0; i < receivedProfilePictureIds.Length; i++)
        {
            GameObject entry = Instantiate(leaderboardEntryPrefab, leaderboardEntryContainer.transform);
            TextMeshProUGUI rankText = entry.transform.Find("RankingText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI nameText = entry.transform.Find("NameText").GetComponent<TextMeshProUGUI>();

            bool isUnranked = i >= receivedProfilePictureIds.Length - receivedUnrankedPlayerCount;
            string rankDisplay = isUnranked ? "?" : $"{i + 1}.";

            if (i == localPlayerRank)
            {
                rankText.text = $"<color=#ff0000>{rankDisplay}</color>";
                nameText.text = $"<color=#ff0000>You</color>";
            }
            else
            {
                rankText.text = rankDisplay;
                nameText.text = receivedPlayerNames[i];
            }

            int profilePicId = int.Parse(receivedProfilePictureIds[i]);
            Image profilePicture = entry.transform.Find("ProfilePictureButton").GetComponent<Image>();
            profilePicture.sprite = profilePictureSprites[profilePicId];

            string clientId = receivedClientIds[i];
            Button profileButton = entry.transform.Find("ProfilePictureButton").GetComponent<Button>();

            if (!unpeekableClientIds.Contains(clientId) && !disableAllPeekButtons)
            {
                profileButton.onClick.AddListener(() => peekManager.Peek(ulong.Parse(clientId)));
            }
            else
            {
                profileButton.interactable = false;
                SetButtonColor(profileButton, Color.red);
            }
        }

        GameObject hideButton = Instantiate(hideLeaderboardButtonPrefab, leaderboardEntryContainer.transform);
        hideButton.GetComponent<Button>().onClick.AddListener(HideLongFormLeaderboard);
    }

    /// <summary>
    /// Creates the short-form leaderboard, showing only the local player's ranking.
    /// </summary>
    private void CreateShortFormLeaderboard()
    {
        GameObject entry = Instantiate(leaderboardEntryPrefab, leaderboardEntryContainer.transform);
        TextMeshProUGUI rankText = entry.transform.Find("RankingText").GetComponent<TextMeshProUGUI>();
        bool isUnranked = localPlayerRank >= receivedProfilePictureIds.Length - receivedUnrankedPlayerCount;
        rankText.text = isUnranked ? "<color=#ff0000>?</color>" : $"<color=#ff0000>{localPlayerRank + 1}.</color>";

        TextMeshProUGUI nameText = entry.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        nameText.text = "<color=#ff0000>You</color>";

        int profilePicId = int.Parse(receivedProfilePictureIds[localPlayerRank]);
        Image profilePicture = entry.transform.Find("ProfilePictureButton").GetComponent<Image>();
        profilePicture.sprite = profilePictureSprites[profilePicId];

        Button profileButton = entry.transform.Find("ProfilePictureButton").GetComponent<Button>();
        profileButton.interactable = false;

        SetButtonColor(profileButton, Color.red);

        GameObject showButton = Instantiate(showLeaderboardButtonPrefab, leaderboardEntryContainer.transform);
        showButton.GetComponent<Button>().onClick.AddListener(ShowLongFormLeaderboard);
    }

    /// <summary>
    /// Sets the color of the given button to the specified color.
    /// </summary>
    /// <param name="button">The button whose color will be set.</param>
    /// <param name="color">The color to apply to the button.</param>
    private void SetButtonColor(Button button, Color color)
    {
        ColorBlock colorBlock = button.colors;
        colorBlock.disabledColor = color;
        button.colors = colorBlock;
    }

    /// <summary>
    /// Refreshes the leaderboard UI by destroying old entries and creating new ones based on the current state.
    /// </summary>
    public void RefreshLeaderboard()
    {
        DestroyLeaderboardEntries();

        if (isLongFormLeaderboardShown)
        {
            CreateLongFormLeaderboard();
        }
        else
        {
            CreateShortFormLeaderboard();
        }
    }

    /// <summary>
    /// Displays the long-form leaderboard, showing detailed rankings for all players.
    /// </summary>
    public void ShowLongFormLeaderboard()
    {
        isLongFormLeaderboardShown = true;
        RefreshLeaderboard();
    }

    /// <summary>
    /// Hides the long-form leaderboard, showing only the short-form leaderboard.
    /// </summary>
    public void HideLongFormLeaderboard()
    {
        isLongFormLeaderboardShown = false;
        RefreshLeaderboard();
    }
}
