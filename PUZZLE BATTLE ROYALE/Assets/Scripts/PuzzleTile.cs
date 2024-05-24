using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Tilemaps;

public class PuzzleTile : MonoBehaviour
{
    private Vector3 offset;
    private Vector3 originalPosition;
    private bool isSelected = false;
    private int indexX;
    private int indexY;
    private GridTile snappedGridTile;
    private TilesManager tilesManager;

    private void Start()
    {
        tilesManager = transform.parent.GetComponent<TilesManager>();
    }
    public bool IsSelected()
    {
        return isSelected;
    }
    public void DeselectTile()
    {
        isSelected = false;
    }
    public Vector3 GetOffset()
    {
        return offset;
    }
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
    public GridTile GetSnappedGridTile()
    {
        return snappedGridTile;
    }
    public void SetSnappedGridTile(GridTile newGridTile)
    {
        snappedGridTile = newGridTile;
    }
    public void ClearSnappedGridTile()
    {
        snappedGridTile.SetStatus(0);
        snappedGridTile = null;
    }
    public int[] GetIndexes()
    {
        int[] output = {indexX, indexY};
        return output;
    }
    public void SetIndexes(int x, int y)
    {
        indexX = x;
        indexY = y;
    }
    private void OnMouseDown()
    {
        if (Input.touchCount == 1 || Input.GetMouseButtonDown(0)) // 2nd condition for PC testing
        {
            tilesManager.PutTileOnTop(transform.position.z);
            originalPosition = transform.position;
            tilesManager.SetTileDragging(true);
            if (isSelected) // Calculates offsets for all selected, otherwise only for itself
            {
                tilesManager.CalculateOffsets();
            }
            else 
            {
                offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
        }
    }

    private void OnMouseDrag()
    {
        if (Input.touchCount < 2) // For PC debugging, on touch device should be == 1
        {
            if (isSelected) // When dragging selected, move all selected
            {
                tilesManager.MoveSelected();
            }
            else // Else move only this one 
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                transform.position = new Vector3(mousePosition.x + offset.x, mousePosition.y + offset.y, transform.position.z);

                if (snappedGridTile)
                {
                    snappedGridTile.SetStatus(0);
                    snappedGridTile = null;
                }
            }
        }
    }

    private void OnMouseUp()
    {
        tilesManager.SetTileDragging(false);

        if (transform.position == originalPosition) // Triggers only when the tile was dragged, but not if only clicked
        {
            if(isSelected) // Experiment for selection visualization
            {
                transform.localScale = new Vector3(1, 1, 1);
            } else
            {
                transform.localScale = new Vector3(0.95f, 0.95f, 1);
            }
            isSelected = !isSelected; // Switches selected status
        }
        else 
        {
            if (isSelected) // Snaps all selected without changing anything else
            {
                tilesManager.SnapSelectedToGrid();
            }
            else // If wasnt selected deselectes all selected and snaps itself to grid
            {
                tilesManager.DeselectAllTiles();
                tilesManager.SnapTileToGrid(transform);
            }
        }
    }


}
