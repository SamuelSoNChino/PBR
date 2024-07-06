using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages the puzzle game, including tile generation, movement, shuffling, and player interactions.
/// </summary>
public class PuzzleManager : NetworkBehaviour
{
    /// <summary>
    /// Manages the puzzle tiles in the scene.
    /// </summary>
    [SerializeField] private TilesManagerMultiplayer tilesManager;

    /// <summary>
    /// Manages the grid where tiles are placed.
    /// </summary>
    [SerializeField] private GridManagerMultiplayer gridManager;

    /// <summary>
    /// Manages player data and interactions.
    /// </summary>
    [SerializeField] private PlayerManager playerManager;

    /// <summary>
    /// Manages the multiplayer functionality.
    /// </summary>
    [SerializeField] private MultiplayerManager multiplayerManager;

    /// <summary>
    /// Manages the pan and zoom functionality.
    /// </summary>
    [SerializeField] private PanZoom panZoom;

    /// <summary>
    /// Manages the peeking functionality.
    /// </summary>
    [SerializeField] private PeekManager peekManager;

    // -----------------------------------------------------------------------
    // Tile IDs
    // -----------------------------------------------------------------------

    /// <summary>
    /// List of unique IDs for puzzle tiles.
    /// </summary>
    private List<int> puzzleTileIds;

    /// <summary>
    /// List of unique IDs for grid tiles.
    /// </summary>
    private List<int> gridTileIds;

    /// <summary>
    /// Generates a list of unique IDs.
    /// </summary>
    /// <param name="numberOfIds">The number of IDs to generate.</param>
    /// <returns>A list of unique IDs.</returns>
    private List<int> GenerateIds(int numberOfIds)
    {
        List<int> ids = new();
        while (ids.Count < numberOfIds)
        {
            int id = UnityEngine.Random.Range(1, 100000);
            if (!ids.Contains(id))
            {
                ids.Add(id);
            }
        }
        return ids;
    }

    /// <summary>
    /// Generates new unique IDs for the puzzle and grid tiles.
    /// </summary>
    /// <param name="numberOfTiles">The number of tiles on one side of the puzzle.</param>
    public void GenerateNewPuzzleKey(int numberOfTiles)
    {
        puzzleTileIds = GenerateIds(numberOfTiles * numberOfTiles);
        gridTileIds = GenerateIds(numberOfTiles * numberOfTiles);
    }

    /// <summary>
    /// Gets the list of unique puzzle tile IDs.
    /// </summary>
    /// <returns>A list of puzzle tile IDs.</returns>
    public List<int> GetPuzzleTileIds()
    {
        return new List<int>(puzzleTileIds);
    }

    /// <summary>
    /// Gets the list of unique grid tile IDs.
    /// </summary>
    /// <returns>A list of grid tile IDs.</returns>
    public List<int> GetGridTileIds()
    {
        return new List<int>(gridTileIds);
    }

    /// <summary>
    /// Checks if a puzzle tile and a grid tile match.
    /// </summary>
    /// <param name="puzzleTileId">The unique ID of the puzzle tile.</param>
    /// <param name="gridTileId">The unique ID of the grid tile.</param>
    /// <returns>True if the tiles match, otherwise false.</returns>
    public bool CheckTiles(int puzzleTileId, int gridTileId)
    {
        int puzzleTileIdIndex = puzzleTileIds.IndexOf(puzzleTileId);
        return gridTileId == gridTileIds[puzzleTileIdIndex];
    }

    // -----------------------------------------------------------------------
    // Grid Tiles
    // -----------------------------------------------------------------------

    /// <summary>
    /// Dictionary mapping grid tile positions to their IDs.
    /// </summary>
    private Dictionary<Vector2, int> gridTilesByPositions = new();

    /// <summary>
    /// Adds a grid tile to the dictionary by its position.
    /// </summary>
    /// <param name="position2D">The 2D position of the grid tile.</param>
    /// <param name="gridTileId">The unique ID of the grid tile.</param>
    public void AddGridTileByPosition(Vector2 position2D, int gridTileId)
    {
        gridTilesByPositions.Add(position2D, gridTileId);
    }

    /// <summary>
    /// Resets the dictionary of grid tiles.
    /// </summary>
    public void ResetGridTilesByPositions()
    {
        gridTilesByPositions = new();
    }

    /// <summary>
    /// Updates the grid state for a specific puzzle tile based on its position.
    /// </summary>
    /// <param name="player">The player object.</param>
    /// <param name="puzzleTileId">The unique ID of the puzzle tile.</param>
    public void UpdateGridForPuzzleTile(Player player, int puzzleTileId)
    {
        Vector3 puzzleTilePosition = player.GetPuzzleTilePosition(puzzleTileId);
        Vector2 puzzleTilePosition2D = new(puzzleTilePosition.x, puzzleTilePosition.y);

        if (gridTilesByPositions.Keys.Contains(puzzleTilePosition2D))
        {
            int gridTileId = gridTilesByPositions[puzzleTilePosition2D];

            if (CheckTiles(puzzleTileId, gridTileId))
            {
                player.ModifyGridTileCorrectlyOccupied(gridTileId, true);
                CheckGridCompleteness(player);
                player.ModifyPuzzleTileSnappedGridTile(puzzleTileId, gridTileId);
            }
        }
        // -1 means that the puzzle tile is not snapped on any grid tile
        else if (player.GetPuzzleTileSnappedGridTile(puzzleTileId) != -1)
        {
            int snappedGridTileId = player.GetPuzzleTileSnappedGridTile(puzzleTileId);
            player.ModifyGridTileCorrectlyOccupied(snappedGridTileId, false);
            player.ModifyPuzzleTileSnappedGridTile(puzzleTileId, snappedGridTileId);
        }

    }

    /// <summary>
    /// Updates the grid state for all puzzle tiles for a specific client.
    /// </summary>
    /// <param name="clientId">The unique ID of the client.</param>
    public void UpdateGridForAllTiles(ulong clientId)
    {
        Player player = playerManager.FindPlayerByClientId(clientId);
        foreach (int puzzleTileId in puzzleTileIds)
        {
            UpdateGridForPuzzleTile(player, puzzleTileId);
        }
    }

    /// <summary>
    /// Checks if the entire grid is correctly occupied by puzzle tiles.
    /// </summary>
    /// <param name="player">The player object.</param>
    public void CheckGridCompleteness(Player player)
    {
        foreach (int gridTileId in gridTileIds)
        {
            if (!player.GetGridTileCorrectlyOccupied(gridTileId))
            {
                return;
            }
        }
        multiplayerManager.EndGame(player.ClientId);
    }

    // -----------------------------------------------------------------------
    // Tile Shuffle
    // -----------------------------------------------------------------------

    /// <summary>
    /// Top right bound of the area where tiles can be shuffled.
    /// </summary>
    [SerializeField] private Vector3 topRightShuffleBound;

    /// <summary>
    /// Bottom left bound of the area where tiles can be shuffled.
    /// </summary>
    [SerializeField] private Vector3 bottomLeftShuffleBound;

    /// <summary>
    /// The size of the whole puzzle image.
    /// </summary>
    [SerializeField] private int puzzleSize;

    /// <summary>
    /// Shuffles all tiles randomly within the shuffle bounds and also shuffles their z-values.
    /// </summary>
    /// <param name="seed">Optional seed for random number generation.</param>
    public void ShuffleAllTiles(int seed)
    {
        UnityEngine.Random.InitState(seed);
        int tileSize = puzzleSize / (int)Math.Sqrt(puzzleTileIds.Count);

        float minX = bottomLeftShuffleBound.x;
        float maxX = topRightShuffleBound.x - tileSize;
        float minY = bottomLeftShuffleBound.y;
        float maxY = topRightShuffleBound.y - tileSize;

        foreach (int puzzleTileId in puzzleTileIds)
        {
            // Temporary solution
            Player firstPlayer = playerManager.GetAllPlayers()[0];
            float zPosition = firstPlayer.GetPuzzleTilePosition(puzzleTileId).z;

            Vector3 newPosition = new(UnityEngine.Random.Range(minX, maxX), UnityEngine.Random.Range(minY, maxY), zPosition);

            foreach (Player player in playerManager.GetAllPlayers())
            {
                player.ModifyPuzzleTilePosition(puzzleTileId, newPosition);
                SetNewTilePositionRpc(player.ClientId, puzzleTileId, newPosition);
            }
        }
    }

    // -----------------------------------------------------------------------
    // Tile Movement
    // -----------------------------------------------------------------------

    /// <summary>
    /// Updates the position of a tile on the server.
    /// </summary>
    /// <param name="tileId">The unique ID of the tile.</param>
    /// <param name="newPosition">The new position of the tile.</param>
    public void UpdateServerPosition(int tileId, Vector3 newPosition)
    {
        UpdateServerPositionRpc(NetworkManager.Singleton.LocalClientId, tileId, newPosition);
    }

    /// <summary>
    /// RPC to update the position of a tile on the server.
    /// </summary>
    /// <param name="clientId">The unique ID of the client.</param>
    /// <param name="puzzleTileId">The unique ID of the puzzle tile.</param>
    /// <param name="newPosition">The new position of the puzzle tile.</param>
    [Rpc(SendTo.Server)]
    public void UpdateServerPositionRpc(ulong clientId, int puzzleTileId, Vector3 newPosition)
    {
        Player player = playerManager.FindPlayerByClientId(clientId);

        if (peekManager.IsClientPeeking(clientId))
        {
            ulong targetOfPeekClientId = peekManager.GetTargetOfPeekUser(clientId);
            Player targetOfPeekPlayer = playerManager.FindPlayerByClientId(targetOfPeekClientId);
            if (player.HasPuzzleTileMovementPermission)
            {
                targetOfPeekPlayer.ModifyPuzzleTilePosition(puzzleTileId, newPosition);
            }
            else
            {
                SetNewTilePositionRpc(clientId, puzzleTileId, targetOfPeekPlayer.GetPuzzleTilePosition(puzzleTileId));
            }
        }
        else
        {
            if (player.HasPuzzleTileMovementPermission)
            {
                player.ModifyPuzzleTilePosition(puzzleTileId, newPosition);
                UpdateGridForPuzzleTile(player, puzzleTileId);
            }
            else
            {
                SetNewTilePositionRpc(clientId, puzzleTileId, player.GetPuzzleTilePosition(puzzleTileId));
            }
        }
    }

    /// <summary>
    /// RPC to set the new position of a tile on the clients.
    /// </summary>
    /// <param name="clientId">The unique ID of the client.</param>
    /// <param name="puzzleTileId">The unique ID of the puzzle tile.</param>
    /// <param name="newPosition">The new position of the puzzle tile.</param>
    [Rpc(SendTo.ClientsAndHost)]
    public void SetNewTilePositionRpc(ulong clientId, int puzzleTileId, Vector3 newPosition)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            PuzzleTileMultiplayer puzzleTile = tilesManager.FindPuzzleTileById(puzzleTileId).GetComponent<PuzzleTileMultiplayer>();
            puzzleTile.Position = newPosition;
        }
    }

    // -----------------------------------------------------------------------
    // Tile Movement Permissions
    // -----------------------------------------------------------------------

    /// <summary>
    /// Disables tile movement for all players or a specific player.
    /// </summary>
    /// <param name="clientId">The unique ID of the client. Defaults to 1234567890 for all players.</param>
    public void DisableTileMovement(ulong clientId = 1234567890)
    {
        if (clientId == 1234567890)
        {
            foreach (Player targetPlayer in playerManager.GetAllPlayers())
            {
                targetPlayer.HasPuzzleTileMovementPermission = false;
                DisableAllCollidersRpc(targetPlayer.ClientId);
            }
        }
        else
        {
            Player targetPlayer = playerManager.FindPlayerByClientId(clientId);
            targetPlayer.HasPuzzleTileMovementPermission = false;
            DisableAllCollidersRpc(targetPlayer.ClientId);
        }
    }

    /// <summary>
    /// RPC to disable all colliders on the clients.
    /// </summary>
    /// <param name="clientId">The unique ID of the client.</param>
    [Rpc(SendTo.ClientsAndHost)]
    public void DisableAllCollidersRpc(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            tilesManager.DisableAllColliders();
        }
    }

    /// <summary>
    /// Enables tile movement for all players or a specific player.
    /// </summary>
    /// <param name="clientId">The unique ID of the client. Defaults to 1234567890 for all players.</param>
    public void EnableTileMovement(ulong clientId = 1234567890)
    {
        if (clientId == 1234567890)
        {
            foreach (Player targetPlayer in playerManager.GetAllPlayers())
            {
                targetPlayer.HasPuzzleTileMovementPermission = true;
                EnableAllCollidersRpc(targetPlayer.ClientId);
            }
        }
        else
        {
            Player targetPlayer = playerManager.FindPlayerByClientId(clientId);
            targetPlayer.HasPuzzleTileMovementPermission = true;
            EnableAllCollidersRpc(targetPlayer.ClientId);
        }
    }

    /// <summary>
    /// RPC to enable all colliders on the clients.
    /// </summary>
    /// <param name="clientId">The unique ID of the client.</param>
    [Rpc(SendTo.ClientsAndHost)]
    public void EnableAllCollidersRpc(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            tilesManager.EnableAllColliders();
        }
    }

    // -----------------------------------------------------------------------
    // PanZoom Management
    // -----------------------------------------------------------------------

    /// <summary>
    /// Enables touch input for all players or a specific player.
    /// </summary>
    /// <param name="clientId">The unique ID of the client. Defaults to 1234567890 for all players.</param>
    public void EnableTouchInput(ulong clientId = 1234567890)
    {
        if (clientId == 1234567890)
        {
            foreach (Player targetPlayer in playerManager.GetAllPlayers())
            {
                EnableTouchInputRpc(targetPlayer.ClientId);
            }
        }
        else
        {
            EnableTouchInputRpc(clientId);
        }
    }

    /// <summary>
    /// RPC to enable touch input on the clients.
    /// </summary>
    /// <param name="clientId">The unique ID of the client.</param>
    [Rpc(SendTo.ClientsAndHost)]
    public void EnableTouchInputRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            panZoom.EnableTouchInput();
        }
    }

    /// <summary>
    /// Disables touch input for all players or a specific player.
    /// </summary>
    /// <param name="clientId">The unique ID of the client. Defaults to 1234567890 for all players.</param>
    public void DisableTouchInput(ulong clientId = 1234567890)
    {
        if (clientId == 1234567890)
        {
            foreach (Player targetPlayer in playerManager.GetAllPlayers())
            {
                DisableTouchInputRpc(targetPlayer.ClientId);
            }
        }
        else
        {
            DisableTouchInputRpc(clientId);
        }
    }

    /// <summary>
    /// RPC to disable touch input on the clients.
    /// </summary>
    /// <param name="clientId">The unique ID of the client.</param>
    [Rpc(SendTo.ClientsAndHost)]
    public void DisableTouchInputRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            panZoom.DisableTouchInput();
        }
    }

    // -----------------------------------------------------------------------
    // Reset Functionality
    // -----------------------------------------------------------------------

    /// <summary>
    /// RPC to destroy all puzzle and grid tiles on the clients.
    /// </summary>
    [Rpc(SendTo.ClientsAndHost)]
    public void DestroyAllTilesClientRpc()
    {
        List<GameObject> puzzleTileObjects = tilesManager.GetAllPuzzleTiles();
        foreach (GameObject puzzleTileObject in puzzleTileObjects)
        {
            Destroy(puzzleTileObject);
        }

        List<GameObject> gridTileObjects = gridManager.GetAllGridTiles();
        foreach (GameObject gridTileObject in gridTileObjects)
        {
            Destroy(gridTileObject);
        }
    }

    // -----------------------------------------------------------------------
    // Peek Functionality
    // -----------------------------------------------------------------------

    /// <summary>
    /// Sets the positions of the puzzle tiles for a client to match another client's puzzle tiles.
    /// </summary>
    /// <param name="clientId">The unique ID of the client.</param>
    /// <param name="otherClientId">The unique ID of the other client.</param>
    public void SetOtherClientsPositions(ulong clientId, ulong otherClientId)
    {
        Player otherPlayer = playerManager.FindPlayerByClientId(otherClientId);

        foreach (int puzzleTileId in puzzleTileIds)
        {
            SetNewTilePositionRpc(clientId, puzzleTileId, otherPlayer.GetPuzzleTilePosition(puzzleTileId));
        }
    }

    /// <summary>
    /// RPC to deselect all tiles on the client.
    /// </summary>
    /// <param name="clientId">The unique ID of the client.</param>
    [Rpc(SendTo.ClientsAndHost)]
    public void DeselectAllClientTilesRpc(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            tilesManager.DeselectAllTiles();
        }
    }

    /// <summary>
    /// RPC to reset the snapped state of all tiles on the client.
    /// </summary>
    /// <param name="clientId">The unique ID of the client.</param>
    [Rpc(SendTo.ClientsAndHost)]
    public void ResetClientsSnappedTilesRpc(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            tilesManager.UnsnapAllFromGrid();
            tilesManager.SnapAllToGrid();
        }
    }
}
