using System;
using UnityEngine;

/// <summary>
/// Manages all the puzzle tiles in the scene, handling their mass selection, movement, and snapping to the grid.
/// </summary>
public class TilesManager : MonoBehaviour
{
    /// <summary>
    /// Indicates whether any tile is currently being dragged (moved).
    /// </summary>
    private bool anyTileDragging = false;

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
    public void ShuffleAllTiles(int seed = 0)
    {
        // If the seed was set during the call, initialize the RNG state to the seed
        if (seed != 0)
        {
            UnityEngine.Random.InitState(seed);
        }

        // Calculate approximate size of a single tile
        int tileSize = puzzleSize / (int)Math.Sqrt(transform.childCount);

        // Calculate possible x values for the tiles 
        float minX = bottomLeftShuffleBound.x;
        float maxX = topRightShuffleBound.x - tileSize;

        // Calculate possible y values for the tiles 
        float minY = bottomLeftShuffleBound.y;
        float maxY = topRightShuffleBound.y - tileSize;

        // Iterate through each puzzle tile
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            PuzzleTile puzzleTile = child.GetComponent<PuzzleTile>();

            // Randomly generate x and y values in the shuffle area, z can be set to 0 since puzzleTile.Move ignores Z values
            Vector3 newPosition = new(UnityEngine.Random.Range(minX, maxX), UnityEngine.Random.Range(minY, maxY), 0);

            // Move the tile to the new position
            puzzleTile.Move(newPosition);
        }

        // Repeat the random MoveToFront for the number of puzzle tiles
        for (int _ = 0; _ < transform.childCount; _++)
        {
            // Randomly choose a puzzle tile and move it to the front
            int childIndex = UnityEngine.Random.Range(0, transform.childCount);
            Transform child = transform.GetChild(childIndex);
            PuzzleTile puzzleTile = child.GetComponent<PuzzleTile>();
            puzzleTile.MoveToFront();
        }
    }

    /// <summary>
    /// Moves all selected tiles to the current mouse position.
    /// </summary>
    public void MoveSelectedToMouse()
    {
        // Iterate through each puzzle tile
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            PuzzleTile puzzleTile = child.GetComponent<PuzzleTile>();

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
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.GetComponent<PuzzleTile>().DeselectTile();
        }
    }

    /// <summary>
    /// Calculates the mouse offsets for all puzzle tiles.
    /// </summary>
    public void CalculateAllMouseOffsets()
    {
        // Iterate through each puzzle tile
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.GetComponent<PuzzleTile>().CalculateMouseOffset();
        }
    }

    /// <summary>
    /// Snaps all tiles to the grid.
    /// </summary>
    public void SnapAllToGrid()
    {
        // Iterate through each puzzle tile
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform tileChild = transform.GetChild(i);
            PuzzleTile puzzleTile = tileChild.GetComponent<PuzzleTile>();

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
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform tileChild = transform.GetChild(i);
            PuzzleTile puzzleTile = tileChild.GetComponent<PuzzleTile>();

            // Unsnaps each tile to the grid
            puzzleTile.UnsnapFromGrid();

        }
    }

    /// <summary>
    /// Checks if any tile is currently being dragged.
    /// </summary>
    /// <returns>True if any tile is being dragged, false otherwise.</returns>
    public bool IsAnyTileDragging()
    {
        return anyTileDragging;
    }

    /// <summary>
    /// Sets the dragging state of any tile.
    /// </summary>
    /// <param name="newState">The new dragging state.</param>
    public void SetAnyTileDragging(bool newState)
    {
        anyTileDragging = newState;
    }

    /// <summary>
    /// Disables all the colliders to prevent manipulating the tiles.
    /// </summary>
    public void DisableAllColliders()
    {
        // Iterate through each puzzle tile
        for (int i = 0; i < transform.childCount; i++)
        {
            // Disable the collider
            Transform tileChild = transform.GetChild(i);
            PuzzleTile puzzleTile = tileChild.GetComponent<PuzzleTile>();
            puzzleTile.DisableCollider();
        }
    }

    /// <summary>
    /// Enables all the colliders to allow manipulating the tiles.
    /// </summary>
    public void EnableAllColliders()
    {
        // Iterate through each puzzle tile
        for (int i = 0; i < transform.childCount; i++)
        {
            // Enable the collider
            Transform tileChild = transform.GetChild(i);
            PuzzleTile puzzleTile = tileChild.GetComponent<PuzzleTile>();
            puzzleTile.EnableCollider();
        }
    }

    /// <summary>
    /// Gets the positions of all puzzle tiles.
    /// </summary>
    /// <returns>An array of positions for all puzzle tiles.</returns>
    public Vector3[] GetAllPositions()
    {
        // Prepare an array for storing all the positions 
        Vector3[] allPositions = new Vector3[transform.childCount];

        // Iterate through each puzzle tile
        for (int i = 0; i < transform.childCount; i++)
        {
            // Save the position of the puzzle tile to the array
            Transform tileChild = transform.GetChild(i);
            PuzzleTile puzzleTile = tileChild.GetComponent<PuzzleTile>();
            allPositions[i] = puzzleTile.GetPosition();
        }
        return allPositions;
    }

    /// <summary>
    /// Sets the positions of all puzzle tiles.
    /// </summary>
    /// <param name="newPositions">An array of new positions for all puzzle tiles.</param>
    public void SetAllPositions(Vector3[] newPositions)
    {
        // Iterate through each puzzle tile
        for (int i = 0; i < transform.childCount; i++)
        {
            // Set a new position for the puzzle tile from the newPositions array
            Transform tileChild = transform.GetChild(i);
            PuzzleTile puzzleTile = tileChild.GetComponent<PuzzleTile>();
            puzzleTile.SetPosition(newPositions[i]);
        }
    }

    /// <summary>
    /// Destroys all puzzle tiles.
    /// </summary>
    public void DestroyAllTiles()
    {
        // Iterate through each puzzle tile
        for (int i = 0; i < transform.childCount; i++)
        {
            // Destroy the child
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}