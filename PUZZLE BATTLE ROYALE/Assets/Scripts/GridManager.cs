using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles all of the grid tile in the scene.
/// </summary>
public class GridManager : MonoBehaviour
{
    /// <summary>
    /// Checks whether all the puzzle tiles are placed correctly using each grid tile's status. Ends the game if yes.
    /// </summary>
    public void CheckCompleteness()
    {
        bool allCorrect = true;
        // Loop through all grid tiles
        for (int i = 0; i < transform.childCount; i++)
        {
            // Checks whether all grid tiles have a correctly placed puzzle tile on them
            if (transform.GetChild(i).GetComponent<GridTile>().GetStatus() != 2)
            {
                allCorrect = false;
                break;
            }
        }
        // If all are placed correctly, end the game
        if (allCorrect)
        {
            if (SceneManager.GetActiveScene().name == "PuzzleMultiplayer")
            {
                GameObject.Find("Puzzle").GetComponent<MultiplayerManager>().EndGame();
            }
            else
            {
                GameObject.Find("Puzzle").GetComponent<SingleplayerManager>().EndGame();
            }

        }
    }

    /// <summary>
    /// Resets the status to unnocupied for all grid tiles.
    /// </summary
    public void ResetCompleteness()
    {
        // Loop through all grid tiles
        for (int i = 0; i < transform.childCount; i++)
        {
            // Sets the status for each grid tile to 0 - unoccupied
            Transform child = transform.GetChild(i);
            GridTile gridTile = child.GetComponent<GridTile>();
            gridTile.SetStatus(0);
        }
    }
}