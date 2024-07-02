using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PuzzleManager : NetworkBehaviour
{
    [SerializeField] private TilesManagerMultiplayer tilesManager;
    [SerializeField] private GridManagerMultiplayer gridManager;



    /// |---------------------------------|
    /// |           Tile IDs              |
    /// |---------------------------------|

    private List<int> puzzleTileIds;
    private List<int> gridTileIds;

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

    public void GenerateNewPuzzleKey(int numberOfTiles)
    {
        puzzleTileIds = GenerateIds(numberOfTiles * numberOfTiles);
        gridTileIds = GenerateIds(numberOfTiles * numberOfTiles);
    }

    public List<int> GetPuzzleTileIds()
    {
        return new List<int>(puzzleTileIds);
    }

    public List<int> GetGridTileIds()
    {
        return new List<int>(gridTileIds);
    }



    /// |---------------------------------|
    /// |       Grid Tiles Checking       |
    /// |---------------------------------|

    public void UpdateGridForTile(ulong clientId, int puzzleTileId)
    {
        PuzzleTileMultiplayer puzzleTile = tilesManager.FindTileById(puzzleTileId).GetComponent<PuzzleTileMultiplayer>();
        Vector3 position = puzzleTile.GetClientPosition(clientId);

        Vector2 position2D = new(position.x, position.y);
        GameObject gridTileObject = gridManager.FindTileBy2DPosition(position2D);
        if (gridTileObject != null)
        {
            GridTileMultiplayer gridTile = gridTileObject.GetComponent<GridTileMultiplayer>();

            if (CheckTiles(puzzleTileId, gridTile.GetId()))
            {
                gridTile.ModifyClientStatus(clientId, true);
                gridManager.CheckCompleteness(clientId);
                puzzleTile.ModifyClientSnappeedGridTile(clientId, gridTileObject);
                return;
            }
        }

        if (puzzleTile.GetClientSnappedGridTile(clientId) != null)
        {
            GridTileMultiplayer gridTile = puzzleTile.GetClientSnappedGridTile(clientId).GetComponent<GridTileMultiplayer>();
            gridTile.ModifyClientStatus(clientId, false);
            puzzleTile.ModifyClientSnappeedGridTile(clientId, null);
        }

    }

    public bool CheckTiles(int puzzleTileId, int gridTileId)
    {
        int puzzleTileIdIndex = puzzleTileIds.IndexOf(puzzleTileId);
        return gridTileId == gridTileIds[puzzleTileIdIndex];
    }



    /// |---------------------------------|
    /// |   Client Info Initialization    |
    /// |---------------------------------|

    public void InitializeClientPositions()
    {
        List<GameObject> puzzleTileObjects = tilesManager.GetAllPuzzleTiles();
        foreach (GameObject puzzleTileObject in puzzleTileObjects)
        {
            PuzzleTileMultiplayer puzzleTile = puzzleTileObject.GetComponent<PuzzleTileMultiplayer>();
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                puzzleTile.InitializeClientPosition(clientId);
            }
        }
    }

    public void InitializeClientSnappedGridTiles()
    {
        List<GameObject> puzzleTileObjects = tilesManager.GetAllPuzzleTiles();
        foreach (GameObject puzzleTileObject in puzzleTileObjects)
        {
            PuzzleTileMultiplayer puzzleTile = puzzleTileObject.GetComponent<PuzzleTileMultiplayer>();
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                puzzleTile.InitializeClientSnappedGridTile(clientId);
            }
        }
    }

    public void InitializeClientStatuses()
    {
        List<GameObject> gridTileObjects = gridManager.GetAllGridTiles();
        foreach (GameObject gridTileObject in gridTileObjects)
        {
            GridTileMultiplayer gridTile = gridTileObject.GetComponent<GridTileMultiplayer>();
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                gridTile.InitializeClientStatus(clientId);
            }
        }
    }

    public void InitializeClientMovementPermissions()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            InitializeClientMovementPermission(clientId);
        }
    }



    /// |---------------------------------|
    /// |          Tile Shuffle           |
    /// |---------------------------------|

    /// <summary>
    /// Top right bound of the area where tiles can be shuffled.
    /// </summary>
    [SerializeField] private Vector3 topRightShuffleBound;

    /// <summary>
    /// Bottom left bound of the area where tiles can be shuffled.
    /// </summary>
    [SerializeField] private Vector3 bottomLeftShuffleBound;

    /// <summary>
    /// The size of the whole puzzle image .
    /// </summary>
    [SerializeField] private int puzzleSize;


    /// <summary>
    /// Shuffles all tiles randomly within the shuffle bounds and also shuffles their z-values.
    /// </summary>
    /// <param name="seed">Optional seed for random number generation.</param>
    public void ShuffleAllTiles(int seed)
    {
        UnityEngine.Random.InitState(seed);

        // Calculate approximate size of a single tile
        int tileSize = puzzleSize / (int)Math.Sqrt(tilesManager.GetAllPuzzleTiles().Count);

        // Calculate possible x values for the tiles 
        float minX = bottomLeftShuffleBound.x;
        float maxX = topRightShuffleBound.x - tileSize;

        // Calculate possible y values for the tiles 
        float minY = bottomLeftShuffleBound.y;
        float maxY = topRightShuffleBound.y - tileSize;

        // Iterate through each puzzle tile
        foreach (GameObject puzzleTileObject in tilesManager.GetAllPuzzleTiles())
        {
            PuzzleTileMultiplayer puzzleTile = puzzleTileObject.GetComponent<PuzzleTileMultiplayer>();

            float zPosition = puzzleTile.GetPosition().z;

            // Randomly generates x and y values in the shuffle area, z can be set to 0 since puzzleTile.Move ignores Z values
            Vector3 newPosition = new(UnityEngine.Random.Range(minX, maxX), UnityEngine.Random.Range(minY, maxY), zPosition);

            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                puzzleTile.ModifyClientPosition(clientId, newPosition);
                SetNewTilePositionRpc(clientId, puzzleTile.GetId(), newPosition);
            }
        }
    }



    /// |---------------------------------|
    /// |         Tile Movement           |
    /// |---------------------------------|]

    [SerializeField] private PeekManager peekManager;

    public void UpdateServerPosition(int tileId, Vector3 newPosition)
    {
        UpdateServerPositionRpc(NetworkManager.Singleton.LocalClientId, tileId, newPosition);
    }

    [Rpc(SendTo.Server)]
    public void UpdateServerPositionRpc(ulong clientId, int tileId, Vector3 newPosition)
    {
        PuzzleTileMultiplayer puzzleTile = tilesManager.FindTileById(tileId).GetComponent<PuzzleTileMultiplayer>();

        if (peekManager.IsClientPeeking(clientId))
        {
            ulong targetOfPeekClientId = peekManager.GetTargetOfPeekUser(clientId);
            if (GetClientMovementPermission(clientId))
            {
                puzzleTile.ModifyClientPosition(targetOfPeekClientId, newPosition);
            }
            else
            {
                SetNewTilePositionRpc(clientId, tileId, puzzleTile.GetClientPosition(targetOfPeekClientId));
            }
        }
        else
        {
            if (GetClientMovementPermission(clientId))
            {
                puzzleTile.ModifyClientPosition(clientId, newPosition);
                UpdateGridForTile(clientId, tileId);
            }
            else
            {
                SetNewTilePositionRpc(clientId, puzzleTile.GetId(), puzzleTile.GetClientPosition(clientId));
            }
        }

    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetNewTilePositionRpc(ulong clientId, int tileId, Vector3 newPosition)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            PuzzleTileMultiplayer puzzleTile = tilesManager.FindTileById(tileId).GetComponent<PuzzleTileMultiplayer>();
            puzzleTile.SetPosition(newPosition);
        }
    }


    /// |---------------------------------|
    /// |    Tile Movement Permissions    |
    /// |---------------------------------|

    private Dictionary<ulong, bool> clientMovementPermissions = new();

    public void InitializeClientMovementPermission(ulong clientId)
    {
        clientMovementPermissions.Add(clientId, false);
    }

    public void ModifyClientMovementPermission(ulong clientId, bool newPermission)
    {
        clientMovementPermissions[clientId] = newPermission;
    }

    public bool GetClientMovementPermission(ulong clientId)
    {
        return clientMovementPermissions[clientId];
    }

    public void DisableTileMovement(ulong clientId = 1234567890)
    {
        List<ulong> clientIds = new();
        if (clientId == 1234567890)
        {
            clientIds = (List<ulong>)NetworkManager.Singleton.ConnectedClientsIds;
        }
        else
        {
            clientIds.Add(clientId);
        }

        foreach (ulong targetClientId in clientIds)
        {
            DisableAllCollidersRpc(targetClientId);
            ModifyClientMovementPermission(targetClientId, false);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void DisableAllCollidersRpc(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            tilesManager.DisableAllColliders();
        }
    }

    public void EnableTileMovement(ulong clientId = 1234567890)
    {
        List<ulong> clientIds = new();
        if (clientId == 1234567890)
        {
            clientIds = (List<ulong>)NetworkManager.Singleton.ConnectedClientsIds;
        }
        else
        {
            clientIds.Add(clientId);
        }

        foreach (ulong targetClientId in clientIds)
        {
            EnableAllCollidersRpc(targetClientId);
            ModifyClientMovementPermission(targetClientId, true);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void EnableAllCollidersRpc(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            tilesManager.EnableAllColliders();
        }
    }



    /// |---------------------------------|
    /// |        PanZoom Management       |
    /// |---------------------------------|

    [SerializeField] private PanZoom panZoom;

    public void EnableTouchInput(ulong clientId = 1234567890)
    {
        List<ulong> clientIds = new();
        if (clientId == 1234567890)
        {
            clientIds = (List<ulong>)NetworkManager.Singleton.ConnectedClientsIds;
        }
        else
        {
            clientIds.Add(clientId);
        }

        foreach (ulong targetClientId in clientIds)
        {
            EnableTouchInputRpc(targetClientId);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void EnableTouchInputRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            panZoom.EnableTouchInput();
        }
    }

    public void DisableTouchInput(ulong clientId = 1234567890)
    {
        List<ulong> clientIds = new();
        if (clientId == 1234567890)
        {
            clientIds = (List<ulong>)NetworkManager.Singleton.ConnectedClientsIds;
        }
        else
        {
            clientIds.Add(clientId);
        }

        foreach (ulong targetClientId in clientIds)
        {
            DisableTouchInputRpc(targetClientId);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void DisableTouchInputRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            panZoom.DisableTouchInput();
        }
    }



    /// |---------------------------------|
    /// |       Reset Functionality       |
    /// |---------------------------------|

    public void DestroyAllTilesServer()
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



    /// |---------------------------------|
    /// |       Peek Functionality        |
    /// |---------------------------------|

    public void SetOtherClientsPositions(ulong clientId, ulong otherClientId)
    {
        foreach (GameObject puzzTileObject in tilesManager.GetAllPuzzleTiles())
        {
            PuzzleTileMultiplayer puzzleTile = puzzTileObject.GetComponent<PuzzleTileMultiplayer>();
            SetNewTilePositionRpc(clientId, puzzleTile.GetId(), puzzleTile.GetClientPosition(otherClientId));
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void DeselectAllClientTilesRpc(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            tilesManager.DeselectAllTiles();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ResetClientsSnappedTilesRpc(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            tilesManager.UnsnapAllFromGrid();
            tilesManager.SnapAllToGrid();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void UpdateGridForAllTilesRpc(ulong clientId)
    {
        foreach (GameObject puzzleTileObject in tilesManager.GetAllPuzzleTiles())
        {
            PuzzleTileMultiplayer puzzleTile = puzzleTileObject.GetComponent<PuzzleTileMultiplayer>();
            UpdateGridForTile(clientId, puzzleTile.GetId());
        }
    }
}
