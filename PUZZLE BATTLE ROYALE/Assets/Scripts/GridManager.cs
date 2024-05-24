using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager: MonoBehaviour
{
    public bool CheckCompleteness()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).GetComponent<GridTile>().GetStatus() != 2) // Triggers if the correct tile isn't placed on the grid tile
            {
                return false;
            }
        }
        return true;
    }
}
