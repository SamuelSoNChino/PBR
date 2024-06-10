using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ConnectionTestManager : MonoBehaviour
{
    [SerializeField] private string serverUrl;
    [SerializeField] private GameObject connectionErrorScreen;
    [SerializeField] private float testingFrequency;

    private static ConnectionTestManager instance;

    private void Awake()
    {
        // If wasn't initiaized yet
        if (instance == null)
        {
            // Sets the instance to this and apply DontDestroyonLoad
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Destroy the object trying to bocome ConnectionTestManagaer, since one already exists
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Tests the connection right at the start of the game
        TestConnection();

        // Repeats the connection test every 5 seconds
        InvokeRepeating(nameof(TestConnection), testingFrequency, testingFrequency);
    }

    public void TestConnection()
    {
        // Starts the coroutine for testing connection
        StartCoroutine(TestConnectionCoroutine());
    }

    private IEnumerator TestConnectionCoroutine()
    {
        // Sends a request to the python server to test connection
        UnityWebRequest connectionTestRequest = UnityWebRequest.Get($"{serverUrl}/test_connection");
        yield return connectionTestRequest.SendWebRequest();

        // If the request wasn't successful, loads the connection error screen and breaks the coroutine
        if (connectionTestRequest.result != UnityWebRequest.Result.Success)
        {
            connectionErrorScreen.SetActive(true);
            yield break;
        }

        // Downloads the response from the server
        string response = connectionTestRequest.downloadHandler.text;

        // If the response was "OK", hides te connection error screen, otherwise loads it
        if (response == "OK")
        {
            connectionErrorScreen.SetActive(false);
            Debug.Log("Successfully tested connection to the server");
        }
        else
        {
            connectionErrorScreen.SetActive(true);
        }
    }
}
