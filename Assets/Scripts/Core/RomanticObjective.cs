using UnityEngine;

public class RomanticObjective : IObjective
{
    public float Evaluate(Node self, Graph graph)
    {
        float maxLove = 0;
        foreach (var e in self.connections)
            if (e.type == EdgeType.Love) maxLove = Mathf.Max(maxLove, e.weight);
        return maxLove;
    }
}