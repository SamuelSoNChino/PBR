using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ConnectionTestManager : MonoBehaviour
{
    [SerializeField] private string serverUrl;
    [SerializeField] private GameObject connectionErrorScreen;

    private static ConnectionTestManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        TestConnection();

        InvokeRepeating(nameof(TestConnection), 5f, 5f);
    }

    public void TestConnection()
    {
        StartCoroutine(TestConnectionCoroutine());
    }

    private IEnumerator TestConnectionCoroutine()
    {
        UnityWebRequest connectionTestRequest = UnityWebRequest.Get($"{serverUrl}/test_connection");
        yield return connectionTestRequest.SendWebRequest();

        if (connectionTestRequest.result != UnityWebRequest.Result.Success)
        {
            connectionErrorScreen.SetActive(true);
            yield break;
        }
        connectionErrorScreen.SetActive(false);
        Debug.Log("Successfully tested connection to the server");
    }
}
