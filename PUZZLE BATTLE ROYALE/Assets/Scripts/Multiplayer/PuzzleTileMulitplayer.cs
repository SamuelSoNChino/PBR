using UnityEngine;

/// <summary>
/// The <c>PuzzleTileMultiplayer</c> class represents an individual tile in a puzzle game. This class manages the tile's 
/// selection status, movement, snapping to the grid, and interactions with the mouse. It is only used on client side.
/// </summary>
public class PuzzleTileMultiplayer : MonoBehaviour
{
    /// <summary>
    /// The puzzle manager responsible for managing the overall puzzle and communication with server.
    /// </summary>
    private PuzzleManager puzzleManager;

    /// <summary>
    /// Loads the specific Tilemanager, puzzleManager and GridManager.
    /// </summary>
    private void Start()
    {
        puzzleManager = GameObject.Find("Puzzle").GetComponent<PuzzleManager>();
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
    /// Gets the current position of the puzzle tile.
    /// </summary>
    public Vector3 Position
    {
        get { return transform.position; }
        set { transform.position = value; }
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
            puzzleManager.StartHoldingTile(tileId);

            // Lets the PanZoom script know, that a tile is being dragged, so the player doesn't also pan ath the same time
            Camera.main.GetComponent<PanZoom>().SetHoldingTile(true);

            CalculateMouseOffset();
        }
    }

    /// <summary>
    /// Handles the mouse drag event to drag the tile.
    /// </summary>
    private void OnMouseDrag()
    {
        if (Input.touchCount == 1 || Input.GetMouseButton(0)) // 2nd condition only for PC testing
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Move(mousePosition, mouseOffset);
        }
    }

    /// <summary>
    /// Handles the mouse up event to stop dragging and snap the tile to the grid.
    /// </summary>
    private void OnMouseUp()
    {
        puzzleManager.StopHoldingTile(tileId);

        // Lets the PanZoom script know, that a tile is not being dragged anymore
        Camera.main.GetComponent<PanZoom>().SetHoldingTile(false);
    }
}
