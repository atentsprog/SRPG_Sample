using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Actor
{
    static public Player SelectPlayer;
    Animator animator;
    void Start()
    {
        SelectPlayer = this;
        animator = GetComponentInChildren<Animator>();
        GroundManager.Instance.AddBlockInfo(transform.position, BlockType.Player, this);
    }

    public void PlayAnimation(string nodeName)
    {
        animator.Play(nodeName, 0, 0);
    }

    internal void OnTouch(Vector3 position)
    {
        Vector2Int findPos = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
        FindPath(findPos);
    }
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
        var map = GroundManager.Instance.map;
        List<Vector2Int> path = PathFinding2D.find4(playerPos, goalPos, map, passableValues);
        if (path.Count == 0)
            Debug.Log("길이 없다");
        else
        {
            // 월래 위치에선 플레이어 정보 삭제
            GroundManager.Instance.RemoveBlockInfo(Player.SelectPlayer.transform.position, BlockType.Player);
            Player.SelectPlayer.PlayAnimation("Walk");
            FollowTarget.Instance.SetTarget(Player.SelectPlayer.transform);
            path.RemoveAt(0);
            foreach (var item in path)
            {
                Vector3 playerNewPos = new Vector3(item.x, 0, item.y);
                player.LookAt(playerNewPos);
                //player.position = playerNewPos;
                player.DOMove(playerNewPos, moveTimePerUnit).SetEase(moveEase);
                yield return new WaitForSeconds(moveTimePerUnit);
            }
            Player.SelectPlayer.PlayAnimation("Idle");
            FollowTarget.Instance.SetTarget(null);
            // 이동한 위치에는 플레이어 정보 추가
            GroundManager.Instance.AddBlockInfo(Player.SelectPlayer.transform.position, BlockType.Player, this);
        }
    }

    internal bool OnMoveable(Vector3 position)
    {
        Vector2Int goalPos = position.ToVector2Int();
        Vector2Int playerPos = transform.position.ToVector2Int();
        var map = GroundManager.Instance.map;
        var path = PathFinding2D.find4(playerPos, goalPos, map, passableValues);
        if (path.Count == 0)
            Debug.Log("길 업따 !");
        else if (path.Count > 5)
            Debug.Log("이동모태 !");
        else
            return true;

        return false;
    }

    public Ease moveEase = Ease.InBounce;
    public float moveTimePerUnit = 0.3f;
}
