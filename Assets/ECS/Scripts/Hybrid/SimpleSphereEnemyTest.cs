using UnityEngine;

/// <summary>
/// Simple test to create a sphere enemy and verify ECS integration
/// </summary>
public class SimpleSphereEnemyTest : MonoBehaviour
{
    [Header("Test Settings")]
    public KeyCode testKey = KeyCode.T;
    
    void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            CreateSimpleTestEnemy();
        }
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 100));
        GUILayout.Label("Simple Sphere Enemy Test", GUI.skin.box);
        GUILayout.Label($"Press {testKey} to create test enemy");
        
        if (GUILayout.Button("Create Test Enemy"))
        {
            CreateSimpleTestEnemy();
        }
        
        GUILayout.EndArea();
    }
    
    void CreateSimpleTestEnemy()
    {
        // Create basic sphere
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "TestSphereEnemy";
        sphere.tag = "Enemy";
        sphere.transform.position = transform.position + Vector3.right * 3f;
        
        // Set red color
        var renderer = sphere.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.red;
        }
        
        // Add the ECS component
        var sphereEnemyObject = sphere.AddComponent<SphereEnemyObject>();
        sphereEnemyObject.aiVersion = "Test_1.0";
        
        Debug.Log($"Created simple test sphere enemy: {sphere.name}");
    }
}