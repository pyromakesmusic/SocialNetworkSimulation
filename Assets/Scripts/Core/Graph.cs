using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Graph
{
    public List<Node> nodes = new List<Node>();
    public List<Edge> edges = new List<Edge>();
    private int nextId = 0;

    public Node CreateNode()
    {
        var node = new Node();
        node.id = nextId++;
        nodes.Add(node);
        return node;
    }

    public Edge Connect(Node a, Node b, EdgeType type, float weight)
    {
        var edge = new Edge(a, b, type, weight);
        edges.Add(edge);
        a.connections.Add(edge);
        b.connections.Add(edge);
        return edge;
    }

    public int GetMaxDistanceFrom(Node start)
    {
        // Breadth-first search
        var visited = new HashSet<Node> { start };
        var queue = new Queue<(Node, int)>();
        queue.Enqueue((start, 0));
        int maxDist = 0;

        while (queue.Count > 0)
        {
            var (node, dist) = queue.Dequeue();
            maxDist = Mathf.Max(maxDist, dist);

            foreach (var edge in node.connections)
            {
                var neighbor = edge.nodeA == node ? edge.nodeB : edge.nodeA;
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue((neighbor, dist + 1));
                }
            }
        }
        return maxDist;
    }

    public void RemoveEdge(Edge edge)
    {
        edges.Remove(edge);
        edge.nodeA.connections.Remove(edge);
        edge.nodeB.connections.Remove(edge);
    }

    public void RestoreEdge(Edge edge)
    {
        edges.Add(edge);
        edge.nodeA.connections.Add(edge);
        edge.nodeB.connections.Add(edge);
    }
}