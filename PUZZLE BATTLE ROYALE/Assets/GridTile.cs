using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile : MonoBehaviour
{
    public int status;  // 0 - unoccupied, 1 - occupied, not correct, 2 - occupied, correct
    public int indexX;
    public int indexY;
    void Start()
    {
        status = 0;
    }
    public void UpdateStatus(int tileIndexX, int tileIndexY)
    {
        if (indexX == tileIndexX && indexY == tileIndexY)
        {
            status = 2;
        } else
        {
            status = 1;
        }
    }
}
