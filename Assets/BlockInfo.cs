using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockType
{
    Walkable,
    Water,
}
public class BlockInfo : MonoBehaviour
{
    public BlockType blockType;

    Vector3 downMousePosition;
    public float clickDistance = 1;
    void OnMouseDown()
    {
        downMousePosition = Input.mousePosition;
    }
    void OnMouseUp()
    {
        var upMousePosition = Input.mousePosition;
        if (Vector3.Distance(downMousePosition, upMousePosition) > clickDistance)
        {
            return;
        }
        GroundManager.Instance.OnTouch(transform.position);
    }
}
