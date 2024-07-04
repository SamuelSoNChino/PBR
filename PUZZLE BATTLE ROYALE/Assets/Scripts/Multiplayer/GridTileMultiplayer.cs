using UnityEngine;

/// <summary>
/// Represents an individual grid tile. Is only used on client side.
/// </summary>
public class GridTileMultiplayer : MonoBehaviour
{
    // -----------------------------------------------------------------------
    // Ids
    // -----------------------------------------------------------------------

    /// <summary>
    /// Unique ID of the grid tile.
    /// </summary>
    private int tileId;

    /// <summary>
    /// Gets or sets the unique ID of the grid tile.
    /// </summary>
    public int TileId
    {
        get { return tileId; }
        set { tileId = value; }
    }

    // -----------------------------------------------------------------------
    // Occupied State
    // -----------------------------------------------------------------------

    /// <summary>
    /// Flag indicating if this grid tile is occupied by a puzzle piece to determine local snapping to grid.
    /// </summary>
    private bool isOccupied;

    /// <summary>
    /// Gets or sets the occupied state of this grid tile, which is used to determine local snapping to grid.
    /// </summary>
    public bool IsOccupied
    {
        get { return isOccupied; }
        set { isOccupied = value; }
    }
}