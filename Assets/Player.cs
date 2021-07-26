using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Actor
{
    public static List<Player> Players = new List<Player>();
    public override ActorTypeEnum ActorType { get => ActorTypeEnum.Plyer; }

    static public Player SelectedPlayer;

    new protected void Awake()
    {
        base.Awake();
        Players.Add(this);
    }
    new protected void OnDestroy()
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
        Transform tr = transform;
        Vector2Int playerPos = tr.position.ToVector2Int();

        var map = GroundManager.Instance.blockInfoMap;
        List<Vector2Int> path = PathFinding2D.find4(playerPos, goalPos, (Dictionary<Vector2Int, BlockInfo>)map, passableValues);
        if (path.Count == 0)
            Debug.Log("길이 없다");
        else
        {
            // 월래 위치에선 플레이어 정보 삭제
            GroundManager.Instance.RemoveBlockInfo(Player.SelectedPlayer.transform.position, BlockType.Player);
            PlayAnimation("Walk");
            FollowTarget.Instance.SetTarget(Player.SelectedPlayer.transform);
            path.RemoveAt(0);
            foreach (var item in path)
            {
                Vector3 playerNewPos = new Vector3(item.x, 0, item.y);
                tr.LookAt(playerNewPos);
                tr.DOMove(playerNewPos, moveTimePerUnit).SetEase(moveEase);
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

            completeMove = true;
        }
    }

    internal bool CanAttackTarget(Actor actor)
    {
        //같은팀을 공격대상으로 하지 않기
        if (actor.ActorType != ActorTypeEnum.Monster)
            return false;

        // 공격 가능한 범위 안에 있는지 확인.
        if (completeAct)
            return false;

        if (IsAttackablePosition(actor.transform.position) == false)
            return false;

        return true;
    }

    internal void AttackToTarget(Actor actor)
    {
        ClearEnemyExistPoint();

        StartCoroutine(AttackToTargetCo(actor));
    }

    public float attackTime = 1;
    private IEnumerator AttackToTargetCo(Actor attackTarget)
    {
        transform.LookAt(attackTarget.transform);

        animator.Play("Attack");
        yield return attackTarget.TakeHitCo(power);
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
    public List<BlockInfo> enemyExistPoint = new List<BlockInfo>();
    internal bool ShowAttackableArea()
    {
        //현재 위치에서 공격 가능한 지역을 체크하자.
        Vector2Int currentPos = transform.position.ToVector2Int();
        var map = GroundManager.Instance.blockInfoMap;

        //공격가능한 지역에 적이 있는지 확인하자.
        foreach (var item in attackablePoints)
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

    public Ease moveEase = Ease.InBounce;


}
