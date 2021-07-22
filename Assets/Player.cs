using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Actor
{
    public override ActorTypeEnum ActorType { get => ActorTypeEnum.Plyer; }

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
    public BlockType passableValues = BlockType.Walkable | BlockType.Water;
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
        //todo:타겟 방향 바라보기.
        StartCoroutine(AttackToTargetCo(actor));
    }

    public float attackTime = 1;
    private IEnumerator AttackToTargetCo(Actor actor)
    {
        LookAtOnlyYAxis(actor.transform.position);

        animator.Play("Attack");
        actor.TakeHit(power);
        yield return new WaitForSeconds(attackTime);
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

    public List<BlockInfo> enemyExistPoint = new List<BlockInfo>();
    internal bool ShowAttackableArea()
    {
        bool existEnemy = false;
        //현재 위치에서 공격 가능한 지역을 체크하자.
        Vector2Int currentPos = transform.position.ToVector2Int();
        var map = GroundManager.Instance.blockInfoMap;

        Debug.Assert(enemyExistPoint.Count == 0);

        //공격가능한 지역에 적이 있는지 확인하자.
        foreach (var item in attackablePoints)
        {
            Vector2Int pos = item + currentPos; //item의 월드 지역 위치;

            if (map.ContainsKey(pos))
            {
                if (IsExistEnemy(map[pos])) //map[pos]에 적이 있는가? -> 적인지 판단은 actorType으로 하자.
                {
                    enemyExistPoint.Add(map[pos]);
                    map[pos].ToChangeColor(Color.red);
                    existEnemy = true;
                }
            }
        }

        return existEnemy;
    }

    private bool IsExistEnemy(BlockInfo blockInfo)
    {
        //if (blockInfo.actor == null)
        //    return false;

        if (blockInfo.blockType.HasFlag(BlockType.Monster) == false)
            return false;

        Debug.Assert(blockInfo.actor != null, "액터는 꼭 있어야 해!");

        return true;
    }

    public Ease moveEase = Ease.InBounce;
    public float moveTimePerUnit = 0.3f;
}
