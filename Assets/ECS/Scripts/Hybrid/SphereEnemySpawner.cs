using UnityEngine;
using Unity.Entities;

/// <summary>
/// Spawner for sphere enemies in the ECS system
/// Creates sphere enemies at timed intervals
/// </summary>
public class SphereEnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public float spawnTime = 3f;
    public int maxEnemies = 5;
    
    [Header("Sphere Enemy Settings")]
    public string aiVersion = "1.0";
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float moveSpeed = 3f;
    public float patrolRadius = 5f;
    
    private float currentTime = 0f;
    
    void Update()
    {
        currentTime += Time.deltaTime;
        
        if (currentTime >= spawnTime)
        {
            // Count existing sphere enemies
            var existingEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            int sphereEnemyCount = 0;
            
            foreach (var enemy in existingEnemies)
            {
                if (enemy.GetComponent<SphereEnemyObject>() != null)
                {
                    sphereEnemyCount++;
                }
            }
            
            if (sphereEnemyCount < maxEnemies)
            {
                SpawnSphereEnemy();
            }
            
            currentTime = 0f;
        }
    }
    
    public void SpawnSphereEnemy()
    {
        // Create sphere primitive
        var sphereEnemy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphereEnemy.name = $"SphereEnemy_{Time.time:F2}";
        sphereEnemy.tag = "Enemy";
        
        // Position randomly around spawner
        var randomPos = transform.position + Random.insideUnitSphere * 5f;
        randomPos.y = Mathf.Max(randomPos.y, 0.5f); // Keep above ground
        sphereEnemy.transform.position = randomPos;
        
        // Add the ECS hybrid component
        var sphereEnemyObject = sphereEnemy.AddComponent<SphereEnemyObject>();
        
        // Configure the sphere enemy
        sphereEnemyObject.aiVersion = aiVersion;
        sphereEnemyObject.detectionRange = detectionRange;
        sphereEnemyObject.attackRange = attackRange;
        sphereEnemyObject.moveSpeed = moveSpeed;
        sphereEnemyObject.patrolRadius = patrolRadius;
        
        Debug.Log($"[SphereEnemySpawner] Spawned sphere enemy: {sphereEnemy.name} at {randomPos}");
    }
    
    [ContextMenu("Spawn Enemy Now")]
    public void SpawnEnemyNow()
    {
        SpawnSphereEnemy();
    }
    
    [ContextMenu("Clear All Sphere Enemies")]
    public void ClearAllSphereEnemies()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            if (enemy.GetComponent<SphereEnemyObject>() != null)
            {
                DestroyImmediate(enemy);
            }
        }
        Debug.Log("[SphereEnemySpawner] Cleared all sphere enemies");
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw spawn radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 5f);
        
        // Draw detection range preview
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}