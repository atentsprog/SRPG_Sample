﻿using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

static public class GroundExtention
{
    static public Vector2Int ToVector2Int(this Vector3 v3)
    {
        return new Vector2Int( Mathf.RoundToInt(v3.x)
            , Mathf.RoundToInt(v3.z));
    }
    static public Vector3 ToVector3(this Vector2Int v2Int, float y)
    {
        return new Vector3(v2Int.x, y, v2Int.y);
    }
    static public Vector3 ToVector3Snap(this Vector3 v3)
    {
        return new Vector3(Mathf.Round(v3.x), v3.y, Mathf.Round(v3.z));
    }
}

public class GroundManager : SingletonMonoBehavior<GroundManager>
{
    public Vector2Int playerPos; // 여기서 부터 시작
    //public Dictionary<Vector2Int, BlockType> blockInfoMap = new Dictionary<Vector2Int, BlockType>(); // A*에서 사용
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
            //Vector2Int intPos = new Vector2Int((int)pos.x, (int)pos.z); <-- 에러 코드
            Vector2Int intPos = pos.ToVector2Int();
            //blockInfoMap[intPos] = item.blockType;

            if (useDebugMode)
            {
                item.UpdateDebugInfo();
            }
            blockInfoMap[intPos] = item;
        }
    }

    public List<GameObject> debugTextGos = new List<GameObject>();


    public void AddBlockInfo(Vector3 position, BlockType addBlockType, ItemData dropItem)
    {
        // 씬에 드랍 아이템을 보여주자.
        var dropItemGo =(GameObject)Instantiate(Resources.Load("DropItem"));
        var sprite = Resources.Load("Icon/" + dropItem.iconName, typeof(Sprite));
        dropItemGo.GetComponentInChildren<SpriteRenderer>().sprite =(Sprite)sprite;
        dropItemGo.transform.position = position;
        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
        blockInfoMap[pos].blockType |= addBlockType;
        blockInfoMap[pos].dropItemID = dropItem.ID;
        blockInfoMap[pos].dropItemGo = dropItemGo;
        if (useDebugMode)
            blockInfoMap[pos].UpdateDebugInfo();
    }

    public void AddBlockInfo(Vector3 position, BlockType addBlockType, Actor actor)
    {
        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
        if (blockInfoMap.ContainsKey(pos) == false)
        {
            Debug.LogError($"{pos} 위치에 맵이 없다");
        }

        //map[pos] = map[pos] | addBlockType;   // 기존 값에 추가하겠다.
        //blockInfoMap[pos] |= addBlockType;               // 기존 값에 추가하겠다.
        blockInfoMap[pos].blockType |= addBlockType;
        blockInfoMap[pos].actor = actor;
        if (useDebugMode)
            blockInfoMap[pos].UpdateDebugInfo();
    }
    public void RemoveBlockInfo(Vector3 position, BlockType removeBlockType)
    {
        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
        if (blockInfoMap.ContainsKey(pos) == false)
        {
            Debug.LogError($"{pos} 위치에 맵이 없다");
        }

        //blockInfoMap[pos] &= ~removeBlockType;               // 기존 값에서 삭제하겠다.
        blockInfoMap[pos].blockType &= ~removeBlockType;
        blockInfoMap[pos].actor = null;
        if (useDebugMode)
            blockInfoMap[pos].UpdateDebugInfo();
    }

    internal BlockInfo GetBlockInfo(Vector3 position)
    {
        var intPos = position.ToVector2Int();
        blockInfoMap.TryGetValue(intPos, out BlockInfo result);
        return result;
    }

    internal void RemoveItemInfo(Vector3 position)
    {
        RemoveBlockInfo(position, BlockType.Item);
        GetBlockInfo(position).DeleteItemInfo();
    }
}
