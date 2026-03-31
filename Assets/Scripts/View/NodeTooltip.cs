using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // or TMPro if using TextMeshPro

public class NodeTooltip : MonoBehaviour
{
    public Canvas canvas;
    public GameObject tooltipPanel;
    public Text tooltipText; // Or TMP_Text if using TextMeshPro

    private Camera mainCam;

    void Awake()
    {
        mainCam = Camera.main;
        tooltipPanel.SetActive(false);
    }

    void Update()
    {
        // Move tooltip with mouse
        if (tooltipPanel.activeSelf)
        {
            Vector3 mousePos = Input.mousePosition;
            tooltipPanel.transform.position = mousePos + new Vector3(10f, -10f, 0f); // offset
        }

        HandleHover();
    }

    void HandleHover()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            NodeView nodeView = hit.collider.GetComponent<NodeView>();
            if (nodeView != null)
            {
                Node node = nodeView.node;
                tooltipPanel.SetActive(true);

                // Count connections dynamically by EdgeType
                var typeCounts = new Dictionary<EdgeType, int>();
                foreach (EdgeType type in Enum.GetValues(typeof(EdgeType)))
                    typeCounts[type] = 0;

                foreach (var edge in node.connections)
                    typeCounts[edge.type]++;

                // Build connections string dynamically
                string connectionsStr = "";
                foreach (var kvp in typeCounts)
                    connectionsStr += $"{kvp.Key}={kvp.Value}, ";

                connectionsStr = connectionsStr.TrimEnd(',', ' ');

                // Set tooltip text
                tooltipText.text = $@"Node {node.id}
Connections: {connectionsStr}
Score: {node.Evaluate(GraphManagerInstance.Graph):F2}
Resources: {node.resources:F2}";
                return;
            }
        }

        // Not hovering over a node
        tooltipPanel.SetActive(false);
    }

    // Optional: reference your GraphManager instance if needed
    public GraphManager GraphManagerInstance;
}