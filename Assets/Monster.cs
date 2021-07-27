using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Monster : Actor
{
    public static List<Monster> Monsters = new List<Monster>();
    public override ActorTypeEnum ActorType { get => ActorTypeEnum.Monster; }

    new protected void Awake()
    {
        base.Awake();
        Monsters.Add(this);
    }
    new protected void OnDestroy()
    {
        base.OnDestroy();
        Monsters.Remove(this);
    }
    void Start()
    {
        GroundManager.Instance.AddBlockInfo(transform.position, BlockType.Monster, this);
        animator = GetComponentInChildren<Animator>();
    }

    protected override void OnDie()
    {
        print("몬스터가 죽었다. 게임 끝났는지 확인하자.");
        if (Monsters.Where(x => x.status != StatusType.Die).Count() == 0)
        {
            //플레이어가 모두 죽었다
            CenterNotifyUI.Instance.Show("당신의 승리");
            StageManager.GameState = GameStateType.StageClear;
        }
    }

    //internal override void TakeHit(int power)
    //{
    //    //맞은 데미지 표시하자.
    //    GameObject damageTextGo = (GameObject)Instantiate(Resources.Load("DamageText"), transform);
    //    damageTextGo.transform.localPosition = new Vector3(0, 1.3f, 0);
    //    damageTextGo.GetComponent<TextMeshPro>().text = power.ToString();
    //    Destroy(damageTextGo, 2);

    //    hp -= power;
    //    animator.Play("TakeHit");
    //}

    internal IEnumerator AutoPlay()
    {
        // 가장 가까운 플레이어 확인.
        // 공격범위안에 적이 있는지 확인.
        // 있다면 바로 공격 없다면 가장 가까운 플레이어 방향으로 이동.

        Player nearestPlayer = FindNearestPlayer();
        if (IsAttackablePosition(nearestPlayer.transform.position))
        {
            yield return AttackToTargetCo(nearestPlayer);
        }
        else
        {
            yield return MoveToPositionCo(nearestPlayer.transform);

            if (IsAttackablePosition(nearestPlayer.transform.position))
            {
                yield return AttackToTargetCo(nearestPlayer);
            }
        }
    }

    private IEnumerator MoveToPositionCo(Transform destTr)
    {
        Vector2Int destPos = destTr.position.ToVector2Int();
        Transform myTr = transform;
        Vector2Int myPos = myTr.position.ToVector2Int();


        // 몬스터가 있는곳에는 이동시키자 말자.

        var map = GroundManager.Instance.blockInfoMap;
        List<Vector2Int> path = PathFinding2D.find4(myPos, destPos, map, passableValues);
        if (path.Count == 0)
            Debug.Log("길이 없다");
        else
        {
            // 월래 위치에선 몬스터 정보 삭제
            GroundManager.Instance.RemoveBlockInfo(myTr.position, BlockType.Monster);
            PlayAnimation("Walk");
            FollowTarget.Instance.SetTarget(myTr);
            path.RemoveAt(0);   // 자기위치
            path.RemoveAt(path.Count - 1);  // 플레이어 위치.

            // 최대 이동거리만큼 이동하자.
            if (path.Count > moveDistance)
                path.RemoveRange(moveDistance, path.Count - moveDistance);
            //예) 이동가능거리가 2인데 패스가 3개라면
            // 3 > 2
            // (2, 1)

            foreach (var item in path)
            {
                Vector3 playerNewPos = new Vector3(item.x, 0, item.y);
                myTr.LookAt(playerNewPos);
                myTr.DOMove(playerNewPos, moveTimePerUnit);
                yield return new WaitForSeconds(moveTimePerUnit);
            }
            PlayAnimation("Idle");
            FollowTarget.Instance.SetTarget(null);

            // 이동한 위치에는 정보 추가
            GroundManager.Instance.AddBlockInfo(myTr.position, BlockType.Monster, this);

            completeMove = true;
        }
    }

    public float attackTime = 1;
    private IEnumerator AttackToTargetCo(Player attackTarget)
    {
        transform.LookAt(attackTarget.transform);

        animator.Play("Attack");
        yield return attackTarget.TakeHitCo(power);
        yield return new WaitForSeconds(attackTime);

        completeAct = true;
    }

    private Player FindNearestPlayer()
    {
        var myPos = transform.position;
        var nearestPlayer = Player.Players
            .Where( x => x.status != StatusType.Die)
            .OrderBy(x => Vector3.Distance(myPos, x.transform.position))
            .First();
        return nearestPlayer;
    }
}
