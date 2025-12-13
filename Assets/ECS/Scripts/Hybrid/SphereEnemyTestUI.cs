using UnityEngine;
using Unity.Entities;

/// <summary>
/// Restored working UI for sphere enemy testing
/// Combines all the functionality that was working before
/// </summary>
public class SphereEnemyTestUI : MonoBehaviour
{
    [Header("Test Controls")]
    public KeyCode spawnKey = KeyCode.Space;
    public KeyCode clearKey = KeyCode.C;
    public KeyCode toggleDebugKey = KeyCode.D;
    
    [Header("Spawn Settings")]
    public int maxEnemies = 5;
    public float spawnRadius = 8f;
    
    private int enemyCount = 0;
    
    void Update()
    {
        if (Input.GetKeyDown(spawnKey))
        {
            SpawnSphereEnemy();
        }
        
        if (Input.GetKeyDown(clearKey))
        {
            ClearAllSphereEnemies();
        }
        
        if (Input.GetKeyDown(toggleDebugKey))
        {
            ToggleAllDebugUI();
        }
        
        // Update enemy count
        UpdateEnemyCount();
    }
    
    void OnGUI()
    {
        // Main test panel
        GUILayout.BeginArea(new Rect(10, 10, 350, 200));
        GUILayout.Label("ECS Sphere Enemy Tester", GUI.skin.box);
        
        GUILayout.Label($"Active Sphere Enemies: {enemyCount}");
        GUILayout.Label($"Press {spawnKey} to spawn enemy");
        GUILayout.Label($"Press {clearKey} to clear all enemies");
        GUILayout.Label($"Press {toggleDebugKey} to toggle debug UI");
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Spawn Test Sphere Enemy"))
        {
            SpawnSphereEnemy();
        }
        
        if (GUILayout.Button("Clear All Sphere Enemies"))
        {
            ClearAllSphereEnemies();
        }
        
        if (GUILayout.Button("Toggle Debug UI"))
        {
            ToggleAllDebugUI();
        }
        
        // ECS info
        var world = World.DefaultGameObjectInjectionWorld;
        if (world != null)
        {
            var entityManager = world.EntityManager;
            var sphereEnemyQuery = entityManager.CreateEntityQuery(typeof(SphereEnemyData));
            GUILayout.Label($"ECS Sphere Entities: {sphereEnemyQuery.CalculateEntityCount()}");
            sphereEnemyQuery.Dispose();
        }
        
        GUILayout.EndArea();
        
        // Debug window
        GUILayout.BeginArea(new Rect(10, 220, 350, 150));
        GUILayout.Label("Debug Information", GUI.skin.box);
        
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            GUILayout.Label($"Player Position: {player.transform.position}");
        }
        else
        {
            GUILayout.Label("No Player found!");
        }
        
        // Show AI system status
        if (world != null)
        {
            GUILayout.Label("ECS Systems: Active");
        }
        else
        {
            GUILayout.Label("ECS Systems: Not Found");
        }
        
        GUILayout.EndArea();
    }
    
    public void SpawnSphereEnemy()
    {
        if (enemyCount >= maxEnemies)
        {
            Debug.LogWarning($"Maximum enemies ({maxEnemies}) reached!");
            return;
        }
        
        // Create sphere primitive
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = $"SphereEnemy_{Time.time:F1}";
        sphere.tag = "Enemy";
        
        // Position randomly around spawner
        var spawnPos = transform.position + Random.insideUnitSphere * spawnRadius;
        spawnPos.y = Mathf.Max(spawnPos.y, 0.5f);
        sphere.transform.position = spawnPos;
        
        // Add the ECS component
        var sphereEnemyObject = sphere.AddComponent<SphereEnemyObject>();
        
        // Configure with random settings for testing
        sphereEnemyObject.aiVersion = "Test_" + Random.Range(1, 5);
        sphereEnemyObject.detectionRange = Random.Range(6f, 12f);
        sphereEnemyObject.attackRange = Random.Range(1.5f, 3f);
        sphereEnemyObject.moveSpeed = Random.Range(2f, 4f);
        sphereEnemyObject.patrolRadius = Random.Range(3f, 6f);
        sphereEnemyObject.showDebugUI = true;
        
        Debug.Log($"[SphereEnemyTestUI] Spawned: {sphere.name} with AI version {sphereEnemyObject.aiVersion}");
        enemyCount++;
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
        
        enemyCount = 0;
        Debug.Log($"[SphereEnemyTestUI] Cleared {cleared} sphere enemies");
    }
    
    public void ToggleAllDebugUI()
    {
        var sphereEnemies = FindObjectsOfType<SphereEnemyObject>();
        foreach (var sphereEnemy in sphereEnemies)
        {
            sphereEnemy.showDebugUI = !sphereEnemy.showDebugUI;
        }
        Debug.Log($"[SphereEnemyTestUI] Toggled debug UI for {sphereEnemies.Length} enemies");
    }
    
    private void UpdateEnemyCount()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        int count = 0;
        
        foreach (var enemy in enemies)
        {
            if (enemy.GetComponent<SphereEnemyObject>() != null)
            {
                count++;
            }
        }
        
        enemyCount = count;
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw spawn radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        
        // Draw max enemy positions
        Gizmos.color = Color.red;
        for (int i = 0; i < maxEnemies; i++)
        {
            float angle = (i / (float)maxEnemies) * 360f * Mathf.Deg2Rad;
            Vector3 pos = transform.position + new Vector3(
                Mathf.Cos(angle) * spawnRadius * 0.7f,
                0.5f,
                Mathf.Sin(angle) * spawnRadius * 0.7f
            );
            Gizmos.DrawWireCube(pos, Vector3.one * 0.5f);
        }
    }
}