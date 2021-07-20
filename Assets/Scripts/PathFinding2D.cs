using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public static class PathFinding2D
{
    /**
     * find a path in grid tilemaps
     */
    public static List<Vector2Int> find4(Vector2Int from, Vector2Int to, Dictionary<Vector2Int, int> map, List<int> passableValues)
    {
        return astar(from, to, map, passableValues);
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
    static float GetDistance(Vector2Int a, Vector2Int b)
    {
        float xDistance = a.x - b.x;
        float yDistance = a.y - b.y;
        return xDistance * xDistance + yDistance * yDistance;
    }

    static List<Vector2Int> astar(Vector2Int from, Vector2Int to, Dictionary<Vector2Int, int> map, List<int> passableValues)
    {
        var result = new List<Vector2Int>();
        if (from == to)
        {
            result.Add(from);
            return result;
        }
        Node finalNode;
        List<Node> openList = new List<Node>();

        finalNode = null;
        if (FindDest(new Node(null, from, GetDistance(from, to), 0), openList, map, to, out finalNode, passableValues))
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
        openList.Add(currentNode);

        foreach (var item in GetNeighbors(currentNode.pos))
        {
            if (map.ContainsKey(item) && passableValues.Contains(map[item]))
            {
                findTemp(openList, currentNode, item, to);
            }
        }
        var next = openList.FindAll(obj => obj.open).Min();
        return FindDest(next, openList, map, to, out finalNode, passableValues);
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

    class Node:IComparable
    {
        public Node preNode;
        public Vector2Int pos;
        public float fScore;
        public float hScore;
        public float gScore;
        public bool open = true;

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