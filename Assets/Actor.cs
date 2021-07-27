using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatusType
{
    Normal,
    Sleep,
    Die,
}

public enum ActorTypeEnum
{
    NotInit, // 
    Plyer,
    Monster,
}
public class Actor : MonoBehaviour
{
    public virtual ActorTypeEnum ActorType { get => ActorTypeEnum.NotInit; }

    public string nickname = "이름 이력해주세요";
    public string iconName;
    public int power = 10;
    public float hp = 20;
    public float mp = 0;
    public float maxHp = 20;
    public float maxMp = 0;
    public StatusType status;

    public int moveDistance = 5;

    public bool completeMove;
    public bool completeAct;
    public bool CompleteTurn { get => completeMove && completeAct; }

    // 공격 범위를 모아두자.
    public List<Vector2Int> attackableLocalPositions = new List<Vector2Int>();

    public float moveTimePerUnit = 0.3f;
    protected Animator animator;
    public BlockType passableValues = BlockType.Walkable | BlockType.Water;

    
    protected void Awake()
    {
        var attackPoints = GetComponentsInChildren<AttackPoint>(true);

        // 앞쪽에 있는 공격 포인트들.
        foreach (var item in attackPoints)
            attackableLocalPositions.Add(item.transform.localPosition.ToVector2Int());

        // 오른쪽에 있는 공격 포인트들.
        transform.Rotate(0, 90, 0);
        foreach (var item in attackPoints)
            attackableLocalPositions.Add((item.transform.position - transform.position).ToVector2Int());

        // 뒤쪽에 있는 공격 포인트들.
        transform.Rotate(0, 90, 0);
        foreach (var item in attackPoints)
            attackableLocalPositions.Add((item.transform.position - transform.position).ToVector2Int());

        // 왼쪽에 있는 공격 포인트들.
        transform.Rotate(0, 90, 0);
        foreach (var item in attackPoints)
            attackableLocalPositions.Add((item.transform.position - transform.position).ToVector2Int());

        // 다시 앞쪽 보도록 돌림.
        transform.Rotate(0, 90, 0);
    }

    internal virtual void TakeHit(int power)
    {
        //맞은 데미지 표시하자.
        hp -= power;
    }


    /// <summary>
    /// 타겟 위치가 공격 가능한 지역인지 확인.
    /// </summary>
    /// <param name="enemyPosition">체크할 지역</param>
    /// <returns>공격 가능하면 true</returns>
    protected bool IsInAttackableArea(Vector3 enemyPosition)
    {
        Vector2Int enmyPositionVector2 = enemyPosition.ToVector2Int();
        Vector2Int currentPos = transform.position.ToVector2Int();

        //공격가능한 지역에 적이 있는지 확인하자.
        foreach (var item in attackableLocalPositions)
        {
            // pos : 공격 가능한 월드 포지션
            Vector2Int pos = item + currentPos; //item의 월드 지역 위치;

            if (pos == enmyPositionVector2)
                return true;
        }

        return false;
    }

    protected IEnumerator FindPathCo(Vector2Int destPos) //AttackToTargetCo
    {
        Transform myTr = transform;
        Vector2Int myPos = myTr.position.ToVector2Int();
        Vector3 myPosVector3 = myTr.position;
        var map = GroundManager.Instance.blockInfoMap;
        List<Vector2Int> path = PathFinding2D.find4(myPos, destPos, map, passableValues);
        if (path.Count == 0)
            Debug.Log("길이 없다");
        else
        {
            // 월래 위치에선 플레이어 정보 삭제
            GroundManager.Instance.RemoveBlockInfo(myPosVector3, GetBlockType());
            PlayAnimation("Walk");
            FollowTarget.Instance.SetTarget(myTr);
            path.RemoveAt(0);
            foreach (var item in path)
            {
                Vector3 playerNewPos = new Vector3(item.x, myPosVector3.y, item.y);
                myTr.LookAt(playerNewPos);
                myTr.DOMove(playerNewPos, moveTimePerUnit);
                yield return new WaitForSeconds(moveTimePerUnit);
            }
            PlayAnimation("Idle");
            FollowTarget.Instance.SetTarget(null);
            // 이동한 위치에는 플레이어 정보 추가
            GroundManager.Instance.AddBlockInfo(myPosVector3, GetBlockType(), this);

            completeMove = true;

            OnCompleteMove();
        }
    }
    public virtual BlockType GetBlockType()
    {
        Debug.LogError("자식에서 GetBlockType함수 오버라이드 해야함");
        return BlockType.None;
    }
    protected virtual  void OnCompleteMove()
    {
    }

    public void PlayAnimation(string nodeName)
    {
        animator.Play(nodeName, 0, 0);
    }
}
