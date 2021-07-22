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
        Ally,   // �Ʊ�(Player, NPC..)
        Enemy,  // ����(����, Boss..)
    }
    public virtual ActorType actorType => ActorType.NotInit;

    public string nickname = "�̸� �̷����ּ���";
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
        // ����
        foreach(var item in attackPoints)
            attackablePoint.Add((item.transform.position - transform.position).ToVector2Int());
        // ������
        transform.Rotate(0, 90, 0);
        foreach (var item in attackPoints)
            attackablePoint.Add((item.transform.position - transform.position).ToVector2Int());
        // ��
        transform.Rotate(0, 90, 0);
        foreach (var item in attackPoints)
            attackablePoint.Add((item.transform.position - transform.position).ToVector2Int());
        // ����
        transform.Rotate(0, 90, 0);
        foreach (var item in attackPoints)
            attackablePoint.Add((item.transform.position - transform.position).ToVector2Int());

        // �ٽ� ���� ����
        transform.Rotate(0, 90, 0);
    }

    internal virtual void AttackToTarget(Actor actor)
    {
        Debug.LogError($"{actor.nickname} �������� ������ �ڽ� Ŭ������ �ۼ�����");
    }

    internal static bool CanAttackTarget(Actor actor)
    {
        //�������� ���ݴ������ ���� �ʱ�
        if (CurrentAttackActor.actorType == actor.actorType)
            return false;

        // ���� ������ ���� �ȿ� �ִ��� Ȯ��.


        return true;
    }
    public static List<BlockInfo> EnemyExistPoint = new List<BlockInfo>();
    public static void ClearEnemyExistPoint()
    {
        EnemyExistPoint.ForEach(x => x.ToChangeOriginalColor());
    }
    internal bool ShowAttackableBlock()
    {
        //���� ��ġ���� ���� ������ ������ üũ����.
        Vector2Int currentPos = transform.position.ToVector2Int();
        EnemyExistPoint.Clear();
        var blockInfoMap = GroundManager.Instance.blockInfoMap;
        ActorType myActorType = actorType;
        foreach (var item in attackablePoint)
        {
            Vector2Int pos = currentPos + item;

            //pos�� ���� �ִ��� Ȯ��.
            if(blockInfoMap.ContainsKey(pos) && blockInfoMap[pos].actor && blockInfoMap[pos].actor.actorType != myActorType)
            {
                EnemyExistPoint.Add(blockInfoMap[pos]);
            }
        }

        //attackablePoints �� �Ӱ� ǥ������.
        EnemyExistPoint.ForEach(x => x.ToChangeColor(Color.red));
        CurrentAttackActor = this;

        return EnemyExistPoint.Count > 0;
    }

    internal virtual void TakeHit(int power)
    {
        Debug.Log("TakeHit::�ڽĵ��� ������ ������");
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
        //���� ������ ǥ������.
        animator.Play("TakeHit", 0, 0);
        hp -= power;
    }
}
