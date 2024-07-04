using UnityEngine;

/// <summary>
/// The <c>PuzzleTileMultiplayer</c> class represents an individual tile in a puzzle game. This class manages the tile's 
/// selection status, movement, snapping to the grid, and interactions with the mouse. It is only used on client side.
/// </summary>
public class PuzzleTileMultiplayer : MonoBehaviour
{
    /// <summary>
    /// The parent class containing methods for managing all puzzle tiles.
    /// </summary>
    private TilesManagerMultiplayer tilesManager;

    /// <summary>
    /// The class containing methods for managing all grid tiles.
    /// </summary>
    private GridManagerMultiplayer gridManager;

    /// <summary>
    /// The puzzle manager responsible for managing the overall puzzle and communication with server.
    /// </summary>
    private PuzzleManager puzzleManager;

    /// <summary>
    /// Loads the specific Tilemanager, puzzleManager and GridManager.
    /// </summary>
    private void Start()
    {
        tilesManager = GameObject.Find("Tiles").GetComponent<TilesManagerMultiplayer>();
        puzzleManager = GameObject.Find("Puzzle").GetComponent<PuzzleManager>();
        gridManager = GameObject.Find("Grid").GetComponent<GridManagerMultiplayer>();
    }

    // -----------------------------------------------------------------------
    // Ids
    // -----------------------------------------------------------------------

    /// <summary>
    /// The unique identifier for this puzzle tile.
    /// </summary>
    private int tileId;

    /// <summary>
    /// Gets or sets the unique identifier for this puzzle tile.
    /// </summary>
    public int TileId
    {
        get { return tileId; }
        set { tileId = value; }
    }

    // -----------------------------------------------------------------------
    // Positions
    // -----------------------------------------------------------------------

    /// <summary>
    /// The vector between the puzzle tile and mouse position.
    /// </summary>
    private Vector3 mouseOffset;

    /// <summary>
    /// The vector between the puzzle tile and mouse position.
    /// </summary>
    public Vector3 MouseOffset
    {
        get { return mouseOffset; }
        set { mouseOffset = value; }
    }

    /// <summary>
    /// Calculates the offset between the tile and the mouse position.
    /// </summary>
    public void CalculateMouseOffset()
    {
        mouseOffset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    /// <summary>
    /// Stores the original position after clicking the tile to determine if the tile should get selected.
    /// </summary>
    private Vector3 originalPosition;

    /// <summary>
    /// Gets the current position of the puzzle tile.
    /// </summary>
    public Vector3 Position
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    // -----------------------------------------------------------------------
    // Selection
    // -----------------------------------------------------------------------

    /// <summary>
    /// Holds the value of whether the tile is selected for multi-dragging.
    /// </summary>
    private bool isSelected = false;

    /// <summary>
    /// Gets or sets the selection status of the tile.
    /// </summary>
    public bool IsSelected
    {
        get { return IsSelected; }
        set
        {
            isSelected = value;
            // Selection is temporarily visualized by changing the scale
            if (value)
            {
                transform.localScale = new Vector3(0.95f, 0.95f, 1);
            }
            else
            {
                new Vector3(1, 1, 1);
            }
        }
    }

    // -----------------------------------------------------------------------
    // Grid and Snapping
    // -----------------------------------------------------------------------

    /// <summary>
    /// The grid tile the puzzle tile is snapped to (placed on).
    /// </summary>
    private GridTileMultiplayer snappedGridTile;

    /// <summary>
    /// Gets or sets the grid tile the puzzle tile is snapped to.
    /// </summary>
    public GridTileMultiplayer SnappedGridTile
    {
        get { return snappedGridTile; }
        set
        {
            snappedGridTile = value;
            if (snappedGridTile != null)
            {
                snappedGridTile.IsOccupied = true;
            }
        }
    }

    // -----------------------------------------------------------------------
    // Movement
    // -----------------------------------------------------------------------

    /// <summary>
    /// Moves the puzzle tile to a new position with an optional offset, without changing the Z value.
    /// </summary>
    /// <param name="newPosition">The new position.</param>
    /// <param name="offset">The offset to apply.</param>
    public void Move(Vector3 newPosition, Vector3 offset = default)
    {
        Vector3 movedPosition = new(newPosition.x + offset.x, newPosition.y + offset.y, transform.position.z);
        Position = movedPosition;
        puzzleManager.UpdateServerPosition(TileId, movedPosition);

        // Unsnaps itself from the grid tile
        if (snappedGridTile != null)
        {
            UnsnapFromGrid();
        }
    }

    /// <summary>
    /// Increments the Z position of the tile.
    /// </summary>
    public void IncrementZ()
    {
        Position = new Vector3(transform.position.x, transform.position.y, transform.position.z + 1);
    }

    /// <summary>
    /// Moves this puzzle tile to the front.
    /// </summary>
    public void MoveToFront()
    {
        foreach (GameObject puzzleTileObject in tilesManager.GetAllPuzzleTiles())
        {
            PuzzleTileMultiplayer puzzleTile = puzzleTileObject.GetComponent<PuzzleTileMultiplayer>();
            if (puzzleTile.Position.z < Position.z)
            {
                puzzleTile.IncrementZ();
                puzzleManager.UpdateServerPosition(puzzleTile.TileId, puzzleTile.Position);
            }
        }
        // Moves this puzzle tile to the front spot (z=1)
        Position = new(transform.position.x, transform.position.y, 1);
        puzzleManager.UpdateServerPosition(TileId, Position);
    }

    // -----------------------------------------------------------------------
    // Snapping
    // -----------------------------------------------------------------------

    /// <summary>
    /// Snaps the puzzle tile to the grid tile containing the puzzle tile's center, if there is any.
    /// </summary>
    public void SnapToGrid()
    {
        SpriteRenderer tileSRenderer = GetComponent<SpriteRenderer>();
        Transform grid = GameObject.Find("Grid").transform;
        float gridZ = grid.GetChild(0).position.z;

        // Uses Z value of the grid so "Contains" method can be used later
        Vector3 tileCenter = new(tileSRenderer.bounds.center.x, tileSRenderer.bounds.center.y, gridZ);

        foreach (GameObject gridTileObejct in gridManager.GetAllGridTiles())
        {
            SpriteRenderer gridSRenderer = gridTileObejct.GetComponent<SpriteRenderer>();
            GridTileMultiplayer gridTile = gridTileObejct.GetComponent<GridTileMultiplayer>();

            if (gridSRenderer.bounds.Contains(tileCenter) && !gridTile.IsOccupied)
            {
                Move(new Vector3(gridTileObejct.transform.position.x, gridTileObejct.transform.position.y, Position.z));
                SnappedGridTile = gridTile;
                break;
            }
        }
    }

    /// <summary>
    /// Clears the snapped grid tile, setting its status to unoccupied.
    /// </summary>
    public void UnsnapFromGrid()
    {
        if (snappedGridTile != null)
        {
            snappedGridTile.IsOccupied = false;
        }
        snappedGridTile = null;
    }

    // -----------------------------------------------------------------------
    // Colliders
    // -----------------------------------------------------------------------

    /// <summary>
    /// Disables the tile's collider to prevent manipulating it.
    /// </summary>
    public void DisableCollider()
    {
        GetComponent<BoxCollider2D>().enabled = false;
    }

    /// <summary>
    /// Enables the tile's collider to allow manipulating it.
    /// </summary>
    public void EnableCollider()
    {
        GetComponent<BoxCollider2D>().enabled = true;
    }

    // -----------------------------------------------------------------------
    // Mouse Events
    // -----------------------------------------------------------------------

    /// <summary>
    /// Handles the mouse down event to start dragging the tile.
    /// </summary>
    private void OnMouseDown()
    {
        if (Input.touchCount == 1 || Input.GetMouseButtonDown(0)) // 2nd condition only for PC testing
        {
            MoveToFront();
            originalPosition = transform.position;

            // Lets the PanZoom script know, that a tile is being dragged, so the player doesn't also pan ath the same time
            Camera.main.GetComponent<PanZoom>().SetAnyTileDragging(true);

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
            if (isSelected)
            {
                tilesManager.MoveSelectedWithMouse();
            }
            else
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
        // Lets the PanZoom script know, that a tile is not being dragged anymore
        Camera.main.GetComponent<PanZoom>().SetAnyTileDragging(false);

        // Triggers only when the tile was clicked without changing position
        if (transform.position == originalPosition)
        {
            isSelected = !isSelected;
        }
        else
        {
            if (isSelected)
            {
                tilesManager.SnapAllToGrid();
            }
            else
            {
                SnapToGrid();
            }
        }
    }
}
