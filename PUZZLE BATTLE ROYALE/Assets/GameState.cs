using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public bool gameActive;
    void Start()
    {
        gameActive = true;
    }

    public void EndGame()
    {
        gameActive = false;
    }
}
