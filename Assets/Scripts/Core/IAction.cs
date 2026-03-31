public interface IAction
{
    void Execute(Node self, Graph graph);
    float Evaluate(Node self, Graph graph);
}