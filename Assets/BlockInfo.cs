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
        if( actor && actor == Player.SelectPlayer)
        {
            // 영역 표시.
            //actor.moveDistance
            // 첫번째 이동으로 갈수 있는것을 첫번째 라인에 추가.
            ShowMoveDistance(actor.moveDistance);
        }
        else
            Player.SelectPlayer.OnTouch(transform.position);
    }

    
    private void ShowMoveDistance(int moveDistance)
    {
        Quaternion rotate = Quaternion.Euler(0, 45, 0);
        var blocks = Physics.OverlapBox(transform.position, Vector3.one * ((float)moveDistance / 2), rotate);

        foreach (var item in blocks)
        {
            if (Player.SelectPlayer.OnMoveable(item.transform.position))
                item.GetComponent<BlockInfo>()?.ToChangeRedColor();
        }
    }

    string debugTextPrefab = "DebugTextPrefab";
    GameObject debugTextGos;
    internal Actor actor;

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
    Renderer m_Renderer;
    private Color m_MouseOverColor = Color.red;
    private Color m_OriginalColor;
    private void Awake()
    {
        m_Renderer = GetComponentInChildren<Renderer>();
        m_OriginalColor = m_Renderer.material.color;
    }
    void OnMouseOver()
    {
        // Change the color of the GameObject to red when the mouse is over GameObject
        ToChangeRedColor();
        if (actor)
        {
            ActorStatusUI.Instance.Show(actor);
        }
    }

    public void ToChangeRedColor()
    {
        m_Renderer.material.color = m_MouseOverColor;
    }

    void OnMouseExit()
    {
        // Reset the color of the GameObject back to normal
        m_Renderer.material.color = m_OriginalColor;

        if (actor)
        {
            ActorStatusUI.Instance.Close();
        }
    }
}
