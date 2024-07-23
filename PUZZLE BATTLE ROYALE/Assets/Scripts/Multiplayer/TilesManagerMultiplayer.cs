using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all the puzzle tiles in the scene, handling their mass selection, movement, and snapping to the grid. 
/// This manager is only used on the client side.
/// </summary>
public class TilesManagerMultiplayer : MonoBehaviour
{
    /// <summary>
    /// Retrieves all puzzle tiles in the scene.
    /// </summary>
    /// <returns>A list of all puzzle tile GameObjects.</returns>
    public List<GameObject> GetAllPuzzleTiles()
    {
        List<GameObject> puzzleTiles = new();
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject puzzleTileObject = transform.GetChild(i).gameObject;
            puzzleTiles.Add(puzzleTileObject);
        }
        return puzzleTiles;
    }

    /// <summary>
    /// Finds a puzzle tile by its unique ID.
    /// </summary>
    /// <param name="puzzleTileId">The ID of the puzzle tile to find.</param>
    /// <returns>The GameObject of the puzzle tile with the specified ID, or null if not found.</returns>
    public GameObject FindPuzzleTileById(int puzzleTileId)
    {
        foreach (GameObject puzzleTileObject in GetAllPuzzleTiles())
        {
            PuzzleTileMultiplayer puzzleTile = puzzleTileObject.GetComponent<PuzzleTileMultiplayer>();
            if (puzzleTile.TileId == puzzleTileId)
            {
                return puzzleTileObject;
            }
        }
        return null;
    }

    // -----------------------------------------------------------------------
    // Collider Management
    // -----------------------------------------------------------------------

    /// <summary>
    /// Disables all the colliders on the puzzle tiles to prevent interaction.
    /// </summary>
    public void DisableAllColliders()
    {
        foreach (GameObject puzzleTileObject in GetAllPuzzleTiles())
        {
            PuzzleTileMultiplayer puzzleTile = puzzleTileObject.GetComponent<PuzzleTileMultiplayer>();
            puzzleTile.DisableCollider();
        }
    }

    /// <summary>
    /// Enables all the colliders on the puzzle tiles to allow interaction.
    /// </summary>
    public void EnableAllColliders()
    {
        foreach (GameObject puzzleTileObject in GetAllPuzzleTiles())
        {
            PuzzleTileMultiplayer puzzleTile = puzzleTileObject.GetComponent<PuzzleTileMultiplayer>();
            puzzleTile.EnableCollider();
        }
    }
}