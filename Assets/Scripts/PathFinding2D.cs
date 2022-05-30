using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public static class PathFinding2D
{
    /**
     * find a path in grid tilemaps
     */
    public static List<Vector2Int> Find(Vector2Int from, Vector2Int to, Dictionary<Vector2Int, BlockInfo> map, BlockType passableValues)
    {
        return Astar(from, to, map, passableValues);
    }
    static float GetDistance(Vector2Int a, Vector2Int b)
    {
        float xDistance = Mathf.Abs(a.x - b.x);
        float yDistance = Mathf.Abs(a.y - b.y);
        return xDistance * xDistance + yDistance * yDistance;
    }

    static List<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        var neighbors = new List<Vector2Int>();
        neighbors.Add(new Vector2Int(pos.x, pos.y + 1));
        neighbors.Add(new Vector2Int(pos.x, pos.y - 1));
        neighbors.Add(new Vector2Int(pos.x + 1, pos.y));
        neighbors.Add(new Vector2Int(pos.x - 1, pos.y));
        return neighbors;
    }


    static List<Vector2Int> Astar(Vector2Int from, Vector2Int to, Dictionary<Vector2Int, BlockInfo> map, BlockType passableValues)
    {
        var result = new List<Vector2Int>();
        if (from == to)
        {
            result.Add(from);
            return result;
        }
        Node finalNode;
        List<Node> open = new List<Node>();
        if (FindDest(new Node(null, from, GetDistance(from, to), 0), open, map, to, out finalNode, passableValues))
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
                         Dictionary<Vector2Int, BlockInfo> map, Vector2Int to, out Node finalNode, BlockType passableValues)
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
        openList.Add(currentNode);

        foreach (var item in GetNeighbors(currentNode.pos))
        {
            //BlockType itemBlockType = map[item].blockType;
            //if (to == item)
            //    itemBlockType &= ~BlockType.Player;

            if (map.ContainsKey(item) && 
                (passableValues.HasFlag(map[item].blockType) || to == item))
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
            temp.preNode = currentNode;
        }
    }


    //https://ko.wikipedia.org/wiki/A*_%EC%95%8C%EA%B3%A0%EB%A6%AC%EC%A6%98
    class Node :IComparable
    {
        public Node preNode;
        public Vector2Int pos;
        public float fScore => hScore + gScore;
        public float hScore;    //꼭짓점 n으로부터 목표 꼭짓점까지의 추정 경로 가중치
        public float gScore;    //출발 꼭짓점으로부터 꼭짓점 n까지의 경로 가중치
        public bool open = true;

        public Node(Node prePos, Vector2Int pos, float hScore, float gScore)
        {
            this.preNode = prePos;
            this.pos = pos;
            this.hScore = hScore;
            this.gScore = gScore;
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
    }
}