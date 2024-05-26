using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TilesManager : MonoBehaviour
{
    private bool anyTileDragging = false;
    public void MoveSelected()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            PuzzleTile tile = child.GetComponent<PuzzleTile>();
            if (tile.IsSelected())
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3 childOffset = tile.GetMouseOffset();
                tile.Move(mousePosition, childOffset);
                if (tile.GetSnappedGridTile())
                {
                    tile.ClearSnappedGridTile();
                }
            }
        }
    }
    public void DeselectAllTiles()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.GetComponent<PuzzleTile>().DeselectTile();
            child.localScale = new Vector3(1, 1, 1); // Experiment for selection visualization
        }
    }
    public void CalculateAllMouseOffsets()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.GetComponent<PuzzleTile>().CalculateMouseOffset();
        }
    }
    public void SnapSelectedToGrid()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform tileChild = transform.GetChild(i);
            PuzzleTile puzzleTile = tileChild.GetComponent<PuzzleTile>();
            if (puzzleTile.IsSelected())
            {
                puzzleTile.SnapToGrid();
            }
        }
    }
    public bool IsAnyTileDragging()
    {
        return anyTileDragging;
    }
    public void SetAnyTileDragging(bool newState)
    {
        anyTileDragging = newState;
    }
}
