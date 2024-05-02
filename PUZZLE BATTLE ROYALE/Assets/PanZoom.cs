using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanZoom : MonoBehaviour
{
    Vector3 touchStart;
    public bool tileDragging = false;
    public float maxZoom;
    public float minZoom;
    public float incrementScale;
    public GameObject background;
    Vector3 boundLD;
    Vector3 boundRU;
    public int startingZoom;
    public Vector3 startingPos;
    void Start()
    {
        Camera.main.orthographicSize = startingZoom;
        Camera.main.transform.position = startingPos;
        boundLD = background.GetComponent<SpriteRenderer>().bounds.min;
        boundRU = background.GetComponent<SpriteRenderer>().bounds.max;
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

            float prevMagnitude = (touch0PrevPos - touch1PrevPos).magnitude;
            float currMagnitude = (touch0.position - touch1.position).magnitude;

            float difference = currMagnitude - prevMagnitude;
            Zoom(difference * incrementScale);
        }
        else if (Input.GetMouseButton(0) && !tileDragging)
        {
            print(tileDragging);
            Vector3 direction = touchStart - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 newPosition = Camera.main.transform.position + direction;
            float cameraHalfHeight = Camera.main.orthographicSize;
            float cameraHalfWidth = cameraHalfHeight * Camera.main.aspect;

            float minX = boundLD.x + cameraHalfWidth;
            float maxX = boundRU.x - cameraHalfWidth;
            float minY = boundLD.y + cameraHalfHeight;
            float maxY = boundRU.y - cameraHalfHeight;
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
            Camera.main.transform.position = newPosition;
        }
    }
    void Zoom(float increment)
    {
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - increment, minZoom, maxZoom);
    }
}
