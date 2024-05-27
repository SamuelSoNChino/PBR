using UnityEngine;

/// <summary>
/// Manages all the puzzle tiles in the scene, handling their mass selection, movement, and snapping to the grid.
/// </summary>
public class TilesManager : MonoBehaviour
{
    /// <summary>
    /// Indicates whether any tile is currently being dragged (moved).
    /// </summary>
    private bool anyTileDragging = false;

    /// <summary>
    /// Moves all selected tiles to the current mouse position.
    /// </summary>
    public void MoveSelectedToMouse()
    {
        // Iterates through each puzzle tile
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            PuzzleTile puzzleTile = child.GetComponent<PuzzleTile>();
            // Moves each selected tile to the mouse position
            if (puzzleTile.IsSelected())
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3 childOffset = puzzleTile.GetMouseOffset();
                puzzleTile.Move(mousePosition, childOffset);
            }
        }
    }

    /// <summary>
    /// Deselects all puzzle tiles.
    /// </summary>
    public void DeselectAllTiles()
    {
        // Iterates through each puzzle tile
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.GetComponent<PuzzleTile>().DeselectTile();
        }
    }

    /// <summary>
    /// Calculates the mouse offsets for all puzzle tiles.
    /// </summary>
    public void CalculateAllMouseOffsets()
    {
        // Iterates through each puzzle tile
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.GetComponent<PuzzleTile>().CalculateMouseOffset();
        }
    }

    /// <summary>
    /// Snaps all selected tiles to the grid.
    /// </summary>
    public void SnapSelectedToGrid()
    {
        // Iterates through each puzzle tile
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform tileChild = transform.GetChild(i);
            PuzzleTile puzzleTile = tileChild.GetComponent<PuzzleTile>();
            // Snaps each selected tile to the grid
            if (puzzleTile.IsSelected())
            {
                puzzleTile.SnapToGrid();
            }
        }
    }

    /// <summary>
    /// Checks if any tile is currently being dragged.
    /// </summary>
    /// <returns>True if any tile is being dragged, false otherwise.</returns>
    public bool IsAnyTileDragging()
    {
        return anyTileDragging;
    }

    /// <summary>
    /// Sets the dragging state of any tile.
    /// </summary>
    /// <param name="newState">The new dragging state.</param>
    public void SetAnyTileDragging(bool newState)
    {
        anyTileDragging = newState;
    }
}