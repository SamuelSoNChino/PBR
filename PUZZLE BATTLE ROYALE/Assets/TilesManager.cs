using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TilesManager : MonoBehaviour
{
    // Start is called before the first frame update
    public void MoveSelected()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.GetComponent<PuzzleTile>().isSelected)
            {
                child.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + child.GetComponent<PuzzleTile>().offset;
            }
        }
    }
    public void DeselectAllTiles()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.AddComponent<PuzzleTile>().isSelected = false;
        }
    }
}
