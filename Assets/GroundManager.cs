using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GroundManager : SingletonMonoBehavior<GroundManager>
{
    public Vector2Int playerPos; // 여기서 부터 시작
    public Dictionary<Vector2Int, BlockType> map = new Dictionary<Vector2Int, BlockType>(); // A*에서 사용
    public Dictionary<Vector2Int, BlockInfo> blockInfoMap = new Dictionary<Vector2Int, BlockInfo>();
    public BlockType passableValues = BlockType.Walkable | BlockType.Water;
    public Transform player;

    public bool useDebugMode = true;
    public GameObject debugTextPrefab;
    new private void Awake()
    {
        base.Awake();

        // 자식의 모든 BlockInfo 찾자.
        var blockInfos = GetComponentsInChildren<BlockInfo>();
        // 맵을 채워 넣자.

        debugTextGos.ForEach(x => Destroy(x));  // 블럭에 기존에 있던 디버그용 텍스트 삭제
        debugTextGos.Clear();
        foreach (var item in blockInfos)
        {
            var pos = item.transform.position;
            Vector2Int intPos = new Vector2Int((int)pos.x, (int)pos.z);
            map[intPos] = item.blockType;

            if (useDebugMode)
            {
                item.UpdateDebugInfo();
            }
            blockInfoMap[intPos] = item;
            //    StringBuilder debugText = new StringBuilder();// $"{item.blockType}:{intPos.y}";
            //    //ContaingText(debugText, item, BlockType.Walkable);
            //    ContaingText(debugText, item, BlockType.Water);
            //    ContaingText(debugText, item, BlockType.Player);
            //    ContaingText(debugText, item, BlockType.Monster);

            //    //item.name = $"{item.name}:: {posString}";
            //    GameObject textMeshGo = Instantiate(debugTextPrefab, item.transform);
            //    debugTextGos.Add(textMeshGo);
            //    textMeshGo.transform.localPosition = Vector3.zero;
            //    TextMesh textMesh = textMeshGo.GetComponentInChildren<TextMesh>();
            //    textMesh.text = debugText.ToString();
            //}
        }
    }
    internal void OnTouch(Vector3 position)
    {
        //Vector2Int findPos = new Vector2Int((int)position.x, (int)position.z);
        Vector2Int findPos = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
        FindPath(findPos);
    }

    void FindPath(Vector2Int goalPos)
    {
        StopAllCoroutines();
        StartCoroutine(FindPathCo(goalPos));
    }

    public List<GameObject> debugTextGos = new List<GameObject>();
    IEnumerator FindPathCo(Vector2Int goalPos)
    {

        //playerPos.x = (int)player.position.x;
        //playerPos.y =(int)player.position.z;
        playerPos.x = Mathf.RoundToInt(player.position.x);
        playerPos.y = Mathf.RoundToInt(player.position.z);

        List<Vector2Int> path = PathFinding2D.find4(playerPos, goalPos, map, passableValues);
        if (path.Count == 0)
            Debug.Log("길이 없다");
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

    //private void ContaingText(StringBuilder sb, BlockInfo item, BlockType walkable)
    //{
    //    if (item.blockType.HasFlag(walkable))
    //    {
    //        sb.AppendLine(walkable.ToString());
    //    }
    //}

    public Ease moveEase = Ease.InBounce;
    public float moveTimePerUnit = 0.3f;


    internal void AddBlockInfo(Vector3 position, BlockType addBlockType)
    {
        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
        if (map.ContainsKey(pos) == false)
        {
            Debug.LogError($"{pos} 위치에 맵이 없다");
        }

        //map[pos] = map[pos] | addBlockType;   // 기존 값에 추가하겠다.
        map[pos] |= addBlockType;               // 기존 값에 추가하겠다.
        blockInfoMap[pos].blockType |= addBlockType;
        if (useDebugMode)
            blockInfoMap[pos].UpdateDebugInfo();
    }
}
