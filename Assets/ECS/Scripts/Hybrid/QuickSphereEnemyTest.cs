using UnityEngine;

/// <summary>
/// Quick test for sphere enemy functionality
/// </summary>
public class QuickSphereEnemyTest : MonoBehaviour
{
    [Header("Quick Test")]
    public KeyCode spawnKey = KeyCode.Space;
    public KeyCode clearKey = KeyCode.C;
    
    void Update()
    {
        if (Input.GetKeyDown(spawnKey))
        {
            SpawnTestSphereEnemy();
        }
        
        if (Input.GetKeyDown(clearKey))
        {
            ClearAllSphereEnemies();
        }
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 120));
        GUILayout.Label("Sphere Enemy Quick Test", GUI.skin.box);
        GUILayout.Label($"Press {spawnKey} to spawn sphere enemy");
        GUILayout.Label($"Press {clearKey} to clear all sphere enemies");
        
        if (GUILayout.Button("Spawn Test Enemy"))
        {
            SpawnTestSphereEnemy();
        }
        
        if (GUILayout.Button("Clear All Enemies"))
        {
            ClearAllSphereEnemies();
        }
        
        GUILayout.EndArea();
    }
    
    void SpawnTestSphereEnemy()
    {
        // Create sphere primitive
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = $"TestSphereEnemy_{Time.time:F1}";
        sphere.tag = "Enemy";
        
        // Position randomly around this object
        var spawnPos = transform.position + Random.insideUnitSphere * 5f;
        spawnPos.y = Mathf.Max(spawnPos.y, 0.5f);
        sphere.transform.position = spawnPos;
        
        // Add the ECS component
        var sphereEnemyObject = sphere.AddComponent<SphereEnemyObject>();
        sphereEnemyObject.aiVersion = "Test_" + Random.Range(1, 4);
        sphereEnemyObject.detectionRange = 8f;
        sphereEnemyObject.moveSpeed = Random.Range(2f, 4f);
        
        Debug.Log($"[QuickTest] Spawned sphere enemy: {sphere.name} at {spawnPos}");
    }
    
    void ClearAllSphereEnemies()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        int cleared = 0;
        
        foreach (var enemy in enemies)
        {
            if (enemy.GetComponent<SphereEnemyObject>() != null)
            {
                DestroyImmediate(enemy);
                cleared++;
            }
        }
        
        Debug.Log($"[QuickTest] Cleared {cleared} sphere enemies");
    }
}