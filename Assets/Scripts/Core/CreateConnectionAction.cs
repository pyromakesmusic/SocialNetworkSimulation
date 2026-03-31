public class CreateConnectionAction : IAction
{
    private Node target;

    public CreateConnectionAction(Node target)
    {
        this.target = target;
    }

    public void Execute(Node self, Graph graph)
    {
        graph.Connect(self, target, EdgeType.Social, 0.5f);
    }

    public float Evaluate(Node self, Graph graph)
    {
        var edge = graph.Connect(self, target, EdgeType.Social, 0.5f);
        float score = self.Evaluate(graph);

        // rollback
        graph.edges.Remove(edge);
        self.connections.Remove(edge);
        target.connections.Remove(edge);

        return score;
    }
}