using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public int id;
    public float resources = 1f;
    public List<Edge> connections = new List<Edge>();
    public List<WeightedObjective> objectives = new List<WeightedObjective>();
    public float resourceTrend = 0f; // rolling average of recent gains/losses

    public Transform view; // For visualization
    public Vector3 velocity = Vector3.zero;

    public Vector3 Position
    {
        get
        {
            if (view != null)
            {
                Vector3 pos = view.position;
                pos.y = 0f; // keep on floor
                return pos;
            }
            else
                return Vector3.zero;
        }
        set
        {
            if (view != null)
            {
                Vector3 pos = value;
                pos.y = 0f; // clamp to floor
                view.position = pos;
            }
        }
    }


    public float Evaluate(Graph graph)
    {
        float score = 0f;
        foreach (var obj in objectives)
            score += obj.weight * obj.objective.Evaluate(this, graph);
        return score;
    }

    public List<IAction> GetPossibleActions(Graph graph)
    {
        var actions = new List<IAction>();
        int maxEdgesPerNode = 9;

        foreach (var other in graph.nodes)
        {
            if (other == this) continue;

            bool alreadyConnected = connections.Exists(e => e.nodeA == other || e.nodeB == other);

            if (!alreadyConnected && connections.Count < maxEdgesPerNode)
                actions.Add(new CreateConnectionAction(other));
        }

        // Strengthen or weaken existing connections
        foreach (var edge in connections)
        {
            actions.Add(new StrengthenConnectionAction(edge));
            actions.Add(new WeakenConnectionAction(edge));

            // Includes destroy edge possibility for weak connections
            if (edge.weight < 0.2f)
                actions.Add(new DestroyConnectionAction(edge));
        }

        // Try connecting to others
        foreach (var other in graph.nodes)
        {
            if (other == this) continue;

            bool alreadyConnected = connections.Exists(e =>
                e.nodeA == other || e.nodeB == other);

            if (!alreadyConnected)
                actions.Add(new CreateConnectionAction(other));
        }

        return actions;
    }

    public IAction ChooseBestAction(Graph graph)
    {
        var actions = GetPossibleActions(graph);

        float bestScore = Evaluate(graph);
        IAction bestAction = null;

        foreach (var action in actions)
        {
            float score = action.Evaluate(this, graph);
            if (score > bestScore)
            {
                bestScore = score;
                bestAction = action;
            }
        }

        return bestAction;
    }

    public Vector3 ComputeConnectionInfluence()
    {
        Vector3 force = Vector3.zero;

        foreach (var edge in connections)
        {
            if (edge.nodeA == this)
            {
                force += GetEdgeInfluence(edge, edge.nodeB);
            }
            else if (edge.nodeB == this)
            {
                force += GetEdgeInfluence(edge, edge.nodeA);
            }
        }

        return force;
    }

    private Vector3 GetEdgeInfluence(Edge edge, Node other)
    {
        if (other.view == null) return Vector3.zero;

        // Only consider XZ plane
        Vector3 dir = other.Position - this.Position;
        dir.y = 0;

        // Determine strength and direction based on type
        float multiplier = 1f; // default attraction
        switch (edge.type)
        {
            case EdgeType.Social:
            case EdgeType.Love:
            case EdgeType.Family:
            case EdgeType.Business:
                multiplier = 1f;  // move towards
                break;
            case EdgeType.Enemy:
                multiplier = -1f; // move away
                break;
        }

        return dir.normalized * multiplier * edge.weight;
    }
}