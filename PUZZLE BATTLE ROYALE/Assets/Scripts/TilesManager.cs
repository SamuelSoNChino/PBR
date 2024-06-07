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
    /// Shuffles all tiles randomly on the area specified by ShuffleBounds and
    /// also shuffles their z values by randomly moving tile to the front.
    /// </summary>
    public void ShuffleAllTiles(int seed = 0)
    {
        // If the seed was set during the call, initializates the rng state to the seed
        if (seed != 0)
        {
            Random.InitState(seed);
        }
        

        // Calculates approxmate size of a single tile
        int tileSize = puzzleSize / PlayerPrefs.GetInt("numberOfTiles");

        // Calculates possible x values for the tiles 
        float minX = bottomLeftShuffleBound.x;
        float maxX = topRightShuffleBound.x - tileSize;

        // Calculates possible y values for the tiles 
        float minY = bottomLeftShuffleBound.y;
        float maxY = topRightShuffleBound.y - tileSize;

        // Iterates through each puzzle tile
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            PuzzleTile puzzleTile = child.GetComponent<PuzzleTile>();

            // Randolmy generates x and y values in the shuffle area, z can be set to 0 since puzzleTile.Move ignores Z values
            Vector3 newPosition = new(Random.Range(minX, maxX), Random.Range(minY, maxY), 0);

            // Moves to tile to the new position
            puzzleTile.Move(newPosition);
        }

        // Repeat the random MoveToFront for the number of puzzle tiles
        for (int _ = 0; _ < transform.childCount; _++)
        {
            // Randomly chooses a puzzle tile and moves it to the front
            int childIndex = Random.Range(0, transform.childCount);
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
        // Iterates through each puzzle tile
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            PuzzleTile puzzleTile = child.GetComponent<PuzzleTile>();
            // Moves each selected tile to the mouse position
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
        // Iterates through each puzzle tile
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
        // Iterates through each puzzle tile
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.GetComponent<PuzzleTile>().CalculateMouseOffset();
        }
    }

    /// <summary>
    /// Snaps all selected tiles to the grid.
    /// </summary>
    public void SnapSelectedToGrid()
    {
        // Iterates through each puzzle tile
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform tileChild = transform.GetChild(i);
            PuzzleTile puzzleTile = tileChild.GetComponent<PuzzleTile>();
            // Snaps each selected tile to the grid
            if (puzzleTile.IsSelected())
            {
                puzzleTile.SnapToGrid();
            }
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
    /// Disables all the colliders to prevent moving the tiles.
    /// </summary>
    public void DisableAllColliders()
    {
        // Iterates through each puzzle tile
        for (int i = 0; i < transform.childCount; i++)
        {
            // Disables the collider
            Transform tileChild = transform.GetChild(i);
            BoxCollider2D collider = tileChild.GetComponent<BoxCollider2D>();
            collider.enabled = false;
        }
    }

    /// <summary>
    /// Enables all the colliders to allow moving the tiles.
    /// </summary>
    public void EnableAllColliders()
    {
        // Iterates through each puzzle tile
        for (int i = 0; i < transform.childCount; i++)
        {
            // Enables the collider
            Transform tileChild = transform.GetChild(i);
            BoxCollider2D collider = tileChild.GetComponent<BoxCollider2D>();
            collider.enabled = true;
        }
    }


    /// <summary>
    /// Deactivates all the colliders to prevent moving the tiles.
    /// </summary>
    public void DestroyAllTiles()
    {
        // Iterates through each puzzle tile
        for (int i = 0; i < transform.childCount; i++)
        {
            // Destroys the child
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}