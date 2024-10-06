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
        ownerOfPuzzleCurrentlyManipulating = this;
        playersCurrenlyManipulaingPuzzle = new() { this };
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
    /// The ID of the background skin sprite.
    /// </summary>
    private int backgroundSkinId;

    /// <summary>
    /// Gets or sets the ID of the background skin sprite.
    /// </summary>
    public int BackgroundSkinId
    {
        get { return backgroundSkinId; }
        set { backgroundSkinId = value; }
    }

    /// <summary>
    /// The ID of the profile picutre sprite.
    /// </summary>
    private int profilePictureId;

    /// <summary>
    /// Gets or sets the ID of the profile picutre sprite.
    /// </summary>
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

    /// <summary>
    /// List of all equipped powers of the player.
    /// </summary>
    private List<Power> powers = new();

    /// <summary>
    /// Gets the list of all equipped powers of the player.
    /// </summary>
    public List<Power> Powers
    {
        get { return powers; }
    }

    /// <summary>
    /// Adds a new power the list of equipped powers. Limit is 3.
    /// </summary>
    /// <param name="power">Power to add. </param>
    public void AddPower(Power power)
    {
        if (powers.Count == 3)
        {
            Debug.Log("Player already has 3 powers.");
            return;
        }
        powers.Add(power);
    }

    /// <summary>
    /// Resets the lsit of player's equipped powers.
    /// </summary>
    public void ResetPowers()
    {
        powers = new();
    }

    /// <summary>
    /// Checks whether the player has a specific power.
    /// </summary>
    /// <param name="power">Power to check.</param>
    /// <returns>Bool value of whether player has the power.</returns>
    public bool HasPower(Power power)
    {
        return powers.Contains(power);
    }

    /// <summary>
    /// Checks whether the player has a specific power by name.
    /// </summary>
    /// <param name="powerName">The name of the power to check.</param>
    /// <returns>Bool value of whether player has the power.</returns>
    public bool HasPower(string powerName)
    {
        foreach (Power power in powers)
        {
            if (power.Name == powerName)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Gets the index of the power in player's equipped power list.
    /// </summary>
    /// <param name="power">Specific power</param>
    /// <returns>Index of the specific power.</returns>
    public int GetPowerIndex(Power power)
    {
        return powers.IndexOf(power);
    }

    /// <summary>
    /// Gets the power from equipped power list at a specific index.
    /// </summary>
    /// <param name="index">Index of the power</param>
    /// <returns>Power at the index</returns>
    public Power GetPowerAtIndex(int index)
    {
        return powers[index];
    }

    /// <summary>
    /// List that contains all powers currently on cooldown
    /// </summary>
    private List<Power> powersOnCooldown = new();

    /// <summary>
    /// Puts the power on the powers on cooldown list.
    /// </summary>
    /// <param name="power">The power to put on the cooldown</param>
    public void PutPowerOnCooldown(Power power)
    {
        powersOnCooldown.Add(power);
    }

    /// <summary>
    /// Removes the power from the powers on cooldown list.
    /// </summary>
    /// <param name="power">The power to put off the cooldown</param>
    public void PutPowerOffCooldown(Power power)
    {
        powersOnCooldown.Remove(power);
    }

    /// <summary>
    /// Checks whether the power is currenly on cooldown.
    /// </summary>
    /// <param name="power">Power to check</param>
    /// <returns>Bool value of whether the power is currenly on cooldown</returns>
    public bool IsPowerOnCooldown(Power power)
    {
        return powersOnCooldown.Contains(power);
    }

    // -----------------------------------------------------------------------
    // Puzzle Tiles
    // -----------------------------------------------------------------------

    /// <summary>
    /// Holds the Player whose puzzle tiles is this player currenly manipulating. Is initialized in constructor.
    /// </summary>
    private Player ownerOfPuzzleCurrentlyManipulating;

    /// <summary>
    /// Gets or sets the Player whose puzzle tiles is this player currenly manipulating.
    /// </summary>
    public Player OwnerOfPuzzleCurrentlyManipulating
    {
        get { return ownerOfPuzzleCurrentlyManipulating; }
        set { ownerOfPuzzleCurrentlyManipulating = value; }
    }

    /// <summary>
    /// List of players who are currenly manipulating this player's puzzle tiles.
    /// </summary>
    private List<Player> playersCurrenlyManipulaingPuzzle;

    /// <summary>
    /// Gets the list of players who are currenly manipulating this player's puzzle tiles.
    /// </summary>
    public List<Player> PlayersCurrenlyManipulaingPuzzle
    {
        get { return playersCurrenlyManipulaingPuzzle; }
    }

    /// <summary>
    /// Adds a player to the list of players who are currenly manipulating this player's puzzle tiles.
    /// </summary>
    /// <param name="player">Player to add.</param>
    public void AddPlayerCurrenlyManipulatingPuzzle(Player player)
    {
        playersCurrenlyManipulaingPuzzle.Add(player);
    }

    /// <summary>
    /// Removes a player from the list of players who are currenly manipulating this player's puzzle tiles.
    /// </summary>
    /// <param name="player">PLayer to remove</param>
    public void RemovePlayerCurrenlyManipulatingPuzzle(Player player)
    {
        playersCurrenlyManipulaingPuzzle.Remove(player);
    }

    /// <summary>
    /// Resets the list of players who are currenly manipulating this player's puzzle tiles.
    /// </summary>
    public void ResetPlayersCurrenlyManipulaingPuzzle()
    {
        playersCurrenlyManipulaingPuzzle = new() { this };
    }

    public bool IsTileHeldByAnotherPlayer(int puzzleTileId)
    {
        foreach (Player player in playersCurrenlyManipulaingPuzzle)
        {
            if (player.HeldPuzzleTileId == puzzleTileId)
            {
                return true;
            }
        }
        return false;
    }

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
    // SnappingToGrid
    // -----------------------------------------------------------------------

    /// <summary>
    /// Flag that determines whether player's puzzle tiles will snap to grid.
    /// </summary>
    private bool snapToGridEnabled = true;

    /// <summary>
    /// Gets or sets the flag that determines whether player's puzzle tiles will snap to grid.
    /// </summary>
    public bool SnapToGridEnabled
    {
        get { return snapToGridEnabled; }
        set { snapToGridEnabled = value; }
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
    /// The value of whether the player can exit peeking by using either sotp button or peeking at otehr players.
    /// </summary>
    private bool peekExitOnCooldown = false;

    /// <summary>
    /// Gets or sets the value of whether the player can exit peeking by using either stop button or peeking at other players.
    /// </summary>
    public bool PeekExitOnCooldown
    {
        get { return peekExitOnCooldown; }
        set { peekExitOnCooldown = value; }
    }

    /// <summary>
    /// List of players this player can't peek on (either due to cooldown or powers).
    /// </summary>
    private List<Player> unpeekablePlayers = new();

    /// <summary>
    /// Gets the list of players this player can't peek on (either due to cooldown or powers).
    /// </summary>
    public List<Player> UnpeekablePlayers
    {
        get { return unpeekablePlayers; }
    }

    /// <summary>
    /// Determines if the player can peek on the specified player.
    /// </summary>
    /// <param name="player">The player to check if peeking is allowed.</param>
    /// <returns>True if the player can peek, false otherwise.</returns>
    public bool CanPeekOnPlayer(Player player)
    {
        return !unpeekablePlayers.Contains(player);
    }

    /// <summary>
    /// Adds a player to the list of players that cannot be peeked on.
    /// </summary>
    /// <param name="player">The player to add to the unpeekable list.</param>
    public void AddUnpeekablePlayer(Player player)
    {
        unpeekablePlayers.Add(player);
    }

    /// <summary>
    /// Removes a player from the list of players that cannot be peeked on.
    /// </summary>
    /// <param name="player">The player to remove from the unpeekable list.</param>
    public void RemoveUnpeekablePlayer(Player player)
    {
        unpeekablePlayers.Remove(player);
    }

    /// <summary>
    /// Resets the list of unpeekable players, making all players peekable again.
    /// </summary>
    public void ResetUnpeekablePlayers()
    {
        unpeekablePlayers = new();
    }
}