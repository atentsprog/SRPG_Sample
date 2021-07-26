﻿using System;
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
        //ClearMoveableArea();
        downMousePosition = Input.mousePosition;
    }


    void OnMouseUp()
    {
        var upMousePosition = Input.mousePosition;
        if (Vector3.Distance(downMousePosition, upMousePosition) > clickDistance)
        {
            return;
        }

        switch (StageManager.GameState)
        {
            case GameStateType.SelectPlayer:
                SelectPlayer();
                break;
            case GameStateType.SelectedPlayerMoveOrAct:
                SelectedPlayerMoveOrAct();
                break;
            case GameStateType.SelectToAttackTarget:  //이동후에 공격할 타겟을 선택. 공격할 타겟이 없다면 SelectPlayer로 변경
                SelectToAttackTarget();
                break;

            default:
            case GameStateType.NotInit:
            case GameStateType.IngPlayerMove:
            case GameStateType.MonsterTurn:
                Debug.Log($"블럭을 클릭할 수 없는 상태 입니다:" +
                    $"{StageManager.GameState}");

                break;
        }
    }

    /// <summary>
    /// 이동후에 공격할 타겟일 수 있는 블럭을 선택했음.
    /// 블락에 공격할 타겟이 있고 공격 가능한 타겟이면 공격
    /// </summary>
    private void SelectToAttackTarget()
    {
        if (Player.SelectedPlayer.enemyExistPoint.Contains(this))
        {
            if (Player.SelectedPlayer.CanAttackTarget(actor))
            {
                Player.SelectedPlayer.AttackToTarget(actor);
            }
            else
            {
                //대상을 공격할 수 없습니다.
                NotifyUI.Instance.Show("대상을 공격할 수 없습니다.");
            }
        }
    }

    private void SelectedPlayerMoveOrAct()
    {
        // 공격 대상이 있다면 공격 하자.(액터가 몬스터라면)
        if (actor)
        {
            //현재 공격 하는 액터가 공격 대상으로 할 수 있는지 확인.
            // todo:공격 범위 안에 있는지 확인해줘야함.
            if (Player.SelectedPlayer.CanAttackTarget(actor))
            {
                ClearMoveableArea();
                Player.SelectedPlayer.AttackToTarget(actor);
            }
            else
            {
                //대상을 공격할 수 없습니다.
                NotifyUI.Instance.Show("대상을 공격할 수 없습니다.");
            }
        }
        else
        {
            // 플레이어 이동블락 클릭
            if (highLightedMoveableArea.Contains(this))
            {
                Player.SelectedPlayer.ClearEnemyExistPoint();
                Player.SelectedPlayer.MoveToPosition(transform.position);
                ClearMoveableArea();
                StageManager.GameState = GameStateType.IngPlayerMove;
            }
        }
    }

    private void SelectPlayer()
    {
        if (actor == null)
            return;

        if(actor.GetType() == typeof(Player))
        {
            //Player.SelectedPlayer = actor as Player;
            Player player = (Player)actor;
            if(player.CompleteTurn)
            {
                CenterNotifyUI.Instance.Show(
                    "모든 행동이 끝난 플레이어 입니다");
                return;
            }

            Player.SelectedPlayer = player;

            //이동 가능한 영역 표시.
            if (player.completeMove == false)
                ShowMoveableArea(Player.SelectedPlayer.moveDistance);
            else
            {
                ToChangeBlueColor();
                highLightedMoveableArea.Add(this);
                CenterNotifyUI.Instance.Show("이미 이동했습니다");
            }


            // 현재 위치에서 공격 가능한 영역 표시.
            if ( player.completeAct == false)
                Player.SelectedPlayer.ShowAttackableArea();
            else
                CenterNotifyUI.Instance.Show("이미 행동했습니다");

            StageManager.GameState = GameStateType.SelectedPlayerMoveOrAct;
        }
    }

    private void ShowMoveableArea(int moveDistance)
    {
        Quaternion rotate = Quaternion.Euler(0, 45, 0);
        Vector3 halfExtents = (moveDistance / Mathf.Sqrt(2)) * 0.99f * Vector3.one;

        var blocks = Physics.OverlapBox(transform.position
            , halfExtents, rotate);

        foreach (var item in blocks)
        {
            if (Player.SelectedPlayer.OnMoveable(item.transform.position, moveDistance))
            {
                var block = item.GetComponent<BlockInfo>();
                if (block && block.actor == null)
                {
                    block.ToChangeBlueColor();
                    highLightedMoveableArea.Add(block);
                }
            }
        }
    }
    static List<BlockInfo> highLightedMoveableArea = new List<BlockInfo>();
    public static void ClearMoveableArea()
    {
        highLightedMoveableArea.ForEach(x => x.ToChangeOriginalColor());
        highLightedMoveableArea.Clear();
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

        var intPos = transform.position.ToVector2Int();
        name = $"{name} {intPos.x}:{intPos.y}";

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
    private Color moveableColor = Color.blue;
    private Color m_OriginalColor;
    private void Awake()
    {
        m_Renderer = GetComponentInChildren<Renderer>();
        m_OriginalColor = m_Renderer.material.color;
    }
    void OnMouseOver()
    {
        if (actor)
        {
            ActorStatusUI.Instance.Show(actor);
        }
    }

    public void ToChangeBlueColor()
    {
        m_Renderer.material.color = moveableColor;
    }
    public void ToChangeOriginalColor()
    {
        m_Renderer.material.color = m_OriginalColor;
    }
    internal void ToChangeColor(Color color)
    {
        m_Renderer.material.color = color;
    }

    void OnMouseExit()
    {
        if (actor)
        {
            ActorStatusUI.Instance.Close();
        }
    }
}
