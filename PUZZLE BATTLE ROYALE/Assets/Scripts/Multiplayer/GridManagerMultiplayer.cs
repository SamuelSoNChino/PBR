using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles all of the grid tiles in the scene for a multiplayer game. Is only used on client side.
/// </summary>
public class GridManagerMultiplayer : MonoBehaviour
{
    /// <summary>
    /// Gets a list of all grid tiles in the scene.
    /// </summary>
    /// <returns>A list of all grid tile game objects.</returns>
    public List<GameObject> GetAllGridTiles()
    {
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
    /// <param name="gridTileId">The ID of the grid tile to find.</param>
    /// <returns>The grid tile game object if found; otherwise, null.</returns>
    public GameObject FindTileById(int gridTileId)
    {
        foreach (GameObject gridTileObject in GetAllGridTiles())
        {
            GridTileMultiplayer gridTile = gridTileObject.GetComponent<GridTileMultiplayer>();

            if (gridTile.TileId == gridTileId)
            {
                return gridTileObject.gameObject;
            }
        }
        return null;
    }
}