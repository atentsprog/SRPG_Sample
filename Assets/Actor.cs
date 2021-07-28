﻿using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    Player,
    Monster,
}
public class Actor : MonoBehaviour
{
    public virtual ActorTypeEnum ActorType { get => ActorTypeEnum.NotInit; }

    public string nickname = "이름을 입력해주세요";
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

    public float attackTime = 1;

    
    protected void Awake()
    {
        var attackPoints = GetComponentsInChildren<AttackPoint>(true);
        animator = GetComponentInChildren<Animator>();
        // 앞쪽에 있는 공격 포인트들.
        foreach (var item in attackPoints)
            attackableLocalPositions.Add((item.transform.position - transform.position).ToVector2Int());

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

    //protected void OnDestroy()
    //{
    //    if (GroundManager.ApplicationQuit) // <- 길더라도 이로직 권장
    //        return;

    //    GroundManager.Instance.RemoveBlockInfo(transform.position, GetBlockType());
    //}

    protected void OnDestroy()
    {
        GroundManager.Instance?.RemoveBlockInfo(transform.position, GetBlockType());
    }

    public float takeHitTime = 0.7f;
    internal IEnumerator TakeHitCo(int power)
    {
        //맞은 데미지 표시하자.
        GameObject damageTextGoInResoruce = (GameObject)Resources.Load("DamageText");
        var pos = transform.position;
        pos.y = 1.3f;
        GameObject damageTextGo = Instantiate(damageTextGoInResoruce
            , pos
            , damageTextGoInResoruce.transform.rotation, transform);

        damageTextGo.GetComponent<TextMeshPro>().text = power.ToString();
        Destroy(damageTextGo, 2);

        hp -= power;
        animator.Play("TakeHit");
        yield return new WaitForSeconds(takeHitTime);

        if (hp <= 0)
        {
            animator.Play("Die");
            status = StatusType.Die;

            OnDie();
        }
    }

    protected virtual void OnDie()
    {
        Debug.LogError("자식들이 오버라이드 해서 구현해야함, 여기 호출되면 안됨");
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
            path.RemoveAt(0); // 자기 위치 지우기.
            
            // 몬스터일 때는 마지막 지점을 삭제해야한다.
            if ( ActorType == ActorTypeEnum.Monster)
                path.RemoveAt(path.Count - 1);

            //path[0]; // 1회째 움직일곳
            //path[1]; // 2회째 움직일곳
            //path[2]; // 3회째 움직일곳
            //path[3]// <- 4회째 이동, 이거부터 끝까지 제거 하자.
            // 최대 이동거리만큼 이동하자.
            if (path.Count > moveDistance) //3
                path.RemoveRange(moveDistance, path.Count - moveDistance);

            ////moveDistance:3
            ////path.Count : 5


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
            GroundManager.Instance.AddBlockInfo(myTr.position, GetBlockType(), this);
            completeMove = true;
            OnCompleteMove();
        }
    }
    public virtual BlockType GetBlockType()
    {
        Debug.LogError($"{GetType()}, 자식에서 GetBlockType함수 오버라이드 해야함");
        return BlockType.None;
    }
    protected virtual  void OnCompleteMove()
    {
    }

    public void PlayAnimation(string nodeName)
    {
        animator.Play(nodeName, 0, 0);
    }

    protected IEnumerator AttackToTargetCo(Actor attackTarget)
    {
        transform.LookAt(attackTarget.transform);

        animator.Play("Attack");
        StartCoroutine(attackTarget.TakeHitCo(power));

        // 스플레시 데미지 적용하자.
        SubAttackArea[] subAttackAreas = transform.GetComponentsInChildren<SubAttackArea>(true);
        ActorTypeEnum myActorType = ActorType;
        foreach(var item in subAttackAreas)
        {
            var pos = item.transform.position.ToVector2Int();
            if(GroundManager.Instance.blockInfoMap.TryGetValue(pos, out BlockInfo block))
            {
                if (block.actor == null)
                    continue;

                Actor subAttackTarget = block.actor;
                
                switch (item.target)
                {
                    case SubAttackArea.Target.EnemyOnly:
                        if (subAttackTarget.ActorType == myActorType)
                            continue;
                        break;
                    case SubAttackArea.Target.AllyOnly:
                        if (subAttackTarget.ActorType != myActorType)
                            continue;
                        break;
                }
                int subAttackPower = (int)(power * item.damageRatio);
                StartCoroutine(subAttackTarget.TakeHitCo(subAttackPower));
            }
        }

        yield return new WaitForSeconds(attackTime);


        completeAct = true;
    }
}
