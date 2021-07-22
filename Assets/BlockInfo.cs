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
        //HighLightedBlock = null;
        downMousePosition = Input.mousePosition;
    }

    private void ClearMoveableArea()
    {
        HighLightedMoveableArea.ForEach(x => x.ToChangeOriginalColor());
        HighLightedMoveableArea.Clear();
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
            case GameStateType.SelectMoveBlockOrAttackTarget:
                SelectMoveBlockOrAttackTarget();
                break;
            case GameStateType.AttackToTarget:
                break;

            case GameStateType.SelectToAttackTarget:
                SelectToAttackTarget();
                break;

            case GameStateType.IngPlayerMove:
            case GameStateType.NotInit:
            case GameStateType.MonsterTurn:
                Debug.Log($"블럭을 클릭할 수 없는 상태 입니다:{StageManager.GameState}");
                return;
        }

        //return;
        ///////////////////////////////////
        /////
        //bool clearAttackTarget = true;

        //if (Player.SelectedPlayer == null)
        //{
        //    if (actor && actor.actorType == Actor.ActorType.Ally)
        //        Player.SelectedPlayer = (Player)actor;
        //}
        //else
        //{
        //    HighLightedBlock = this;
        //    // 이동 가능한 영역 표시.
        //    ShowMoveDistance(actor.moveDistance);

        //    // 현재 위치에서 공격 가능한 영역 표시.
        //    actor.ShowAttackableBlock();
        //    clearAttackTarget = false;
        //}


        /////////
        //if ( actor)
        //{
        //    ////현재 공격 하는 액터가 있다면
        //    //if (Actor.IsExistAttackActor)
        //    //{
        //    //    //현재 공격 하는 액터가 공격 대상으로 할 수 있는지 확인.
        //    //    if(Actor.CanAttackTarget(actor))
        //    //    {
        //    //        Actor.CurrentAttackActor.AttackToTarget(actor);
        //    //    }

        //    //}
        //    //else
        //    {
        //        if (actor == Player.SelectedPlayer)
        //        {
        //            if (HighLightedBlock)
        //            {
        //                ClearMoveableArea();
        //                HighLightedBlock = null;
        //            }
        //            else
        //            {
        //                HighLightedBlock = this;
        //                // 이동 가능한 영역 표시.
        //                ShowMoveDistance(actor.moveDistance);

        //                // 현재 위치에서 공격 가능한 영역 표시.
        //                actor.ShowAttackableBlock();
        //                clearAttackTarget = false;
        //            }
        //        }
        //    }
        //}
        //else
        //    Player.SelectedPlayer.OnTouch(transform.position);

        //if (clearAttackTarget)
        //{
        //    Actor.CurrentAttackActor = null;
        //    Actor.ClearEnemyExistPoint();;
        //}
    }

    private void SelectToAttackTarget()
    {
        if (Actor.EnemyExistPoint.Contains(this))
        {
            if (Actor.CanAttackTarget(actor))
            {
                Actor.CurrentAttackActor.AttackToTarget(actor);
            }
        }
    }

    private void SelectMoveBlockOrAttackTarget()
    {
        if (actor)
        {
            //현재 공격 하는 액터가 공격 대상으로 할 수 있는지 확인.
            // todo:공격 범위 안에 있는지 확인해줘야함.
            if (Actor.CanAttackTarget(actor))
            {
                Actor.CurrentAttackActor.AttackToTarget(actor);
            }
        }
        else
        {
            if (HighLightedMoveableArea.Contains(this))
            {
                Player.SelectedPlayer.OnTouch(transform.position);
                ClearMoveableArea();
                StageManager.GameState = GameStateType.IngPlayerMove;
            }
        }
    }

    private void SelectPlayer()
    {
        if (actor == null)
            return;

        if (actor.actorType == Actor.ActorType.Ally)
        {
            Player.SelectedPlayer = actor as Player;

            //HighLightedBlock = this;
            // 이동 가능한 영역 표시.
            ShowMoveDistance(actor.moveDistance);

            // 현재 위치에서 공격 가능한 영역 표시.
            actor.ShowAttackableBlock();
            StageManager.GameState = GameStateType.SelectMoveBlockOrAttackTarget;
        }
    }

    private void ShowMoveDistance(int moveDistance) // ShowMoveableArea
    {
        Quaternion rotate = Quaternion.Euler(0, 45, 0);
        var blocks = Physics.OverlapBox(transform.position, Vector3.one * (moveDistance - 0.9f), rotate);

        foreach (var item in blocks)
        {
            if (Player.SelectedPlayer.OnMoveable(item.transform.position))
            {
                var block = item.GetComponent<BlockInfo>();
                if (block)
                {
                    block.ToChangeMoveableAreaColor();
                    HighLightedMoveableArea.Add(block);
                }
            }
        }
    }
    //static BlockInfo HighLightedBlock;
    static List<BlockInfo> HighLightedMoveableArea = new List<BlockInfo>();

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
        name = $"{intPos.x}:{intPos.y}";
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

    public void ToChangeColor(Color color)
    {
        m_Renderer.material.color = color;
    }
    public void ToChangeMoveableAreaColor()
    {
        m_Renderer.material.color = moveableColor;
    }
    public void ToChangeOriginalColor()
    {
        m_Renderer.material.color = m_OriginalColor;
    }

    void OnMouseExit()
    {
        if (actor)
        {
            ActorStatusUI.Instance.Close();
        }
    }
}
