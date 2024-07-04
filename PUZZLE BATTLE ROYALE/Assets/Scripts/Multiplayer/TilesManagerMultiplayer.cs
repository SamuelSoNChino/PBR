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

    // -----------------------------------------------------------------------
    // Tile Selection and Movement
    // -----------------------------------------------------------------------

    /// <summary>
    /// Moves all selected puzzle tiles to the current mouse position.
    /// </summary>
    public void MoveSelectedWithMouse()
    {
        foreach (GameObject puzzleTileObject in GetAllPuzzleTiles())
        {
            PuzzleTileMultiplayer puzzleTile = puzzleTileObject.GetComponent<PuzzleTileMultiplayer>();

            if (puzzleTile.IsSelected)
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3 mouseOffset = puzzleTile.MouseOffset;
                puzzleTile.Move(mousePosition, mouseOffset);
            }
        }
    }

    /// <summary>
    /// Deselects all puzzle tiles.
    /// </summary>
    public void DeselectAllTiles()
    {
        foreach (GameObject puzzleTileObject in GetAllPuzzleTiles())
        {
            PuzzleTileMultiplayer puzzleTile = puzzleTileObject.GetComponent<PuzzleTileMultiplayer>();
            puzzleTile.IsSelected = false;
        }
    }

    /// <summary>
    /// Calculates the mouse offsets for all puzzle tiles.
    /// </summary>
    public void CalculateAllMouseOffsets()
    {
        foreach (GameObject puzzleTileObject in GetAllPuzzleTiles())
        {
            PuzzleTileMultiplayer puzzleTile = puzzleTileObject.GetComponent<PuzzleTileMultiplayer>();
            puzzleTile.CalculateMouseOffset();
        }
    }

    // -----------------------------------------------------------------------
    // Grid Snapping
    // -----------------------------------------------------------------------

    /// <summary>
    /// Snaps all puzzle tiles to the grid.
    /// </summary>
    public void SnapAllToGrid()
    {
        foreach (GameObject puzzleTileObject in GetAllPuzzleTiles())
        {
            PuzzleTileMultiplayer puzzleTile = puzzleTileObject.GetComponent<PuzzleTileMultiplayer>();
            puzzleTile.SnapToGrid();
        }
    }

    /// <summary>
    /// Unsnaps all puzzle tiles from the grid.
    /// </summary>
    public void UnsnapAllFromGrid()
    {
        foreach (GameObject puzzleTileObject in GetAllPuzzleTiles())
        {
            PuzzleTileMultiplayer puzzleTile = puzzleTileObject.GetComponent<PuzzleTileMultiplayer>();
            puzzleTile.UnsnapFromGrid();
        }
    }
}