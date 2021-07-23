﻿using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Actor
{
    public static List<Player> Players = new List<Player>();
    public override ActorTypeEnum ActorType { get => ActorTypeEnum.Plyer; }

    static public Player SelectedPlayer;
    Animator animator;
    new private void Awake()
    {
        base.Awake();
        Players.Add(this);
    }
    new private void OnDestroy()
    {
        base.OnDestroy();
        Players.Remove(this);
    }
    void Start()
    {
        //SelectedPlayer = this;
        animator = GetComponentInChildren<Animator>();
        GroundManager.Instance.AddBlockInfo(transform.position, BlockType.Player, this);
        FollowTarget.Instance.SetTarget(transform);
    }

    public void PlayAnimation(string nodeName)
    {
        animator.Play(nodeName, 0, 0);
    }

    internal void MoveToPosition(Vector3 position)
    {
        Vector2Int findPos = position.ToVector2Int();//
        FindPath(findPos);
    }
    //public float moveDistanceMultiply = 1.2
    void FindPath(Vector2Int goalPos)
    {
        StopAllCoroutines();
        StartCoroutine(FindPathCo(goalPos));
    }

    IEnumerator FindPathCo(Vector2Int goalPos)
    {
        Transform player = transform;
        Vector2Int playerPos = new Vector2Int(Mathf.RoundToInt(player.position.x)
            , Mathf.RoundToInt(player.position.z));
        playerPos.x = Mathf.RoundToInt(player.position.x);
        playerPos.y = Mathf.RoundToInt(player.position.z);
        var map = GroundManager.Instance.blockInfoMap;
        List<Vector2Int> path = PathFinding2D.find4(playerPos, goalPos, (Dictionary<Vector2Int, BlockInfo>)map, passableValues);
        if (path.Count == 0)
            Debug.Log("길이 없다");
        else
        {
            // 월래 위치에선 플레이어 정보 삭제
            GroundManager.Instance.RemoveBlockInfo(Player.SelectedPlayer.transform.position, BlockType.Player);
            Player.SelectedPlayer.PlayAnimation("Walk");
            FollowTarget.Instance.SetTarget(Player.SelectedPlayer.transform);
            path.RemoveAt(0);
            foreach (var item in path)
            {
                Vector3 playerNewPos = new Vector3(item.x, 0, item.y);
                player.LookAt(playerNewPos);
                //player.position = playerNewPos;
                player.DOMove(playerNewPos, moveTimePerUnit).SetEase(moveEase);
                yield return new WaitForSeconds(moveTimePerUnit);
            }
            Player.SelectedPlayer.PlayAnimation("Idle");
            FollowTarget.Instance.SetTarget(null);
            // 이동한 위치에는 플레이어 정보 추가
            GroundManager.Instance.AddBlockInfo(Player.SelectedPlayer.transform.position, BlockType.Player, this);

            completeMove = true;

            bool existAttackTarget = ShowAttackableArea();
            if (existAttackTarget)
                StageManager.GameState = GameStateType.SelectToAttackTarget;
            else
                StageManager.GameState = GameStateType.SelectPlayer;
        }
    }

    internal bool CanAttackTarget(Actor actor)
    {
        //같은팀을 공격대상으로 하지 않기
        if (actor.ActorType != ActorTypeEnum.Monster)
            return false;

        // 공격 가능한 범위 안에 있는지 확인.
        return true;
    }

    internal void AttackToTarget(Actor actor)
    {
        ClearEnemyExistPoint();

        StartCoroutine(AttackToTargetCo(actor));
    }

    public float attackTime = 1;
    private IEnumerator AttackToTargetCo(Actor actor)
    {
        LookAtOnlyYAxis(actor.transform.position);

        animator.Play("Attack");
        actor.TakeHit(power);
        yield return new WaitForSeconds(attackTime);

        completeAct = true;
        StageManager.GameState = GameStateType.SelectPlayer;
    }

    internal bool OnMoveable(Vector3 position, int maxDistance)
    {
        Vector2Int goalPos = position.ToVector2Int();
        Vector2Int playerPos = transform.position.ToVector2Int();
        var map = GroundManager.Instance.blockInfoMap;
        var path = PathFinding2D.find4(playerPos, goalPos, (Dictionary<Vector2Int, BlockInfo>)map, passableValues);
        if (path.Count == 0)
        {
            //Debug.Log("길 업따 !");
            return false;
        }
        else if (path.Count > maxDistance + 1)
        {
            //Debug.Log("이동모태 !");
            return false;
        }

        return true;
    }

    public void ClearEnemyExistPoint()
    {
        enemyExistPoint.ForEach(x => x.ToChangeOriginalColor());
        enemyExistPoint.Clear();
    }

    internal bool ShowAttackableArea()
    {
        bool existEnemy = SetAttackableEnemyPoint().Count > 0;

        enemyExistPoint.ForEach(x => x.ToChangeColor(Color.red));

        return existEnemy;
    }

    public Ease moveEase = Ease.InBounce;
    public float moveTimePerUnit = 0.3f;


    protected override bool IsExistEnemy(BlockInfo blockInfo)
    {
        if (blockInfo.blockType.HasFlag(BlockType.Monster) == false)
            return false;

        Debug.Assert(blockInfo.actor != null, "액터는 꼭 있어야 해!");

        return true;
    }
}
