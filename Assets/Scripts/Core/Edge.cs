public enum EdgeType {
    Social,
    Enemy,
    Love,
    Family,
    Business
}

public class Edge
{
    public Node nodeA;
    public Node nodeB;
    public EdgeType type;
    public float weight;

    public Edge(Node a, Node b, EdgeType t, float w)
    {
        nodeA = a;
        nodeB = b;
        type = t;
        weight = w;
    }
}