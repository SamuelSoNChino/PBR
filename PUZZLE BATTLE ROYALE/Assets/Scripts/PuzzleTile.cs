using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Tilemaps;

public class PuzzleTile : MonoBehaviour
{
    /// <summary>
    /// The vector between the puzzle tile and mouse position.
    /// </summary>
    private Vector3 offset;

    /// <summary>
    /// Stores the original position when the tile is being dragged.
    /// </summary>
    private Vector3 originalPosition;

    /// <summary>
    /// Holds the value of whether the tile is selected for multi-dragging.
    /// </summary>
    private bool isSelected = false;

    /// <summary>
    /// X index matching with the correct grid tile.
    /// </summary>
    private int indexX;

    /// <summary>
    /// Y index matching with the correct grid tile.
    /// </summary>
    private int indexY;

    /// <summary>
    /// The grid tile the puzzle tile is snapped to (placed on).
    /// </summary>
    private GridTile snappedGridTile;

    /// <summary>
    /// The parent class containing methods for managing all puzzle tiles.
    /// </summary>
    private TilesManager tilesManager;

    /// <summary>
    /// Loads the specific tile manager.
    /// </summary>
    private void Start()
    {
        tilesManager = transform.parent.GetComponent<TilesManager>();
    }

    /// <summary>
    /// Checks if the tile is selected.
    /// </summary>
    /// <returns>True if the tile is selected, false otherwise.</returns>
    public bool IsSelected()
    {
        return isSelected;
    }

    /// <summary>
    /// Deselects the tile.
    /// </summary>
    public void DeselectTile()
    {
        isSelected = false;
    }

    /// <summary>
    /// Gets the offset between the tile and the mouse position.
    /// </summary>
    /// <returns>The offset vector.</returns>
    public Vector3 GetOffset()
    {
        return offset;
    }

    /// <summary>
    /// Calculates the offset between the tile and the mouse position.
    /// </summary>
    public void CalculateOffset()
    {
        offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    /// <summary>
    /// Gets the grid tile the puzzle tile is snapped to.
    /// </summary>
    /// <returns>The snapped grid tile.</returns>
    public GridTile GetSnappedGridTile()
    {
        return snappedGridTile;
    }

    /// <summary>
    /// Sets the grid tile the puzzle tile is snapped to.
    /// </summary>
    /// <param name="newGridTile">The new grid tile.</param>
    public void SetSnappedGridTile(GridTile newGridTile)
    {
        snappedGridTile = newGridTile;
    }

    /// <summary>
    /// Clears the snapped grid tile, setting its status to unoccupied.
    /// </summary>
    public void ClearSnappedGridTile()
    {
        snappedGridTile.SetStatus(0);
        snappedGridTile = null;
    }

    /// <summary>
    /// Gets the indexes of the tile.
    /// </summary>
    /// <returns>An array containing the X and Y indexes.</returns>
    public int[] GetIndexes()
    {
        int[] output = { indexX, indexY };
        return output;
    }

    /// <summary>
    /// Sets the indexes of the tile.
    /// </summary>
    /// <param name="x">The X index.</param>
    /// <param name="y">The Y index.</param>
    public void SetIndexes(int x, int y)
    {
        indexX = x;
        indexY = y;
    }

    /// <summary>
    /// Handles the mouse down event to start dragging the tile.
    /// </summary>
    private void OnMouseDown()
    {
        if (Input.touchCount == 1 || Input.GetMouseButtonDown(0)) // 2nd condition for PC testing
        {
            tilesManager.PutTileOnTop(transform.position.z);
            originalPosition = transform.position;
            tilesManager.SetTileDragging(true);

            // Calculates offsets for all selected tiles, otherwise only for itself
            if (isSelected)
            {
                tilesManager.CalculateAllOffsets();
            }
            else
            {
                CalculateOffset();
            }
        }
    }

    /// <summary>
    /// Handles the mouse drag event to drag the tile.
    /// </summary>
    private void OnMouseDrag()
    {
        // The second condition is only for PC debugging, on touch device should only be == 1 (Check whether it is even needed)
        if ((Input.touchCount == 1 || Input.touchCount == 0)) 
        {
            if (isSelected) // When dragging selected, move all selected
            {
                tilesManager.MoveSelected();
            }
            else // Else move only this one
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                transform.position = new Vector3(mousePosition.x + offset.x, mousePosition.y + offset.y, transform.position.z);

                if (snappedGridTile)
                {
                    snappedGridTile.SetStatus(0);
                    snappedGridTile = null;
                }
            }
        }
    }

    /// <summary>
    /// Handles the mouse up event to stop dragging and snap the tile to the grid.
    /// </summary>
    private void OnMouseUp()
    {
        tilesManager.SetTileDragging(false);

        if (transform.position == originalPosition) // Triggers only when the tile was clicked without dragging it
        {
            // Experiment for selection visualization
            if (isSelected) 
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                transform.localScale = new Vector3(0.95f, 0.95f, 1);
            }

            isSelected = !isSelected; // Switches selected status
        }
        else
        {
            if (isSelected) // Snaps all selected without changing anything else
            {
                tilesManager.SnapSelectedToGrid();
            }
            else // If wasn't selected, deselects all selected and snaps itself to grid
            {
                tilesManager.DeselectAllTiles();
                tilesManager.SnapTileToGrid(transform);
            }
        }
    }
}