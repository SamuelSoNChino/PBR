using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents an individual grid tile. This class manages determining whether 
/// the puzzle tiles are placed correctly and checking whether the puzzle is completed.
/// </summary>
public class GridTileMultiplayer : MonoBehaviour
{
    /// <summary>
    /// Unique ID of the grid tile.
    /// </summary>
    private int tileId;

    /// <summary>
    /// Sets the ID of the grid tile.
    /// </summary>
    /// <param name="newTileId">The new ID to set.</param>
    public void SetId(int newTileId)
    {
        tileId = newTileId;
    }

    /// <summary>
    /// Gets the ID of the grid tile.
    /// </summary>
    /// <returns>The ID of the grid tile.</returns>
    public int GetId()
    {
        return tileId;
    }

    /// <summary>
    /// Gets the position of the grid tile in the game world.
    /// </summary>
    /// <returns>The position of the grid tile.</returns>
    public Vector3 GetPosition()
    {
        return transform.position;
    }

    /// |---------------------------------|
    /// |              SERVER             |
    /// |---------------------------------|

    /// <summary>
    /// Dictionary to store client statuses for this grid tile.
    /// </summary>
    private Dictionary<ulong, bool> clientStatuses = new();

    /// <summary>
    /// Initializes the client status for a given client ID.
    /// </summary>
    /// <param name="clientId">The ID of the client to initialize.</param>
    public void InitializeClientStatus(ulong clientId)
    {
        clientStatuses.Add(clientId, false); // Default status is false (not correct)
    }

    /// <summary>
    /// Modifies the client status for a given client ID.
    /// </summary>
    /// <param name="clientId">The ID of the client to modify.</param>
    /// <param name="newStatus">The new status to set.</param>
    public void ModifyClientStatus(ulong clientId, bool newStatus)
    {
        clientStatuses[clientId] = newStatus;
    }

    /// <summary>
    /// Gets the client status for a given client ID.
    /// </summary>
    /// <param name="clientId">The ID of the client to get status for.</param>
    /// <returns>The status of the client for this grid tile.</returns>
    public bool GetClientStatus(ulong clientId)
    {
        return clientStatuses[clientId];
    }

    /// |---------------------------------|
    /// |             CLIENT              |
    /// |---------------------------------|

    /// <summary>
    /// Flag indicating if this grid tile is occupied by a puzzle piece.
    /// </summary>
    private bool isOccupied;

    /// <summary>
    /// Checks if this grid tile is occupied by a puzzle piece.
    /// </summary>
    /// <returns>True if the tile is occupied, false otherwise.</returns>
    public bool IsOccupied()
    {
        return isOccupied;
    }

    /// <summary>
    /// Sets the occupied state of this grid tile.
    /// </summary>
    /// <param name="newState">The new state to set (true for occupied, false for not).</param>
    public void SetOccupied(bool newState)
    {
        isOccupied = newState;
    }
}