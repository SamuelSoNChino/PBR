using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Tilemaps;

public class PuzzleTile : MonoBehaviour
{
    public Vector3 offset;
    private Vector3 originalPosition;
    public bool isSelected = false;
    public int indexX;
    public int indexY;
    public GridTile snappedGridTile;
    private TilesManager tilesManager;

    private void Start()
    {
        tilesManager = transform.parent.GetComponent<TilesManager>();
    }
    
    private void OnMouseDown()
    {
        if (Input.touchCount == 1 || Input.GetMouseButtonDown(0)) // 2nd condition for PC testing
        {
            tilesManager.PutTileOnTop(transform.position.z);
            originalPosition = transform.position;
            tilesManager.tileDragging = true;
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
                    snappedGridTile.status = 0;
                    snappedGridTile = null;
                }
            }
        }
    }

    private void OnMouseUp()
    {
        tilesManager.tileDragging = false;

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
