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
        return (a - b).sqrMagnitude;
    }

    public static List<Vector2Int> Astar(Vector2Int from, Vector2Int to, Dictionary<Vector2Int, int> map, List<int> passableValues)
    {
        var result = new List<Vector2Int>();
        if (from == to)
        {
            result.Add(from);
            return result;
        }
        List<Node> openList = new List<Node>();
        Node firstNode = new Node(null, from, GetDistance(from, to), 0);
        openList.Add(firstNode);

        if (FindDest(firstNode, openList, map, to, out Node finalNode, passableValues))
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

    static bool FindDest(Node currentNode, List<Node> openList,
                         Dictionary<Vector2Int, int> map, Vector2Int to, out Node finalNode, List<int> passableValues)
    {
        if (currentNode == null) {
            finalNode = null;
            return false;
        }
        else if (currentNode.pos == to)
        {
            finalNode = currentNode;
            return true;
        }

        currentNode.open = false;

        foreach (var item in GetNeighbors(currentNode.pos))
        {
            if (map.ContainsKey(item) && passableValues.Contains(map[item]))
            {
                FindTemp(openList, currentNode, item, to);
            }
        }
        var next = openList.FindAll(obj => obj.open).Min();
        return FindDest(next, openList, map, to, out finalNode, passableValues);
    }

    static void FindTemp(List<Node> openList, Node currentNode, Vector2Int from, Vector2Int to)
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

    class Node:IComparable
    {
        public Node preNode;
        public Vector2Int pos;
        public float fScore;    //  h + g
        public float hScore;    // 목표지점에서의 길이
        public int gScore;      // 계산된 스텝 (첫번째 계산은 0, 진행될 수록 1씩 증가)
        public bool open = true; // true면 찾아봐야할 길, false는 이미 찾아본길

        public Node(Node prePos, Vector2Int pos, float hScore, int gScore)
        {
            this.preNode = prePos;
            this.pos = pos;
            this.hScore = hScore;
            this.gScore = gScore;
            this.fScore = hScore + gScore;
        }

        public int CompareTo(object obj)
        {
            if (!(obj is Node temp)) return 1;

            if (Mathf.Abs(this.fScore - temp.fScore) > 0.01f) {
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