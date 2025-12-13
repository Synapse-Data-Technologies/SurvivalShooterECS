using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

public class NavMeshBaker : EditorWindow
{
    [MenuItem("Tools/Bake NavMesh for Sphere Enemies")]
    public static void BakeNavMesh()
    {
        // Find the Floor object and make it Navigation Static
        GameObject floor = GameObject.Find("Floor");
        if (floor != null)
        {
            // Set Navigation Static flag
            GameObjectUtility.SetStaticEditorFlags(floor, StaticEditorFlags.NavigationStatic);
            Debug.Log("Set Floor as Navigation Static");
        }
        
        // Bake the NavMesh
        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
        Debug.Log("NavMesh baked successfully!");
    }
    
    [MenuItem("Tools/Check NavMesh Status")]
    public static void CheckNavMeshStatus()
    {
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        Debug.Log($"NavMesh has {triangulation.vertices.Length} vertices and {triangulation.indices.Length / 3} triangles");
        
        if (triangulation.vertices.Length == 0)
        {
            Debug.LogWarning("No NavMesh found! Use 'Tools/Bake NavMesh for Sphere Enemies' to bake one.");
        }
        else
        {
            Debug.Log("NavMesh is present and ready for navigation.");
        }
    }
}