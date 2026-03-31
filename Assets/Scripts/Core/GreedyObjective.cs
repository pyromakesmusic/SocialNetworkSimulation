public class GreedyObjective : IObjective
{
    public float Evaluate(Node self, Graph graph) => self.resources;
}