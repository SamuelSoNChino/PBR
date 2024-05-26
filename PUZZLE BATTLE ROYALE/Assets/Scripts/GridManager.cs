using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    /// <summary>
    /// Checks whether all the puzzle tiles are placed correctly using each grid tile's status.
    /// </summary>
    /// <returns>True if all puzzle tiles are correctly placed, false otherwise.</returns>
    public bool CheckCompleteness()
    {
        // Loop through all children of this transform
        for (int i = 0; i < transform.childCount; i++)
        {
            // Check if the puzzle tile isn't placed on the correct grid tile
            if (transform.GetChild(i).GetComponent<GridTile>().GetStatus() != 2)
            {
                return false;
            }
        }
        return true;
    }
}