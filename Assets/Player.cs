using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Actor
{
    public override ActorType actorType => ActorType.Ally;
    static public Player SelectedPlayer;
    Animator animator;
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

    internal void OnTouch(Vector3 position)
    {
        Vector2Int findPos = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
        FindPath(findPos);
    }
    void FindPath(Vector2Int goalPos)
    {
        StopAllCoroutines();
        StartCoroutine(FindPathCo(goalPos));
    }
    public BlockType passableValues = BlockType.Walkable | BlockType.Water;
    IEnumerator FindPathCo(Vector2Int goalPos)
    {
        Transform player = transform;
        Vector2Int playerPos = new Vector2Int(Mathf.RoundToInt(player.position.x)
            , Mathf.RoundToInt(player.position.z));
        playerPos.x = Mathf.RoundToInt(player.position.x);
        playerPos.y = Mathf.RoundToInt(player.position.z);
        var map = GroundManager.Instance.blockInfoMap;
        List<Vector2Int> path = PathFinding2D.find4(playerPos, goalPos, map, passableValues);
        if (path.Count == 0)
            Debug.Log("길이 없다");
        else
        {
            // 월래 위치에선 플레이어 정보 삭제
            GroundManager.Instance.RemoveBlockInfo(transform.position, BlockType.Player);
            PlayAnimation("Walk");
            FollowTarget.Instance.SetTarget(transform);
            path.RemoveAt(0);
            foreach (var item in path)
            {
                Vector3 playerNewPos = new Vector3(item.x, 0, item.y);
                player.LookAt(playerNewPos);
                player.DOMove(playerNewPos, moveTimePerUnit).SetEase(moveEase);
                yield return new WaitForSeconds(moveTimePerUnit);
            }
            PlayAnimation("Idle");
            FollowTarget.Instance.SetTarget(null);
            // 이동한 위치에는 플레이어 정보 추가
            GroundManager.Instance.AddBlockInfo(transform.position, BlockType.Player, this);

            bool existAttackTarget = ShowAttackableBlock();
            if (existAttackTarget)
                StageManager.GameState = GameStateType.SelectToAttackTarget;
            else
                StageManager.GameState = GameStateType.SelectPlayer;
        }
    }


    internal bool OnMoveable(Vector3 position)
    {
        Vector2Int goalPos = position.ToVector2Int();
        Vector2Int playerPos = transform.position.ToVector2Int();
        var map = GroundManager.Instance.blockInfoMap;
        var path = PathFinding2D.find4(playerPos, goalPos, map, passableValues);

        return path.Count > 0 && path.Count <= 5;

        //if (path.Count == 0)
        //    Debug.Log("길 업따 !");
        //else if (path.Count > 5)
        //    Debug.Log("이동모태 !");
        //else
        //    return true;

        //return false;
    }

    public Ease moveEase = Ease.InBounce;
    public float moveTimePerUnit = 0.3f;

    internal override void AttackToTarget(Actor actor)
    {
        //todo:타겟 방향 바라보기.
        StartCoroutine(AttackToTargetCo(actor));
    }

    public float attackTime = 1;
    private IEnumerator AttackToTargetCo(Actor actor)
    {
        animator.Play("Attack");
        actor.TakeHit(power);
        yield return new WaitForSeconds(attackTime);
        StageManager.GameState = GameStateType.SelectPlayer;

        Actor.CurrentAttackActor = null;
        ClearEnemyExistPoint();
    }
}
