using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile : MonoBehaviour
{
    private int status;  // 0 - unoccupied, 1 - occupied, not correct, 2 - occupied, correct
    private int indexX;
    private int indexY;
    void Start()
    {
        status = 0;
    }
    public void UpdateStatus(int tileIndexX, int tileIndexY) // Updates status by comparing the puzzle tile and grid tile indexes
    {
        if (indexX == tileIndexX && indexY == tileIndexY)
        {
            status = 2;
        } else
        {
            status = 1;
        }
    }
    public void SetIndexes(int x, int y)
    {
        indexX = x;
        indexY = y;
    }
    public int GetStatus()
    {
        return status;
    }
    public void SetStatus(int newStatus)
    {
        status = newStatus;
    }
}
