using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles all of the grid tile in the scene.
/// </summary>
public class GridManager : MonoBehaviour
{
    /// <summary>
    /// Checks whether all the puzzle tiles are placed correctly using each grid tile's status.
    /// </summary>
    /// <returns>True if all puzzle tiles are correctly placed, false otherwise.</returns>
    public bool CheckCompleteness()
    {
        // Loop through all grid tiles
        for (int i = 0; i < transform.childCount; i++)
        {
            // Checks whether all grid tiles have a correctly placed puzzle tile on them
            if (transform.GetChild(i).GetComponent<GridTile>().GetStatus() != 2)
            {
                return false;
            }
        }
        return true;
    }
}