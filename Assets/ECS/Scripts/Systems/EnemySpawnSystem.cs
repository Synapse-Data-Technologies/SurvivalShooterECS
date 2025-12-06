using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Modern ISystem implementation for enemy spawning.
/// Spawns enemies at timed intervals from EnemySpawner entities.
/// </summary>
public partial struct EnemySpawnSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        // Require at least one spawner to exist before running
        state.RequireForUpdate<EnemySpawnerData>();
    }

    public void OnUpdate(ref SystemState state)
    {
        // Check if player exists and is alive
        var playerQuery = SystemAPI.QueryBuilder()
            .WithAll<PlayerData, HealthData>()
            .Build();

        if (playerQuery.IsEmpty)
        {
            // No player exists, don't spawn enemies
            return;
        }

        // Get player health
        var playerHealth = 0;
        foreach (var health in SystemAPI.Query<RefRO<HealthData>>()
            .WithAll<PlayerData>())
        {
            playerHealth = health.ValueRO.Value;
            break; // Only one player
        }

        // Don't spawn if player is dead
        if (playerHealth <= 0)
        {
            return;
        }

        var dt = SystemAPI.Time.DeltaTime;

        // Query for all spawner entities with EnemySpawnerData
        var spawnerQuery = SystemAPI.QueryBuilder()
            .WithAll<EnemySpawnerData>()
            .Build();

        var spawnerEntities = spawnerQuery.ToEntityArray(state.WorldUpdateAllocator);

        // Process each spawner entity
        foreach (var entity in spawnerEntities)
        {
            // Get the EnemySpawnerData component
            var spawnerData = state.EntityManager.GetComponentData<EnemySpawnerData>(entity);

            // Get managed components (Transform and EnemySpawner MonoBehaviour)
            Transform transform = null;
            EnemySpawner spawnerMB = null;

            if (state.EntityManager.HasComponent<Transform>(entity))
            {
                transform = state.EntityManager.GetComponentObject<Transform>(entity);
            }

            if (state.EntityManager.HasComponent<EnemySpawner>(entity))
            {
                spawnerMB = state.EntityManager.GetComponentObject<EnemySpawner>(entity);
            }

            // Null safety checks for managed components
            if (transform == null)
            {
                Debug.LogWarning($"[EnemySpawnSystem] Null Transform on entity {entity}, skipping");
                continue;
            }

            if (spawnerMB == null)
            {
                Debug.LogWarning($"[EnemySpawnSystem] Null EnemySpawner MonoBehaviour on entity {entity}, skipping");
                continue;
            }

            // Increment spawn timer
            spawnerData.CurrentTime += dt;

            // Check if it's time to spawn
            if (spawnerData.CurrentTime >= spawnerData.SpawnTime)
            {
                // Spawn enemy at spawner location
                Object.Instantiate(
                    spawnerMB.Enemy,
                    transform.position,
                    quaternion.identity);

                // Reset timer
                spawnerData.CurrentTime = 0f;
            }

            // Write the updated spawner data back to the entity
            state.EntityManager.SetComponentData(entity, spawnerData);
        }
    }
}
