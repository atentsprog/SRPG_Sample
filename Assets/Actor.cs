using System.Collections;
using System.Collections.Generic;
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
    public List<Vector2Int> attackableLocalPositions = new List<Vector2Int>();
    private void Awake()
    {
        var attackPoints = GetComponentsInChildren<AttackPoint>(true);

        // 앞쪽에 있는 공격 포인트들.
        foreach (var item in attackPoints)
            attackableLocalPositions.Add(item.transform.localPosition.ToVector2Int());

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

    internal virtual void TakeHit(int power)
    {
        //맞은 데미지 표시하자.
        hp -= power;
    }
}
