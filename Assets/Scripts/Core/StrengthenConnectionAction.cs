public class StrengthenConnectionAction : IAction
{
    private Edge edge;
    private float delta;

    public StrengthenConnectionAction(Edge edge, float delta = 0.1f)
    {
        this.edge = edge;
        this.delta = delta;
    }

    public void Execute(Node self, Graph graph)
    {
        edge.weight += delta;
    }

    public float Evaluate(Node self, Graph graph)
    {
        edge.weight += delta;
        float score = self.Evaluate(graph);
        edge.weight -= delta;
        return score;
    }
}