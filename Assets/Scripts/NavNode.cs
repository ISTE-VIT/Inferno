using UnityEngine;
using System.Collections.Generic;

public class NavNode : MonoBehaviour
{
    public List<NavNode> neighbors = new List<NavNode>();
    public bool isExit = false;
    private static float connectionThreshold = 10f;

    private void Start()
    {
        AutoConnectNodes();
    }

    public void AutoConnectNodes()
    {
        NavNode[] allNodes = FindObjectsByType<NavNode>(FindObjectsSortMode.None);
        foreach (var node in allNodes)
        {
            if (node == this) continue;

            float distance = Vector3.Distance(transform.position, node.transform.position);
            if (distance <= connectionThreshold)
            {
                // Ensure no wall is blocking the connection
                if (!Physics.Linecast(transform.position, node.transform.position))
                {
                    if (!neighbors.Contains(node))
                    {
                        neighbors.Add(node);
                        node.neighbors.Add(this); // Ensure bidirectional link
                    }
                }
            }
        }
    }

    public void CreateNewNode()
    {
        GameObject newNode = new GameObject("NavNode");
        newNode.transform.position = transform.position + Vector3.right * 2; // Adjust position as needed
        NavNode newNavNode = newNode.AddComponent<NavNode>();
        newNavNode.AutoConnectNodes();
    }

    public static void RecalculateAllNeighbors()
    {
        NavNode[] allNodes = FindObjectsByType<NavNode>(FindObjectsSortMode.None);
        foreach (var node in allNodes)
        {
            node.neighbors.Clear();
            node.AutoConnectNodes();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isExit ? Color.green : Color.blue;
        Gizmos.DrawSphere(transform.position, 0.5f);

        foreach (var neighbor in neighbors)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, neighbor.transform.position);
        }
    }
}