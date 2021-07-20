using DG.Tweening;
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

    static Vector3 downMousePosition;
    const float clickDistance = 1;
    void OnMouseDown()
    {
        downMousePosition = Input.mousePosition;
    }
    void OnMouseUp()
    {
        var upMousePosition = Input.mousePosition;
        if (Vector3.Distance(downMousePosition, upMousePosition) > clickDistance)
        {
            return; //클릭이 아니라 드래그 이므로 리턴
        }

        //만약 지금 있는 칸에 플레이어가 있다면 플레이어의 움직일 수 있는 영역을 표시하자.
        var point = transform.position.ToIntVector2();
        if (GroundManager.Instance.map[point].HasFlag(BlockType.Player))
        {
            // 이 좌표에 있는 플레이어를 선택된 플레이어로 지정하자
            // var SelectPlayer = 여기서 선택
            // 우선 임시로 SelectPlayer에 바로 지정하자.
            Player.SelectPlayer.SetSelectPlayer();
        }
        else
        { 
            Player.SelectPlayer.MoveTo(transform.position.ToIntVector2());
        }
    }
}
