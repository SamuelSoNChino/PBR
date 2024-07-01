using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// The <c>PuzzleTile</c> class represents an individual tile in a puzzle game. This class manages the tile's 
/// selection status, movement, snapping to the grid, and interactions with the mouse.
/// </summary>
public class PuzzleTileMultiplayer : MonoBehaviour
{

    /// |---------------------------------|
    /// |            UNIVERSAL            |
    /// |---------------------------------|

    /// <summary>
    /// 
    /// </summary>
    private int tileId;

    /// <summary>
    /// Ge
    /// </summary>
    public int GetId()
    {
        return tileId;
    }

    /// <summary>
    /// S
    /// </summary>
    public void SetId(int newTileId)
    {
        tileId = newTileId;
    }



    /// |---------------------------------|
    /// |              SERVER             |
    /// |---------------------------------|

    private Dictionary<ulong, Vector3> clientPositions = new();

    public void InitializeClientPosition(ulong clientId)
    {
        clientPositions.Add(clientId, transform.position);
    }

    public void ModifyClientPosition(ulong clientId, Vector3 newPosition)
    {
        clientPositions[clientId] = newPosition;
    }

    public Vector3 GetClientPosition(ulong clientId)
    {
        return clientPositions[clientId];
    }

    private Dictionary<ulong, GameObject> clientSnappedGridTiles = new();

    public void InitializeClientSnappedGridTile(ulong clientId)
    {
        clientSnappedGridTiles.Add(clientId, null);
    }

    public void ModifyClientSnappeedGridTile(ulong clientId, GameObject snappedGridTile)
    {
        clientSnappedGridTiles[clientId] = snappedGridTile;
    }

    public GameObject GetClientSnappedGridTile(ulong clientId)
    {
        return clientSnappedGridTiles[clientId];
    }

    /// |---------------------------------|
    /// |             CLIENT              |
    /// |---------------------------------|

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
    /// The grid tile the puzzle tile is snapped to (placed on).
    /// </summary>
    private GridTileMultiplayer snappedGridTile;

    /// <summary>
    /// The parent class containing methods for managing all puzzle tiles.
    /// </summary>
    private TilesManagerMultiplayer tilesManager;

    private PuzzleManager puzzleManager;

    /// <summary>
    /// Loads the specific tile manager.
    /// </summary>
    private void Start()
    {
        tilesManager = transform.parent.GetComponent<TilesManagerMultiplayer>();
        puzzleManager = GameObject.Find("PuzzleManager").GetComponent<PuzzleManager>();
    }

    /// <summary>
    /// Sets a new position of the puzzle tile.
    /// </summary>
    /// <param name="newPosition">The new position.</param>
    public void SetPosition(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    /// <summary>
    /// Gets the current position of the puzzle tile.
    /// </summary>
    /// <returns>The current position of the puzzle tile.</returns>
    public Vector3 GetPosition()
    {
        return transform.position;
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
    /// Moves the puzzle tile to a new position with an optional offset without changing the Z value.
    /// </summary>
    /// <param name="newPosition">The new position.</param>
    /// <param name="offset">The offset to apply.</param>
    public void Move(Vector3 newPosition, Vector3 offset = default)
    {
        Vector3 movedPosition = new(newPosition.x + offset.x, newPosition.y + offset.y, transform.position.z);
        SetPosition(movedPosition);
        puzzleManager.UpdateServerPosition(GetId(), movedPosition);

        // Unsnaps itself from the grid tile
        if (snappedGridTile)
        {
            UnsnapFromGrid();
        }
    }

    /// <summary>
    /// Increments the Z position of the tile.
    /// </summary>
    public void IncrementZ()
    {
        SetPosition(new Vector3(transform.position.x, transform.position.y, transform.position.z + 1));
    }

    /// <summary>
    /// Moves this puzzle tile to the front.
    /// </summary>
    public void MoveToFront()
    {
        // Iterates through all other puzzle tiles 
        for (int i = 0; i < transform.parent.childCount; i++)
        {
            Transform puzzleTileTransform = transform.parent.GetChild(i);
            // Moves each tile that's in front of this one back (by incrementing z)
            if (puzzleTileTransform.position.z < transform.position.z)
            {
                PuzzleTileMultiplayer puzzleTile = puzzleTileTransform.GetComponent<PuzzleTileMultiplayer>();
                puzzleTile.IncrementZ();
                puzzleManager.UpdateServerPosition(puzzleTile.GetId(), puzzleTile.GetPosition());
            }
        }
        // Moves this puzzle tile in the front spot (z=1)
        SetPosition(new Vector3(transform.position.x, transform.position.y, 1));
        puzzleManager.UpdateServerPosition(GetId(), GetPosition());
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
            Transform gridChildObject = grid.GetChild(j);
            SpriteRenderer gridSRenderer = gridChildObject.GetComponent<SpriteRenderer>();
            GridTileMultiplayer gridTile = gridChildObject.GetComponent<GridTileMultiplayer>();

            // If the grid tile sprite contains tileCenter and it is unoccupied
            if (gridSRenderer.bounds.Contains(tileCenter) && !gridTile.IsOccupied())
            {
                // Moves the puzzle tile to the same position as the grid tile (preserving the puzzle tile's Z value), which contains tileCenter and is unoccupied
                Move(new Vector3(gridChildObject.position.x, gridChildObject.position.y, transform.position.z));
                SetSnappedGridTile(gridTile);
                gridTile.SetOccupied(true);
                // Doesnt need to continue as it can always snap only to a single grid tile
                break;
            }
        }
    }

    /// <summary>
    /// Sets the grid tile the puzzle tile is snapped to.
    /// </summary>
    /// <param name="newGridTile">The new grid tile.</param>
    public void SetSnappedGridTile(GridTileMultiplayer newGridTile)
    {
        snappedGridTile = newGridTile;
    }

    /// <summary>
    /// Clears the snapped grid tile, setting its status to unoccupied.
    /// </summary>
    public void UnsnapFromGrid()
    {
        if (snappedGridTile != null)
        {
            snappedGridTile.SetOccupied(false);
        }
        snappedGridTile = null;
    }

    /// <summary>
    /// Disables the tile's collider to prevent manipulating it.
    /// </summary>
    public void DisableCollider()
    {
        GetComponent<BoxCollider2D>().enabled = false;
    }

    /// <summary>
    /// Enaables the tile's collider to allow manipulating it.
    /// </summary>
    public void EnableCollider()
    {
        GetComponent<BoxCollider2D>().enabled = true;
    }

    /// <summary>
    /// Handles the mouse down event to start dragging the tile.
    /// </summary>
    private void OnMouseDown()
    {
        if (Input.touchCount == 1 || Input.GetMouseButtonDown(0)) // 2nd condition only for PC testing
        {
            // Moves the tile to the front to show it was clicked and saves its original position
            MoveToFront();
            originalPosition = transform.position;

            // Lets other components know that a tile is being dragged (or just clicked)
            Camera.main.GetComponent<PanZoom>().SetAnyTileDragging(true);

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
        // When moving a not selected tile deselect all the tiles
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
        Camera.main.GetComponent<PanZoom>().SetAnyTileDragging(false);

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
                tilesManager.SnapAllToGrid();
            }
            // If wasn't selected snaps itself to grid
            else
            {
                SnapToGrid();
            }
        }
    }
}