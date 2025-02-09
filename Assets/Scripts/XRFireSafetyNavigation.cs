using UnityEngine;
using System.Collections.Generic;

public class XRFireSafetyNavigation : MonoBehaviour
{
    public Transform xrOrigin;        // Reference to the XR Origin
    public Transform directionalArrow; // Reference to the 3D arrow model
    public LineRenderer pathRenderer;  // Reference to visualize the path
    public float arrowHeight = 1.5f;  // Height of the floating arrow above ground
    public float arrowUpdateSpeed = 5f;
    public Color pathColor = new Color(0, 1, 0, 0.5f); // Semi-transparent green
    
    // Visual feedback settings
    public GameObject environmentalMarkerPrefab; // Prefab for environmental markers
    public float markerSpacing = 2f;            // Space between environmental markers
    private List<GameObject> activeMarkers = new List<GameObject>();
    
    private List<NavNode> allNodes;
    private List<NavNode> currentPath = new List<NavNode>();
    private NavNode nearestNode;
    private NavNode targetExit;

    private void Start()
    {
        // Initialize components
        allNodes = new List<NavNode>(FindObjectsByType<NavNode>(FindObjectsSortMode.None));
        
        // Setup line renderer
        if (pathRenderer != null)
        {
            pathRenderer.startWidth = 0.1f;
            pathRenderer.endWidth = 0.1f;
            pathRenderer.material = new Material(Shader.Find("Sprites/Default"));
            pathRenderer.startColor = pathColor;
            pathRenderer.endColor = pathColor;
        }

        // Position arrow at proper height
        if (directionalArrow != null)
        {
            Vector3 arrowPos = directionalArrow.position;
            arrowPos.y = arrowHeight;
            directionalArrow.position = arrowPos;
        }
    }

    private void Update()
    {
        if (xrOrigin == null) return;

        UpdatePathAndGuides();
        UpdateArrowDirection();
        UpdateEnvironmentalMarkers();
    }

    void UpdatePathAndGuides()
    {
        // Find nearest node to player
        nearestNode = GetClosestNode(xrOrigin.position);
        if (nearestNode == null) return;

        // Find best exit
        targetExit = GetBestExit(nearestNode);
        if (targetExit == null) return;

        // Calculate path
        currentPath = AStarPathfinding(nearestNode, targetExit);
        
        // Update line renderer
        if (pathRenderer != null && currentPath.Count > 0)
        {
            Vector3[] positions = new Vector3[currentPath.Count + 1];
            positions[0] = xrOrigin.position;
            for (int i = 0; i < currentPath.Count; i++)
            {
                positions[i + 1] = currentPath[i].transform.position;
            }
            pathRenderer.positionCount = positions.Length;
            pathRenderer.SetPositions(positions);
        }
    }

    void UpdateArrowDirection()
    {
        if (directionalArrow == null || currentPath.Count < 1) return;

        // Get next waypoint position
        Vector3 targetPosition = currentPath[0].transform.position;
        if (currentPath.Count > 1)
        {
            float distanceToFirst = Vector3.Distance(xrOrigin.position, currentPath[0].transform.position);
            if (distanceToFirst < 1f && currentPath.Count > 1)
            {
                targetPosition = currentPath[1].transform.position;
            }
        }

        // Update arrow position to follow XR Origin
        Vector3 newArrowPosition = xrOrigin.position;
        newArrowPosition.y = arrowHeight;
        directionalArrow.position = newArrowPosition;

        // Calculate direction to target
        Vector3 direction = targetPosition - xrOrigin.position;
        direction.y = 0; // Keep the direction on the horizontal plane

        if (direction != Vector3.zero)
        {
            // Smooth rotation
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            directionalArrow.rotation = Quaternion.Slerp(
                directionalArrow.rotation,
                targetRotation,
                Time.deltaTime * arrowUpdateSpeed
            );
        }
    }

    void UpdateEnvironmentalMarkers()
    {
        // Clear old markers
        foreach (var marker in activeMarkers)
        {
            Destroy(marker);
        }
        activeMarkers.Clear();

        if (currentPath.Count < 2 || environmentalMarkerPrefab == null) return;

        // Place new markers along the path
        for (int i = 0; i < currentPath.Count - 1; i++)
        {
            Vector3 start = currentPath[i].transform.position;
            Vector3 end = currentPath[i + 1].transform.position;
            float distance = Vector3.Distance(start, end);
            
            // Place markers along the path segment
            int markersInSegment = Mathf.FloorToInt(distance / markerSpacing);
            for (int j = 0; j < markersInSegment; j++)
            {
                float t = (float)j / markersInSegment;
                Vector3 markerPosition = Vector3.Lerp(start, end, t);
                GameObject marker = Instantiate(environmentalMarkerPrefab, markerPosition, Quaternion.identity);
                activeMarkers.Add(marker);
            }
        }
    }

    NavNode GetClosestNode(Vector3 position)
    {
        NavNode closest = null;
        float minDist = Mathf.Infinity;

        foreach (var node in allNodes)
        {
            float dist = Vector3.Distance(position, node.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = node;
            }
        }

        return closest;
    }

    NavNode GetBestExit(NavNode startNode)
    {
        NavNode bestExit = null;
        float shortestPathCost = Mathf.Infinity;

        foreach (var node in allNodes)
        {
            if (!node.isExit) continue;

            List<NavNode> tempPath = AStarPathfinding(startNode, node);
            float pathCost = CalculatePathCost(tempPath);

            if (pathCost < shortestPathCost)
            {
                shortestPathCost = pathCost;
                bestExit = node;
            }
        }

        return bestExit;
    }

    float CalculatePathCost(List<NavNode> path)
    {
        float cost = 0f;
        for (int i = 0; i < path.Count - 1; i++)
        {
            cost += Vector3.Distance(path[i].transform.position, path[i + 1].transform.position);
        }
        return cost;
    }

    List<NavNode> AStarPathfinding(NavNode start, NavNode goal)
    {
        PriorityQueue<NavNode> openSet = new PriorityQueue<NavNode>();
        openSet.Enqueue(start, 0);

        Dictionary<NavNode, NavNode> cameFrom = new Dictionary<NavNode, NavNode>();
        Dictionary<NavNode, float> gScore = new Dictionary<NavNode, float> { [start] = 0 };
        Dictionary<NavNode, float> fScore = new Dictionary<NavNode, float> { [start] = Heuristic(start, goal) };

        while (openSet.Count > 0)
        {
            NavNode current = openSet.Dequeue();
            if (current == goal) return ReconstructPath(cameFrom, current);

            foreach (NavNode neighbor in current.neighbors)
            {
                float tentativeGScore = gScore[current] + Vector3.Distance(current.transform.position, neighbor.transform.position);

                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, goal);

                    if (!openSet.Contains(neighbor)) openSet.Enqueue(neighbor, fScore[neighbor]);
                }
            }
        }
        return new List<NavNode>(); // No path found
    }

    float Heuristic(NavNode a, NavNode b)
    {
        return Vector3.Distance(a.transform.position, b.transform.position);
    }

    List<NavNode> ReconstructPath(Dictionary<NavNode, NavNode> cameFrom, NavNode current)
    {
        List<NavNode> path = new List<NavNode> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        return path;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Draw path
        if (currentPath.Count > 1)
        {
            Gizmos.color = new Color(0, 1, 0, 0.5f); // Semi-transparent green
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i].transform.position, currentPath[i + 1].transform.position);
            }
        }

        // Draw line from XR Origin to nearest node
        if (nearestNode != null && xrOrigin != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.5f); // Semi-transparent red
            Gizmos.DrawLine(xrOrigin.position, nearestNode.transform.position);
        }

        // Highlight exit nodes
        if (targetExit != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetExit.transform.position, 1f);
        }
    }
}

public class PriorityQueue<T>
{
    private List<KeyValuePair<T, float>> elements = new List<KeyValuePair<T, float>>();

    public int Count => elements.Count;

    public void Enqueue(T item, float priority)
    {
        elements.Add(new KeyValuePair<T, float>(item, priority));
        int c = elements.Count - 1;
        while (c > 0 && elements[c].Value < elements[(c - 1) / 2].Value)
        {
            (elements[c], elements[(c - 1) / 2]) = (elements[(c - 1) / 2], elements[c]);
            c = (c - 1) / 2;
        }
    }

    public T Dequeue()
    {
        if (elements.Count == 0) return default;
        T bestItem = elements[0].Key;
        elements[0] = elements[^1];
        elements.RemoveAt(elements.Count - 1);
        
        int c = 0;
        while (true)
        {
            int min = c;
            int left = 2 * c + 1, right = 2 * c + 2;

            if (left < elements.Count && elements[left].Value < elements[min].Value) min = left;
            if (right < elements.Count && elements[right].Value < elements[min].Value) min = right;
            if (min == c) break;

            (elements[c], elements[min]) = (elements[min], elements[c]);
            c = min;
        }
        return bestItem;
    }

    public bool Contains(T item)
    {
        return elements.Exists(e => EqualityComparer<T>.Default.Equals(e.Key, item));
    }
}