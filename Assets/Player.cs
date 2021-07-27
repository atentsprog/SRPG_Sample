using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Actor
{
    public static List<Player> Players = new List<Player>();
    new protected void Awake()
    {
        base.Awake();
        Players.Add(this);
    }
    protected void OnDestroy()
    {
        Players.Remove(this);
    }

    public override ActorTypeEnum ActorType { get => ActorTypeEnum.Plyer; }

    static public Player SelectedPlayer;

    void Start()
    {
        //SelectedPlayer = this;
        //animator = GetComponentInChildren<Animator>();
        GroundManager.Instance.AddBlockInfo(transform.position, BlockType.Player, this);
        FollowTarget.Instance.SetTarget(transform);
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

    internal bool CanAttackTarget(Actor enemy)
    {
        //같은팀을 공격대상으로 하지 않기
        if (enemy.ActorType != ActorTypeEnum.Monster)
            return false;

        // 공격 가능한 범위 안에 있는지 확인.
        if (IsInAttackableArea(enemy.transform.position) == false)
            return false;

        return true;
    }

    internal void AttackToTarget(Actor actor)
    {
        ClearEnemyExistPoint();
        StartCoroutine(AttackToTargetCo_(actor));
    }

    private IEnumerator AttackToTargetCo_(Actor actor)
    {
        yield return AttackToTargetCo(actor);
        StageManager.GameState = GameStateType.SelectPlayer;
    }

    internal bool OnMoveable(Vector3 position, int maxDistance)
    {
        Vector2Int goalPos = position.ToVector2Int();
        Vector2Int playerPos = transform.position.ToVector2Int();
        var map = GroundManager.Instance.blockInfoMap;
        var path = PathFinding2D.find4(playerPos, goalPos, (Dictionary<Vector2Int, BlockInfo>)map, passableValues);

        if (path.Count == 0 || path.Count > maxDistance + 1)
            return false;

        //if (path.Count == 0)
        //    Debug.Log("길 업따 !");
        //else if (path.Count > maxDistance + 1)
        //    Debug.Log("이동모태 !");
        //else
        //    return true;

        return true;
    }

    public void ClearEnemyExistPoint()
    {
        enemyExistPoint.ForEach( x => x.ToChangeOriginalColor());
        enemyExistPoint.Clear();
    }
    protected override void OnCompleteMove()
    {
        bool existAttackTarget = ShowAttackableArea();
        if (existAttackTarget)
            StageManager.GameState = GameStateType.SelectToAttackTarget;
        else
            StageManager.GameState = GameStateType.SelectPlayer;
    }

    public List<BlockInfo> enemyExistPoint = new List<BlockInfo>();
    internal bool ShowAttackableArea()
    {
        //현재 위치에서 공격 가능한 지역을 체크하자.
        Vector2Int currentPos = transform.position.ToVector2Int();
        var map = GroundManager.Instance.blockInfoMap;

        //공격가능한 지역에 적이 있는지 확인하자.
        foreach (var item in attackableLocalPositions)
        {
            Vector2Int pos = item + currentPos; //item의 월드 지역 위치;

            if (map.ContainsKey(pos))
            {
                if (IsEnemyExist(map[pos])) //map[pos]에 적이 있는가? -> 적인지 판단은 actorType으로 하자.
                {
                    enemyExistPoint.Add(map[pos]);
                }
            }
        }

        enemyExistPoint.ForEach(x => x.ToChangeColor(Color.red));

        return enemyExistPoint.Count > 0;
    }

    private bool IsEnemyExist(BlockInfo blockInfo)
    {
        //if (blockInfo.actor == null)
        //    return false;

        if (blockInfo.blockType.HasFlag(BlockType.Monster) == false)
            return false;

        Debug.Assert(blockInfo.actor != null, "액터는 꼭 있어야 해!");

        return true;
    }

    public override BlockType GetBlockType()
    {
        return BlockType.Player;
    }
}
