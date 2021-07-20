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



    public void SetSelectPlayer()
    {
        SelectPlayer = this;
        //이동 가능한 영역 표시.
        //5칸 표시하자.

        var currentPos = transform.position.ToIntVector2();
        var map = GroundManager.Instance.map;   
        List<List<Vector2Int>> moveableArea = new List<List<Vector2Int>>();
        moveableArea.Add(new List<Vector2Int>() { currentPos });
        for (int step = 1; step < moveableDistance; step++)
        {
            var newLine = new List<Vector2Int>();
            // 뎁스 i로 갈 수 있는 모든 지역을 넣자.
            for (int i = 0; i < step; i++)
            {
                Vector2Int upP = currentPos + new Vector2Int(0, i);
                if (map.ContainsKey(upP) && passableValues.HasFlag(map[upP]))
                    newLine.Add(upP);

                Vector2Int downP = currentPos + new Vector2Int(0, -i);
                if (map.ContainsKey(downP) && passableValues.HasFlag(map[downP]))
                    newLine.Add(downP);

                Vector2Int upRightP = currentPos + new Vector2Int(step - i, i);
                if (map.ContainsKey(upRightP) && passableValues.HasFlag(map[upRightP]))
                    newLine.Add(upRightP);


                Vector2Int downRightP = currentPos + new Vector2Int(step - i, -i);
                if (map.ContainsKey(downRightP) && passableValues.HasFlag(map[downRightP]))
                    newLine.Add(downRightP);

                Vector2Int upLeftP = currentPos + new Vector2Int(-step + i, i);
                if (map.ContainsKey(upLeftP) && passableValues.HasFlag(map[upLeftP]))
                    newLine.Add(upLeftP);

                Vector2Int downLeftP = currentPos + new Vector2Int(-step + i, -i);
                if (map.ContainsKey(downLeftP) && passableValues.HasFlag(map[downLeftP]))
                    newLine.Add(downLeftP);
            }
            moveableArea.Add(newLine);
        }

        StartCoroutine(ShowMoveableAreaCo(moveableArea));
    }

    public List<GameObject> moveableAreaEffects = new List<GameObject>();
    private IEnumerator ShowMoveableAreaCo(List<List<Vector2Int>> moveableArea)
    {
        moveableAreaEffects.ForEach(x => Destroy(x));
        moveableAreaEffects.Clear();

        for (int lineIndex = 0; lineIndex < moveableArea.Count; lineIndex++)
        {
            var line = moveableArea[lineIndex];
            foreach(var item in line)
            {
                //item에 해당하는 곳에 임시 이펙트 생성.
                 var go = Instantiate(moveableAreaEffect, item.TotVector3(), Quaternion.identity);
                moveableAreaEffects.Add(go);
            }
            yield return new WaitForSeconds(0.3f);
        }
    }
    public GameObject moveableAreaEffect;
    public int moveableDistance = 5;
}
