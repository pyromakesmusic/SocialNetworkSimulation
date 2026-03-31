using UnityEngine;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    public GraphManager graphManager; // reference to get all nodes
    public float distance = 10f;
    public float zoomSpeed = 5f;
    public float rotateSpeed = 5f;
    public float panSpeed = 0.5f;

    private float yaw = 0f;
    private float pitch = 20f;
    private Vector3 targetPosition;

    void Start()
    {
        if (graphManager == null)
        {
            Debug.LogError("CameraController: Please assign a GraphManager!");
        }

        targetPosition = Vector3.zero;
    }

    void LateUpdate()
    {
        if (graphManager != null && graphManager.Graph != null)
        {
            targetPosition = GetNodeClusterCenter(graphManager.Graph.nodes);
        }

        // Orbit rotation with right mouse button
        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * rotateSpeed;
            pitch -= Input.GetAxis("Mouse Y") * rotateSpeed;
            pitch = Mathf.Clamp(pitch, 10f, 80f);
        }

        // Zoom with scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, 2f, 50f);

        // Pan with middle mouse button
        if (Input.GetMouseButton(2))
        {
            Vector3 pan = -transform.right * Input.GetAxis("Mouse X") * panSpeed
                          - transform.up * Input.GetAxis("Mouse Y") * panSpeed;
            targetPosition += pan;
        }

        // Update camera position
        Vector3 dir = new Vector3(0, 0, -distance);
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0);
        transform.position = targetPosition + rot * dir;
        transform.LookAt(targetPosition);
    }

    // Compute the average position of all node views
    private Vector3 GetNodeClusterCenter(List<Node> nodes)
    {
        if (nodes.Count == 0) return Vector3.zero;

        Vector3 sum = Vector3.zero;
        int count = 0;
        foreach (var node in nodes)
        {
            if (node.view != null)
            {
                sum += node.view.position;
                count++;
            }
        }
        return count > 0 ? sum / count : Vector3.zero;
    }
}