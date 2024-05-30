using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class MultiplayerManager : MonoBehaviour
{
    [SerializeField] NetworkManager networkManager;
    [SerializeField] string serverUrl;
    [SerializeField] PuzzleGenerator puzzleGenerator;
    [SerializeField] TilesManager tilesManager;
    private string localIP;
    private int seed;
    private string role;
    private string hostIP;

    private void Start()
    {
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        GetLocalIPAddress();
        Debug.Log(localIP); // Haha Peter
        yield return RequestMatch();
        if (role == "HOST")
        {
            networkManager.StartHost();
        }
        else if (role == "CLIENT")
        {
            networkManager.GetComponent<UnityTransport>().ConnectionData.Address = hostIP;
            networkManager.StartClient();
        }
        yield return StartCoroutine(RequestMatch());
        yield return StartCoroutine(puzzleGenerator.RequestPuzzleImage(seed));
        yield return StartCoroutine(puzzleGenerator.RequestGridImage());
        puzzleGenerator.GenerateTiles();
        tilesManager.ShuffleAllTiles();
    }

    private IEnumerator RequestMatch()
    {
        UnityWebRequest matchRequest = UnityWebRequest.Get($"{serverUrl}/request_match?ip={localIP}");
        Debug.Log($"{serverUrl}/request_image?ip={localIP}");
        yield return matchRequest.SendWebRequest();
        if (matchRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Failed to request a match.");
            yield break;
        }
        string[] information = matchRequest.downloadHandler.text.Split(",");
        seed = int.Parse(information[0]);
        role = information[1];
        if (role == "CLIENT")
        {
            hostIP = information[2];
        }
    }
    public void GetLocalIPAddress()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            localIP = GetLocalIPAddressAndroid();
        }
        else
        {
            localIP = GetLocalIPAddressWidnows();
        }
    }

    public static string GetLocalIPAddressWidnows()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new System.Exception("Couldn't find local IP address!");
    }

    public static string GetLocalIPAddressAndroid()
    {
        string ipAddress = string.Empty;

        foreach (var networkInterface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
        {
            if (networkInterface.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
            {
                var properties = networkInterface.GetIPProperties();
                foreach (var address in properties.UnicastAddresses)
                {
                    if (address.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(address.Address))
                    {
                        ipAddress = address.Address.ToString();
                        // We found a valid IP address, return it
                        return ipAddress;
                    }
                }
            }
        }

        if (string.IsNullOrEmpty(ipAddress))
        {
            Debug.LogError("Local IP Address Not Found!");
        }

        return ipAddress;
    }

    private void NotifyPuzzleCompletion()
    {
        if (networkManager.IsServer)
        {
            NotifyPuzzleCompletionClientRpc();
        }
        else
        {
            NotifyPuzzleCompletionServerRpc();
        }
    }

    [ServerRpc]
    private void NotifyPuzzleCompletionServerRpc(ServerRpcParams rpcParams = default)
    {
        NotifyPuzzleCompletionClientRpc();
    }

    [ClientRpc]
    private void NotifyPuzzleCompletionClientRpc(ClientRpcParams rpcParams = default)
    {
        Debug.Log("Puzzle Solved by another player!");
        SceneManager.LoadScene("Menu");
    }

    public void EndGame()
    {
        NotifyPuzzleCompletion();
        SceneManager.LoadScene("EndScreen");
    }
    public void BackToMenu()
    {
        // networkManager.Shutdown();
        SceneManager.LoadScene("Menu");
    }
}
