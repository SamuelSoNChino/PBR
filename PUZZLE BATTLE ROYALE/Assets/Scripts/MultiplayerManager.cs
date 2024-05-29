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

    void Start()
    {
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        string url = "SamuelSoNChino.eu.pythonanywhere.com";
        string ip = GetLocalIPAddress();
        UnityWebRequest matchRequest = UnityWebRequest.Get($"{url}/request_image?{ip}");
        yield return matchRequest.SendWebRequest();
        if (matchRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Failed to request a match.");
            yield break;
        }
        string instruction = matchRequest.downloadHandler.text;
        if (instruction == "WAIT")
        {
            networkManager.StartHost();
        }
        else
        {
            networkManager.GetComponent<UnityTransport>().ConnectionData.Address = instruction;
            networkManager.StartClient();
        }
    }
    public static string GetLocalIPAddress()
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

    void EndGame()
    {
        // TODO
    }
    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
