using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a player with various attributes and behaviors in the game.
/// </summary>
public class Player
{
    /// <summary>
    /// Initializes a new instance of the Player class with the specified name, client ID, and background skin ID.
    /// </summary>
    /// <param name="name">The name of the player.</param>
    /// <param name="clientId">The client ID of the player.</param>
    /// <param name="backgroundSkinId">The index of the background skin.</param>
    public Player(string name, ulong clientId, int backgroundSkinId, int profilePictureId)
    {
        this.clientId = clientId;
        this.name = name;
        this.backgroundSkinId = backgroundSkinId;
        this.profilePictureId = profilePictureId;
    }

    // -----------------------------------------------------------------------
    // Basic Info
    // -----------------------------------------------------------------------

    /// <summary>
    /// The name of the player.
    /// </summary>
    private string name;

    /// <summary>
    /// Gets or sets the name of the player.
    /// </summary>
    public string Name
    {
        get { return name; }
        set { name = value; }
    }

    /// <summary>
    /// The client ID of the player.
    /// </summary>
    private ulong clientId;

    /// <summary>
    /// Gets or sets the client ID of the player.
    /// </summary>
    public ulong ClientId
    {
        get { return clientId; }
        set { clientId = value; }
    }

    // -----------------------------------------------------------------------
    // Skins
    // -----------------------------------------------------------------------

    /// <summary>
    /// The ID of the background skin.
    /// </summary>
    private int backgroundSkinId;

    /// <summary>
    /// Gets or sets the ID of the background skin.
    /// </summary>
    public int BackgroundSkinId
    {
        get { return backgroundSkinId; }
        set { backgroundSkinId = value; }
    }

    private int profilePictureId;

    public int ProfilePictureId
    {
        get { return profilePictureId; }
        set { profilePictureId = value; }
    }

    // -----------------------------------------------------------------------
    // Progress
    // -----------------------------------------------------------------------

    /// <summary>
    /// Player's current percentual progress.
    /// </summary>
    private int progress = 0;

    /// <summary>
    /// Gets or sets player's current percentual progress.
    /// </summary>
    public int Progress
    {
        get { return progress; }
        set { progress = value; }
    }

    // -----------------------------------------------------------------------
    // Powers
    // -----------------------------------------------------------------------

    private List<Power> powers = new();

    public List<Power> Powers { get; }

    public void AddPower(Power power)
    {
        if (powers.Count == 3)
        {
            Debug.Log("Player already has 3 powers.");
            return;
        }
        powers.Add(power);
    }

    public void ResetPowers()
    {
        powers = new();
    }

    public bool HasPower(Power power)
    {
        return powers.Contains(power);
    }

    // -----------------------------------------------------------------------
    // Puzzle Tiles
    // -----------------------------------------------------------------------

    /// <summary>
    /// Initializes a puzzle tile with the specified ID and Z position.
    /// </summary>
    /// <param name="puzzleTileId">The ID of the puzzle tile.</param>
    /// <param name="zPosition">The Z position of the puzzle tile.</param>
    public void InitializePuzzleTile(int puzzleTileId, float zPosition)
    {
        AddPuzzleTilePosition(puzzleTileId, new Vector3(0, 0, zPosition));
        AddPuzzleTileSnappedGridTile(puzzleTileId);
    }

    /// <summary>
    /// Indicates whether the player has permission to move puzzle tiles.
    /// </summary>
    private bool hasPuzzleTileMovementPermission = false;

    /// <summary>
    /// Gets or sets a value indicating whether the player has permission to move puzzle tiles.
    /// </summary>
    public bool HasPuzzleTileMovementPermission
    {
        get { return hasPuzzleTileMovementPermission; }
        set { hasPuzzleTileMovementPermission = value; }
    }

    /// <summary>
    /// A dictionary to store the positions of the player's puzzle tiles.
    /// </summary>
    private Dictionary<int, Vector3> puzzleTilesPositions = new();

    /// <summary>
    /// Adds a puzzle tile position to the player's collection.
    /// </summary>
    /// <param name="puzzleTileId">The ID of the puzzle tile.</param>
    /// <param name="position">The position of the puzzle tile.</param>
    public void AddPuzzleTilePosition(int puzzleTileId, Vector3 position)
    {
        puzzleTilesPositions.Add(puzzleTileId, position);
    }

    /// <summary>
    /// Modifies the position of an existing puzzle tile.
    /// </summary>
    /// <param name="puzzleTileId">The ID of the puzzle tile.</param>
    /// <param name="newPosition">The new position of the puzzle tile.</param>
    public void ModifyPuzzleTilePosition(int puzzleTileId, Vector3 newPosition)
    {
        puzzleTilesPositions[puzzleTileId] = newPosition;
    }

    /// <summary>
    /// Gets the position of a specific puzzle tile.
    /// </summary>
    /// <param name="puzzleTileId">The ID of the puzzle tile.</param>
    /// <returns>The position of the puzzle tile.</returns>
    public Vector3 GetPuzzleTilePosition(int puzzleTileId)
    {
        return puzzleTilesPositions[puzzleTileId];
    }

    /// <summary>
    /// Clears all puzzle tile positions from the player's collection.
    /// </summary>
    public void ClearPuzzleTilesPositions()
    {
        puzzleTilesPositions = new();
    }

    /// <summary>
    /// A dictionary to store the IDs of grid tiles that puzzle tiles are snapped to.
    /// </summary>
    private Dictionary<int, int> puzzleTilesSnappedGridTiles = new();

    /// <summary>
    /// Adds a puzzle tile snapped to a grid tile to the player's collection.
    /// </summary>
    /// <param name="puzzleTileId">The ID of the puzzle tile.</param>
    public void AddPuzzleTileSnappedGridTile(int puzzleTileId)
    {
        puzzleTilesSnappedGridTiles.Add(puzzleTileId, -1);
    }

    /// <summary>
    /// Modifies the grid tile that a puzzle tile is snapped to.
    /// </summary>
    /// <param name="puzzleTileId">The ID of the puzzle tile.</param>
    /// <param name="gridTileId">The ID of the grid tile.</param>
    public void ModifyPuzzleTileSnappedGridTile(int puzzleTileId, int gridTileId)
    {
        puzzleTilesSnappedGridTiles[puzzleTileId] = gridTileId;
    }

    /// <summary>
    /// Gets the grid tile that a specific puzzle tile is snapped to.
    /// </summary>
    /// <param name="puzzleTileId">The ID of the puzzle tile.</param>
    /// <returns>The ID of the grid tile. -1 means there is currently none.</returns>
    public int GetPuzzleTileSnappedGridTile(int puzzleTileId)
    {
        return puzzleTilesSnappedGridTiles[puzzleTileId];
    }

    /// <summary>
    /// Clears all snapped grid tiles from the player's collection.
    /// </summary>
    public void ClearPuzzleTilesSnappedGridTiles()
    {
        puzzleTilesPositions = new();
    }

    /// <summary>
    /// The ID of the puzzle tile currently held by the player.
    /// </summary>
    private int heldPuzzleTileId = -1;

    /// <summary>
    /// Gets or sets the ID of the puzzle tile currently held by the player.
    /// </summary>
    public int HeldPuzzleTileId
    {
        get { return heldPuzzleTileId; }
        set { heldPuzzleTileId = value; }
    }

    // -----------------------------------------------------------------------
    // Grid Tiles
    // -----------------------------------------------------------------------

    /// <summary>
    /// Initializes a grid tile with the specified ID.
    /// </summary>
    /// <param name="gridTileId">The ID of the grid tile.</param>
    public void InitializeGridTile(int gridTileId)
    {
        AddGridTileOccupied(gridTileId);
        AddGridTileCorrectlyOccupied(gridTileId);
    }

    /// <summary>
    /// A dictionary to store whether each grid tile is occupied.
    /// </summary>
    private Dictionary<int, bool> gridTilesOccupied = new();

    /// <summary>
    /// Adds a grid tile to the player's collection of occupied tiles.
    /// </summary>
    /// <param name="gridTileId">The ID of the grid tile.</param>
    public void AddGridTileOccupied(int gridTileId)
    {
        gridTilesOccupied.Add(gridTileId, false);
    }

    /// <summary>
    /// Modifies the occupancy state of a grid tile.
    /// </summary>
    /// <param name="gridTileId">The ID of the grid tile.</param>
    /// <param name="newState">The new occupancy state of the grid tile.</param>
    public void ModifyGridTileOccupied(int gridTileId, bool newState)
    {
        gridTilesOccupied[gridTileId] = newState;
    }

    /// <summary>
    /// Gets the occupancy state of a specific grid tile.
    /// </summary>
    /// <param name="gridTileId">The ID of the grid tile.</param>
    /// <returns>The occupancy state of the grid tile.</returns>
    public bool GetGridTileOccupied(int gridTileId)
    {
        return gridTilesOccupied[gridTileId];
    }

    /// <summary>
    /// Clears all grid tiles from the player's collection of occupied tiles.
    /// </summary>
    public void ClearGridTilesOccupied()
    {
        gridTilesOccupied = new();
    }

    /// <summary>
    /// A dictionary to store whether each grid tile is correctly occupied by a puzzle tile.
    /// </summary>
    private Dictionary<int, bool> gridTilesCorrectlyOccupied = new();

    /// <summary>
    /// Adds a grid tile to the player's collection of correctly occupied tiles.
    /// </summary>
    /// <param name="gridTileId">The ID of the grid tile.</param>
    public void AddGridTileCorrectlyOccupied(int gridTileId)
    {
        gridTilesCorrectlyOccupied.Add(gridTileId, false);
    }

    /// <summary>
    /// Modifies the occupancy state of a grid tile.
    /// </summary>
    /// <param name="gridTileId">The ID of the grid tile.</param>
    /// <param name="newState">The new occupancy state of the grid tile.</param>
    public void ModifyGridTileCorrectlyOccupied(int gridTileId, bool newState)
    {
        gridTilesCorrectlyOccupied[gridTileId] = newState;
    }

    /// <summary>
    /// Gets the occupancy state of a specific grid tile.
    /// </summary>
    /// <param name="gridTileId">The ID of the grid tile.</param>
    /// <returns>The occupancy state of the grid tile.</returns>
    public bool GetGridTileCorrectlyOccupied(int gridTileId)
    {
        return gridTilesCorrectlyOccupied[gridTileId];
    }

    /// <summary>
    /// Clears all grid tiles from the player's collection of correctly occupied tiles.
    /// </summary>
    public void ClearGridTilesCorrectlyOccupied()
    {
        gridTilesCorrectlyOccupied = new();
    }

    // -----------------------------------------------------------------------
    // Peeking
    // -----------------------------------------------------------------------

    /// <summary>
    /// Value of whether the player is currently peeking at other player.
    /// </summary>
    private bool isPeeking = false;

    /// <summary>
    /// Gets or sets the value of whether the player is currently peeking at other player.
    /// </summary>
    public bool IsPeeking
    {
        get { return isPeeking; }
        set { isPeeking = value; }
    }

    /// <summary>
    /// The target of the player's peek. Is null when player is not currently peeking.
    /// </summary>
    private Player targetOfPeekPlayer = null;

    /// <summary>
    /// Gets or sets the target of the player's peek. Is null when player is not currently peeking.
    /// </summary>
    public Player TargetOfPeekPlayer
    {
        get { return targetOfPeekPlayer; }
        set { targetOfPeekPlayer = value; }
    }

    /// <summary>
    /// Value of whether the player's peek use is on cooldown.
    /// </summary>
    private bool peekUseOnCooldown = false;

    /// <summary>
    /// Gets or sets the value of whether the player's peek use is on cooldown.
    /// </summary>
    public bool PeekUseOnCooldown
    {
        get { return peekUseOnCooldown; }
        set { peekUseOnCooldown = value; }
    }

    /// <summary>
    /// Value of whether the player's peek exit is on cooldown.
    /// </summary>
    private bool peekExitOnCooldown = false;

    /// <summary>
    /// Gets or sets the value of whether the player's peek exit is on cooldown.
    /// </summary>
    public bool PeekExitOnCooldown
    {
        get { return peekExitOnCooldown; }
        set { peekExitOnCooldown = value; }
    }
}