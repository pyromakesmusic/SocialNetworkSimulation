public class AmbitiousObjective : IObjective
{
    public float Evaluate(Node self, Graph graph)
    {
        return graph.GetMaxDistanceFrom(self);
    }
}