using DG.Tweening;
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
    static public Vector3 ToVector2Int(this Vector2Int v2Int, float y)
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
    public Vector2Int playerPos; // ���⼭ ���� ����
    //public Dictionary<Vector2Int, BlockType> blockInfoMap = new Dictionary<Vector2Int, BlockType>(); // A*���� ���
    public Dictionary<Vector2Int, BlockInfo> blockInfoMap = new Dictionary<Vector2Int, BlockInfo>();
    public BlockType passableValues = BlockType.Walkable | BlockType.Water;
    public Transform player;

    public bool useDebugMode = true;
    public GameObject debugTextPrefab;
    new private void Awake()
    {
        base.Awake();

        // �ڽ��� ��� BlockInfo ã��.
        var blockInfos = GetComponentsInChildren<BlockInfo>();
        // ���� ä�� ����.

        debugTextGos.ForEach(x => Destroy(x));  // ���� ������ �ִ� ����׿� �ؽ�Ʈ ����
        debugTextGos.Clear();
        foreach (var item in blockInfos)
        {
            var pos = item.transform.position;
            //Vector2Int intPos = new Vector2Int((int)pos.x, (int)pos.z); <-- ���� �ڵ�
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



    public void AddBlockInfo(Vector3 position, BlockType addBlockType, Actor actor)
    {
        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
        if (blockInfoMap.ContainsKey(pos) == false)
        {
            Debug.LogError($"{pos} ��ġ�� ���� ����");
        }

        //map[pos] = map[pos] | addBlockType;   // ���� ���� �߰��ϰڴ�.
        //blockInfoMap[pos] |= addBlockType;               // ���� ���� �߰��ϰڴ�.
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
            Debug.LogError($"{pos} ��ġ�� ���� ����");
        }

        //blockInfoMap[pos] &= ~removeBlockType;               // ���� ������ �����ϰڴ�.
        blockInfoMap[pos].blockType &= ~removeBlockType;
        blockInfoMap[pos].actor = null;
        if (useDebugMode)
            blockInfoMap[pos].UpdateDebugInfo();
    }
}
