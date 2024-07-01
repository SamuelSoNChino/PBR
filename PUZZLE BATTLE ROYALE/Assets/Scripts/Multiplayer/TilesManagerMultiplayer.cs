using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages all the puzzle tiles in the scene, handling their mass selection, movement, and snapping to the grid.
/// </summary>
public class TilesManagerMultiplayer : MonoBehaviour
{

    /// |---------------------------------|
    /// |            UNIVERSAL            |
    /// |---------------------------------|

    [SerializeField] private PuzzleManager puzzleManager;

    public List<GameObject> GetAllPuzzleTiles()
    {
        List<GameObject> puzzleTiles = new();
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject puzzleTileObject = transform.GetChild(i).gameObject;
            puzzleTiles.Add(puzzleTileObject);
        }
        return puzzleTiles;
    }

    public GameObject FindTileById(int tileId)
    {
        foreach (GameObject puzzleTileObject in GetAllPuzzleTiles())
        {
            PuzzleTileMultiplayer puzzleTile = puzzleTileObject.GetComponent<PuzzleTileMultiplayer>();
            if (puzzleTile.GetId() == tileId)
            {
                return puzzleTileObject;
            }
        }
        return null;
    }

    /// |---------------------------------|
    /// |             CLIENT              |
    /// |---------------------------------|

    /// <summary>
    /// Disables all the colliders to prevent manipulating the tiles.
    /// </summary>
    public void DisableAllColliders()
    {
        // Iterate through each puzzle tile
        foreach (GameObject puzzleTileObject in GetAllPuzzleTiles())
        {
            // Disable the collider
            PuzzleTileMultiplayer puzzleTile = puzzleTileObject.GetComponent<PuzzleTileMultiplayer>();
            puzzleTile.DisableCollider();
        }
    }

    /// <summary>
    /// Enables all the colliders to allow manipulating the tiles.
    /// </summary>
    public void EnableAllColliders()
    {
        // Iterate through each puzzle tile
        foreach (GameObject puzzleTileObject in GetAllPuzzleTiles())
        {
            PuzzleTileMultiplayer puzzleTile = puzzleTileObject.GetComponent<PuzzleTileMultiplayer>();
            puzzleTile.EnableCollider();
        }
    }

    /// <summary>
    /// Moves all selected tiles to the current mouse position.
    /// </summary>
    public void MoveSelectedToMouse()
    {
        // Iterate through each puzzle tile
        foreach (GameObject puzzleTileObject in GetAllPuzzleTiles())
        {
            PuzzleTileMultiplayer puzzleTile = puzzleTileObject.GetComponent<PuzzleTileMultiplayer>();

            // Move each selected tile to the mouse position
            if (puzzleTile.IsSelected())
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3 childOffset = puzzleTile.GetMouseOffset();
                puzzleTile.Move(mousePosition, childOffset);
            }
        }
    }

    /// <summary>
    /// Deselects all puzzle tiles.
    /// </summary>
    public void DeselectAllTiles()
    {
        // Iterate through each puzzle tile
        foreach (GameObject puzzleTileObject in GetAllPuzzleTiles())
        {
            PuzzleTileMultiplayer puzzleTile = puzzleTileObject.GetComponent<PuzzleTileMultiplayer>();
            puzzleTile.DeselectTile();
        }
    }

    /// <summary>
    /// Calculates the mouse offsets for all puzzle tiles.
    /// </summary>
    public void CalculateAllMouseOffsets()
    {
        // Iterate through each puzzle tile
        foreach (GameObject puzzleTileObject in GetAllPuzzleTiles())
        {
            PuzzleTileMultiplayer puzzleTile = puzzleTileObject.GetComponent<PuzzleTileMultiplayer>();
            puzzleTile.CalculateMouseOffset();
        }
    }

    /// <summary>
    /// Snaps all tiles to the grid.
    /// </summary>
    public void SnapAllToGrid()
    {
        // Iterate through each puzzle tile
        foreach (GameObject puzzleTileObject in GetAllPuzzleTiles())
        {
            PuzzleTileMultiplayer puzzleTile = puzzleTileObject.GetComponent<PuzzleTileMultiplayer>();

            // Snap each tile to the grid
            puzzleTile.SnapToGrid();
        }
    }

    /// <summary>
    /// Unsnaps all tiles from the grid.
    /// </summary>
    public void UnsnapAllFromGrid()
    {
        // Iterate through each puzzle tile
        foreach (GameObject puzzleTileObject in GetAllPuzzleTiles())
        {
            PuzzleTileMultiplayer puzzleTile = puzzleTileObject.GetComponent<PuzzleTileMultiplayer>();

            // Unsnaps each tile to the grid
            puzzleTile.UnsnapFromGrid();
        }
    }
}