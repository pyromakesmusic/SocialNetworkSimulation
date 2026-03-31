using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class EdgeView : MonoBehaviour
{
    public Edge edge;       // Logical edge

    private LineRenderer lr;

    // Width range
    public float minWidth = 0.1f;
    public float maxWidth = 0.3f;

    // Weight range for width scaling
    public float minWeight = 1f;
    public float maxWeight = 10f;

    // Map EdgeType to colors
    private static readonly Dictionary<EdgeType, Color> EdgeColors = new Dictionary<EdgeType, Color>
    {
        { EdgeType.Social, Color.green },
        { EdgeType.Enemy, Color.red },
        { EdgeType.Love, Color.magenta },
        { EdgeType.Family, Color.cyan },
        { EdgeType.Business, Color.yellow }
    };

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();

        // Ensure LineRenderer has at least 2 positions
        lr.positionCount = 2;

        // Use a simple material that supports color
        if (lr.material == null)
        {
            lr.material = new Material(Shader.Find("Sprites/Default"));
        }
    }

    private void Update()
    {
        // Only draw if we have a valid edge and its node transforms
        if (edge == null || edge.nodeA?.view == null || edge.nodeB?.view == null) return;

        // Update positions
        lr.SetPosition(0, edge.nodeA.view.position);
        lr.SetPosition(1, edge.nodeB.view.position);

        // Determine color based on type
        Color baseColor;
        if (!EdgeColors.TryGetValue(edge.type, out baseColor))
            baseColor = Color.white;

        lr.startColor = lr.endColor = baseColor;

        // Scale width based on weight
        float t = Mathf.InverseLerp(minWeight, maxWeight, edge.weight);
        float width = Mathf.Lerp(minWidth, maxWidth, t);
        float intensity = Mathf.Lerp(0.5f, 1f, t);
        lr.startWidth = lr.endWidth = width;

        Color colorWithAlpha = new Color(baseColor.r * intensity, baseColor.g * intensity, baseColor.b * intensity, 1f);
        lr.startColor = lr.endColor = colorWithAlpha;
    }
}