using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundManager : MonoBehaviour
{
    public Vector2Int playerPos; // ���⼭ ���� ����
    public Vector2Int goalPos; // ���⸦ ã��
    public Dictionary<Vector2Int, int> map = new Dictionary<Vector2Int, int>();
    public List<int> passableValues = new List<int>();

    public Transform player;
    public Transform goal;

    [ContextMenu("��ã�� �׽�Ʈ")]
    void Start()
    {
        StartCoroutine(FindPathCo());
    }
    IEnumerator FindPathCo()
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
        goalPos.x = (int)goal.position.x;
        goalPos.y = (int)goal.position.z;

        List<Vector2Int> path = PathFinding2D.find4(playerPos, goalPos, map, passableValues);
        if (path.Count == 0)
            Debug.Log("���� ����");
        else
        {
            foreach (var item in path)
            {
                Vector3 playerNewPos = new Vector3(item.x, 0, item.y);
                player.position = playerNewPos;
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}
