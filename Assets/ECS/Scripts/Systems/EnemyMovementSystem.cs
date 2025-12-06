using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Modern ISystem implementation for enemy movement.
/// Moves enemies toward the player using NavMeshAgent pathfinding.
/// </summary>
public partial struct EnemyMovementSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        Debug.Log("[EnemyMovementSystem] OnCreate called - system is being created!");
        // Only run when enemies exist
        state.RequireForUpdate<EnemyData>();
    }

    public void OnUpdate(ref SystemState state)
    {
        Debug.Log("[EnemyMovementSystem] OnUpdate called!");
        
        // Find player and check if alive
        var playerQuery = SystemAPI.QueryBuilder()
            .WithAll<PlayerData, HealthData, Transform>()
            .Build();

        if (playerQuery.IsEmpty)
        {
            Debug.LogWarning("[EnemyMovementSystem] Player query is empty");
            return;
        }

        var playerEntities = playerQuery.ToEntityArray(state.WorldUpdateAllocator);
        if (playerEntities.Length == 0)
        {
            return;
        }

        var playerEntity = playerEntities[0];
        var playerHealth = state.EntityManager.GetComponentData<HealthData>(playerEntity);
        
        // Null safety check for player Transform
        if (!state.EntityManager.HasComponent<Transform>(playerEntity))
        {
            Debug.LogWarning("[EnemyMovementSystem] Player entity missing Transform component");
            return;
        }
            
        var playerTransform = state.EntityManager.GetComponentObject<Transform>(playerEntity);
        if (playerTransform == null)
        {
            Debug.LogWarning("[EnemyMovementSystem] Null Transform on player entity");
            return;
        }

        var playerPosition = playerTransform.position;
        var playerAlive = playerHealth.Value > 0;

        // Query for all living enemies
        var enemyQuery = SystemAPI.QueryBuilder()
            .WithAll<EnemyData, HealthData>()
            .WithNone<DeadData>()
            .Build();

        var enemyEntities = enemyQuery.ToEntityArray(state.WorldUpdateAllocator);
        
        Debug.Log($"[EnemyMovementSystem] Found {enemyEntities.Length} enemies to process");

        // Process each enemy
        foreach (var entity in enemyEntities)
        {
            var enemyHealth = state.EntityManager.GetComponentData<HealthData>(entity);
            
            // Get NavMeshAgent managed component
            if (!state.EntityManager.HasComponent<NavMeshAgent>(entity))
            {
                Debug.LogWarning($"[EnemyMovementSystem] Enemy entity {entity} missing NavMeshAgent component");
                continue;
            }

            var agent = state.EntityManager.GetComponentObject<NavMeshAgent>(entity);
            
            if (agent == null)
            {
                Debug.LogWarning($"[EnemyMovementSystem] Null NavMeshAgent on entity {entity}");
                continue;
            }

            // Move toward player if both are alive
            if (enemyHealth.Value > 0 && playerAlive)
            {
                Debug.Log($"[EnemyMovementSystem] Setting destination for enemy {entity} to {playerPosition}");
                agent.SetDestination(playerPosition);
            }
            else
            {
                Debug.Log($"[EnemyMovementSystem] Disabling agent for enemy {entity} (enemyHP={enemyHealth.Value}, playerAlive={playerAlive})");
                agent.enabled = false;
            }
        }
    }
}
