using UnityEngine;
using System.Collections.Generic;

public class GraphManager : MonoBehaviour
{
    public GameObject nodePrefab;
    public GameObject edgePrefab;
    [Header("Simulation Settings")]
    [Tooltip("Number of nodes to create at start.")]
    public int numberOfNodes = 5; // default if user doesn't change it

    [Tooltip("Maximum number of objectives per node.")]
    public int objectivesPerNode = 1; // default

    private Graph graph;

    public Graph Graph => graph;  // C# property

    public float tickInterval = 1f;
    private float timer = 0f;

    private Dictionary<Edge, GameObject> edgeObjects = new Dictionary<Edge, GameObject>();

    private List<Vector3> usedPositions = new List<Vector3>();
    private float minDistance = 1.0f; // minimum distance between nodes

    public Renderer floorRenderer;
    private Texture2D resourceTexture;
    public Transform floorTransform;   // drag your floor here in Inspector


    void Start()
    {
        resourceTexture = floorRenderer.material.mainTexture as Texture2D;

        graph = new Graph();

        // 1? Create nodes
        List<Node> nodes = new List<Node>();
        for (int i = 0; i < numberOfNodes; i++)
        {
            Node n = graph.CreateNode();

            // Assign objectives
            for (int j = 0; j < objectivesPerNode; j++)
            {
                n.objectives.Add(new WeightedObjective(GetRandomObjective(), 1f));
            }

            nodes.Add(n);
        }

        // 2? Spawn node views at random positions on the floor
        foreach (var n in nodes)
        {
            Vector3 pos = GetRandomStartPosition();
            SpawnNode(n, pos);
        }

        // 3? Optionally create random initial connections
        // For each node, try connecting to 1-2 other nodes randomly
        foreach (var n in nodes)
        {
            int connectionsToCreate = Random.Range(1, 3); // 1 or 2 connections
            for (int i = 0; i < connectionsToCreate; i++)
            {
                Node other = nodes[Random.Range(0, nodes.Count)];
                if (other == n) continue;

                bool alreadyConnected = n.connections.Exists(e => e.nodeA == other || e.nodeB == other);
                if (!alreadyConnected)
                {
                    EdgeType type = GetRandomEdgeType();
                    float weight = Random.Range(0.1f, 0.5f);
                    graph.Connect(n, other, type, weight);
                }
            }
        }

        // 4? Spawn edge views
        SpawnEdges();
    }

    // Helper: pick a random objective
    IObjective GetRandomObjective()
    {
        int pick = Random.Range(0, 5);
        switch (pick)
        {
            case 0: return new GreedyObjective();
            case 1: return new SocialObjective();
            case 2: return new PeacefulObjective();
            case 3: return new RomanticObjective();
            case 4: return new AmbitiousObjective();
            default: return new GreedyObjective();
        }
    }

    // Helper: pick a random edge type
    EdgeType GetRandomEdgeType()
    {
        int pick = Random.Range(0, 3);
        switch (pick)
        {
            case 0: return EdgeType.Social;
            case 1: return EdgeType.Enemy;
            case 2: return EdgeType.Love;
            default: return EdgeType.Social;
        }
    }

    // Helper: random position on floor
    Vector3 GetRandomStartPosition()
    {
        float range = 3f; // floor size
        Vector3 pos;
        int attempts = 0;

        do
        {
            pos = new Vector3(
                Random.Range(-range, range),
                0f, // floor
                Random.Range(-range, range)
            );
            attempts++;
            if (attempts > 50) break; // fallback to prevent infinite loop
        }
        while (usedPositions.Exists(p => Vector3.Distance(p, pos) < minDistance));

        usedPositions.Add(pos);
        return pos;
    }

    void SpawnNode(Node node, Vector3 pos)
    {
        // y is already 0 from GetRandomStartPosition()
        var go = Instantiate(nodePrefab, pos, Quaternion.identity);
        go.GetComponent<NodeView>().node = node;
        node.view = go.transform;
    }

    void SpawnEdges()
    {
        foreach (var edge in graph.edges)
        {
            var go = Instantiate(edgePrefab);
            var view = go.GetComponent<EdgeView>();
            view.edge = edge;      // only assign the edge
            edgeObjects[edge] = go; // track edge GameObject
        }
    }

    float SampleBrightness(Vector3 worldPos)
    {
        Vector2 uv = WorldToUV(worldPos, floorTransform, GetFloorSize());

        uv.x = Mathf.Clamp01(uv.x);
        uv.y = Mathf.Clamp01(uv.y);

        return resourceTexture.GetPixelBilinear(uv.x, uv.y).r;
    }

    Vector3 GetTerrainGradient(Vector3 pos, float sampleDistance)
    {
        float center = SampleBrightness(pos);

        float right = SampleBrightness(pos + new Vector3(sampleDistance, 0f, 0f));
        float left = SampleBrightness(pos - new Vector3(sampleDistance, 0f, 0f));

        float forward = SampleBrightness(pos + new Vector3(0f, 0f, sampleDistance));
        float back = SampleBrightness(pos - new Vector3(0f, 0f, sampleDistance));

        float dx = right - left;
        float dz = forward - back;

        return new Vector3(dx, 0f, dz);
    }

    Vector2 GetFloorSize()
    {
        return new Vector2(
            10f * floorTransform.localScale.x,
            10f * floorTransform.localScale.z
        );
    }

    Vector2 WorldToUV(Vector3 worldPos, Transform floorTransform, Vector2 floorSize)
    {
        Vector3 localPos = floorTransform.InverseTransformPoint(worldPos);

        float u = (localPos.x / floorSize.x) + 0.5f;
        float v = (localPos.z / floorSize.y) + 0.5f;

        return new Vector2(u, v);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= tickInterval)
        {
            timer = 0f;
            StepSimulation();
        }
    }
    void UpdateEdgeViews()
    {
        // Create new edges
        foreach (var edge in graph.edges)
        {
            if (!edgeObjects.ContainsKey(edge))
            {
                var go = Instantiate(edgePrefab);
                var view = go.GetComponent<EdgeView>();
                view.edge = edge;  // only assign edge
                edgeObjects[edge] = go;
            }
        }

        // Remove destroyed edges
        var toRemove = new List<Edge>();

        foreach (var kvp in edgeObjects)
        {
            if (!graph.edges.Contains(kvp.Key))
            {
                Destroy(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var edge in toRemove)
            edgeObjects.Remove(edge);
    }

    void StepSimulation()
    {
        // Parameters for distance-based decay
        float decayPerUnitDistance = 0.01f; // how much weight decreases per unit of distance per tick
        float minWeight = 0.1f; // clamp so edges never disappear completely

        foreach (var edge in graph.edges)
        {
            if (edge.nodeA.view == null || edge.nodeB.view == null) continue;

            // Compute XZ distance between nodes
            Vector3 delta = edge.nodeB.Position - edge.nodeA.Position;
            delta.y = 0f; // ignore vertical distance
            float distance = delta.magnitude;

            // Reduce weight proportional to distance
            edge.weight -= distance * decayPerUnitDistance;

            // Clamp to minimum
            if (edge.weight < minWeight)
                edge.weight = minWeight;
        }

        // Small chance to mutate edge types (except Family)
        float mutationChance = 0.01f; // 1% chance per edge per tick

        foreach (var edge in graph.edges)
        {
            if (Random.value < mutationChance)
            {
                // Get all EdgeTypes except the current type and Family
                EdgeType[] types = (EdgeType[])System.Enum.GetValues(typeof(EdgeType));
                List<EdgeType> otherTypes = new List<EdgeType>();
                foreach (var t in types)
                {
                    if (t != edge.type && t != EdgeType.Family)
                        otherTypes.Add(t);
                }

                if (otherTypes.Count > 0)
                    edge.type = otherTypes[Random.Range(0, otherTypes.Count)];
            }
        }

        foreach (var node in graph.nodes)
        {
            if (node.view == null) continue;

            float desperation = Mathf.Clamp01(-node.resourceTrend);

            // --- ACTION ---
            var action = node.ChooseBestAction(graph);
            action?.Execute(node, graph);

            Vector3 pos = node.view.position;

            float moveRange = 0.2f;
            float adjustedMoveRange = moveRange * (1f + desperation);
            moveRange = adjustedMoveRange;

            float maxInfluenceDistance = 5f;
            float separationDistance = 2f;
            float separationStrength = 1.5f;

            // --- RANDOM ---
            Vector3 randomMove = new Vector3(
                Random.Range(-moveRange, moveRange),
                0f,
                Random.Range(-moveRange, moveRange)
            );

            // --- CONNECTION INFLUENCE ---
            Vector3 influence = Vector3.zero;

            foreach (var edge in node.connections)
            {
                Node other = edge.nodeA == node ? edge.nodeB : edge.nodeA;
                if (other.view == null) continue;

                Vector3 dir = other.Position - node.Position;
                dir.y = 0f;

                float distance = dir.magnitude;
                if (distance > maxInfluenceDistance || distance == 0f) continue;

                float typeMultiplier = edge.type == EdgeType.Enemy ? -0.1f : 0.1f;
                float distanceFactor = 1f - (distance / maxInfluenceDistance);

                influence += dir.normalized * typeMultiplier * edge.weight * distanceFactor;

                float influenceScale = 1f + desperation;

                influence += dir.normalized * typeMultiplier * edge.weight * distanceFactor * influenceScale;
            }

            // --- SEPARATION ---
            foreach (var other in graph.nodes)
            {
                if (other == node || other.view == null) continue;

                Vector3 offset = node.Position - other.Position;
                offset.y = 0f;

                float distance = offset.magnitude;
                if (distance <= 0f || distance >= separationDistance) continue;

                influence += offset.normalized * (separationDistance - distance) * separationStrength;
            }


            float terrainStrength = (1f + desperation);
            float sampleDistance = 0.5f;    // how far to probe around the node

            Vector3 terrainGradient = GetTerrainGradient(node.view.position, sampleDistance);

            // Normalize safely
            Vector3 terrainDir = terrainGradient.sqrMagnitude > 0f
                ? terrainGradient.normalized
                : Vector3.zero;

            // Add to influence
            influence += terrainDir * terrainStrength;

            // --- FINAL MOVE ---
            Vector3 influenceDir = influence.sqrMagnitude > 0f
                ? influence.normalized
                : Vector3.zero;

            Vector3 desiredMove = randomMove + influenceDir * moveRange;

            float inertia = 0.85f;   // higher = more momentum (0.8–0.95 good range)
            float responsiveness = 0.2f; // how quickly velocity follows desired move
            float maxSpeed = 0.6f;

            node.velocity = Vector3.Lerp(node.velocity, desiredMove, responsiveness);
            node.velocity *= inertia;
            node.velocity = Vector3.ClampMagnitude(node.velocity, maxSpeed);

            pos += node.velocity;
            pos.y = 0f;
            node.view.position = pos;

            float distanceMoved = node.velocity.magnitude;

            // =====================================================
            // ================= RESOURCE LOGIC =====================
            // =====================================================

            float movementCostPerUnit = 0.05f;
            float gainPerPositiveEdge = 0.1f;
            float connectionCostMultiplier = 0.02f;

            float resourceDelta = 0f;

            // --- Movement cost ---
            resourceDelta -= distanceMoved * movementCostPerUnit;

            // --- Edge gains ---
            foreach (var edge in node.connections)
            {
                if (edge.type == EdgeType.Enemy) continue;

                resourceDelta += gainPerPositiveEdge * edge.weight;
            }

            // --- Connection maintenance cost ---
            int nonFamilyConnections = 0;
            foreach (var edge in node.connections)
            {
                if (edge.type != EdgeType.Family)
                    nonFamilyConnections++;
            }

            resourceDelta -= nonFamilyConnections * connectionCostMultiplier;

            float memoryFactor = 0.1f; // 0.05–0.2 is a good range

            node.resourceTrend = Mathf.Lerp(node.resourceTrend, resourceDelta, memoryFactor);
            

            // =====================================================
            // ================= MAP MULTIPLIER =====================
            // =====================================================

            Vector2 uv = WorldToUV(node.view.position, floorTransform, GetFloorSize());

            float brightness = resourceTexture.GetPixelBilinear(uv.x, uv.y).r;

            float mapStrength = 2f; // tune this
            float mapMultiplier = 1f + (brightness - 0.5f) * mapStrength;
            mapMultiplier = Mathf.Clamp(mapMultiplier, 0.25f, 2f);

            // Apply map effect ONLY to gains (not costs)
            if (resourceDelta > 0f)
                resourceDelta *= mapMultiplier;

            node.resources += resourceDelta;

            if (node.resources < 0f)
                node.resources = 0f;
        }

        //float deathChance = 0.01f; // 1% chance per tick

        //List<Node> nodesToRemove = new List<Node>();

        //foreach (var node in graph.nodes)
        //{
        //    // Optional: also kill nodes with no resources
        //    if (Random.value < deathChance || node.resources <= 0f)
        //    {
        //        nodesToRemove.Add(node);
        //    }
        //}

        //// Remove dead nodes
        //foreach (var deadNode in nodesToRemove)
        //{
        //    // Destroy visual
        //    if (deadNode.view != null)
        //        Destroy(deadNode.view.gameObject);

        //    // Remove connected edges
        //    foreach (var edge in deadNode.connections.ToArray())
        //    {
        //        graph.RemoveEdge(edge);
        //    }

        //    graph.nodes.Remove(deadNode);
        //}

        //float loveSpawnChance = 0.02f; // 2% chance per Love edge per tick

        //List<Node> newNodes = new List<Node>();

        //foreach (var edge in graph.edges.ToArray())
        //{
        //    if (edge.type == EdgeType.Love)
        //    {
        //        if (Random.value < loveSpawnChance)
        //        {
        //            // Create new node
        //            Node newNode = graph.CreateNode();

        //            // Example: inherit one random objective from one of the parents
        //            WeightedObjective inheritedObjective = null;
        //            if (edge.nodeA.objectives.Count > 0)
        //                inheritedObjective = edge.nodeA.objectives[Random.Range(0, edge.nodeA.objectives.Count)];

        //            if (inheritedObjective != null)
        //                newNode.objectives.Add(new WeightedObjective(inheritedObjective.objective, inheritedObjective.weight));

        //            // Position near one parent
        //            Vector3 parentPos = edge.nodeA.Position;
        //            Vector3 offset = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        //            SpawnNode(newNode, parentPos + offset);

        //            newNodes.Add(newNode);
        //        }
        //    }
        //}

        //graph.nodes.AddRange(newNodes);


        Debug.Log("---- STEP ----");
        Debug.Log(resourceTexture.width);
        foreach (var node in graph.nodes)
            Debug.Log($"Node {node.id} score: {node.Evaluate(graph)}");

        UpdateEdgeViews();
    }
}