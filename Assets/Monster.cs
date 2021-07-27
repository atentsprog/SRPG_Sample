using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        // 가장 가까이에 있는 Player를 찾자.
        Player enemyPlayer = GetNearestPlayer();
        // 공격 가능한 위치에 있다면 바로 공격하자.
        if (IsInAttackableArea(enemyPlayer.transform.position))
        {
            // 바로 공격하자.
            yield return AttackToTargetCo(enemyPlayer);
        }
        else
        {
            // Player쪽으로 이동하자.
            yield return FindPathCo(enemyPlayer.transform.position.ToVector2Int());
            // 공격 할 수 있으면 공격하자.

            if (IsInAttackableArea(enemyPlayer.transform.position))
            {
                yield return AttackToTargetCo(enemyPlayer);
            }
        }
    }

    private Player GetNearestPlayer()
    {
        var myPos = transform.position;
        var nearestPlayer = Player.Players
            .Where(x => x.status != StatusType.Die)
            .OrderBy(x => Vector3.Distance(x.transform.position, myPos))
            .Single();

        return nearestPlayer;
    }


    public override ActorTypeEnum ActorType { get => ActorTypeEnum.Monster; }

    protected void Start()
    {
        GroundManager.Instance.AddBlockInfo(transform.position, BlockType.Monster, this);
        animator = GetComponentInChildren<Animator>();
    }

    public override BlockType GetBlockType()
    {
        return BlockType.Monster;
    }
}
