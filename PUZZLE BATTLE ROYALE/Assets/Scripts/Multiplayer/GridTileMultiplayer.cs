using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents an individual grid tile. This class manages determining whether  
/// the puzzle tiles are placed correctly and checking whether the puzzle is completed.
/// </summary>
public class GridTileMultiplayer : MonoBehaviour
{
    /// <summary>
    /// Represents the status of the grid tile:
    /// 0 - unoccupied, 1 - occupied, not correct, 2 - occupied, correct.
    /// </summary>
    private int status;

    /// <summary>
    /// X index matching with the correct puzzle tile.
    /// </summary>
    private int indexX;

    /// <summary>
    /// Y index matching with the correct puzzle tile.
    /// </summary>
    private int indexY;

    /// <summary>
    /// Initializes the grid tile's status to unoccupied (0).
    /// </summary>
    void Start()
    {
        status = 0;
    }

    /// <summary>
    /// Updates the status of the grid tile when a puzzle tile was snapped to it.
    /// </summary>
    /// <param name="tileIndexX">The X index of the puzzle tile.</param>
    /// <param name="tileIndexY">The Y index of the puzzle tile.</param>
    public void UpdateStatus(int tileIndexX, int tileIndexY)
    {
        // If the puzzle tile indexes and grid tile indexes are the same (puzzle tile being in the correct position)
        if (indexX == tileIndexX && indexY == tileIndexY)
        {
            // Sets the status to occupied, correct
            status = 2;
        }
        else
        {
            // Sets the status to occupied, not correct
            status = 1;
        }
    }

    /// <summary>
    /// Sets the X and Y indexes for the grid tile.
    /// </summary>
    /// <param name="x">The X index to set.</param>
    /// <param name="y">The Y index to set.</param>
    public void SetIndexes(int x, int y)
    {
        indexX = x;
        indexY = y;
    }

    /// <summary>
    /// Gets the current status of the grid tile.
    /// </summary>
    /// <returns>The status of the grid tile.</returns>
    public int GetStatus()
    {
        return status;
    }

    /// <summary>
    /// Sets a new status for the grid tile.
    /// </summary>
    /// <param name="newStatus">The new status to set.</param>
    public void SetStatus(int newStatus)
    {
        status = newStatus;
    }
}