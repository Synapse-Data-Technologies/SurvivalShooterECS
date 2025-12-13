using UnityEngine;
using Unity.Entities;

/// <summary>
/// Simple tester for sphere enemies in ECS
/// </summary>
public class SphereEnemyTester : MonoBehaviour
{
    [Header("Testing")]
    public KeyCode spawnKey = KeyCode.Space;
    public KeyCode clearKey = KeyCode.C;
    
    void Update()
    {
        if (Input.GetKeyDown(spawnKey))
        {
            CreateTestSphereEnemy();
        }
        
        if (Input.GetKeyDown(clearKey))
        {
            ClearAllSphereEnemies();
        }
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.Label("ECS Sphere Enemy Tester", GUI.skin.box);
        GUILayout.Label($"Press {spawnKey} to spawn sphere enemy");
        GUILayout.Label($"Press {clearKey} to clear all sphere enemies");
        
        if (GUILayout.Button("Spawn Test Sphere Enemy"))
        {
            CreateTestSphereEnemy();
        }
        
        if (GUILayout.Button("Clear All Sphere Enemies"))
        {
            ClearAllSphereEnemies();
        }
        
        // Show ECS info
        var world = World.DefaultGameObjectInjectionWorld;
        if (world != null)
        {
            var entityManager = world.EntityManager;
            var sphereEnemyQuery = entityManager.CreateEntityQuery(typeof(SphereEnemyData));
            GUILayout.Label($"ECS Sphere Enemies: {sphereEnemyQuery.CalculateEntityCount()}");
            sphereEnemyQuery.Dispose();
        }
        
        GUILayout.EndArea();
    }
    
    public void CreateTestSphereEnemy()
    {
        // Create sphere primitive
        var sphereEnemy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphereEnemy.name = $"TestSphereEnemy_{Time.time:F2}";
        sphereEnemy.tag = "Enemy";
        
        // Position near the tester
        var pos = transform.position + Random.insideUnitSphere * 3f;
        pos.y = Mathf.Max(pos.y, 0.5f);
        sphereEnemy.transform.position = pos;
        
        // Add the ECS hybrid component
        var sphereEnemyObject = sphereEnemy.AddComponent<SphereEnemyObject>();
        
        // Configure with test settings
        sphereEnemyObject.aiVersion = "Test_" + Random.Range(1, 5);
        sphereEnemyObject.detectionRange = 8f;
        sphereEnemyObject.attackRange = 2f;
        sphereEnemyObject.moveSpeed = Random.Range(2f, 4f);
        sphereEnemyObject.patrolRadius = 4f;
        
        Debug.Log($"[SphereEnemyTester] Created test sphere enemy: {sphereEnemy.name}");
    }
    
    public void ClearAllSphereEnemies()
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
        
        Debug.Log($"[SphereEnemyTester] Cleared {cleared} sphere enemies");
    }
}