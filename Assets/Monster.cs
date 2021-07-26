using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Monster : Actor
{
    public static List<Monster> Monsters = new List<Monster>();
    new protected void Awake()
    {
        base.Awake();
        Monsters.Add(this);
    }
    protected void OnDestroy()
    {
        Monsters.Remove(this);
    }
    internal IEnumerator AutoAttackCo()
    {
        print("몬스터 턴 구현 해야함");
        yield return null;
    }

    public override ActorTypeEnum ActorType { get => ActorTypeEnum.Monster; }
    Animator animator;
    protected void Start()
    {
        GroundManager.Instance.AddBlockInfo(transform.position, BlockType.Monster, this);
        animator = GetComponentInChildren<Animator>();
    }

    internal override void TakeHit(int power)
    {
        //맞은 데미지 표시하자.
        GameObject damageTextGo = (GameObject)Instantiate(Resources.Load("DamageText"), transform);
        damageTextGo.transform.localPosition = new Vector3(0, 1.3f, 0);
        damageTextGo.GetComponent<TextMeshPro>().text = power.ToString();
        Destroy(damageTextGo, 2);

        hp -= power;
        animator.Play("TakeHit");
    }
}
