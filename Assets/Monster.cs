using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum StatusType
{
    Normal,
    Sleep,
    Die,
}

public class Actor : MonoBehaviour
{
    public enum ActorType
    {
        NotInit,
        Ally,   // 아군(Player, NPC..)
        Enemy,  // 적군(몬스터, Boss..)
    }
    public virtual ActorType actorType => ActorType.NotInit;

    public string nickname = "이름 이력해주세요";
    public string iconName;
    public int power = 10;
    public float hp = 20;
    public float mp = 0;
    public float maxHp = 20;
    public float maxMp = 0;
    public StatusType status;

    public int moveDistance = 5;
    public List<Vector2Int> attackablePoint = new List<Vector2Int>();
    internal static Actor CurrentAttackActor;

    //public static bool IsExistAttackActor { get => CurrentAttackActor != null; }

    private void Awake()
    {
        var attackPoints = GetComponentsInChildren<AttackPoint>();
        // 정면
        foreach(var item in attackPoints)
            attackablePoint.Add((item.transform.position - transform.position).ToVector2Int());
        // 오른쪽
        transform.Rotate(0, 90, 0);
        foreach (var item in attackPoints)
            attackablePoint.Add((item.transform.position - transform.position).ToVector2Int());
        // 뒤
        transform.Rotate(0, 90, 0);
        foreach (var item in attackPoints)
            attackablePoint.Add((item.transform.position - transform.position).ToVector2Int());
        // 왼쪽
        transform.Rotate(0, 90, 0);
        foreach (var item in attackPoints)
            attackablePoint.Add((item.transform.position - transform.position).ToVector2Int());

        // 다시 정면 보기
        transform.Rotate(0, 90, 0);
    }

    internal virtual void AttackToTarget(Actor actor)
    {
        Debug.LogError($"{actor.nickname} 을때리는 로직을 자식 클래스에 작성하자");
    }

    internal static bool CanAttackTarget(Actor actor)
    {
        //같은팀을 공격대상으로 하지 않기
        if (CurrentAttackActor.actorType == actor.actorType)
            return false;

        // 공격 가능한 범위 안에 있는지 확인.


        return true;
    }
    public static List<BlockInfo> EnemyExistPoint = new List<BlockInfo>();
    public static void ClearEnemyExistPoint()
    {
        EnemyExistPoint.ForEach(x => x.ToChangeOriginalColor());
    }
    internal bool ShowAttackableBlock()
    {
        //현재 위치에서 공격 가능한 지역을 체크하자.
        Vector2Int currentPos = transform.position.ToVector2Int();
        EnemyExistPoint.Clear();
        var blockInfoMap = GroundManager.Instance.blockInfoMap;
        ActorType myActorType = actorType;
        foreach (var item in attackablePoint)
        {
            Vector2Int pos = currentPos + item;

            //pos에 적이 있는지 확인.
            if(blockInfoMap.ContainsKey(pos) && blockInfoMap[pos].actor && blockInfoMap[pos].actor.actorType != myActorType)
            {
                EnemyExistPoint.Add(blockInfoMap[pos]);
            }
        }

        //attackablePoints 을 붉게 표시하자.
        EnemyExistPoint.ForEach(x => x.ToChangeColor(Color.red));
        CurrentAttackActor = this;

        return EnemyExistPoint.Count > 0;
    }

    internal virtual void TakeHit(int power)
    {
        Debug.Log("TakeHit::자식들이 재정의 해주자");
        hp -= power;
    }
}
public class Monster : Actor
{
    public override ActorType actorType => ActorType.Enemy;
    private void Awake()
    {
        
    }
    Animator animator;
    void Start()
    {
        GroundManager.Instance.AddBlockInfo(transform.position, BlockType.Monster, this);
        animator = GetComponentInChildren<Animator>();
    }

    internal override void TakeHit(int power)
    {
        //맞은 데미지 표시하자.
        animator.Play("TakeHit", 0, 0);
        hp -= power;
    }
}
