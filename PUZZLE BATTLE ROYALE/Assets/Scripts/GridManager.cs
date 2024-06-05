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
        // Assumes that all the grid tiles have a correct puzzle tile placed on them
        bool allCorrect = true;

        // Iterates through all grid tiles
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            GridTile gridTile = child.GetComponent<GridTile>();

            // Checks whether the grid tile has a correctly placed puzzle tile (status = 2)
            if (gridTile.GetStatus() != 2)
            {
                // Assumption was wrong, one tile is placed incorrectly or unoccupied, no need to continue
                allCorrect = false;
                break;
            }
        }
        // If all are placed correctly, end the game
        if (allCorrect)
        {
            // Checks if the the player is currently playign singleplayer or multiplayer and calls the correct EndGame method.
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
        // Iterates through all grid tiles
        for (int i = 0; i < transform.childCount; i++)
        {
            // Sets the status for each grid tile to 0 - unoccupied
            Transform child = transform.GetChild(i);
            GridTile gridTile = child.GetComponent<GridTile>();
            gridTile.SetStatus(0);
        }
    }
}