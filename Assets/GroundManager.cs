using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class GroundManagerExtension
{
    static public Vector2Int ToIntVector2(this Vector3 vector3)
    {
        return new Vector2Int(Mathf.RoundToInt(vector3.x), Mathf.RoundToInt(vector3.z));
    }
}

public class GroundManager : SingletonMonoBehavior<GroundManager>
{
    public Dictionary<Vector2Int, BlockType> map = new Dictionary<Vector2Int, BlockType>();
   
    public bool useDebugMode = true;
    public GameObject debugTextPrefab;
    new private void Awake()
    {
        base.Awake();

        // �ڽ��� ��� BlockInfo ã��.
        var blockInfos = GetComponentsInChildren<BlockInfo>();

        // ���� ä�� ����.
        foreach (var item in blockInfos)
        {
            var pos = item.transform.position;
            Vector2Int intPos = new Vector2Int((int)pos.x, (int)pos.z);
            map[intPos] = item.blockType;

            if (useDebugMode)
            {
                string posString = $"{intPos.x}:{intPos.y}";
                item.name = $"{item.name}:: {posString}";
                GameObject textMeshGo = Instantiate(debugTextPrefab, item.transform);
                textMeshGo.transform.localPosition = Vector3.zero;
                TextMesh textMesh = textMeshGo.GetComponent<TextMesh>();
                textMesh.text = posString;
            }
        }
    }
}
