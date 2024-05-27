using UnityEngine;

/// <summary>
/// The <c>PuzzleTile</c> class represents an individual tile in a puzzle game. This class manages the tile's 
/// selection status, movement, snapping to the grid, and interactions with the mouse.
/// </summary>
public class PuzzleTile : MonoBehaviour
{
    /// <summary>
    /// The vector between the puzzle tile and mouse position.
    /// </summary>
    private Vector3 mouseOffset;

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
    /// Selects the tile and visually indicates its selection.
    /// </summary>
    public void SelectTile()
    {
        isSelected = true;
        transform.localScale = new Vector3(0.95f, 0.95f, 1);
    }

    /// <summary>
    /// Deselects the tile and visually indicates its deselection.
    /// </summary>
    public void DeselectTile()
    {
        isSelected = false;
        transform.localScale = new Vector3(1, 1, 1);
    }

    /// <summary>
    /// Gets the offset between the tile and the mouse position.
    /// </summary>
    /// <returns>The offset vector.</returns>
    public Vector3 GetMouseOffset()
    {
        return mouseOffset;
    }

    /// <summary>
    /// Calculates the offset between the tile and the mouse position.
    /// </summary>
    public void CalculateMouseOffset()
    {
        mouseOffset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
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
    /// Moves the puzzle tile to a new position with an optional offset.
    /// </summary>
    /// <param name="newPosition">The new position.</param>
    /// <param name="offset">The offset to apply.</param>
    public void Move(Vector3 newPosition, Vector3 offset = default)
    {
        transform.position = new Vector3(newPosition.x + offset.x, newPosition.y + offset.y, transform.position.z);

        // Unsnaps itself from the grid tile
        if (snappedGridTile)
        {
            ClearSnappedGridTile();
        }
    }

    /// <summary>
    /// Snaps the puzzle tile to the nearest available grid tile.
    /// </summary>
    public void SnapToGrid()
    {
        SpriteRenderer tileSRenderer = transform.GetComponent<SpriteRenderer>();

        Transform grid = GameObject.Find("Grid").transform;
        // Grid has constant Z values set during generation
        float gridZ = grid.GetChild(0).position.z;
        // Uses Z value of the grind so Contains method can be used later, as puzzle tiles have different z values
        Vector3 tileCenter = new(tileSRenderer.bounds.center.x, tileSRenderer.bounds.center.y, gridZ);

        // Iterates through each grid tile
        for (int j = 0; j < grid.childCount; j++)
        {
            Transform gridChild = grid.GetChild(j);
            SpriteRenderer gridSRenderer = gridChild.GetComponent<SpriteRenderer>();
            GridTile gridTile = gridChild.GetComponent<GridTile>();

            // Moves the puzzle tile to the same position as the grid tile (preserving the puzzle tile's Z value), which contains tileCenter and is unoccupied
            if (gridSRenderer.bounds.Contains(tileCenter) && gridTile.GetStatus() == 0)
            {
                Move(new Vector3(gridChild.position.x, gridChild.position.y, transform.position.z));
                SetSnappedGridTile(gridTile);
                gridTile.UpdateStatus(indexX, indexY);
                // Doesnt need to continue as it can always snap only to a single grid tile
                break;
            }
        }

        if (GameObject.Find("Grid").GetComponent<GridManager>().CheckCompleteness()) // Checks for game completeness
        {
            GameObject.Find("Puzzle").GetComponent<GameManager>().EndGame();
        }
    }

    /// <summary>
    /// Increments the Z position of the tile.
    /// </summary>
    public void IncrementZ()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + 1);
    }

    /// <summary>
    /// Moves this puzzle tile to the front.
    /// </summary>
    public void PutOnTop()
    {
        for (int i = 0; i < transform.parent.childCount; i++)
        {
            Transform child = transform.parent.GetChild(i);
            // Moves each tile that's in front of this one back (by incrementing z)
            if (child.position.z < transform.position.z)
            {
                child.GetComponent<PuzzleTile>().IncrementZ();
            }
        }
        // Moves this puzzle tile in the front spot (z=1)
        transform.position = new Vector3(transform.position.x, transform.position.y, 1);
    }

    /// <summary>
    /// Handles the mouse down event to start dragging the tile.
    /// </summary>
    private void OnMouseDown()
    {
        if (Input.touchCount == 1 || Input.GetMouseButtonDown(0)) // 2nd condition only for PC testing
        {
            PutOnTop();
            originalPosition = transform.position;

            // Lets other components know that a tile is being dragged (or just clicked)
            tilesManager.SetAnyTileDragging(true);

            // Calculates offsets for all selected tiles, otherwise only for itself
            if (isSelected)
            {
                tilesManager.CalculateAllMouseOffsets();
            }
            else
            {
                CalculateMouseOffset();
            }
        }
    }

    /// <summary>
    /// Handles the mouse drag event to drag the tile.
    /// </summary>
    private void OnMouseDrag()
    {
        if (Input.touchCount == 1 || Input.GetMouseButton(0)) // 2nd condition only for PC testing
        {
            if (isSelected) // When dragging a selected tile, move all selected
            {
                tilesManager.MoveSelectedToMouse();
            }
            else // Else move only this one
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Move(mousePosition, mouseOffset);
            }
        }
        // When moving a not selected tile deselects all the tiles
        if (!isSelected && originalPosition != transform.position)
        {
            tilesManager.DeselectAllTiles();
        }
    }

    /// <summary>
    /// Handles the mouse up event to stop dragging and snap the tile to the grid.
    /// </summary>
    private void OnMouseUp()
    {
        // Lets other components know that the tile is not being dragged anymore
        tilesManager.SetAnyTileDragging(false);

        // Triggers only when the tile was clicked without changing position
        if (transform.position == originalPosition)
        {
            // Switches tile's selected status
            if (isSelected)
            {
                DeselectTile();
            }
            else
            {
                SelectTile();
            }
        }
        // When the tile's position was changed
        else
        {
            // Snaps all selected
            if (isSelected)
            {
                tilesManager.SnapSelectedToGrid();
            }
            // If wasn't selected snaps itself to grid
            else
            {
                SnapToGrid();
            }
        }
    }
}