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
    public Dictionary<Vector2Int, AttackPoint> attackablePoints = new Dictionary<Vector2Int, AttackPoint>();
    protected Animator animator;
    public float moveTimePerUnit = 0.3f;
    public BlockType passableValues = BlockType.Walkable | BlockType.Water;
    protected void Awake()
    {
        Actors.Add(this);
        var attackPoints = GetComponentsInChildren<AttackPoint>(true);

        // 앞쪽에 있는 공격 포인트들.
        foreach (var item in attackPoints)
            attackablePoints.Add((item.transform.position - transform.position).ToVector2Int(), item);

        // 오른쪽에 있는 공격 포인트들.
        transform.Rotate(0, 90, 0);
        foreach (var item in attackPoints)
            attackablePoints.Add((item.transform.position - transform.position).ToVector2Int(), item);

        // 뒤쪽에 있는 공격 포인트들.
        transform.Rotate(0, 90, 0);
        foreach (var item in attackPoints)
            attackablePoints.Add((item.transform.position - transform.position).ToVector2Int(), item);

        // 왼쪽에 있는 공격 포인트들.
        transform.Rotate(0, 90, 0);
        foreach (var item in attackPoints)
            attackablePoints.Add((item.transform.position - transform.position).ToVector2Int(), item);

        // 다시 앞쪽 보도록 돌림.
        transform.Rotate(0, 90, 0);
    }

    protected void OnDestroy()
    {
        Actors.Remove(this);
    }

    public float takeHitTime = 0.7f;
    internal virtual IEnumerator TakeHitCo(int power)
    {
        //맞은 데미지 표시하자.
        GameObject damageTextResourceGo = (GameObject)Resources.Load("DamageText");
        GameObject damageTextGo = Instantiate(damageTextResourceGo, transform);
        damageTextGo.transform.localPosition = new Vector3(0, 1.3f, 0);
        damageTextGo.transform.rotation = damageTextResourceGo.transform.rotation;
        damageTextGo.GetComponent<TextMeshPro>().text = power.ToString();
        Destroy(damageTextGo, 2);

        hp -= power;
        animator.Play("TakeHit");
        yield return null;
        if (hp <= 0)
        {
            yield return new WaitForSeconds(takeHitTime);
            animator.Play("Die");
            status = StatusType.Die;
            OnDie();
        }
    }
    public BlockType GetBlockType
    {
        get
        {
            switch (ActorType)
            {
                case ActorTypeEnum.Plyer:
                    return BlockType.Player;
                case ActorTypeEnum.Monster:
                    return BlockType.Monster;
                default:
                    Debug.Log($"{ActorType}을 정의해 주세요");
                    return BlockType.None;
            }
        }
    }

    protected virtual void OnDie()
    {
        print("몬스터와 플레이어가 죽었을때 실행될 로직을 각자 정의하게 하자");
    }

    public void PlayAnimation(string nodeName)
    {
        animator.Play(nodeName, 0, 0);
    }

    protected bool IsAttackablePosition(Vector3 position)
    {
        Vector2Int currentPos = transform.position.ToVector2Int();
        Vector2Int chekcPoint = position.ToVector2Int(); 

        foreach (var item in attackablePoints.Keys)
        {
            Vector2Int pos = item + currentPos; //item의 월드 지역 위치;
            if (pos == chekcPoint)
                return true;
        }

        return false;
    }
}
