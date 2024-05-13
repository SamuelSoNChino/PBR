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
            if (child.GetComponent<PuzzleTile>().isSelected)
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3 childOffset = child.GetComponent<PuzzleTile>().offset;
                print(childOffset);
                child.position = new Vector3(mousePosition.x + childOffset.x, mousePosition.y + childOffset.y, child.transform.position.z);
            }
        }
    }
    public void DeselectAllTiles()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.GetComponent<PuzzleTile>().isSelected = false;
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
}
