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
    /// Gets a list of all grid tiles in the scene.
    /// </summary>
    /// <returns>A list of all grid tile game objects.</returns>
    public List<GameObject> GetAllGridTiles()
    {
        // Prepares the list for all the grid tiles
        List<GameObject> gridTiles = new();

        // Iterates through all the grid tiles
        for (int i = 0; i < transform.childCount; i++)
        {
            // Adds the grid tile to the list
            GameObject gridTileObject = transform.GetChild(i).gameObject;
            gridTiles.Add(gridTileObject);
        }
        return gridTiles;
    }

    /// <summary>
    /// Finds a grid tile by its ID.
    /// </summary>
    /// <param name="tileId">The ID of the grid tile to find.</param>
    /// <returns>The grid tile game object if found; otherwise, null.</returns>
    public GameObject FindTileById(int tileId)
    {
        // Iterates through all the grid tiles
        foreach (GameObject gridTileObject in GetAllGridTiles())
        {
            GridTileMultiplayer gridTile = gridTileObject.GetComponent<GridTileMultiplayer>();

            // If the corresponding grid tile is found, returns it
            if (gridTile.GetId() == tileId)
            {
                return gridTileObject.gameObject;
            }
        }

        // Otherwise returns null
        return null;
    }

    /// <summary>
    /// Finds a grid tile by its 2D position.
    /// </summary>
    /// <param name="position2D">The 2D position of the grid tile to find.</param>
    /// <returns>The grid tile game object if found; otherwise, null.</returns>
    public GameObject FindTileBy2DPosition(Vector2 position2D)
    {
        // Iterates through all the grid tiles
        foreach (GameObject gridTileObject in GetAllGridTiles())
        {
            GridTileMultiplayer gridTile = gridTileObject.GetComponent<GridTileMultiplayer>();
            Vector2 gridTilePosition2D = new Vector2(gridTile.GetPosition().x, gridTile.GetPosition().y);

            // If the corresponding grid tile is found, returns it
            if (gridTilePosition2D == position2D)
            {
                return gridTileObject;
            }
        }

        // Otherwise returns null
        return null;
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
        foreach (GameObject gridTileObject in GetAllGridTiles())
        {
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