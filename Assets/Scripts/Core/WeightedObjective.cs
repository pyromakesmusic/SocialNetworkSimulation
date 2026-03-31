public class WeightedObjective
{
    public IObjective objective;
    public float weight;

    public WeightedObjective(IObjective obj, float w)
    {
        objective = obj;
        weight = w;
    }
}