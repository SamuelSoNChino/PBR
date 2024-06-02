using System.Collections;
using Unity.Services.Core;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;

public class MultiplayerManager : NetworkBehaviour
{
    [SerializeField] private string serverUrl;
    [SerializeField] private PuzzleGenerator puzzleGenerator;
    [SerializeField] private TilesManager tilesManager;
    [SerializeField] private StartScreenManager startScreenManager;
    [SerializeField] private EndScreenManagerMultiplayer endScreenManagerMultiplayer;

    private string role;
    private int seed;
    private string relayJoinCode;
    private bool gameEnded = false;
    private ulong winnerClientId = 0;

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        TaskCompletionSource<bool> tcs = new();
        StartCoroutine(RequestMatch(tcs));
        await tcs.Task;

        if (role == "HOST")
        {
            await CreateRelay();
            tcs = new();
            StartCoroutine(UploadRelayJoinCode(tcs));
            await tcs.Task;
            tcs = new();
            NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
            {
                if (clientId != NetworkManager.Singleton.LocalClientId)
                {
                    tcs.SetResult(true);
                }
            };
            await tcs.Task;
            StartGameHost();
        }
        else if (role == "CLIENT")
        {
            await JoinRelay();
        }
    }

    private void StartGameHost()
    {
        StartGameClientRpc();
        StartCoroutine(StartGame());
    }

    [ClientRpc]
    private void StartGameClientRpc()
    {
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        startScreenManager.StopMatchmakingCycle();
        StartCoroutine(startScreenManager.StartCountdown());
        puzzleGenerator.SetNumberOfTiles(2);
        yield return StartCoroutine(puzzleGenerator.RequestPuzzleImage(seed));
        yield return StartCoroutine(puzzleGenerator.RequestGridImage());
        puzzleGenerator.GenerateTiles();
        tilesManager.ShuffleAllTiles();
    }

    private async Task CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
            relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Successsfully created relay with code: " + relayJoinCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async Task JoinRelay()
    {
        try
        {
            Debug.Log("Joining relay with code: " + relayJoinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    private IEnumerator RequestMatch(TaskCompletionSource<bool> tcs)
    {
        UnityWebRequest matchRequest = UnityWebRequest.Get($"{serverUrl}/request_match");
        yield return matchRequest.SendWebRequest();
        if (matchRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Failed to request a match.");
            yield break;
        }
        string[] information = matchRequest.downloadHandler.text.Split(",");
        role = information[0];
        seed = int.Parse(information[1]);
        if (role == "CLIENT")
        {
            relayJoinCode = information[2];
        }
        Debug.Log("Requested match successfully: " + information);
        tcs.SetResult(true);
    }

    private IEnumerator UploadRelayJoinCode(TaskCompletionSource<bool> tcs)
    {
        string requestUrl = $"{serverUrl}/upload_relay_join_code?relay_join_code={relayJoinCode}&seed={seed}";
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
        Debug.Log("Uploaded the join code successfuly");
        tcs.SetResult(true);
    }

    public void EndGame()
    {
        if (IsServer)
        {
            if (!gameEnded)
            {
                gameEnded = true;
                winnerClientId = NetworkManager.Singleton.LocalClientId;
                endScreenManagerMultiplayer.LoadWinningScreen();
                NotifyClientsGameOverClientRpc(winnerClientId);
            }
        }
        else
        {
            NotifyServerGameOverServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ClientRpc]
    private void NotifyClientsGameOverClientRpc(ulong winningClientId)
    {
        if (NetworkManager.Singleton.LocalClientId != winningClientId)
        {
            endScreenManagerMultiplayer.LoadLosingScreen();
        }
        else
        {
            endScreenManagerMultiplayer.LoadWinningScreen();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void NotifyServerGameOverServerRpc(ulong clientId)
    {
        if (!gameEnded)
        {
            gameEnded = true;
            winnerClientId = clientId;
            endScreenManagerMultiplayer.LoadLosingScreen();
            NotifyClientsGameOverClientRpc(winnerClientId);
        }
    }
}
