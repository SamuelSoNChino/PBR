using System.Collections.Generic;
using UnityEngine;

public class Player
{
    // Basic Info

    private string name;
    public string Name
    {
        get { return name; }
        set { name = value; }
    }

    private ulong clientId;
    public ulong ClientId
    {
        get { return clientId; }
        set { clientId = value; }
    }



    // Skins

    private int backgroundSkinIndex;
    public int BackgroundSkinIndex
    {
        get { return backgroundSkinIndex; }
        set { backgroundSkinIndex = value; }
    }



    // Abilities

    // TODO



    // Puzzle Tiles

    private bool hasPlayerMovementPermission = false;

    public bool HasPlayerMovementPermission
    {
        get { return hasPlayerMovementPermission; }
        set { hasPlayerMovementPermission = value; }
    }


    private Dictionary<int, Vector3> puzzleTilesPositions = new();

    public void InitializePuzzleTilePosition(int puzzleTileId, Vector3 position)
    {
        puzzleTilesPositions.Add(puzzleTileId, position);
    }

    public void ModifyPuzzleTilePosition(int puzzleTileId, Vector3 newPosition)
    {
        puzzleTilesPositions[puzzleTileId] = newPosition;
    }

    public Vector3 GetPuzzleTilePosition(int puzzleTileId)
    {
        return puzzleTilesPositions[puzzleTileId];
    }



    private Dictionary<int, int> puzzleTilesSnappedGridTiles = new();

    public void InitializePuzzleTileSnappedGridTile(int puzzleTileId)
    {
        puzzleTilesSnappedGridTiles.Add(puzzleTileId, -1);
    }

    public void ModifyPuzzleTileSnappedGridTile(int puzzleTileId, int gridTileId)
    {
        puzzleTilesSnappedGridTiles[puzzleTileId] = gridTileId;
    }

    public int GetPuzzleTileSnappedGridTile(int puzzleTileId)
    {
        return puzzleTilesSnappedGridTiles[puzzleTileId];
    }



    // Grid Tiles

    private Dictionary<int, bool> gridTilesCorrectlyOccupied = new();

    public void InitializeGridTileCorrectlyOccupied(int gridTileId)
    {
        gridTilesCorrectlyOccupied.Add(gridTileId, false);
    }

    public void ModifyGridTileCorrectlyOccupied(int gridTileId, bool newState)
    {
        gridTilesCorrectlyOccupied[gridTileId] = newState;
    }

    public bool GetGridTileCorrectlyOccupied(int gridTileId)
    {
        return gridTilesCorrectlyOccupied[gridTileId];
    }



    // Peeking

    // TODO
}