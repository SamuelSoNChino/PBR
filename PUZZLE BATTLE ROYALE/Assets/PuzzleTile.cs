using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PuzzleTile : MonoBehaviour
{
    public Vector3 offset;
    private Vector3 originalPosition;
    public bool isSelected = false;
    public int indexX;
    public int indexY;
    public GridTile snappedGridTile;
    private TilesManager TilesManager;

    private void Start()
    {
        TilesManager = transform.parent.GetComponent<TilesManager>();
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
                currentTile.UpdateStatus(indexX, indexY);
                snappedGridTile = currentTile;
            }
        }
    }
    
    private void OnMouseDown()
    {
        if (Input.touchCount == 1 || Input.GetMouseButtonDown(0)) // 2nd condition for PC testing
        {
            TilesManager.PutTileOnTop(transform.position.z);
            originalPosition = transform.position;
            
            TilesManager.CalculateOffsets();
            TilesManager.tileDragging = true;
            isSelected = true;

            if (snappedGridTile)
            {
                snappedGridTile.status = 0;
                snappedGridTile = null;
            }
        }
    }

    private void OnMouseDrag()
    {
        if (Input.touchCount < 2) // For PC debugging, on touch device should be == 1
        {
            TilesManager.MoveSelected();
        }
    }

    private void OnMouseUp()
    {
        TilesManager.tileDragging = false;

        if (transform.position != originalPosition)
        {
            isSelected = false;
        }

        SnapToGrid();
        if (transform.parent.parent.Find("Grid").GetComponent<GridManager>().CheckComplete())
        {
            transform.parent.parent.GetComponent<GameState>().EndGame();
        }
    }


}
