using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NavNode))]
public class NavNodeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NavNode navNode = (NavNode)target;

        if (GUILayout.Button("Create New Node"))
        {
            navNode.CreateNewNode();
        }

        if (GUILayout.Button("Recalculate Neighbors"))
        {
            NavNode.RecalculateAllNeighbors();
        }
    }
}