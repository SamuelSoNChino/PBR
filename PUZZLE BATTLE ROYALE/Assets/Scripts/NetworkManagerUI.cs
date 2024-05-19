using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetwworkManagerUI: MonoBehaviour
{
    [SerializeField] private Button hostB;
    [SerializeField] private Button clientB;
    [SerializeField] private Button serverB;    
    private void Awake()
    {
        hostB.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
            Debug.Log("Started a host ");
        });
        clientB.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
        });
        serverB.onClick.AddListener(() => {
            NetworkManager.Singleton.StartServer();
        });
    }
}
