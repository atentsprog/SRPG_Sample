using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum BlockType
{
    None
    , Walkable  = 1 << 0
    , Water     = 1 << 1
    , Player    = 1 << 2
    , Enemy     = 1 << 3
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

        Player.SelectPlayer.MoveTo(transform.position.ToIntVector2());
    }
}
