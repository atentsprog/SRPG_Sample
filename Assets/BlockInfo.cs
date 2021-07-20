using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Flags]
public enum BlockType
{
    None        = 0,
    Walkable    = 1 << 0,
    Water       = 1 << 1,
    Player      = 1 << 2,
    Monster     = 1 << 3,
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
        Player.SelectPlayer.OnTouch(transform.position);
    }

    string debugTextPrefab = "DebugTextPrefab";
    GameObject debugTextGos;
    internal void UpdateDebugInfo()
    {
        if (debugTextGos == null)
        {
            GameObject textMeshGo = Instantiate((GameObject)Resources.Load(debugTextPrefab), transform);
            debugTextGos = textMeshGo;
            textMeshGo.transform.localPosition = Vector3.zero;
        }

        StringBuilder debugText = new StringBuilder();// $"{item.blockType}:{intPos.y}";
                                                      //ContaingText(debugText, item, BlockType.Walkable);
        ContaingText(debugText, BlockType.Water);
        ContaingText(debugText, BlockType.Player);
        ContaingText(debugText, BlockType.Monster);

        GetComponentInChildren<TextMesh>().text = debugText.ToString();
    }
    private void ContaingText(StringBuilder sb, BlockType walkable)
    {
        if (blockType.HasFlag(walkable))
        {
            sb.AppendLine(walkable.ToString());
        }
    }
}
