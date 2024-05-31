using System.Collections;
using Unity.Services.Core;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication;
using System.Threading.Tasks;

public class MultiplayerManager : NetworkBehaviour
{
    [SerializeField] NetworkManager networkManager;
    [SerializeField] string serverUrl;
    [SerializeField] PuzzleGenerator puzzleGenerator;
    [SerializeField] TilesManager tilesManager;
    private string role;
    private int seed;
    private string relayJoinCode;

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        StartCoroutine(RequestMatch(tcs));
        await tcs.Task;

        if (role == "HOST")
        {
            CreateRelay();
        }
        else if (role == "CLIENT")
        {
            // TODO
        }
    }

    private IEnumerator StartGame()
    {
        puzzleGenerator.SetNumberOfTiles(2);
        yield return StartCoroutine(puzzleGenerator.RequestPuzzleImage(seed));
        yield return StartCoroutine(puzzleGenerator.RequestGridImage());
        puzzleGenerator.GenerateTiles();
        tilesManager.ShuffleAllTiles();
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
        tcs.SetResult(true);
    }


    public void EndGame()
    {
        SceneManager.LoadScene("EndScreen");
    }
    public void BackToMenu()
    {
        // networkManager.Shutdown();
        SceneManager.LoadScene("Menu");
    }
}
