using UnityEngine;

/// <summary>
/// Represents an individual grid tile. This class manages determining whether 
/// the puzzle tiles are placed correctly and checking whether the puzzle is completed.
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
    /// Flag indicating if this grid tile is occupied by a puzzle piece.
    /// </summary>
    private bool isOccupied;

    /// <summary>
    /// Gets or sets the occupied state of this grid tile.
    /// </summary>
    public bool IsOccupied
    {
        get { return isOccupied; }
        set { isOccupied = value; }
    }
}