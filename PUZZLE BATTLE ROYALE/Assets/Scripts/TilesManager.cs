using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TilesManager : MonoBehaviour
{
    public bool tileDragging = false;
    public void MoveSelected()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            PuzzleTile tile = child.GetComponent<PuzzleTile>();
            if (tile.isSelected)
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3 childOffset = tile.offset;
                child.position = new Vector3(mousePosition.x + childOffset.x, mousePosition.y + childOffset.y, child.transform.position.z);
                if (tile.snappedGridTile)
                {
                    tile.snappedGridTile.status = 0;
                    tile.snappedGridTile = null;
                }
            }
        }
    }
    public void DeselectAllTiles()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.GetComponent<PuzzleTile>().isSelected = false;
            child.localScale = new Vector3(1, 1, 1); // Experiment for selection visualization
        }
    }
    public void CalculateOffsets()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.GetComponent<PuzzleTile>().offset = child.transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }
    public void PutTileOnTop(float zValue)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Vector3 childPos = transform.GetChild(i).transform.position;
            if (childPos.z == zValue) 
            {
                Vector3 newChildPos = new Vector3(childPos.x, childPos.y, 1f);
                transform.GetChild(i).transform.position = newChildPos;
            } else if (childPos.z < zValue)
            {
                Vector3 newChildPos = new Vector3(childPos.x, childPos.y, childPos.z + 1);
                transform.GetChild(i).transform.position = newChildPos;
            }
        }
    }
    public void SnapTileToGrid(Transform tileTransform)
    {
        PuzzleTile puzzleTile = tileTransform.GetComponent<PuzzleTile>();
        SpriteRenderer tileSRenderer = tileTransform.GetComponent<SpriteRenderer>();

        Transform grid = GameObject.Find("Grid").transform;
        float gridZ = grid.GetChild(0).position.z; // Becuase tiles have all different z coords, while grid has a constant z coord
        Vector3 tileCenter = new Vector3(tileSRenderer.bounds.center.x, tileSRenderer.bounds.center.y, gridZ);
        
        for (int j = 0; j < grid.childCount; j++)
        {
            Transform gridChild = grid.GetChild(j);
            SpriteRenderer gridSRenderer = gridChild.GetComponent<SpriteRenderer>();
            GridTile gridTile = gridChild.GetComponent<GridTile>();

            if (gridSRenderer.bounds.Contains(tileCenter) && gridTile.status == 0)
            {
                tileTransform.position = new Vector3(gridChild.position.x, gridChild.position.y, tileTransform.position.z);
                puzzleTile.snappedGridTile = gridTile;

                gridTile.UpdateStatus(puzzleTile.indexX, puzzleTile.indexY);
                break; // Doesnt need to continue as it can snap only to a single grid tile
            }
        }

        if (GameObject.Find("Grid").GetComponent<GridManager>().CheckComplete()) // Checks for game completeness
        {
            GameObject.Find("Puzzle").GetComponent<GameState>().EndGame();
        }
    }
    public void SnapSelectedToGrid()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform tileChild = transform.GetChild(i);
            PuzzleTile puzzleTile = tileChild.GetComponent<PuzzleTile>();
            if (puzzleTile.isSelected)
            {
                SnapTileToGrid(tileChild);
            }  
        }
    }
}
