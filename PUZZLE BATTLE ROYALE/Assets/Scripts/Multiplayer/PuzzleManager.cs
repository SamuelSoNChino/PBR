using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

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

            if (player.HasPuzzleTileMovementPermission && player.HeldPuzzleTileId == puzzleTileId)
            {
                targetOfPeekPlayer.ModifyPuzzleTilePosition(puzzleTileId, newPosition);

                //TODO Vymislioet
                //UpdateGridForPuzzleTile(targetOfPeekPlayer, puzzleTileId);
            }
            else
            {
                SetNewTilePositionRpc(clientId, puzzleTileId, targetOfPeekPlayer.GetPuzzleTilePosition(puzzleTileId));
            }
        }
        else
        {
            if (player.HasPuzzleTileMovementPermission && player.HeldPuzzleTileId == puzzleTileId)
            {
                player.ModifyPuzzleTilePosition(puzzleTileId, newPosition);
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
    /// <returns>A list of neighbouring tile IDs in the format [TOP, BOTTOM, LEFT, RIGTH], Id is -1 if the neigbour doesn't exist.</returns>
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

        if (progressChanged)
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
    /// Notifies the server that a player has started holding a puzzle tile.
    /// </summary>
    /// <param name="clientId">The client ID of the player.</param>
    /// <param name="puzzleTileId">The ID of the puzzle tile being held.</param>
    [Rpc(SendTo.Server)]
    private void StartHoldingTileRpc(ulong clientId, int puzzleTileId)
    {
        Player player = playerManager.FindPlayerByClientId(clientId);

        Debug.Log($"Client: {clientId} started holding tile: {puzzleTileId}");

        if (player.HeldPuzzleTileId == -1)
        {
            player.HeldPuzzleTileId = puzzleTileId;

            MovePuzzleTileToFront(player, puzzleTileId);

            if (player.GetPuzzleTileSnappedGridTile(puzzleTileId) != -1)
            {
                UnsnapTileFromGrid(player, puzzleTileId);
            }
        }
    }

    /// <summary>
    /// Starts holding a puzzle tile by the local player.
    /// </summary>
    /// <param name="puzzleTileId">The ID of the puzzle tile to be held.</param>
    public void StartHoldingTile(int puzzleTileId)
    {
        StartHoldingTileRpc(NetworkManager.Singleton.LocalClientId, puzzleTileId);
    }

    /// <summary>
    /// Notifies the server that a player has stopped holding a puzzle tile.
    /// </summary>
    /// <param name="clientId">The client ID of the player.</param>
    /// <param name="puzzleTileId">The ID of the puzzle tile that was being held.</param>
    [Rpc(SendTo.Server)]
    public void StopHoldingTileRpc(ulong clientId, int puzzleTileId)
    {
        Player player = playerManager.FindPlayerByClientId(clientId);

        Debug.Log($"Client: {clientId} stopped holding tile: {puzzleTileId}");

        if (player.HeldPuzzleTileId == puzzleTileId)
        {
            player.HeldPuzzleTileId = -1;

            SnapTileToGrid(player, puzzleTileId);
        }
    }

    /// <summary>
    /// Stops holding a puzzle tile by the local player.
    /// </summary>
    /// <param name="puzzleTileId">The ID of the puzzle tile to stop holding.</param>
    public void StopHoldingTile(int puzzleTileId)
    {
        StopHoldingTileRpc(NetworkManager.Singleton.LocalClientId, puzzleTileId);
    }

    /// <summary>
    /// Moves the held puzzle tile to the front of the player's view, bringing it to the foreground.
    /// </summary>
    /// <param name="player">The player holding the tile.</param>
    /// <param name="heldPuzzleTileId">The ID of the held puzzle tile.</param>
    public void MovePuzzleTileToFront(Player player, int heldPuzzleTileId)
    {
        Vector3 currentPosition = player.GetPuzzleTilePosition(heldPuzzleTileId);

        foreach (int puzzleTileId in puzzleTileIds)
        {
            if (player.GetPuzzleTilePosition(puzzleTileId).z < currentPosition.z)
            {
                Vector3 newPosition = player.GetPuzzleTilePosition(puzzleTileId) + new Vector3(0, 0, 1);
                SetNewTilePositionRpc(player.ClientId, puzzleTileId, newPosition);
            }
        }

        Vector3 newHeldPosition = new(currentPosition.x, currentPosition.y, 1);
        SetNewTilePositionRpc(player.ClientId, heldPuzzleTileId, newHeldPosition);
    }

    // -----------------------------------------------------------------------
    // Snapping to Grid
    // -----------------------------------------------------------------------

    /// <summary>
    /// Snaps a puzzle tile to the closest grid tile if it is within the snapping range.
    /// </summary>
    /// <param name="player">The player whose tile is being snapped.</param>
    /// <param name="puzzleTileId">The ID of the puzzle tile to snap.</param>
    private void SnapTileToGrid(Player player, int puzzleTileId)
    {
        Vector3 puzzleTilePosition = player.GetPuzzleTilePosition(puzzleTileId);
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

            if (!player.GetGridTileOccupied(gridTileId) &&
                puzzleTileCenter.x > gridTile2DPosition.x &&
                puzzleTileCenter.y > gridTile2DPosition.y &&
                puzzleTileCenter.x < maxX &&
                puzzleTileCenter.y < maxY)
            {
                player.ModifyPuzzleTileSnappedGridTile(puzzleTileId, gridTileId);
                player.ModifyGridTileOccupied(gridTileId, true);

                Vector3 newPosition = new(gridTile2DPosition.x, gridTile2DPosition.y, puzzleTilePosition.z);
                player.ModifyPuzzleTilePosition(puzzleTileId, newPosition);
                SetNewTilePositionRpc(player.ClientId, puzzleTileId, newPosition);

                UpdateGridForPuzzleTile(player, puzzleTileId, gridTileId);

                UpdatePlayerProgress(player);

                break;
            }
        }
    }

    /// <summary>
    /// Unsnap a puzzle tile from its current grid tile, freeing the grid tile for other tiles.
    /// </summary>
    /// <param name="player">The player whose tile is being unsnapped.</param>
    /// <param name="puzzleTileId">The ID of the puzzle tile to unsnap.</param>
    private void UnsnapTileFromGrid(Player player, int puzzleTileId)
    {
        player.ModifyGridTileOccupied(player.GetPuzzleTileSnappedGridTile(puzzleTileId), false);
        player.ModifyPuzzleTileSnappedGridTile(puzzleTileId, -1);
        UpdatePlayerProgress(player);
    }

    /// <summary>
    /// Snaps all puzzle tiles of the player to the grid.
    /// </summary>
    /// <param name="player">The player whose tiles are being snapped.</param>
    public void SnapAllTilesToGrid(Player player)
    {
        foreach (int puzzleTileId in puzzleTileIds)
        {
            SnapTileToGrid(player, puzzleTileId);
        }
    }

    /// <summary>
    /// Unsnap all puzzle tiles of the player from the grid.
    /// </summary>
    /// <param name="player">The player whose tiles are being unsnapped.</param>
    public void UnsnapAllTilesFromGrid(Player player)
    {
        foreach (int puzzleTileId in puzzleTileIds)
        {
            UnsnapTileFromGrid(player, puzzleTileId);
        }
    }

    /// <summary>
    /// Resets the snapped state of all puzzle tiles for a player, reapplying snapping logic.
    /// </summary>
    /// <param name="clientId">The client ID of the player.</param>
    public void ResetPlayerSnappedTiles(ulong clientId)
    {
        Player player = playerManager.FindPlayerByClientId(clientId);

        UnsnapAllTilesFromGrid(player);
        SnapAllTilesToGrid(player);
    }

    /// <summary>
    /// Updates the grid state for a specific puzzle tile based on its position.
    /// </summary>
    /// <param name="player">The player object.</param>
    /// <param name="puzzleTileId">The unique ID of the puzzle tile.</param>
    /// <param name="snappedGridTileId">The ID of the grid tile the puzzle tile is snapped to.</param>
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
}