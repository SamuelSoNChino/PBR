using System.Collections;
using Unity.Services.Core;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the multiplayer aspects of the game, including matchmaking, relay setup, game start, rematch handling, and game end.
/// </summary>
public class MultiplayerManager : NetworkBehaviour
{
    /// <summary>
    /// URL of the server used for matchmaking.
    /// </summary>
    [SerializeField] private string serverUrl;

    /// <summary>
    /// Reference to the PlayerManager script.
    /// </summary>
    [SerializeField] private PlayerManager playerManager;

    /// <summary>
    /// Reference to the PuzzleManager script.
    /// </summary>
    [SerializeField] private PuzzleManager puzzleManager;

    /// <summary>
    /// Reference to the PuzzleGenerator script.
    /// </summary>
    [SerializeField] private PuzzleGeneratorMultiplayer puzzleGenerator;

    /// <summary>
    /// Reference to the BackgroundManager script.
    /// </summary>
    [SerializeField] private BackgroundManagerMultiplayer backgroundManager;

    /// <summary>
    /// Reference to the LeaderboardManager script.
    /// </summary>
    [SerializeField] private LeaderboardManager leaderboardManager;

    /// <summary>
    /// Reference to the StartScreenManagerMultiplayer script.
    /// </summary>
    [SerializeField] private StartScreenManagerMultiplayer startScreenManagerMultiplayer;

    /// <summary>
    /// Reference to the EndScreenManagerMultiplayer script.
    /// </summary>
    [SerializeField] private EndScreenManagerMultiplayer endScreenManagerMultiplayer;

    /// <summary>
    /// Number of tiles of the puzzle.
    /// </summary>    
    [SerializeField] private int numberOfTiles;

    /// <summary>
    /// Number of players requested to be in the match. 2 is the default value if player haven't yet chosen differently in options.
    /// </summary>    
    private int numberOfPlayers = 2;

    /// <summary>
    /// Role of the player (HOST or CLIENT) in the multiplayer session.
    /// </summary>
    private string role;

    /// <summary>
    /// Relay join code for connecting to the relay server.
    /// </summary>
    private string relayJoinCode;

    /// <summary>
    /// TaskCompletionSource used to signal when a client has connected.
    /// </summary>
    private TaskCompletionSource<bool> allPlayersConnected;

    // -----------------------------------------------------------------------
    // Matchmaking
    // -----------------------------------------------------------------------

    /// <summary>
    /// Initializes matchmaking, signs in anonymously if not already signed in, requests a match, and handles relay setup.
    /// </summary>
    private async void Start()
    {
        startScreenManagerMultiplayer.StartMatchmakingCycle();

        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        if (PlayerPrefs.HasKey("numberOfPlayers"))
        {
            numberOfPlayers = PlayerPrefs.GetInt("numberOfPlayers");
        }

        TaskCompletionSource<bool> matchRequestCompleted = new();
        StartCoroutine(RequestMatch(matchRequestCompleted));
        await matchRequestCompleted.Task;

        if (role == "HOST")
        {
            playerManager.AddNewPlayer(new Player("Vajdik", NetworkManager.Singleton.LocalClientId, 0));

            await CreateRelay();

            TaskCompletionSource<bool> uploadRequestCompleted = new();
            StartCoroutine(UploadRelayJoinCode(uploadRequestCompleted));
            await uploadRequestCompleted.Task;

            // Initializes the TCS field that tracks whether the client connected to the relay or not
            allPlayersConnected = new TaskCompletionSource<bool>();

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            // The button enables the player to cancel matchmaking, when it is taking too long
            startScreenManagerMultiplayer.EnableCancelButton();

            await allPlayersConnected.Task;

            startScreenManagerMultiplayer.DisableCancelButton();

            // Removes callback after successful connection to prevent problems in other multiplayer sessions
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;

            StartCoroutine(StartNewGameServer());
        }
        else if (role == "CLIENT")
        {
            await JoinRelay();

            // Then waits for Rpc instructions from the server
        }
    }

    /// <summary>
    /// Adds a new player to player database and ends the matchmaking if the numebr of players has been reached.
    /// </summary>
    /// <param name="clientId">The ID of the client that connected.</param>
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client connected: {clientId}. Total connected clients: {NetworkManager.Singleton.ConnectedClients.Count}");

        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            playerManager.AddNewPlayer(new Player("pepis", clientId, 1));

            Debug.Log($"New player added. Current connected clients: {NetworkManager.Singleton.ConnectedClients.Count}");

            if (NetworkManager.Singleton.ConnectedClients.Count == numberOfPlayers)
            {
                allPlayersConnected.SetResult(true);
            }
        }
    }

    /// <summary>
    /// Removes the disconnected player from the player database
    /// </summary>
    /// <param name="clientId">.</param>
    private void OnClientDisconnected(ulong clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            playerManager.RemovePlayer(clientId);
        }
    }

    /// <summary>
    /// Requests a match from the server and waits for the response.
    /// </summary>
    /// <param name="requestCompleted">TaskCompletionSource to signal when the request is completed.</param>
    /// <returns>IEnumerator for the coroutine.</returns>
    private IEnumerator RequestMatch(TaskCompletionSource<bool> requestCompleted)
    {
        UnityWebRequest matchRequest = UnityWebRequest.Get($"{serverUrl}/request_match?number_of_players={numberOfPlayers}");
        yield return matchRequest.SendWebRequest();

        if (matchRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Failed to request a match.");
            yield break;
        }


        string[] information = matchRequest.downloadHandler.text.Split(",");
        role = information[0];
        if (role == "CLIENT")
        {
            relayJoinCode = information[1];
        }

        Debug.Log("Requested match successfully: " + information);

        requestCompleted.SetResult(true);
    }

    /// <summary>
    /// Uploads the relay join code and seed to the server.
    /// </summary>
    /// <param name="uploadCompleted">TaskCompletionSource to signal when the upload is completed.</param>
    /// <returns>IEnumerator for the coroutine.</returns>
    private IEnumerator UploadRelayJoinCode(TaskCompletionSource<bool> uploadCompleted)
    {
        string requestUrl = $"{serverUrl}/upload_relay_join_code?relay_join_code={relayJoinCode}&number_of_players={numberOfPlayers}";
        UnityWebRequest uploadRequest = UnityWebRequest.Get(requestUrl);
        yield return uploadRequest.SendWebRequest();

        if (uploadRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Failed to upload the relay join code.");
            yield break;
        }

        string response = uploadRequest.downloadHandler.text;
        if (response != "OK")
        {
            Debug.Log("API couldn't process the request.");
            yield break;
        }
        Debug.Log("Uploaded the join code successfully");

        uploadCompleted.SetResult(true);
    }

    /// <summary>
    /// Creates a relay for the host and starts the host session.
    /// </summary>
    /// <returns>Task representing the asynchronous operation.</returns>
    private async Task CreateRelay()
    {
        try
        {
            // Creates allocation for 1 client to join in
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(numberOfPlayers - 1);

            // Gets the relay join code from the allocation
            relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Successfully created relay with code: " + relayJoinCode);

            // Passes the relay server data to NetworkManager to allow connection between the players using Netcode
            RelayServerData relayServerData = new(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            // Starts the Netcode session as a host using the relayServerData
            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    /// <summary>
    /// Joins a relay using the relay join code and starts the client session.
    /// </summary>
    /// <returns>Task representing the asynchronous operation.</returns>
    private async Task JoinRelay()
    {
        try
        {
            // Creates joinAllocation using the relay join code passed from the host
            Debug.Log("Joining relay with code: " + relayJoinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

            // Passes the relay server data to NetworkManager to allow connection between the players using Netcode
            RelayServerData relayServerData = new(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            // Starts the Netcode session as a client using the relayServerData
            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }


    // -----------------------------------------------------------------------
    // Canceling a Match
    // -----------------------------------------------------------------------

    /// <summary>
    /// Cancels the matchmaking process and returns to the main menu.
    /// </summary>
    public async void CancelMatchmaking()
    {
        if (role == "HOST")
        {
            TaskCompletionSource<bool> codeRemovalCompleted = new();
            StartCoroutine(RequestJoinCodeRemoval(codeRemovalCompleted));
            await codeRemovalCompleted.Task;

            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("Menu");
        }
    }

    /// <summary>
    /// Sends a request to remove the relay join code and seed from the server.
    /// </summary>
    /// <param name="codeRemovalCompleted">TaskCompletionSource to signal when the code removal is completed.</param>
    /// <returns>IEnumerator for the coroutine.</returns>
    private IEnumerator RequestJoinCodeRemoval(TaskCompletionSource<bool> codeRemovalCompleted)
    {
        string requestUrl = $"{serverUrl}/request_join_code_removal?number_of_players={numberOfPlayers}";
        UnityWebRequest webRequest = UnityWebRequest.Get(requestUrl);
        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Failed to remove the join code from the server");
            yield break;
        }

        Debug.Log("Removed the join code successfully.");

        codeRemovalCompleted.SetResult(true);
    }

    // -----------------------------------------------------------------------
    // Game Start
    // -----------------------------------------------------------------------

    /// <summary>
    /// Starts a new game, handling countdown, puzzle generation etc.
    /// </summary>
    private IEnumerator StartNewGameServer()
    {
        startScreenManagerMultiplayer.StopMatchmakingCycleRpc();
        startScreenManagerMultiplayer.StartCountdownRpc();

        TaskCompletionSource<bool> countdownFinished = new();
        // Sets up an independent timer on the server to keep track of the countdown
        StartCoroutine(StartTimer(countdownFinished, 4));

        puzzleManager.GenerateNewPuzzleKey(numberOfTiles);

        puzzleGenerator.SetNumberOfTiles(numberOfTiles);
        int seed = Random.Range(1, 999999);

        yield return StartCoroutine(puzzleGenerator.RequestPuzzleImage(seed));
        yield return StartCoroutine(puzzleGenerator.RequestGridImage());

        puzzleGenerator.GenerateGridTiles();
        puzzleGenerator.GeneratePuzzleTiles();

        puzzleManager.ShuffleAllTiles(seed);

        backgroundManager.SetAllClientsDefaultBackgrounds();

        leaderboardManager.InitializeRanking();

        yield return new WaitUntil(() => countdownFinished.Task.IsCompleted);

        puzzleManager.EnableTileMovement();
        puzzleManager.EnableTouchInput();
    }

    /// <summary>
    /// Starts a simple timer and keeps track of its state using TSC.
    /// </summary>
    /// <param name="timerCompleted">TSC that keeps tracks of whether the duration has passed.</param>
    /// <param name="timerDuration">The duration the timer should run for.</param>
    private IEnumerator StartTimer(TaskCompletionSource<bool> timerCompleted, float timerDuration)
    {
        yield return new WaitForSeconds(timerDuration);
        timerCompleted.SetResult(true);
    }

    /// |---------------------------------|
    /// |            REMATCH              |
    /// |---------------------------------|

    /// <summary>
    /// Sends a server RPC to request a rematch.
    /// </summary>
    /// <param name="clientId">The ID of the client requesting the rematch.</param>
    [Rpc(SendTo.Server)]
    public void RequestRematchServerRpc(ulong clientId)
    {
        // Sends a request for a rematch to all clients
        RequestRematchClientRpc(clientId);
    }

    /// <summary>
    /// Sends a client RPC to notify other clients of the rematch request.
    /// </summary>
    /// <param name="clientId">The ID of the client requesting the rematch.</param>
    [Rpc(SendTo.ClientsAndHost)]
    private void RequestRematchClientRpc(ulong clientId)
    {
        // Triggers only for other player than the one that sent the rematch request to the server
        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            // Lets the other player know that there was a request for a rematch
            endScreenManagerMultiplayer.EnableRematchPending();
        }
    }

    /// <summary>
    /// Sends a server RPC to accept the rematch request.
    /// </summary>
    [Rpc(SendTo.Server)]
    public void AcceptRematchServerRpc()
    {
        // Starts the coroutine for the server to set up the rematch
        StartCoroutine(StartRematchServer());
    }

    /// <summary>
    /// Starts a rematch, handling countdown, puzzle generation etc.
    /// </summary>
    /// <returns>IEnumerator for the coroutine.</returns>
    private IEnumerator StartRematchServer()
    {
        // Unloads the end screen on all clients and starts the countdown
        endScreenManagerMultiplayer.UnloadEndScreenRpc();
        startScreenManagerMultiplayer.StartCountdownRpc();

        // Sets up an independent timer on the server to keep track of the countdown
        TaskCompletionSource<bool> countdownFinished = new();
        StartCoroutine(StartTimer(countdownFinished, 4));

        // Destroys all the tiles, including all the server information about clients, except moving permissions
        puzzleManager.DestroyAllTilesClientRpc();

        puzzleManager.ResetGridTilesByPositions();

        // Sets the number of tiles for puzzle generation and generates a seed
        puzzleManager.GenerateNewPuzzleKey(numberOfTiles);
        int seed = Random.Range(1, 999999);

        // Requests a new puzzle image based on the new seed and waits until it's downloaded
        yield return StartCoroutine(puzzleGenerator.RequestPuzzleImage(seed));

        // Initializes the destroyed information about clients again
        foreach (Player player in playerManager.GetAllPlayers())
        {
            player.ClearGridTilesCorrectlyOccupied();
            player.ClearPuzzleTilesPositions();
            player.ClearPuzzleTilesSnappedGridTiles();
        }

        // Generates the new puzzle tiles and shuffles them
        puzzleGenerator.GeneratePuzzleTiles();
        puzzleGenerator.GenerateGridTiles();

        // Shuffles all the puzzle tiles
        puzzleManager.ShuffleAllTiles(seed);

        // Waits until the countdown is finished
        yield return new WaitUntil(() => countdownFinished.Task.IsCompleted);

        // Enables touch input and manipulating with the tiles
        puzzleManager.EnableTileMovement();
        puzzleManager.EnableTouchInput();
    }


    // -----------------------------------------------------------------------
    // Game End
    // -----------------------------------------------------------------------

    /// <summary>
    /// Ends the game by disabling the touch input and loading the relevant end screen for all clients.
    /// </summary>
    /// /// <param name="winningClientId">The ID of the client that has completed the puzzle first.</param>
    public void EndGame(ulong winningClientId)
    {
        puzzleManager.DisableTileMovement();
        puzzleManager.DisableTouchInput();

        endScreenManagerMultiplayer.LoadWinningScreenRpc(winningClientId);

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (winningClientId != clientId)
            {
                endScreenManagerMultiplayer.LoadLosingScreenRpc(clientId);
            }
        }

    }
}
