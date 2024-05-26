using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PanZoom : MonoBehaviour
{
    /// <summary>
    /// The initial touch position for panning.
    /// </summary>
    private Vector3 touchStart;

    /// <summary>
    /// The maximum zoom level.
    /// </summary>
    [SerializeField] float maxZoom;

    /// <summary>
    /// The minimum zoom level.
    /// </summary>
    [SerializeField] float minZoom;

    /// <summary>
    /// The scale increment for zooming.
    /// </summary>
    [SerializeField] float incrementScale;

    /// <summary>
    /// The background game object used to set boundaries for panning.
    /// </summary>
    [SerializeField] GameObject background;

    /// <summary>
    /// The bottom-left boundary of the background.
    /// </summary>
    private Vector3 boundBL;

    /// <summary>
    /// The top-right boundary of the background.
    /// </summary>
    private Vector3 boundTR;

    /// <summary>
    /// The starting zoom level.
    /// </summary>
    [SerializeField] int startingZoom;

    /// <summary>
    /// The starting position of the camera.
    /// </summary>
    [SerializeField] Vector3 startingPos;

    /// <summary>
    /// Initializes the starting zoom and position of the camera.
    /// Sets the boundaries based on the background sprite.
    /// </summary>
    void Start()
    {
        Camera.main.orthographicSize = startingZoom;
        Camera.main.transform.position = startingPos;
        boundBL = background.GetComponent<SpriteRenderer>().bounds.min;
        boundTR = background.GetComponent<SpriteRenderer>().bounds.max;
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
        float minX = boundBL.x + cameraHalfWidth;
        float maxX = boundTR.x - cameraHalfWidth;
        float minY = boundBL.y + cameraHalfHeight;
        float maxY = boundTR.y - cameraHalfHeight;

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
        // Stores whether the player is dragging a puzzle tile
        bool tileDragging = GameObject.Find("Tiles").GetComponent<TilesManager>().IsAnyTileDragging(); 

        // Triggers on MouseDown when the player isn't dragging a puzzle tile, stores the initial position for Pan
        if (Input.GetMouseButtonDown(0) && !tileDragging) 
        {
            touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        // Triggers if the player tries to zoom by pinching gesture
        if (Input.touchCount == 2)
        {
            Zoom(Input.GetTouch(0), Input.GetTouch(1));
        }
        // Triggers when the player is dragging without holding a puzzle tile
        else if (Input.GetMouseButton(0) && !tileDragging) 
        {
            GameObject.Find("Tiles").GetComponent<TilesManager>().DeselectAllTiles();
            Pan(touchStart, Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
    }
}