using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public static class PathFinding2D
{
    static List<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        var neighbors = new List<Vector2Int>();
        neighbors.Add(new Vector2Int(pos.x, pos.y + 1));
        neighbors.Add(new Vector2Int(pos.x, pos.y - 1));
        neighbors.Add(new Vector2Int(pos.x + 1, pos.y));
        neighbors.Add(new Vector2Int(pos.x - 1, pos.y));
        return neighbors;
    }
    static float GetDistance(Vector2Int a, Vector2Int b)
    {
        float xDistance = a.x - b.x;
        float yDistance = a.y - b.y;
        return xDistance * xDistance + yDistance * yDistance;
    }

    public static List<Vector2Int> Astar(Vector2Int from, Vector2Int finalDest, Dictionary<Vector2Int, BlockType> map, BlockType passableValues)
    {
        var result = new List<Vector2Int>();
        if (from == finalDest)
        {
            result.Add(from);
            return result;
        }
        Node finalNode;
        List<Node> openList = new List<Node>();

        finalNode = null;
        if (FindDest(new Node(null, from, GetDistance(from, finalDest), 0), openList, map, finalDest, out finalNode, passableValues))
        {
            while (finalNode != null)
            {
                result.Add(finalNode.pos);
                finalNode = finalNode.preNode;
            }
        }
        result.Reverse();
        return result;
    }

    /// <summary>
    /// 1. 열려 있는 노드의 주변 노드를 모은다.
    /// 2. 열려 있는 노드중에서 가장 뎁스가 낮은 것들의 주변을 모두 모은다.
    /// 3. 열려 있는 노드중에 목표지점까지 예상 거리가 가장 작은 것부터 계산한다.
    /// 1 ~ 3를 목표지점에 도착할때까지 반복한다. (노드가(갈 수 있는 지역) 없어도 반복을 중단한다)
    /// </summary>
    static bool FindDest(Node currentNode, List<Node> openList,
                         Dictionary<Vector2Int, BlockType> map, Vector2Int finalDest, out Node finalNode, BlockType passableValues)
    {
        if (currentNode == null)
        {
            finalNode = null;
            return false;
        }
        else if (currentNode.pos == finalDest)
        {
            finalNode = currentNode;
            return true;
        }

        currentNode.open = false;
        openList.Add(currentNode);

        foreach (var item in GetNeighbors(currentNode.pos))
        {
            if (map.ContainsKey(item) && passableValues.HasFlag(map[item]))
            {
                findTemp(openList, currentNode, item, finalDest);
            }
        }
        var next = openList.FindAll(obj => obj.open).Min();
        return FindDest(next, openList, map, finalDest, out finalNode, passableValues);
    }

    static void findTemp(List<Node> openList, Node currentNode, Vector2Int from, Vector2Int to)
    {
        Node temp = openList.Find(obj => obj.pos == (from));
        if (temp == null)
        {
            temp = new Node(currentNode, from, GetDistance(from, to), currentNode.gScore + 1);
            openList.Add(temp);
        }
        else if (temp.open && temp.gScore > currentNode.gScore + 1)
        {
            temp.gScore = currentNode.gScore + 1;
            temp.fScore = temp.hScore + temp.gScore;
            temp.preNode = currentNode;
        }
    }

    class Node : IComparable
    {
        public Node preNode;
        public Vector2Int pos;
        public float fScore;		//  h + g 
        public float hScore;        // 최종 목적지에서 현재 노드의 예상 길이
        public float gScore;        // 지금까지 움직인 횟수 (첫번째 계산은 0, 진행될 수록 1씩 증가)
        public bool open = true;    // true면 찾아봐야할 길, false는 이미 찾아본길

        public Node(Node prePos, Vector2Int pos, float hScore, float gScore)
        {
            this.preNode = prePos;
            this.pos = pos;
            this.hScore = hScore;
            this.gScore = gScore;
            this.fScore = hScore + gScore;
        }

        public int CompareTo(object obj)
        {
            Node temp = obj as Node;

            if (temp == null) return 1;

            if (Mathf.Abs(this.fScore - temp.fScore) > 0.01f)
            {
                return this.fScore > temp.fScore ? 1 : -1;
            }

            if (Mathf.Abs(this.hScore - temp.hScore) > 0.01f)
            {
                return this.hScore > temp.hScore ? 1 : -1;
            }
            return 0;
        }

        public override string ToString()
        {
            return $"x:{pos.x},y:{pos.y}, {open}, f:{fScore}, g:{gScore}, h:{hScore}";
        }
    }
}