public class WeakenConnectionAction : IAction
{
    private Edge edge;
    private float delta;

    public WeakenConnectionAction(Edge edge, float delta = 0.1f)
    {
        this.edge = edge;
        this.delta = delta;
    }

    public void Execute(Node self, Graph graph)
    {
        edge.weight = System.Math.Max(0, edge.weight - delta);
    }

    public float Evaluate(Node self, Graph graph)
    {
        edge.weight -= delta;
        float score = self.Evaluate(graph);
        edge.weight += delta;
        return score;
    }
}