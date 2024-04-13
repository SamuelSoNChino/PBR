using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleTile : MonoBehaviour
{
    private Vector3 offset;
    private bool isDragging = false;

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
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.nearClipPlane;
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }

    private void OnMouseDown()
    {
        PutOnTop();
        offset = transform.position - GetMouseWorldPosition();
        isDragging = true;
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
    }


}
