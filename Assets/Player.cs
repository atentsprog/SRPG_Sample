using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    static public Player SelectPlayer;
    Animator animator;
    void Start()
    {
        SelectPlayer = this;
        animator = GetComponentInChildren<Animator>();
        GroundManager.Instance.AddTileInfo(transform.position, BlockType.Player);
    }

    public void PlayAnimation(string nodeName)
    {
        animator.Play(nodeName, 0, 0);
    }

    public BlockType passableValues;

    internal void MoveTo(Vector2Int destination)
    {
        Dictionary<Vector2Int, BlockType> map = GroundManager.Instance.map;

        var beginPos = transform.position.ToIntVector2();
        List<Vector2Int> path = PathFinding2D.Astar(beginPos, destination, map, passableValues);
        if (path.Count == 0)
        {
            Debug.Log("길이 없다");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(MoveToCo(path));
    }

    private IEnumerator MoveToCo(List<Vector2Int> path)
    {
        var startPos = path[0];
        var destination = path[path.Count - 1];
        GroundManager.Instance.RemoveTileInfo(transform.position, BlockType.Player);
        PlayAnimation("Walk");
        FollowTarget.Instance.SetTarget(Player.SelectPlayer.transform);
        SelectedTile.Instance.SetPosition(destination);

        GroundManager.Instance.AddTileInfo(transform.position, BlockType.Player);
        foreach (var item in path)
        {
            Vector3 playerNewPos = new Vector3(item.x, 0, item.y);
            transform.LookAt(playerNewPos);

            transform.DOMove(playerNewPos, moveTimePerUnit).SetEase(moveEase);
            yield return new WaitForSeconds(moveTimePerUnit);
        }
        PlayAnimation("Idle");
        FollowTarget.Instance.SetTarget(null);
        SelectedTile.Instance.Hide();

        GroundManager.Instance.AddTileInfo(transform.position, BlockType.Player);
    }
    public Ease moveEase = Ease.Linear;
    public float moveTimePerUnit = 0.3f;
}
