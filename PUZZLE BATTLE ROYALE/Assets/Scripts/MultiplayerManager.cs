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
    /// Reference to the PuzzleGenerator script.
    /// </summary>
    [SerializeField] private PuzzleGenerator puzzleGenerator;
    
    /// <summary>
    /// Reference to the TilesManager script.
    /// </summary>
    [SerializeField] private TilesManager tilesManager;
    
    /// <summary>
    /// Reference to the GridManager script.
    /// </summary>
    [SerializeField] private GridManager gridManager;
    
    /// <summary>
    /// Reference to the StartScreenManagerMultiplayer script.
    /// </summary>
    [SerializeField] private StartScreenManagerMultiplayer startScreenManagerMultiplayer;
    
    /// <summary>
    /// Reference to the EndScreenManagerMultiplayer script.
    /// </summary>
    [SerializeField] private EndScreenManagerMultiplayer endScreenManagerMultiplayer;
    
    /// <summary>
    /// Reference to the PanZoom script.
    /// </summary>
    [SerializeField] private PanZoom panZoom;

    /// <summary>
    /// Role of the player (HOST or CLIENT) in the multiplayer session.
    /// </summary>
    private string role;
    
    /// <summary>
    /// Seed used for generating the puzzle.
    /// </summary>
    private int seed;
    
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

            // Waits until the client has connected
            await connectionCompleted.Task;

            // Removes callback after successful connection to prevent problems in other multiplayer sessions
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;

            // Sends an RPC to all clients (including itself) to start the game
            StartGameClientRpc();
        }

        // If the matchmaking server assigned CLIENT role
        else if (role == "CLIENT")
        {
            // Waits until successfully connected to the relay using the relay join code from the server
            await JoinRelay();
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
        seed = int.Parse(information[1]);

        // If the device's role is client, information also contains relay join code
        if (role == "CLIENT")
        {
            relayJoinCode = information[2];
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
        string requestUrl = $"{serverUrl}/upload_relay_join_code?relay_join_code={relayJoinCode}&seed={seed}";
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

    /// <summary>
    /// Cancels the matchmaking process and returns to the main menu.
    /// </summary>
    public async void CancelMatchmaking()
    {
        NetworkManager.Singleton.Shutdown();
        if (relayJoinCode != null)
        {
            TaskCompletionSource<bool> codeRemovalCompleted = new();
            StartCoroutine(RequestJoinCodeRemoval(codeRemovalCompleted));
            await codeRemovalCompleted.Task;
        }
        // TODO: Handle client-specific scenarios
        SceneManager.LoadScene("Menu");
    }

    /// <summary>
    /// Sends a request to remove the relay join code and seed from the server.
    /// </summary>
    /// <param name="codeRemovalCompleted">TaskCompletionSource to signal when the code removal is completed.</param>
    /// <returns>IEnumerator for the coroutine.</returns>
    private IEnumerator RequestJoinCodeRemoval(TaskCompletionSource<bool> codeRemovalCompleted)
    {
        // Sends a request to remove the relay join code and seed from the server
        string requestUrl = $"{serverUrl}/request_join_code_removal?relay_join_code={relayJoinCode}&seed={seed}";
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
    /// Sends an RPC to all clients to start the game.
    /// </summary>
    [ClientRpc]
    private void StartGameClientRpc()
    {
        // Starts a coroutine for a new game
        StartCoroutine(StartNewGame());
    }

    /// <summary>
    /// Starts a new game, handles countdown, and puzzle generation.
    /// </summary>
    /// <returns>IEnumerator for the coroutine.</returns>
    private IEnumerator StartNewGame()
    {
        // Stops the matchmaking starting screen
        startScreenManagerMultiplayer.StopMatchmakingCycle();

        // Starts the countdown on the start screen and tracks whether it is finished 
        TaskCompletionSource<bool> countdownFinished = new();
        StartCoroutine(startScreenManagerMultiplayer.StartCountdown(countdownFinished));

        // Sets the number of tiles for puzzle generation, only a temporary solution (2 for debugging)
        puzzleGenerator.SetNumberOfTiles(2);

        // Request both puzzle and grid images and waits until they are downloaded
        yield return StartCoroutine(puzzleGenerator.RequestPuzzleImage(seed));
        yield return StartCoroutine(puzzleGenerator.RequestGridImage());

        // Generates both puzzle and grid tiles and shuffles puzzle tiles
        puzzleGenerator.GenerateGridTiles();
        puzzleGenerator.GeneratePuzzleTiles();
        tilesManager.ShuffleAllTiles();

        // Waits until the countdown has finished
        yield return new WaitUntil(() => countdownFinished.Task.IsCompleted);

        // Enables touch input and tile manipulation
        tilesManager.EnableAllColliders();
        panZoom.EnableTouchInput();
    }

    /// |---------------------------------|
    /// |            REMATCH              |
    /// |---------------------------------|

    /// <summary>
    /// Sends a server RPC to request a rematch.
    /// </summary>
    /// <param name="clientId">The ID of the client requesting the rematch.</param>
    [ServerRpc(RequireOwnership = false)]
    public void RequestRematchServerRpc(ulong clientId)
    {
        // Sends a request for a rematch to all clients
        RequestRematchClientRpc(clientId);
    }

    /// <summary>
    /// Sends a client RPC to notify other clients of the rematch request.
    /// </summary>
    /// <param name="clientId">The ID of the client requesting the rematch.</param>
    [ClientRpc]
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
    [ServerRpc(RequireOwnership = false)]
    public void AcceptRematchServerRpc()
    {
        // Starts the coroutine for the server to set up the rematch
        StartCoroutine(AcceptRematch());
    }

    /// <summary>
    /// Sets up the rematch by requesting a new seed and starting a new game.
    /// </summary>
    /// <returns>IEnumerator for the coroutine.</returns>
    private IEnumerator AcceptRematch()
    {
        // Requests a new seed and waits until it has been given by the Python server
        yield return StartCoroutine(RequestNewSeed());

        // Sends the seed to all the clients
        SetNewSeedClientRpc(seed);

        // Starts a rematch for all the clients
        StartRematchClientRpc();
    }

    /// <summary>
    /// Requests a new seed from the server.
    /// </summary>
    /// <returns>IEnumerator for the coroutine.</returns>
    private IEnumerator RequestNewSeed()
    {
        // Sends a request for a new seed to the Python server
        UnityWebRequest seedRequest = UnityWebRequest.Get($"{serverUrl}/request_new_seed");
        yield return seedRequest.SendWebRequest();

        // If the request wasn't successful, breaks the coroutine
        if (seedRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Failed to request a new seed");
            yield break;
        }

        // Downloads the new seed and sets it for the host (server)
        seed = int.Parse(seedRequest.downloadHandler.text);
        Debug.Log("Requested the new seed successfully.");
    }

    /// <summary>
    /// Sends a client RPC to set the new seed on the clients.
    /// </summary>
    /// <param name="newSeed">The new seed value.</param>
    [ClientRpc]
    private void SetNewSeedClientRpc(int newSeed)
    {
        // Sets the new seed
        seed = newSeed;
        Debug.Log("New seed: " + seed);
    }

    /// <summary>
    /// Sends a client RPC to start the rematch on the clients.
    /// </summary>
    [ClientRpc]
    public void StartRematchClientRpc()
    {
        // Starts the coroutine for a rematch for all clients
        StartCoroutine(StartRematch());
    }

    /// <summary>
    /// Starts a rematch, handling countdown, and puzzle generation.
    /// </summary>
    /// <returns>IEnumerator for the coroutine.</returns>
    private IEnumerator StartRematch()
    {
        // Unloads the end screen
        endScreenManagerMultiplayer.UnloadEndScreen();

        // Starts the countdown on the start screen and tracks whether it is finished 
        TaskCompletionSource<bool> countdownFinished = new();
        StartCoroutine(startScreenManagerMultiplayer.StartCountdown(countdownFinished));

        // Destroys all the puzzle tiles from last game and resets the grid to prepare for the rematch
        tilesManager.DestroyAllTiles();
        gridManager.ResetCompleteness();

        // Requests a new puzzle image based on the new seed and waits until it's downloaded
        yield return StartCoroutine(puzzleGenerator.RequestPuzzleImage(seed));

        // Generates the new puzzle tiles and shuffles them
        puzzleGenerator.GeneratePuzzleTiles();
        tilesManager.ShuffleAllTiles();

        // Waits until the countdown is finished
        yield return new WaitUntil(() => countdownFinished.Task.IsCompleted);

        // Enables touch input and manipulating with the tiles
        tilesManager.EnableAllColliders();
        panZoom.EnableTouchInput();
    }

    /// |---------------------------------|
    /// |            GAME END             |
    /// |---------------------------------|

    /// <summary>
    /// Notifies the server that the player has completed the puzzle.
    /// </summary>
    public void EndGame()
    {
        // Informs the server that this player has completed the puzzle
        NotifyServerGameOverServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    /// <summary>
    /// Sends a server RPC to notify that the game is over.
    /// </summary>
    /// <param name="clientId">The ID of the client that completed the puzzle.</param>
    [ServerRpc(RequireOwnership = false)]
    private void NotifyServerGameOverServerRpc(ulong clientId)
    {
        // Informs all other players that the game is over, sending them the ID of the winning player
        ulong winnerClientId = clientId;
        NotifyClientsGameOverClientRpc(winnerClientId);
    }

    /// <summary>
    /// Sends a client RPC to notify clients that the game is over and handle the end screen.
    /// </summary>
    /// <param name="winningClientId">The ID of the winning client.</param>
    [ClientRpc]
    private void NotifyClientsGameOverClientRpc(ulong winningClientId)
    {
        // Disables all touch input and tile manipulation
        tilesManager.DisableAllColliders();
        panZoom.DisableTouchInput();

        // If the client isn't the winning player
        if (NetworkManager.Singleton.LocalClientId != winningClientId)
        {
            // Loads the losing screen
            endScreenManagerMultiplayer.LoadLosingScreen();
        }
        else
        {
            // Loads the winning screen
            endScreenManagerMultiplayer.LoadWinningScreen();
        }
    }
}
