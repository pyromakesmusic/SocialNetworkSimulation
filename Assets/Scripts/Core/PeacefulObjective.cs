public class PeacefulObjective : IObjective
{
    public float Evaluate(Node self, Graph graph)
    {
        float penalty = 0;
        foreach (var e in self.connections)
            if (e.type == EdgeType.Enemy) penalty += e.weight;
        return -penalty;
    }
}