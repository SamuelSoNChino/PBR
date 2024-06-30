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
    /// Reference to the PuzzleManager script.
    /// </summary>
    [SerializeField] private PuzzleManager puzzleManager;

    /// <summary>
    /// Reference to the PuzzleGenerator script.
    /// </summary>
    [SerializeField] private PuzzleGeneratorMultiplayer puzzleGenerator;

    /// <summary>
    /// Reference to the TilesManager script.
    /// </summary>
    [SerializeField] private TilesManagerMultiplayer tilesManager;

    /// <summary>
    /// Reference to the BackgroundManager script.
    /// </summary>
    [SerializeField] private BackgroundManagerMultiplayer backgroundManager;

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
    private TaskCompletionSource<bool> connectionCompleted;



    /// |---------------------------------|
    /// |           MATCHMAKING           |
    /// |---------------------------------|

    /// <summary>
    /// Initializes matchmaking, signs in anonymously if not already signed in, requests a match, and handles relay setup.
    /// </summary>
    private async void Start()
    {
        // Starts the matchmaking message cycle on the start screen
        startScreenManagerMultiplayer.StartMatchmakingCycle();

        // Initializes UnityServices
        await UnityServices.InitializeAsync();

        // Logs player's ID after they log in in the debug console
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        // If the player isn't signed in, signs in anonymously
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        // Requests a new match and waits until the request is completed
        TaskCompletionSource<bool> matchRequestCompleted = new();
        StartCoroutine(RequestMatch(matchRequestCompleted));
        await matchRequestCompleted.Task;

        // If the matchmaking server assigned HOST role
        if (role == "HOST")
        {
            // Creates a new relay as a host
            await CreateRelay();

            // Uploads the relay join code on the matchmaking server and waits until the request has been completed
            TaskCompletionSource<bool> uploadRequestCompleted = new();
            StartCoroutine(UploadRelayJoinCode(uploadRequestCompleted));
            await uploadRequestCompleted.Task;

            // Initializes the TCS field that tracks whether the client connected to the relay or not
            connectionCompleted = new TaskCompletionSource<bool>();

            // When a client connects to the relay, calls OnClientConnected method, setting the connectionCompleted as done
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            // The button enables the player to cancel matchmaking, when it is taking too long
            startScreenManagerMultiplayer.EnableCancelButton();

            // Waits until the client has connected
            await connectionCompleted.Task;

            // After the client has connected, the player can no longer use the button to cancel matchmaking
            startScreenManagerMultiplayer.DisableCancelButton();

            // Removes callback after successful connection to prevent problems in other multiplayer sessions
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;

            // Starts the coruoutine for the server to start the game
            StartCoroutine(StartNewGameServer());
        }

        // If the matchmaking server assigned CLIENT role
        else if (role == "CLIENT")
        {
            // Waits until successfully connected to the relay using the relay join code from the server
            await JoinRelay();

            // Then waits for Rpc instructions from the server
        }
    }

    /// <summary>
    /// Called when a client connects to the relay. Sets the connectionCompleted TaskCompletionSource as done.
    /// </summary>
    /// <param name="clientId">The ID of the client that connected.</param>
    private void OnClientConnected(ulong clientId)
    {
        // Condition to exclude the host from triggering it
        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            // Ensure the TaskCompletionSource is not already completed (probably doesn't even need to be here)
            if (connectionCompleted != null && !connectionCompleted.Task.IsCompleted)
            {
                // When a client successfully connects, sets the result to completed
                connectionCompleted.SetResult(true);
            }
        }
    }

    /// <summary>
    /// Requests a match from the server and waits for the response.
    /// </summary>
    /// <param name="requestCompleted">TaskCompletionSource to signal when the request is completed.</param>
    /// <returns>IEnumerator for the coroutine.</returns>
    private IEnumerator RequestMatch(TaskCompletionSource<bool> requestCompleted)
    {
        // Sends a request for a match to the Python server and waits until it was processed
        UnityWebRequest matchRequest = UnityWebRequest.Get($"{serverUrl}/request_match");
        yield return matchRequest.SendWebRequest();

        // If the request wasn't successful, breaks the coroutine
        if (matchRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Failed to request a match.");
            yield break;
        }

        // Splits the information from the server and saves the role and seed
        string[] information = matchRequest.downloadHandler.text.Split(",");
        role = information[0];

        // If the device's role is client, information also contains relay join code
        if (role == "CLIENT")
        {
            relayJoinCode = information[1];
        }
        Debug.Log("Requested match successfully: " + information);

        // Sets requestCompleted to completed, allowing the rest of the code to continue
        requestCompleted.SetResult(true);
    }

    /// <summary>
    /// Uploads the relay join code and seed to the server.
    /// </summary>
    /// <param name="uploadCompleted">TaskCompletionSource to signal when the upload is completed.</param>
    /// <returns>IEnumerator for the coroutine.</returns>
    private IEnumerator UploadRelayJoinCode(TaskCompletionSource<bool> uploadCompleted)
    {
        // Sends a request to upload the relay join code and seed to the Python server
        string requestUrl = $"{serverUrl}/upload_relay_join_code?relay_join_code={relayJoinCode}";
        UnityWebRequest uploadRequest = UnityWebRequest.Get(requestUrl);
        yield return uploadRequest.SendWebRequest();

        // If the request wasn't successful, breaks the coroutine
        if (uploadRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Failed to upload the relay join code.");
            yield break;
        }

        // If the response wasn't "OK", breaks the coroutine
        string response = uploadRequest.downloadHandler.text;
        if (response != "OK")
        {
            Debug.Log("API couldn't process the request.");
            yield break;
        }
        Debug.Log("Uploaded the join code successfully");

        // Sets uploadCompleted to completed, allowing the rest of the code to continue
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
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);

            // Gets the relay join code from the allocation
            relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Successfully created relay with code: " + relayJoinCode);

            // Passes the relay server data to NetworkManager to allow connection between the players using Netcode
            RelayServerData relayServerData = new(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            // Starts the Netcode session as a host using the relayServerData
            NetworkManager.Singleton.StartHost();
        }
        // If something went wrong, logs the exception in the debug console
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
        // If something went wrong, logs the exception in the debug console
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    /// |---------------------------------|
    /// |        CANCELING A MATCH        |
    /// |---------------------------------|

    /// <summary>
    /// Cancels the matchmaking process and returns to the main menu.
    /// </summary>
    public async void CancelMatchmaking()
    {
        //  Only the host can cancel the matchmaking
        if (role == "HOST")
        {
            // Asks the server to remove the relay join code, so no client will try to connect to it
            TaskCompletionSource<bool> codeRemovalCompleted = new();
            StartCoroutine(RequestJoinCodeRemoval(codeRemovalCompleted));
            await codeRemovalCompleted.Task;

            // Shuts down all the network connections
            NetworkManager.Singleton.Shutdown();

            // Sends the player back to menu
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
        // Sends a request to remove the relay join code and seed from the server
        string requestUrl = $"{serverUrl}/request_join_code_removal?relay_join_code={relayJoinCode}";
        UnityWebRequest webRequest = UnityWebRequest.Get(requestUrl);
        yield return webRequest.SendWebRequest();

        // If the request wasn't successful, breaks the coroutine
        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Failed to remove the join code from the server");
            yield break;
        }

        Debug.Log("Removed the join code successfully.");

        // Sets codeRemovalCompleted to completed, allowing the rest of the code to continue
        codeRemovalCompleted.SetResult(true);
    }

    /// |---------------------------------|
    /// |           GAME START            |
    /// |---------------------------------|

    /// <summary>
    /// Starts a new game, handling countdown, puzzle generation etc.
    /// </summary>
    private IEnumerator StartNewGameServer()
    {
        // Stops the matchmaking matchmaking cycle and starts countdown for all the clients
        startScreenManagerMultiplayer.StopMatchmakingCycleRpc();
        startScreenManagerMultiplayer.StartCountdownRpc();

        // Sets up an independent timer on the server to keep track of the countdown
        TaskCompletionSource<bool> countdownFinished = new();
        StartCoroutine(StartTimer(countdownFinished, 4));

        // Collects all the players backgrounds
        backgroundManager.CollectPlayerBackgroundsRpc();

        // Generates a new puzzle key (tile and grid IDs)
        puzzleManager.GenerateNewPuzzleKey(numberOfTiles);

        // Sets the number of tiles for puzzle generation and generates a seed
        puzzleGenerator.SetNumberOfTiles(numberOfTiles);
        int seed = Random.Range(1, 999999);

        // Request both puzzle and grid images and waits until they are downloaded
        yield return StartCoroutine(puzzleGenerator.RequestPuzzleImage(seed));
        yield return StartCoroutine(puzzleGenerator.RequestGridImage());

        // Generates both puzzle and grid tiles
        puzzleGenerator.GenerateGridTiles();
        puzzleGenerator.GeneratePuzzleTiles();

        // Initializes all the necessary information about the clients
        puzzleManager.InitializeClientPositions();
        puzzleManager.InitializeClientSnappedGridTiles();
        puzzleManager.InitializeClientStatuses();
        puzzleManager.InitializeClientMovementPermissions();

        // Shuffles all the puzzle tiles
        puzzleManager.ShuffleAllTiles(seed);

        // Waits until the countdown has finished
        yield return new WaitUntil(() => countdownFinished.Task.IsCompleted);

        // Enables touch input and tile manipulation
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
        puzzleManager.DestroyAllTilesServer();

        // Sets the number of tiles for puzzle generation and generates a seed
        puzzleManager.GenerateNewPuzzleKey(numberOfTiles);
        int seed = Random.Range(1, 999999);

        // Requests a new puzzle image based on the new seed and waits until it's downloaded
        yield return StartCoroutine(puzzleGenerator.RequestPuzzleImage(seed));

        // Generates the new puzzle tiles and shuffles them
        puzzleGenerator.GeneratePuzzleTiles();
        puzzleGenerator.GenerateGridTiles();

        // Initializes the destroyed information about clients again
        puzzleManager.InitializeClientPositions();
        puzzleManager.InitializeClientSnappedGridTiles();
        puzzleManager.InitializeClientStatuses();

        // Shuffles all the puzzle tiles
        puzzleManager.ShuffleAllTiles(seed);

        // Waits until the countdown is finished
        yield return new WaitUntil(() => countdownFinished.Task.IsCompleted);

        // Enables touch input and manipulating with the tiles
        puzzleManager.EnableTileMovement();
        puzzleManager.EnableTouchInput();
    }

    /// |---------------------------------|
    /// |            GAME END             |
    /// |---------------------------------|

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
