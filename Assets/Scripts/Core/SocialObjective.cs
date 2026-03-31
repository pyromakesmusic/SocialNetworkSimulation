public class SocialObjective : IObjective
{
    public float Evaluate(Node self, Graph graph)
    {
        float total = 0;
        foreach (var e in self.connections)
            if (e.type == EdgeType.Social) total += e.weight;
        return total;
    }
}