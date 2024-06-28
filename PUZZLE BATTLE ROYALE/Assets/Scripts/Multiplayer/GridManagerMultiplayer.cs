using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles all of the grid tiles in the scene for a multiplayer game.
/// </summary>
public class GridManagerMultiplayer : MonoBehaviour
{
    /// <summary>
    /// Reference to the MultiplayerManager script.
    /// </summary>
    [SerializeField] private MultiplayerManager multiplayerManager;

    /// <summary>
    /// Reference to the PeekManager script.
    /// </summary>
    [SerializeField] private PeekManager peekManager;

    /// <summary>
    /// Finds a grid tile by its unique ID.
    /// </summary>
    /// <param name="tileId">The ID of the tile to find.</param>
    /// <returns>The GameObject of the grid tile if found, otherwise null.</returns>
    public GameObject FindTileById(int tileId)
    {
        // Iterates through all the grid tiles
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform gridTileObject = transform.GetChild(i);
            GridTileMultiplayer gridTile = gridTileObject.GetComponent<GridTileMultiplayer>();

            // If the corresponding grid tile is found, return it
            if (gridTile.GetId() == tileId)
            {
                return gridTileObject.gameObject;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets all grid tiles in the scene.
    /// </summary>
    /// <returns>A list of all grid tile GameObjects.</returns>
    public List<GameObject> GetAllGridTiles()
    {
        // Prepares a list for all the grid tiles, which will be returned in the end
        List<GameObject> gridTiles = new();

        // Iterates through all the grid tiles
        for (int i = 0; i < transform.childCount; i++)
        {
            // Adds the gridTileObject to the list
            GameObject gridTileObject = transform.GetChild(i).gameObject;
            gridTiles.Add(gridTileObject);
        }
        return gridTiles;
    }

    /// <summary>
    /// Checks if all grid tiles have correctly placed puzzle tiles for a specific client.
    /// </summary>
    /// <param name="clientId">The ID of the client to check the completeness for.</param>
    public void CheckCompleteness(ulong clientId)
    {
        // Assumes that all the grid tiles have a correct puzzle tile placed on them
        bool allCorrect = true;

        // Iterates through all grid tiles
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform gridTileObject = transform.GetChild(i);
            GridTileMultiplayer gridTile = gridTileObject.GetComponent<GridTileMultiplayer>();

            // Checks whether the grid tile has a correctly placed puzzle tile
            if (!gridTile.GetClientStatus(clientId))
            {
                allCorrect = false;
                break;
            }
        }

        // If all are placed correctly, end the game
        if (allCorrect)
        {
            multiplayerManager.EndGame(clientId);
        }
    }
}
