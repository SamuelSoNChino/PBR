using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleTile : MonoBehaviour
{
    public Vector3 offset;
    private Vector3 originalPosition;
    public bool isDragging = false;
    public bool isSelected = false;
    public int correctX;
    public int correctY;
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
                currentTile.UpdateStatus(correctX, correctY);
                snappedTo = currentTile;
            }
        }
    }
    
    private void OnMouseDown()
    {
        if (Input.touchCount == 1 || Input.GetMouseButtonDown(0)) // 2nd condition for PC testing
        {
            PutOnTop();
            originalPosition = transform.position;

            offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);

            isSelected = true;
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
        if (isDragging && Input.touchCount < 2) // For PC debugging, on touch device should be == 1
        {
            transform.parent.GetComponent<TilesManager>().MoveSelected();
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;
        GameObject.Find("Main Camera").GetComponent<PanZoom>().tileDragging = false;

        if (transform.position == originalPosition)
        {
            isSelected = true;
            offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        SnapToGrid();
        if (transform.parent.parent.Find("Grid").GetComponent<GridManager>().CheckComplete())
        {
            transform.parent.parent.GetComponent<GameState>().EndGame();
        }
    }


}
