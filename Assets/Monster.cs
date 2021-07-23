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
    Plyer,
    Monster,
}
public class Actor : MonoBehaviour
{
    public static List<Actor> Actors = new List<Actor>();
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
    public List<Vector2Int> attackablePoints = new List<Vector2Int>();
    protected void Awake()
    {
        Actors.Add(this);
        var attackPoints = GetComponentsInChildren<AttackPoint>(true);

        // 앞쪽에 있는 공격 포인트들.
        foreach (var item in attackPoints)
            attackablePoints.Add(item.transform.localPosition.ToVector2Int());

        // 오른쪽에 있는 공격 포인트들.
        transform.Rotate(0, 90, 0);
        foreach (var item in attackPoints)
            attackablePoints.Add((item.transform.position - transform.position).ToVector2Int());

        // 뒤쪽에 있는 공격 포인트들.
        transform.Rotate(0, 90, 0);
        foreach (var item in attackPoints)
            attackablePoints.Add((item.transform.position - transform.position).ToVector2Int());

        // 왼쪽에 있는 공격 포인트들.
        transform.Rotate(0, 90, 0);
        foreach (var item in attackPoints)
            attackablePoints.Add((item.transform.position - transform.position).ToVector2Int());

        // 다시 앞쪽 보도록 돌림.
        transform.Rotate(0, 90, 0);
    }

    protected void OnDestroy()
    {
        Actors.Remove(this);
    }

    internal virtual void TakeHit(int power)
    {
        //맞은 데미지 표시하자.
        hp -= power;
    }

    protected void LookAtOnlyYAxis(Vector3 position)
    {
        var lookAtPos = new Vector3(position.x, transform.position.y, position.z);
        transform.LookAt(lookAtPos);
    }

    public List<BlockInfo> enemyExistPoint = new List<BlockInfo>();
    public BlockType passableValues = BlockType.Walkable | BlockType.Water;

    public List<BlockInfo> SetAttackableEnemyPoint()
    {
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
                }
            }
        }

        return enemyExistPoint;
    }

    protected virtual bool IsExistEnemy(BlockInfo blockInfo)
    {
        throw new NotImplementedException();
    }
}

public class Monster : Actor
{    protected override bool IsExistEnemy(BlockInfo blockInfo)
    {
        if (blockInfo.blockType.HasFlag(BlockType.Player) == false)
            return false;

        Debug.Assert(blockInfo.actor != null, "액터는 꼭 있어야 해!");

        return true;
    }

    public override ActorTypeEnum ActorType { get => ActorTypeEnum.Monster; }

    Animator animator;

    public static List<Monster> Monsters = new List<Monster>();

    new private void Awake()
    {
        base.Awake();
        Monsters.Add(this);
    }
    new private void OnDestroy()
    {
        base.OnDestroy();
        Monsters.Remove(this);
    }
    void Start()
    {
        GroundManager.Instance.AddBlockInfo(transform.position, BlockType.Monster, this);
        animator = GetComponentInChildren<Animator>();
    }

    internal override void TakeHit(int power)
    {
        //맞은 데미지 표시하자.
        var go = (GameObject)Instantiate(Resources.Load("DamageText"), transform);
        go.transform.localPosition = new Vector3(0, 1.3f, 0);
        go.GetComponent<TextMeshPro>().text = power.ToString();
        Destroy(go, 2);

        hp -= power;
        animator.Play("TakeHit");
    }

    public float attackTime = 1;
    internal IEnumerator AttackTarget(BlockInfo target)
    {
        Player player = target.actor as Player;
        animator.Play("Attack");
        yield return new WaitForSeconds(attackTime);
        player.TakeHit(power);
    }

    internal Actor FindNearestAttackTarget()
    {
        Player nearestPlayer = null;
        int smallestStep = int.MaxValue;

        var mPos = transform.position.ToVector2Int();
        var map = GroundManager.Instance.blockInfoMap;
        foreach (var player in Player.Players)
        {
            var destPos = player.transform.position.ToVector2Int();

            List<Vector2Int> path = PathFinding2D.find4(
                mPos, destPos, map, passableValues);
            if (path.Count < smallestStep)
            {
                nearestPlayer = player; 
                smallestStep = path.Count;
            }
        }
        

        return nearestPlayer;
    }

    internal IEnumerator MoveToTarget(object target)
    {
        yield return null;
    }
}
