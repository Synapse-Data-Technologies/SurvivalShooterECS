using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Working ECS System for sphere enemy AI behavior
/// Simplified version without problematic dependencies
/// </summary>
public partial struct SphereEnemyAISystem_Working : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        // Only run when sphere enemies exist
        state.RequireForUpdate<SphereEnemyData>();
    }

    public void OnUpdate(ref SystemState state)
    {
        // Find player and check if alive
        var playerQuery = SystemAPI.QueryBuilder()
            .WithAll<PlayerData, HealthData, Transform>()
            .Build();

        if (playerQuery.IsEmpty)
        {
            return;
        }

        var playerEntities = playerQuery.ToEntityArray(state.WorldUpdateAllocator);
        if (playerEntities.Length == 0)
        {
            return;
        }

        var playerEntity = playerEntities[0];
        var playerHealth = state.EntityManager.GetComponentData<HealthData>(playerEntity);
        
        if (!state.EntityManager.HasComponent<Transform>(playerEntity))
        {
            return;
        }
            
        var playerTransform = state.EntityManager.GetComponentObject<Transform>(playerEntity);
        if (playerTransform == null)
        {
            return;
        }

        var playerPosition = (float3)playerTransform.position;
        var playerAlive = playerHealth.Value > 0;
        var deltaTime = SystemAPI.Time.DeltaTime;
        var currentTime = (float)SystemAPI.Time.ElapsedTime;

        // Query for all living sphere enemies
        var sphereEnemyQuery = SystemAPI.QueryBuilder()
            .WithAll<SphereEnemyData, SphereEnemyPatrolData, HealthData>()
            .WithNone<DeadData>()
            .Build();

        var sphereEnemyEntities = sphereEnemyQuery.ToEntityArray(state.WorldUpdateAllocator);

        // Process each sphere enemy
        foreach (var entity in sphereEnemyEntities)
        {
            var sphereData = state.EntityManager.GetComponentData<SphereEnemyData>(entity);
            var patrolData = state.EntityManager.GetComponentData<SphereEnemyPatrolData>(entity);
            var enemyHealth = state.EntityManager.GetComponentData<HealthData>(entity);
            
            // Skip if enemy is dead
            if (enemyHealth.Value <= 0)
            {
                continue;
            }

            // Get enemy transform
            if (!state.EntityManager.HasComponent<Transform>(entity))
            {
                continue;
            }

            var enemyTransform = state.EntityManager.GetComponentObject<Transform>(entity);
            if (enemyTransform == null)
            {
                continue;
            }

            var enemyPosition = (float3)enemyTransform.position;
            var distanceToPlayer = math.distance(enemyPosition, playerPosition);
            var previousMode = sphereData.CurrentMode;

            // Simple AI State Machine
            if (distanceToPlayer <= sphereData.DetectionRange && playerAlive)
            {
                // Chase mode
                sphereData.CurrentMode = SphereEnemyData.AIMode.Chase;
                
                // Try NavMesh movement first
                var navAgent = state.EntityManager.GetComponentObject<NavMeshAgent>(entity);
                if (navAgent != null && navAgent.enabled && navAgent.isOnNavMesh)
                {
                    navAgent.SetDestination(playerPosition);
                }
                else
                {
                    // Fallback movement
                    var direction = math.normalize(playerPosition - enemyPosition);
                    direction.y = 0;
                    
                    if (math.length(direction) > 0.1f)
                    {
                        var targetRotation = quaternion.LookRotation(direction, math.up());
                        enemyTransform.rotation = math.slerp(enemyTransform.rotation, targetRotation, 5f * deltaTime);
                        enemyTransform.Translate(Vector3.forward * sphereData.MoveSpeed * deltaTime);
                    }
                }
            }
            else
            {
                // Patrol mode
                sphereData.CurrentMode = SphereEnemyData.AIMode.Patrol;
                
                // Simple patrol - move randomly
                if (math.distance(enemyPosition, patrolData.PatrolTarget) < 1f || 
                    currentTime - sphereData.LastModeChangeTime > 5f)
                {
                    // Set new patrol target
                    var random = Unity.Mathematics.Random.CreateFromIndex((uint)(currentTime * 1000 + entity.Index));
                    var randomDirection = random.NextFloat3(-1f, 1f);
                    randomDirection.y = 0;
                    randomDirection = math.normalize(randomDirection) * random.NextFloat(2f, sphereData.PatrolRadius);
                    
                    patrolData.PatrolTarget = patrolData.StartPosition + randomDirection;
                    sphereData.LastModeChangeTime = currentTime;
                }
                
                // Move towards patrol target
                var navAgent = state.EntityManager.GetComponentObject<NavMeshAgent>(entity);
                if (navAgent != null && navAgent.enabled && navAgent.isOnNavMesh)
                {
                    navAgent.SetDestination(patrolData.PatrolTarget);
                }
                else
                {
                    // Fallback movement
                    var direction = math.normalize(patrolData.PatrolTarget - enemyPosition);
                    direction.y = 0;
                    
                    if (math.length(direction) > 0.1f)
                    {
                        var targetRotation = quaternion.LookRotation(direction, math.up());
                        enemyTransform.rotation = math.slerp(enemyTransform.rotation, targetRotation, 3f * deltaTime);
                        enemyTransform.Translate(Vector3.forward * sphereData.MoveSpeed * deltaTime);
                    }
                }
            }

            // Update mode change time if mode changed
            if (previousMode != sphereData.CurrentMode)
            {
                sphereData.LastModeChangeTime = currentTime;
                Debug.Log($"Sphere Enemy {entity.Index}: {previousMode} -> {sphereData.CurrentMode}");
            }

            // Write updated data back to entity
            state.EntityManager.SetComponentData(entity, sphereData);
            state.EntityManager.SetComponentData(entity, patrolData);
        }
    }
}