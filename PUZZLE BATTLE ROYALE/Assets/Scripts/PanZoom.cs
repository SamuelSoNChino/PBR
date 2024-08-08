using UnityEngine;

/// <summary>
/// The <c>PanZoom</c> class handles the camera's panning and zooming functionality of the game.
/// It allows the player to move the camera by dragging and to zoom in and out using a pinch gesture.
/// </summary>
public class PanZoom : MonoBehaviour
{
    /// <summary>
    /// The starting position of the camera.
    /// </summary>
    [SerializeField] private Vector3 startingPos;

    /// <summary>
    /// The scale increment for zooming.
    /// </summary>
    [SerializeField] private float incrementScale;

    /// <summary>
    /// The starting zoom level.
    /// </summary>
    [SerializeField] private int startingZoom;

    /// <summary>
    /// The maximum zoom level.
    /// </summary>
    [SerializeField] private float maxZoom;

    /// <summary>
    /// The minimum zoom level.
    /// </summary>
    [SerializeField] private float minZoom;

    /// <summary>
    /// The bottom-left boundary of the background.
    /// </summary>
    [SerializeField] private Vector3 bottomLeftBound;

    /// <summary>
    /// The top-right boundary of the background.
    /// </summary>
    [SerializeField] private Vector3 topRightBound;



    /// <summary>
    /// The initial touch position for panning.
    /// </summary>
    private Vector3 touchStart;

    /// <summary>
    /// Indicates whether any tile is currently being dragged (moved).
    /// </summary>
    private bool holdingTile = false;

    /// <summary>
    /// Determines if player can Pan and Zoom. False by default, needs to be enabled before the start of the game.
    /// </summary>
    private bool touchInputEnabled = false;

    /// <summary>
    /// Initializes the starting zoom and position of the camera.
    /// </summary>
    void Start()
    {
        // Initializes the starting zoom and position of the camera
        Camera.main.orthographicSize = startingZoom;
        Camera.main.transform.position = startingPos;
    }

    /// <summary>
    /// Handles the zoom functionality using a pinch gesture.
    /// </summary>
    /// <param name="touch0">The first touch input.</param>
    /// <param name="touch1">The second touch input.</param>
    void Zoom(Touch touch0, Touch touch1)
    {
        // Calculates previous touch positions
        Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
        Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

        // Calculates the difference between the current and previous magnitude (vector length)
        float prevMagnitude = (touch0PrevPos - touch1PrevPos).magnitude;
        float currMagnitude = (touch0.position - touch1.position).magnitude;
        float difference = currMagnitude - prevMagnitude;

        // Scales the difference using the incrementScale constant
        float increment = incrementScale * difference;

        // Sets the new size (zoom) of the camera, ensuring it stays within minZoom and maxZoom
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - increment, minZoom, maxZoom);
    }

    /// <summary>
    /// Handles the panning functionality, allowing the player to move the camera by dragging.
    /// </summary>
    /// <param name="start">The initial position where the drag started.</param>
    /// <param name="end">The position where the drag ended.</param>
    void Pan(Vector3 start, Vector3 end)
    {
        // Simple calculation of the new position
        Vector3 direction = start - end;
        Vector3 newPosition = Camera.main.transform.position + direction;

        // Calculates camera dimensions
        float cameraHalfHeight = Camera.main.orthographicSize;
        float cameraHalfWidth = cameraHalfHeight * Camera.main.aspect;

        // Calculates max and min possible values for camera position based on background bounds
        float minX = bottomLeftBound.x + cameraHalfWidth;
        float maxX = topRightBound.x - cameraHalfWidth;
        float minY = bottomLeftBound.y + cameraHalfHeight;
        float maxY = topRightBound.y - cameraHalfHeight;

        // Ensures the new position stays within background bounds
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
        Camera.main.transform.position = newPosition;
    }

    /// <summary>
    /// Updates the camera position and zoom level based on user input.
    /// </summary>
    void Update()
    {
        // Checks if player can pan and zoom
        if (touchInputEnabled)
        {
            // Triggers on MouseDown when the player isn't dragging a puzzle tile, stores the initial position for Pan
            if (Input.GetMouseButtonDown(0) && !holdingTile)
            {
                touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }

            // Triggers if the player tries to zoom by pinching gesture
            if (Input.touchCount == 2)
            {
                Zoom(Input.GetTouch(0), Input.GetTouch(1));
            }
            // Triggers when the player is dragging without holding a puzzle tile
            else if (Input.GetMouseButton(0) && !holdingTile)
            {
                // Pan using touchStart and current mouse position
                Pan(touchStart, Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }
        }

    }

    /// <summary>
    /// Checks if any tile is currently being dragged.
    /// </summary>
    /// <returns>True if any tile is being dragged, false otherwise.</returns>
    public bool IsHoldingTile()
    {
        return holdingTile;
    }

    /// <summary>
    /// Sets the dragging state of any tile.
    /// </summary>
    /// <param name="newState">The new dragging state.</param>
    public void SetHoldingTile(bool newState)
    {
        holdingTile = newState;
    }

    /// <summary>
    /// Enables Pan and Zoom for the player.
    /// </summary>
    public void EnableTouchInput()
    {
        touchInputEnabled = true;
    }

    /// <summary>
    /// Disable Pan and Zoom for the player.
    /// </summary>
    public void DisableTouchInput()
    {
        touchInputEnabled = false;
    }
}