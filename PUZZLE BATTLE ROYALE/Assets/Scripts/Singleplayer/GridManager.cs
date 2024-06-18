using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles all of the grid tile in the scene.
/// </summary>
public class GridManager : MonoBehaviour
{
    /// <summary>
    /// SingleplayerManager script that maanges the gameflow.
    /// </summary>
    [SerializeField] private SingleplayerManager singleplayerManager;

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
            singleplayerManager.EndGame();
        }
    }
}