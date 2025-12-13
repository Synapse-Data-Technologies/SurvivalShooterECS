using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Transform))]
public class SphereEnemyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        Transform transform = (Transform)target;
        
        if (transform.gameObject.name == "SphereEnemy")
        {
            GUILayout.Space(10);
            GUILayout.Label("Sphere Enemy Setup", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Add Sphere Enemy Components"))
            {
                AddSphereEnemyComponents(transform.gameObject);
            }
        }
    }
    
    private void AddSphereEnemyComponents(GameObject go)
    {
        // Add AI component if not present
        if (go.GetComponent<SphereEnemyAI>() == null)
        {
            go.AddComponent<SphereEnemyAI>();
            Debug.Log("Added SphereEnemyAI component");
        }
        
        // Add Health component if not present
        if (go.GetComponent<SphereEnemyHealth>() == null)
        {
            go.AddComponent<SphereEnemyHealth>();
            Debug.Log("Added SphereEnemyHealth component");
        }
        
        // Add Debug UI component if not present
        if (go.GetComponent<SphereEnemyDebugUI>() == null)
        {
            go.AddComponent<SphereEnemyDebugUI>();
            Debug.Log("Added SphereEnemyDebugUI component");
        }
        
        // Configure material color
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            renderer.material.color = Color.red;
        }
        
        EditorUtility.SetDirty(go);
    }
}