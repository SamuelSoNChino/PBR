using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    [SerializeField] private PowerManager powerManager;

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

    [SerializeField] private LeaderboardManager leaderboardManager;

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
    /// Updates the grid status for a given puzzle tile and grid tile.
    /// </summary>
    /// <param name="player">The player placing the tile.</param>
    /// <param name="puzzleTileId">The ID of the puzzle tile.</param>
    /// <param name="snappedGridTileId">The ID of the grid tile where the puzzle tile is placed.</param>
    public void UpdateGridForPuzzleTile(Player player, int puzzleTileId, int snappedGridTileId)
    {
        if (CheckTiles(puzzleTileId, snappedGridTileId))
        {
            player.ModifyGridTileCorrectlyOccupied(snappedGridTileId, true);
            CheckGridCompleteness(player);
        }
        else
        {
            player.ModifyGridTileCorrectlyOccupied(snappedGridTileId, false);
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
    /// Updates the position of a tile on the server and handles synchronization with clients.
    /// </summary>
    /// <param name="clientId">The unique ID of the client requesting the update.</param>
    /// <param name="puzzleTileId">The unique ID of the puzzle tile to update.</param>
    /// <param name="newPosition">The new position of the puzzle tile.</param>
    [Rpc(SendTo.Server)]
    public void UpdateServerPositionRpc(ulong clientId, int puzzleTileId, Vector3 newPosition)
    {
        Player player = playerManager.FindPlayerByClientId(clientId);
        Player tileOwnerPlayer = player.OwnerOfPuzzleCurrentlyManipulating;

        if (player.HasPuzzleTileMovementPermission && player.HeldPuzzleTileId == puzzleTileId)
        {
            tileOwnerPlayer.ModifyPuzzleTilePosition(puzzleTileId, newPosition);
            UpdateTilePositionForPlayers(player, puzzleTileId, newPosition);
        }
        else
        {
            SetNewTilePositionRpc(clientId, puzzleTileId, tileOwnerPlayer.GetPuzzleTilePosition(puzzleTileId));
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

    /// <summary>
    /// Synchronizes the tile positions for a player with other clients.
    /// </summary>
    /// <param name="player">The player whose view is being updated.</param>
    /// <param name="otherPlayer">The other player whose tile positions are being synchronized.</param>
    public void SetOtherClientsPositions(Player player, Player otherPlayer)
    {
        foreach (int puzzleTileId in puzzleTileIds)
        {
            SetNewTilePositionRpc(player.ClientId, puzzleTileId, otherPlayer.GetPuzzleTilePosition(puzzleTileId));
        }
    }

    /// <summary>
    /// Updates the tile position for a player, either during normal gameplay or when peeking.
    /// </summary>
    /// <param name="player">The player whose tile position is being updated.</param>
    /// <param name="puzzleTileId">The ID of the puzzle tile to update.</param>
    /// <param name="newPosition">The new position of the puzzle tile.</param>
    public void UpdateTilePositionForPlayers(Player player, int puzzleTileId, Vector3 newPosition)
    {
        Player tileOwnerPlayer = player.OwnerOfPuzzleCurrentlyManipulating;
        foreach (Player playerManipulating in tileOwnerPlayer.PlayersCurrenlyManipulaingPuzzle)
        {
            if (playerManipulating != player)
            {
                SetNewTilePositionRpc(playerManipulating.ClientId, puzzleTileId, newPosition);
            }
        }
    }

    // -----------------------------------------------------------------------
    // Tile Movement Permissions
    // -----------------------------------------------------------------------


    /// <summary>
    /// Disables tile movement for all players or a specific player.
    /// </summary>
    /// <param name="targetPlayer">The specific player to disable movement for, or null to disable for all players.</param>
    public void DisableTileMovement(Player targetPlayer = null)
    {
        if (targetPlayer == null)
        {
            foreach (Player player in playerManager.GetAllPlayers())
            {
                player.HasPuzzleTileMovementPermission = false;
                player.HeldPuzzleTileId = -1;
                DisableAllCollidersRpc(player.ClientId);
            }
        }
        else
        {
            targetPlayer.HasPuzzleTileMovementPermission = false;
            targetPlayer.HeldPuzzleTileId = -1;
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
    /// <param name="targetPlayer">The specific player to enable movement for, or null to enable for all players.</param>
    public void EnableTileMovement(Player targetPlayer = null)
    {
        if (targetPlayer == null)
        {
            foreach (Player player in playerManager.GetAllPlayers())
            {
                player.HasPuzzleTileMovementPermission = true;
                EnableAllCollidersRpc(player.ClientId);
            }
        }
        else
        {
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

    /// <summary>
    /// RPC to disable the collider of a specific puzzle tile on the client.
    /// </summary>
    /// <param name="clientId">The unique ID of the client.</param>
    /// <param name="puzzleTileId">The unique ID of the puzzle tile.</param>
    [Rpc(SendTo.ClientsAndHost)]
    public void DisablePuzzleTileColliderRpc(ulong clientId, int puzzleTileId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            PuzzleTileMultiplayer puzzleTile = tilesManager.FindPuzzleTileById(puzzleTileId).GetComponent<PuzzleTileMultiplayer>();
            puzzleTile.DisableCollider();
        }
    }


    /// <summary>
    /// RPC to enable the collider of a specific puzzle tile on the client.
    /// </summary>
    /// <param name="clientId">The unique ID of the client.</param>
    /// <param name="puzzleTileId">The unique ID of the puzzle tile.</param>
    [Rpc(SendTo.ClientsAndHost)]
    public void EnablePuzzleTileColliderRpc(ulong clientId, int puzzleTileId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            PuzzleTileMultiplayer puzzleTile = tilesManager.FindPuzzleTileById(puzzleTileId).GetComponent<PuzzleTileMultiplayer>();
            puzzleTile.EnableCollider();
        }
    }

    /// <summary>
    /// Disables the colliders of a specific puzzle tile for all players who are manipulating on the target player, except the one initiating the action.
    /// </summary>
    /// <param name="player">The player initiating the action, who is peeking.</param>
    /// <param name="puzzleTileId">The unique ID of the puzzle tile whose colliders are to be disabled.</param>
    private void DisableColliderForOtherPlayerManipulating(Player player, int puzzleTileId)
    {
        Player tileOwnerPlayer = player.OwnerOfPuzzleCurrentlyManipulating;
        foreach (Player playerManipulating in tileOwnerPlayer.PlayersCurrenlyManipulaingPuzzle)
        {
            if (playerManipulating != player)
            {
                DisablePuzzleTileColliderRpc(playerManipulating.ClientId, puzzleTileId);
            }
        }
    }

    /// <summary>
    /// Enables the colliders of a specific puzzle tile for all players who are manipulating on the target player, except the one initiating the action.
    /// </summary>
    /// <param name="player">The player initiating the action, who is peeking.</param>
    /// <param name="puzzleTileId">The unique ID of the puzzle tile whose colliders are to be enabled.</param>
    private void EnableColliderForOtherPlayerManipulating(Player player, int puzzleTileId)
    {
        Player tileOwnerPlayer = player.OwnerOfPuzzleCurrentlyManipulating;
        foreach (Player playerManipulating in tileOwnerPlayer.PlayersCurrenlyManipulaingPuzzle)
        {
            if (playerManipulating != player)
            {
                DisablePuzzleTileColliderRpc(playerManipulating.ClientId, puzzleTileId);
            }
        }
    }

    // -----------------------------------------------------------------------
    // PanZoom Management
    // -----------------------------------------------------------------------

    /// <summary>
    /// Enables touch input for all players or a specific player.
    /// </summary>
    /// <param name="targetPlayer">The specific player to enable touch input for, or null to enable for all players.</param>
    public void EnableTouchInput(Player targetPlayer = null)
    {
        if (targetPlayer == null)
        {
            foreach (Player player in playerManager.GetAllPlayers())
            {
                EnableTouchInputRpc(player.ClientId);
            }
        }
        else
        {
            EnableTouchInputRpc(targetPlayer.ClientId);
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
    /// <param name="targetPlayer">The specific player to disable touch input for, or null to disable for all players.</param>
    public void DisableTouchInput(Player targetPlayer = null)
    {
        if (targetPlayer == null)
        {
            foreach (Player player in playerManager.GetAllPlayers())
            {
                DisableTouchInputRpc(player.ClientId);
            }
        }
        else
        {
            DisableTouchInputRpc(targetPlayer.ClientId);
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
    // Progress Tracking
    // -----------------------------------------------------------------------


    /// <summary>
    /// Reference to the TextMeshProUGUI component for displaying the progress.
    /// </summary>
    [SerializeField] private TextMeshProUGUI progressText;

    /// <summary>
    /// Gets the neighbouring tiles of a given tile in the grid.
    /// </summary>
    /// <param name="tileIds">List of tile IDs in the grid.</param>
    /// <param name="tileId">The tile ID to find neighbours for.</param>
    /// <returns>A list of neighbouring tile IDs in the format [TOP, BOTTOM, LEFT, RIGHT], Id is -1 if the neigbour doesn't exist.</returns>
    private List<int> GetNeighbouringTiles(List<int> tileIds, int tileId)
    {
        List<int> neighbouringTiles = new() { -1, -1, -1, -1 }; // Initialize with -1

        int numberOfTiles = (int)Math.Sqrt(tileIds.Count);

        int mainTileIndex = tileIds.IndexOf(tileId);
        int row = mainTileIndex / numberOfTiles;
        int column = mainTileIndex % numberOfTiles;

        // Top neighbor
        if (row != 0)
        {
            neighbouringTiles[0] = tileIds[mainTileIndex - numberOfTiles];
        }

        // Bottom neighbor
        if (row != numberOfTiles - 1)
        {
            neighbouringTiles[1] = tileIds[mainTileIndex + numberOfTiles];
        }

        // Left neighbor
        if (column != 0)
        {
            neighbouringTiles[2] = tileIds[mainTileIndex - 1];
        }

        // Right neighbor
        if (column != numberOfTiles - 1)
        {
            neighbouringTiles[3] = tileIds[mainTileIndex + 1];
        }

        return neighbouringTiles;
    }

    /// <summary>
    /// Gets the neighbouring puzzle tiles of a given puzzle tile.
    /// </summary>
    /// <param name="puzzleTileId">The puzzle tile ID to find neighbours for.</param>
    /// <returns>A list of neighbouring puzzle tile IDs in the format [TOP, BOTTOM, LEFT, RIGTH], Id is -1 if the neigbour doesn't exist.</returns>
    private List<int> GetNeighbouringPuzzleTiles(int puzzleTileId)
    {
        return GetNeighbouringTiles(puzzleTileIds, puzzleTileId);
    }

    /// <summary>
    /// Gets the neighbouring grid tiles of a given grid tile.
    /// </summary>
    /// <param name="gridTileId">The grid tile ID to find neighbours for.</param>
    /// <returns>A list of neighbouring grid tile IDs in the format [TOP, BOTTOM, LEFT, RIGTH], Id is -1 if the neigbour doesn't exist.</returns>
    private List<int> GetNeighbouringGridTiles(int gridTileId)
    {
        return GetNeighbouringTiles(gridTileIds, gridTileId);
    }

    /// <summary>
    /// Counts the number of correct neighbouring tiles for a given puzzle tile.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <param name="puzzleTileId">The puzzle tile ID to check.</param>
    /// <param name="snappedGridTileId">The snapped grid tile ID.</param>
    /// <returns>The number of correct neighbouring tiles.</returns>
    private int CountCorrectNeighbours(Player player, int puzzleTileId, int snappedGridTileId)
    {
        List<int> neighbouringPuzzleTiles = GetNeighbouringPuzzleTiles(puzzleTileId);
        List<int> neighbouringGridTiles = GetNeighbouringGridTiles(snappedGridTileId);

        int numberOfCorrectNeighbours = 0;

        for (int i = 0; i < 4; i++)
        {
            int neighbouringPuzzleTileId = neighbouringPuzzleTiles[i];
            int neighbouringGridTileId = neighbouringGridTiles[i];

            // If the neighbouring puzzle tile exists, it checks whether its snapped grid tiles equals 
            // the center tile's grid neigbour in the same direction
            if (neighbouringPuzzleTileId != -1)
            {
                int neighbouringPuzzleTileSnappedGridTileId = player.GetPuzzleTileSnappedGridTile(neighbouringPuzzleTileId);

                if (neighbouringPuzzleTileSnappedGridTileId != -1 && neighbouringPuzzleTileSnappedGridTileId == neighbouringGridTileId)
                {
                    numberOfCorrectNeighbours += 1;
                }
            }
        }

        return numberOfCorrectNeighbours;
    }

    /// <summary>
    /// Updates the player's progress based on the number of correctly placed neighbouring tiles.
    /// </summary>
    /// <param name="player">The player whose progress is being updated.</param>
    private void UpdatePlayerProgress(Player player)
    {
        int numberOfTiles = (int)Math.Sqrt(puzzleTileIds.Count);

        // Formula for calculating total number of neighbours for n tiles (numberOfTiles):
        // n: 8 [Corner tiles] + (n-2) * 4 * 3  [Edge tiles] + (n-2)^2 * 4 [Center tiles]
        int numberOfNeighbours = 8 + (numberOfTiles - 2) * 12 + (int)Math.Pow(numberOfTiles - 2, 2) * 4;
        int numberOfCorrectNeighbours = 0;

        foreach (int puzzleTileId in puzzleTileIds)
        {
            int snappedGridTileId = player.GetPuzzleTileSnappedGridTile(puzzleTileId);

            // If the puzzle tile is snapped to a grid tile
            if (snappedGridTileId != -1)
            {
                numberOfCorrectNeighbours += CountCorrectNeighbours(player, puzzleTileId, snappedGridTileId);
            }
        }

        float fractionProgress = (float)numberOfCorrectNeighbours / numberOfNeighbours;
        int progress = (int)(fractionProgress * 100);

        // If the progress has chaned, it will later update the leaderboard
        bool progressChanged = player.Progress != progress;

        player.Progress = progress;

        if (progressChanged && !player.HasPower("Solo Leveling"))
        {
            leaderboardManager.UpdateRanking(player);
        }

        UpdateProgressTextRpc(player.ClientId, progress);
    }

    /// <summary>
    /// Updates the progress text display for the player using RPC.
    /// </summary> 
    /// <param name="clientId">The ID of the client to update.</param>
    /// <param name="progress">The progress percentage to display.</param>
    [Rpc(SendTo.ClientsAndHost)]
    public void UpdateProgressTextRpc(ulong clientId, int progress)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            progressText.text = $"{progress} %";
        }
    }

    // -----------------------------------------------------------------------
    // Holding Tiles
    // -----------------------------------------------------------------------

    /// <summary>
    /// Starts holding a tile on the client.
    /// </summary>
    /// <param name="puzzleTileId">The unique ID of the puzzle tile.</param>
    public void StartHoldingTileOnClient(int puzzleTileId)
    {
        StartHoldingTileRpc(NetworkManager.Singleton.LocalClientId, puzzleTileId);
    }

    /// <summary>
    /// RPC to start holding a tile on the server.
    /// </summary>
    /// <param name="clientId">The unique ID of the client.</param>
    /// <param name="puzzleTileId">The unique ID of the puzzle tile.</param>
    [Rpc(SendTo.Server)]
    private void StartHoldingTileRpc(ulong clientId, int puzzleTileId)
    {
        StartHoldingTile(playerManager.FindPlayerByClientId(clientId), puzzleTileId);
    }

    /// <summary>
    /// Starts holding a tile for the specified player.
    /// </summary>
    /// <param name="player">The player holding the puzzle tile.</param>
    /// <param name="puzzleTileId">The unique ID of the puzzle tile.</param>
    private void StartHoldingTile(Player player, int puzzleTileId)
    {
        Player tileOwnerPlayer = player.OwnerOfPuzzleCurrentlyManipulating;

        if (player.HeldPuzzleTileId != -1 || !player.HasPuzzleTileMovementPermission || tileOwnerPlayer.IsTileHeldByAnotherPlayer(puzzleTileId))
        {
            return;
        }

        DisableColliderForOtherPlayerManipulating(player, puzzleTileId);

        player.HeldPuzzleTileId = puzzleTileId;
        MovePuzzleTileToFront(player, puzzleTileId);

        if (tileOwnerPlayer.GetPuzzleTileSnappedGridTile(puzzleTileId) != -1)
        {
            UnsnapTileFromGrid(player, puzzleTileId);
        }
    }

    /// <summary>
    /// Stops holding a tile on the client.
    /// </summary>
    /// <param name="puzzleTileId">The unique ID of the puzzle tile.</param>
    public void StopHoldingTileOnClient(int puzzleTileId)
    {
        StopHoldingTileRpc(NetworkManager.Singleton.LocalClientId, puzzleTileId);
    }

    /// <summary>
    /// RPC to stop holding a tile on the server.
    /// </summary>
    /// <param name="clientId">The unique ID of the client.</param>
    /// <param name="puzzleTileId">The unique ID of the puzzle tile.</param>
    [Rpc(SendTo.Server)]
    public void StopHoldingTileRpc(ulong clientId, int puzzleTileId)
    {
        StopHoldingTile(playerManager.FindPlayerByClientId(clientId), puzzleTileId);
    }

    /// <summary>
    /// Stops holding a tile for the specified player.
    /// </summary>
    /// <param name="player">The player holding the puzzle tile.</param>
    /// <param name="puzzleTileId">The unique ID of the puzzle tile.</param>
    public void StopHoldingTile(Player player, int puzzleTileId)
    {
        if (player.HeldPuzzleTileId == puzzleTileId)
        {
            player.HeldPuzzleTileId = -1;
            SnapTileToGrid(player, puzzleTileId);

            if (player.IsPeeking)
            {
                EnableColliderForOtherPlayerManipulating(player, puzzleTileId);
            }
        }
    }

    /// <summary>
    /// Moves the specified puzzle tile to the front (highest Z-order).
    /// </summary>
    /// <param name="player">The player holding the puzzle tile.</param>
    /// <param name="heldPuzzleTileId">The unique ID of the held puzzle tile.</param>
    public void MovePuzzleTileToFront(Player player, int heldPuzzleTileId)
    {
        Player tileOwnerPlayer = player.OwnerOfPuzzleCurrentlyManipulating;

        Vector3 currentPosition = tileOwnerPlayer.GetPuzzleTilePosition(heldPuzzleTileId);
        foreach (int puzzleTileId in puzzleTileIds)
        {
            if (tileOwnerPlayer.GetPuzzleTilePosition(puzzleTileId).z < currentPosition.z)
            {
                Vector3 newPosition = tileOwnerPlayer.GetPuzzleTilePosition(puzzleTileId) + new Vector3(0, 0, 1);
                tileOwnerPlayer.ModifyPuzzleTilePosition(puzzleTileId, newPosition);
                UpdateTilePositionForPlayers(player, puzzleTileId, newPosition);
            }
        }

        Vector3 newHeldPosition = new(currentPosition.x, currentPosition.y, 1);
        tileOwnerPlayer.ModifyPuzzleTilePosition(heldPuzzleTileId, newHeldPosition);
        UpdateTilePositionForPlayers(player, heldPuzzleTileId, newHeldPosition);
    }

    // -----------------------------------------------------------------------
    // Snapping to Grid
    // -----------------------------------------------------------------------

    /// <summary>
    /// Snaps a puzzle tile to the nearest grid tile if it fits within the boundaries and the grid tile is unoccupied.
    /// </summary>
    /// <param name="player">The player interacting with the puzzle tile.</param>
    /// <param name="puzzleTileId">The unique ID of the puzzle tile.</param>
    private void SnapTileToGrid(Player player, int puzzleTileId)
    {
        Player tileOwnerPlayer = player.OwnerOfPuzzleCurrentlyManipulating;

        if (!tileOwnerPlayer.SnapToGridEnabled)
        {
            return;
        }

        Vector3 puzzleTilePosition = tileOwnerPlayer.GetPuzzleTilePosition(puzzleTileId);
        Vector2 puzzleTile2DPosition = new(puzzleTilePosition.x, puzzleTilePosition.y);

        // Temporary solution to calculate tile size.
        float tileSize = gridTilesByPositions.Keys.ToList()[1].y - gridTilesByPositions.Keys.ToList()[0].y;
        Vector2 puzzleTileCenter = new(puzzleTile2DPosition.x + tileSize / 2, puzzleTile2DPosition.y + tileSize / 2);

        foreach (var kvp in gridTilesByPositions)
        {
            Vector2 gridTile2DPosition = kvp.Key;
            int gridTileId = kvp.Value;

            float maxX = gridTile2DPosition.x + tileSize;
            float maxY = gridTile2DPosition.y + tileSize;

            if (!tileOwnerPlayer.GetGridTileOccupied(gridTileId) &&
                puzzleTileCenter.x > gridTile2DPosition.x &&
                puzzleTileCenter.y > gridTile2DPosition.y &&
                puzzleTileCenter.x < maxX &&
                puzzleTileCenter.y < maxY)
            {
                tileOwnerPlayer.ModifyPuzzleTileSnappedGridTile(puzzleTileId, gridTileId);
                tileOwnerPlayer.ModifyGridTileOccupied(gridTileId, true);

                Vector3 newPosition = new(gridTile2DPosition.x, gridTile2DPosition.y, puzzleTilePosition.z);
                tileOwnerPlayer.ModifyPuzzleTilePosition(puzzleTileId, newPosition);

                UpdateGridForPuzzleTile(tileOwnerPlayer, puzzleTileId, gridTileId);
                UpdatePlayerProgress(tileOwnerPlayer);

                UpdateTilePositionForPlayers(player, puzzleTileId, newPosition);
                break;
            }
        }
    }

    /// <summary>
    /// Unsnap a puzzle tile from its current grid tile, marking the grid tile as unoccupied.
    /// </summary>
    /// <param name="player">The player interacting with the puzzle tile.</param>
    /// <param name="puzzleTileId">The unique ID of the puzzle tile.</param>
    public void UnsnapTileFromGrid(Player player, int puzzleTileId)
    {
        Player tileOwnerPlayer = player.OwnerOfPuzzleCurrentlyManipulating;

        int snappedGridTileId = tileOwnerPlayer.GetPuzzleTileSnappedGridTile(puzzleTileId);
        tileOwnerPlayer.ModifyGridTileOccupied(snappedGridTileId, false);
        tileOwnerPlayer.ModifyGridTileCorrectlyOccupied(snappedGridTileId, false);
        tileOwnerPlayer.ModifyPuzzleTileSnappedGridTile(puzzleTileId, -1);
        UpdatePlayerProgress(tileOwnerPlayer);
    }
}