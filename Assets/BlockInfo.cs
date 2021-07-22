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
            case GameStateType.SelectBlockToMoveOrAttackTarget:
                SelectBlockToMoveOrAttackTarget();
                break;
            case GameStateType.SelectToAttackTarget:
                SelectToAttackTarget();
                break;
            case GameStateType.AttackToTarget:
                AttackToTarget();
                break;

            case GameStateType.NotInit:
            case GameStateType.IngPlayerMove:
            case GameStateType.MonsterTurn:
                Debug.Log($"블럭을 클릭할 수 없는 상태 입니다:" +
                    $"{StageManager.GameState}");
                break;
        }

        //// 이미 빨간 블럭 상태일때 다시 선택하면 빨간 블럭을 원상 복귀 시켜라.


        //// 지금 블럭에 몬스터 있으면 때리자.


        //if( actor && actor == Player.SelectedPlayer)
        //{
        //    // 영역 표시.
        //    //actor.moveDistance
        //    // 첫번째 이동으로 갈수 있는것을 첫번째 라인에 추가.
        //    ShowMoveDistance(actor.moveDistance);
        //}
        //else
        //    Player.SelectedPlayer.OnTouch(transform.position);
    }

    private void AttackToTarget()
    {
        throw new NotImplementedException();
    }

    private void SelectToAttackTarget()
    {
        throw new NotImplementedException();
    }

    private void SelectBlockToMoveOrAttackTarget()
    {
        // 공격 대상이 있다면 공격 하자.(액터가 몬스터라면)
        if (actor)
        {
            //현재 공격 하는 액터가 공격 대상으로 할 수 있는지 확인.
            // todo:공격 범위 안에 있는지 확인해줘야함.
            if (Player.SelectedPlayer.CanAttackTarget(actor))
            {
                Player.SelectedPlayer.AttackToTarget(actor);
            }
        }
        else
        {
            if (highLightedMoveableArea.Contains(this))
            {
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
            Player.SelectedPlayer = (Player)actor;

            //이동 가능한 영역 표시.
            ShowMoveDistance(Player.SelectedPlayer.moveDistance);

            // 현재 위치에서 공격 가능한 영역 표시.
            Player.SelectedPlayer.ShowAttackableArea();
            StageManager.GameState = GameStateType.SelectBlockToMoveOrAttackTarget;
        }
    }

    private void ShowMoveDistance(int moveDistance)
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
                if (block)
                {
                    block.ToChangeBlueColor();
                    highLightedMoveableArea.Add(block);
                }
            }
        }
    }
    static List<BlockInfo> highLightedMoveableArea = new List<BlockInfo>();
    private void ClearMoveableArea()
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
