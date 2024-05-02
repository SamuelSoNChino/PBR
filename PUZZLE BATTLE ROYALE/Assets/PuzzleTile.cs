using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleTile : MonoBehaviour
{
    private Vector3 offset;
    private bool isDragging = false;
    public int x;
    public int y;
    public GridTile snappedTo;

    private void PutOnTop()
    {
        float z_value = transform.position.z;
        for (int i = 0; i < transform.parent.childCount; i++)
        {
            Vector3 childPos = transform.parent.GetChild(i).transform.position;
            if (childPos.z < z_value)
            {
                childPos = new Vector3(childPos.x, childPos.y, childPos.z + 1);
                transform.parent.GetChild(i).transform.position = childPos;
            }
        }
        transform.position = new Vector3(transform.position.x, transform.position.y, 1f);
    }  
    private void SnapToGrid()
    {
        SpriteRenderer tileSRenderer = GetComponent<SpriteRenderer>();
        Transform grid = transform.parent.parent.Find("Grid");
        float grid_z = grid.GetChild(0).position.z;
        Vector3 center = new Vector3(tileSRenderer.bounds.center.x, tileSRenderer.bounds.center.y, grid_z);
        for (int i = 0; i < grid.childCount; i++)
        {
            Transform child = grid.GetChild(i);
            SpriteRenderer gridSRenderer = child.GetComponent<SpriteRenderer>();
            GridTile currentTile = child.GetComponent<GridTile>();
            if (gridSRenderer.bounds.Contains(center) && currentTile.status == 0)
            {
                transform.position = new Vector3(child.position.x, child.position.y, transform.position.z);
                currentTile.UpdateStatus(x, y);
                snappedTo = currentTile;
            }
        }
    }
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.nearClipPlane;
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }

    private void OnMouseDown()
    {
        if (Input.touchCount == 1 || Input.GetMouseButtonDown(0))
        {
            PutOnTop();
            offset = transform.position - GetMouseWorldPosition();
            isDragging = true;
            GameObject.Find("Main Camera").GetComponent<PanZoom>().tileDragging = true;
            if (snappedTo)
            {
                snappedTo.status = 0;
                snappedTo = null;
            }
        }
    }

    private void OnMouseDrag()
    {
        if (isDragging)
        {
            transform.position = GetMouseWorldPosition() + offset;
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;
        GameObject.Find("Main Camera").GetComponent<PanZoom>().tileDragging = false;
        SnapToGrid();
        if (transform.parent.parent.Find("Grid").GetComponent<Grid>().CheckComplete())
        {
            transform.parent.parent.GetComponent<GameState>().EndGame();
        }
    }


}
