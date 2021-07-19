using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundManager : SingletonMonoBehavior<GroundManager>
{
    public Vector2Int playerPos; // ���⼭ ���� ����
    public Dictionary<Vector2Int, int> map = new Dictionary<Vector2Int, int>();
    public List<int> passableValues = new List<int>();
    public Transform player;

    internal void OnTouch(Vector3 position)
    {
        Vector2Int findPos = new Vector2Int((int)position.x, (int)position.z);
        FindPath(findPos);
    }

    void FindPath(Vector2Int goalPos)
    {
        StopAllCoroutines();
        StartCoroutine(FindPathCo(goalPos));
    }
    IEnumerator FindPathCo(Vector2Int goalPos)
    {
        passableValues = new List<int>();
        passableValues.Add((int)BlockType.Walkable);

        // �ڽ��� ��� BlockInfo ã��.
        var blockInfos = GetComponentsInChildren<BlockInfo>();

        // ���� ä�� ����.
        foreach(var item in blockInfos)
        {
            var pos = item.transform.position;
            Vector2Int intPos = new Vector2Int((int)pos.x, (int)pos.z);
            map[intPos] = (int)item.blockType;
        }
        playerPos.x = (int)player.position.x;
        playerPos.y = (int)player.position.z;

        List<Vector2Int> path = PathFinding2D.find4(playerPos, goalPos, map, passableValues);
        if (path.Count == 0)
            Debug.Log("���� ����");
        else
        {
            Player.SelectPlayer.PlayAnimation("Walk");
            FollowTarget.Instance.SetTarget(Player.SelectPlayer.transform);
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
        }
    }
    public Ease moveEase = Ease.InBounce;
    public float moveTimePerUnit = 0.3f;
}
