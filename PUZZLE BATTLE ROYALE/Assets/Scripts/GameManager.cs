using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Net.Sockets;
using System.Net;
using UnityEngine.Networking;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class GameManager : MonoBehaviour
{
    private bool isSingleplayer;
    private Timer timer;
    [SerializeField] NetworkManager networkManager;
    void Start()
    {
        timer = GameObject.Find("Timer").GetComponent<Timer>();
        if (SceneManager.GetActiveScene().name == "PuzzleSingleplayer")
        {
            StartSingePlayer();
            isSingleplayer = true;
        }
        else
        {
            isSingleplayer = false;
            StartCoroutine(StartMultiplayer());
        }
    }
    IEnumerator StartMultiplayer()
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

    private void StartSingePlayer()
    {
        GameObject.Find("Puzzle").GetComponent<PuzzleGenerator>().StartGenerating();
        timer.EnableTimer();
    }

    public void EndGame()
    {
        if (isSingleplayer)
        {
            EndSingleplayer();
        }
        else
        {
            EndMultiplayer();
        }

    }
    private void EndSingleplayer()
    {
        timer.DisableTimer();
        int finalTime = timer.GetCurrentTime();
        PlayerPrefs.SetInt("LastTime", finalTime);
        if (finalTime < PlayerPrefs.GetInt("BestTime") || PlayerPrefs.GetInt("BestTime") < 0)
        {
            PlayerPrefs.SetInt("BestTime", finalTime);
        }
        SceneManager.LoadScene("EndScreen");
    }

    private void EndMultiplayer()
    {
        // TODO
    }
    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
