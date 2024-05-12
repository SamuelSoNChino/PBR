using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager: MonoBehaviour
{
    public bool CheckComplete()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).GetComponent<GridTile>().status != 2)
            {
                return false;
            }
        }
        return true;
    }
}
