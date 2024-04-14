using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile : MonoBehaviour
{
    public int status;  // 0 - unoccupied, 1 - occupied, not correct, 2 - occupied, correct
    public int x;
    public int y;
    void Start()
    {
        status = 0;
    }
    public void UpdateStatus(int x2, int y2)
    {
        if (x == x2 && y == y2)
        {
            status = 2;
        } else
        {
            status = 1;
        }
    }
}
