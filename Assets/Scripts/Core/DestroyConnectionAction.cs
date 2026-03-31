public class DestroyConnectionAction : IAction
{
    private Edge edge;

    public DestroyConnectionAction(Edge edge)
    {
        this.edge = edge;
    }

    public void Execute(Node self, Graph graph)
    {
        graph.RemoveEdge(edge);
    }

    public float Evaluate(Node self, Graph graph)
    {
        // Temporarily remove
        graph.RemoveEdge(edge);

        float score = self.Evaluate(graph);

        // Restore
        graph.RestoreEdge(edge);

        return score;
    }
}