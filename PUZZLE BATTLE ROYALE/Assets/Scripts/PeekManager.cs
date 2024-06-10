using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PeekManager : MonoBehaviour
{
    [SerializeField] private TilesManager tilesManager;

    private Vector3[] originalPositions;
    private ulong targetedClientId = 0;
    private ulong requestingClientId = 0;
    private bool sendPositionsPeriodically;

    public void Peek()
    {
        originalPositions = tilesManager.GetAllPositions();
        tilesManager.DisableAllColliders();
        Debug.Log($"[Client] Client {NetworkManager.Singleton.LocalClientId} requesting peek.");
        RequestPeekServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPeekServerRpc(ulong sentRequestingClientId)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogError("[Server] Error: This method should only run on the server.");
            return;
        }

        requestingClientId = sentRequestingClientId;
        Debug.Log($"[Server] Connected clients: {string.Join(", ", NetworkManager.Singleton.ConnectedClientsIds)}");

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientId != requestingClientId)
            {
                targetedClientId = clientId;
                break;
            }
        }

        Debug.Log($"[Server] Client: {requestingClientId} has requested to use Peek on {targetedClientId}.");
        StartCoroutine(StartPeekTimer());
    }

    private IEnumerator StartPeekTimer()
    {
        Debug.Log("[Server] Requesting targeted client to periodically send positions.");
        StartSendingPositionClientRpc(targetedClientId);
        yield return new WaitForSeconds(5);

        Debug.Log("[Server] The peek timer has ended. Requesting to stop sending positions periodically and restore original puzzle.");
        StopSendingPositionsPeriodicallyClientRpc(targetedClientId);
        RestoreOriginalPuzzleClientRpc(requestingClientId);
        requestingClientId = 0;
        targetedClientId = 0;
    }

    [ClientRpc]
    private void StartSendingPositionClientRpc(ulong clientId)
    {
        Debug.Log($"[Client] Targeted client: {clientId}");
        Debug.Log($"[Client] Local client: {NetworkManager.Singleton.LocalClientId}");
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            Debug.Log("[Client] Starting to send positions periodically.");
            sendPositionsPeriodically = true;
        }
    }

    [ClientRpc]
    private void StopSendingPositionsPeriodicallyClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            Debug.Log("[Client] Stopping to send positions periodically.");
            sendPositionsPeriodically = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendPositionsServerRpc(Vector3[] positions)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogError("[Server] Error: This method should only run on the server.");
            return;
        }

        Debug.Log("[Server] Server received positions from the client.");
        SendPositionsClientRpc(requestingClientId, positions);
    }

    [ClientRpc]
    private void SendPositionsClientRpc(ulong clientId, Vector3[] positions)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("[Client] Client received positions from the server.");
            tilesManager.SetAllPositions(positions);
        }
    }

    [ClientRpc]
    private void RestoreOriginalPuzzleClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            Debug.Log("[Client] Restoring original puzzle.");
            tilesManager.SetAllPositions(originalPositions);
            tilesManager.EnableAllColliders();
            originalPositions = null;
        }
    }

    private void Update()
    {
        if (sendPositionsPeriodically)
        {
            Debug.Log("[Client] Sending positions to the server.");
            SendPositionsServerRpc(tilesManager.GetAllPositions());
        }
    }
}