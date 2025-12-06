# Integration Guide: Modular AI with Existing Systems

This guide explains how to integrate the modular AI plugin architecture with the existing EnemyMovementSystem and other game systems.

## Overview

The integration involves modifying the existing `EnemyMovementSystem` to read from AI behavior components instead of hardcoded player targeting, while maintaining backward compatibility with existing enemies.

## Step-by-Step Integration

### Step 1: Update EnemyMovementSystem

The existing `EnemyMovementSystem` needs to be modified to support the modular AI architecture:

```csharp
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using EnemyAI.Components; // Add this namespace

/// <summary>
/// Modern ISystem implementation for enemy movement.
/// Moves enemies toward targets specified by AI behavior components.
/// Falls back to player targeting for backward compatibility.
/// </summary>
public partial struct EnemyMovementSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        // Only run when enemies exist
        state.RequireForUpdate<EnemyData>();
    }

    public void OnUpdate(ref SystemState state)
    {
        // Process enemies with AI behavior components first
        ProcessAIBehaviorEnemies(ref state);
        
        // Process legacy enemies (without AI behavior) for backward compatibility
        ProcessLegacyEnemies(ref state);
    }

    /// <summary>
    /// Process enemies that have AI behavior components
    /// </summary>
    private void ProcessAIBehaviorEnemies(ref SystemState state)
    {
        // Query for enemies with AI behavior
        foreach (var (behavior, entity) in 
                 SystemAPI.Query<RefRO<EnemyAIBehavior>>()
                          .WithEntityAccess()
                          .WithAll<EnemyData>()
                          .WithNone<DeadData>())
        {
            // Skip inactive behaviors
            if (!behavior.ValueRO.IsActive)
                continue;

            // Get NavMeshAgent managed component
            if (!state.EntityManager.HasComponent<NavMeshAgent>(entity))
                continue;

            var agent = state.EntityManager.GetComponentObject<NavMeshAgent>(entity);
            if (agent == null)
            {
                Debug.LogWarning($"[EnemyMovementSystem] Null NavMeshAgent on entity {entity}");
                continue;
            }

            // Check if enemy is alive
            var enemyHealth = state.EntityManager.GetComponentData<HealthData>(entity);
            if (enemyHealth.Value <= 0)
            {
                agent.enabled = false;
                continue;
            }

            // Move based on AI behavior target
            MoveToAITarget(agent, behavior.ValueRO, entity, ref state);
        }
    }

    /// <summary>
    /// Process legacy enemies without AI behavior components (backward compatibility)
    /// </summary>
    private void ProcessLegacyEnemies(ref SystemState state)
    {
        // Find player for legacy targeting
        var playerQuery = SystemAPI.QueryBuilder()
            .WithAll<PlayerData, HealthData, Transform>()
            .Build();

        if (playerQuery.IsEmpty)
            return;

        var playerEntities = playerQuery.ToEntityArray(state.WorldUpdateAllocator);
        if (playerEntities.Length == 0)
            return;

        var playerEntity = playerEntities[0];
        var playerHealth = state.EntityManager.GetComponentData<HealthData>(playerEntity);
        
        if (!state.EntityManager.HasComponent<Transform>(playerEntity))
            return;
            
        var playerTransform = state.EntityManager.GetComponentObject<Transform>(playerEntity);
        if (playerTransform == null)
            return;

        var playerPosition = playerTransform.position;
        var playerAlive = playerHealth.Value > 0;

        // Query for legacy enemies (without AI behavior)
        var legacyEnemyQuery = SystemAPI.QueryBuilder()
            .WithAll<EnemyData, HealthData>()
            .WithNone<DeadData, EnemyAIBehavior>() // Exclude enemies with AI behavior
            .Build();

        var legacyEnemyEntities = legacyEnemyQuery.ToEntityArray(state.WorldUpdateAllocator);

        // Process each legacy enemy
        foreach (var entity in legacyEnemyEntities)
        {
            var enemyHealth = state.EntityManager.GetComponentData<HealthData>(entity);
            
            // Get NavMeshAgent managed component
            if (!state.EntityManager.HasComponent<NavMeshAgent>(entity))
                continue;

            var agent = state.EntityManager.GetComponentObject<NavMeshAgent>(entity);
            
            if (agent == null)
            {
                Debug.LogWarning($"[EnemyMovementSystem] Null NavMeshAgent on entity {entity}");
                continue;
            }

            // Legacy behavior: move toward player if both are alive
            if (enemyHealth.Value > 0 && playerAlive)
            {
                agent.SetDestination(playerPosition);
                agent.enabled = true;
            }
            else
            {
                // Stop moving if player or enemy is dead
                agent.enabled = false;
            }
        }
    }

    /// <summary>
    /// Move agent based on AI behavior target
    /// </summary>
    private void MoveToAITarget(NavMeshAgent agent, EnemyAIBehavior behavior, Entity entity, ref SystemState state)
    {
        // Check if we should follow an entity
        if (behavior.TargetEntity != Entity.Null)
        {
            // Follow target entity
            if (state.EntityManager.Exists(behavior.TargetEntity) &&
                state.EntityManager.HasComponent<Transform>(behavior.TargetEntity))
            {
                var targetTransform = state.EntityManager.GetComponentObject<Transform>(behavior.TargetEntity);
                if (targetTransform != null)
                {
                    // Check if target is alive (if it has health)
                    if (state.EntityManager.HasComponent<HealthData>(behavior.TargetEntity))
                    {
                        var targetHealth = state.EntityManager.GetComponentData<HealthData>(behavior.TargetEntity);
                        if (targetHealth.Value <= 0)
                        {
                            // Target is dead, stop moving
                            agent.enabled = false;
                            return;
                        }
                    }

                    agent.SetDestination(targetTransform.position);
                    agent.enabled = true;
                    
                    // Apply speed multiplier if specified
                    if (behavior.BehaviorParameter1 > 0)
                    {
                        agent.speed = agent.speed * behavior.BehaviorParameter1;
                    }
                }
            }
            else
            {
                // Target entity doesn't exist, stop moving
                agent.enabled = false;
            }
        }
        else
        {
            // Move to target position
            agent.SetDestination(behavior.TargetPosition);
            agent.enabled = true;
            
            // Apply speed multiplier if specified
            if (behavior.BehaviorParameter1 > 0)
            {
                agent.speed = agent.speed * behavior.BehaviorParameter1;
            }
        }
    }
}
```

### Step 2: Create AI Behavior Manager System

Create a system to manage AI behavior switching and initialization:

```csharp
using Unity.Entities;
using UnityEngine;
using EnemyAI.Components;
using EnemyAI.Plugins;

/// <summary>
/// System that manages AI behavior switching and initialization
/// </summary>
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(EnemyMovementSystem))]
public partial struct AIBehaviorManagerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        // Initialize default chase behavior for enemies without AI behavior
        state.RequireForUpdate<EnemyData>();
    }

    public void OnUpdate(ref SystemState state)
    {
        // Initialize AI behavior for new enemies
        InitializeNewEnemies(ref state);
        
        // Handle behavior switching based on conditions
        HandleBehaviorSwitching(ref state);
        
        // Update all active AI decision systems
        UpdateAIDecisionSystems(ref state);
    }

    /// <summary>
    /// Initialize AI behavior for enemies that don't have it yet
    /// </summary>
    private void InitializeNewEnemies(ref SystemState state)
    {
        // Find enemies without AI behavior and add default chase behavior
        var newEnemyQuery = SystemAPI.QueryBuilder()
            .WithAll<EnemyData>()
            .WithNone<EnemyAIBehavior, DeadData>()
            .Build();

        var newEnemies = newEnemyQuery.ToEntityArray(state.WorldUpdateAllocator);

        foreach (var entity in newEnemies)
        {
            // Add default chase behavior
            state.EntityManager.AddComponentData(entity, new EnemyAIBehavior
            {
                BehaviorType = AIBehaviorType.Chase,
                IsActive = true,
                StateTimer = 0f,
                TargetEntity = Entity.Null,
                BehaviorParameter1 = 1.0f, // Default speed multiplier
                BehaviorParameter2 = 0f
            });

            Debug.Log($"[AIBehaviorManager] Added default chase behavior to entity {entity}");
        }
    }

    /// <summary>
    /// Handle dynamic behavior switching based on game conditions
    /// </summary>
    private void HandleBehaviorSwitching(ref SystemState state)
    {
        // Find player for distance calculations
        var playerQuery = SystemAPI.QueryBuilder()
            .WithAll<PlayerData, Transform, HealthData>()
            .Build();

        if (playerQuery.IsEmpty)
            return;

        var playerEntity = playerQuery.GetSingletonEntity();
        var playerTransform = state.EntityManager.GetComponentObject<Transform>(playerEntity);
        var playerHealth = state.EntityManager.GetComponentData<HealthData>(playerEntity);

        if (playerTransform == null)
            return;

        var playerPosition = playerTransform.position;
        var playerAlive = playerHealth.Value > 0;

        // Check all enemies for behavior switching conditions
        foreach (var (behavior, health, entity) in 
                 SystemAPI.Query<RefRW<EnemyAIBehavior>, RefRO<HealthData>>()
                          .WithEntityAccess()
                          .WithNone<DeadData>())
        {
            var enemyTransform = state.EntityManager.GetComponentObject<Transform>(entity);
            if (enemyTransform == null)
                continue;

            var distanceToPlayer = Vector3.Distance(enemyTransform.position, playerPosition);
            var enemyHealth = health.ValueRO.Value;

            // Example switching logic:
            
            // Switch to flee if health is very low and player is close
            if (enemyHealth < 20 && distanceToPlayer < 5f && 
                behavior.ValueRO.BehaviorType != AIBehaviorType.Flee &&
                playerAlive)
            {
                SwitchBehavior(ref state, entity, AIBehaviorType.Flee);
            }
            // Switch back to chase if health is restored or player is far
            else if ((enemyHealth >= 50 || distanceToPlayer > 15f) && 
                     behavior.ValueRO.BehaviorType == AIBehaviorType.Flee)
            {
                SwitchBehavior(ref state, entity, AIBehaviorType.Chase);
            }
            // Switch to chase if player is alive and we're not already chasing
            else if (playerAlive && 
                     behavior.ValueRO.BehaviorType != AIBehaviorType.Chase &&
                     behavior.ValueRO.BehaviorType != AIBehaviorType.Flee &&
                     distanceToPlayer < 20f)
            {
                SwitchBehavior(ref state, entity, AIBehaviorType.Chase);
            }
        }
    }

    /// <summary>
    /// Update all registered AI decision systems
    /// </summary>
    private void UpdateAIDecisionSystems(ref SystemState state)
    {
        // Get all registered behavior types
        var behaviorTypes = AISystemRegistry.GetRegisteredBehaviorTypes();

        foreach (var behaviorType in behaviorTypes)
        {
            var system = AISystemRegistry.GetPrimarySystem(behaviorType);
            system?.UpdateDecisions(ref state);
        }
    }

    /// <summary>
    /// Switch an entity's behavior type
    /// </summary>
    private void SwitchBehavior(ref SystemState state, Entity entity, AIBehaviorType newBehavior)
    {
        var currentBehavior = state.EntityManager.GetComponentData<EnemyAIBehavior>(entity);
        
        if (currentBehavior.BehaviorType == newBehavior)
            return; // Already using this behavior

        // Deactivate old behavior
        var oldSystem = AISystemRegistry.GetPrimarySystem(currentBehavior.BehaviorType);
        oldSystem?.OnBehaviorDeactivated(ref state, entity);
        
        // Switch to new behavior
        currentBehavior.BehaviorType = newBehavior;
        currentBehavior.StateTimer = 0f;
        currentBehavior.IsActive = true;
        state.EntityManager.SetComponentData(entity, currentBehavior);
        
        // Activate new behavior
        var newSystem = AISystemRegistry.GetPrimarySystem(newBehavior);
        newSystem?.OnBehaviorActivated(ref state, entity);

        Debug.Log($"[AIBehaviorManager] Switched entity {entity} from {oldSystem?.GetType().Name} to {newBehavior}");
    }
}
```

### Step 3: Create Default Chase AI System

Implement the default chase behavior as a proper AI system:

```csharp
using Unity.Entities;
using UnityEngine;
using EnemyAI.Components;
using EnemyAI.Plugins;

/// <summary>
/// Default AI system that implements chase behavior
/// Makes enemies chase the player (default behavior)
/// </summary>
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(EnemyMovementSystem))]
public partial struct ChaseAISystem : ISystem, IAIDecisionSystem
{
    public AIBehaviorType HandledBehaviorType => AIBehaviorType.Chase;
    public int Priority => 50; // Lower priority than specialized behaviors

    public void OnCreate(ref SystemState state)
    {
        // Register this system with the AI manager
        AISystemRegistry.RegisterDecisionSystem(this);
    }

    public void OnUpdate(ref SystemState state)
    {
        UpdateDecisions(ref state);
    }

    public void UpdateDecisions(ref SystemState state)
    {
        // Find the player
        var playerQuery = SystemAPI.QueryBuilder()
            .WithAll<PlayerData, Transform, HealthData>()
            .Build();

        if (playerQuery.IsEmpty)
            return;

        var playerEntity = playerQuery.GetSingletonEntity();
        var playerHealth = state.EntityManager.GetComponentData<HealthData>(playerEntity);

        // Only chase if player is alive
        if (playerHealth.Value <= 0)
        {
            // Player is dead, deactivate chase behaviors
            foreach (var (behavior, entity) in 
                     SystemAPI.Query<RefRW<EnemyAIBehavior>>()
                              .WithEntityAccess())
            {
                if (behavior.ValueRO.BehaviorType == AIBehaviorType.Chase)
                {
                    behavior.ValueRW.IsActive = false;
                }
            }
            return;
        }

        // Update all chase-behavior entities to target the player
        foreach (var (behavior, entity) in 
                 SystemAPI.Query<RefRW<EnemyAIBehavior>>()
                          .WithEntityAccess())
        {
            if (behavior.ValueRO.BehaviorType == AIBehaviorType.Chase && 
                behavior.ValueRO.IsActive)
            {
                // Set player as target
                behavior.ValueRW.TargetEntity = playerEntity;
                behavior.ValueRW.TargetPosition = default; // Not used when following entity
            }
        }
    }

    public void OnBehaviorActivated(ref SystemState state, Entity entity)
    {
        // Find player and set as target
        var playerQuery = SystemAPI.QueryBuilder()
            .WithAll<PlayerData>()
            .Build();

        if (!playerQuery.IsEmpty)
        {
            var playerEntity = playerQuery.GetSingletonEntity();
            var behavior = state.EntityManager.GetComponentData<EnemyAIBehavior>(entity);
            
            behavior.TargetEntity = playerEntity;
            behavior.IsActive = true;
            behavior.StateTimer = 0f;
            
            state.EntityManager.SetComponentData(entity, behavior);
            
            Debug.Log($"[ChaseAISystem] Activated chase behavior for entity {entity}");
        }
    }

    public void OnBehaviorDeactivated(ref SystemState state, Entity entity)
    {
        if (state.EntityManager.HasComponent<EnemyAIBehavior>(entity))
        {
            var behavior = state.EntityManager.GetComponentData<EnemyAIBehavior>(entity);
            behavior.IsActive = false;
            state.EntityManager.SetComponentData(entity, behavior);
            
            Debug.Log($"[ChaseAISystem] Deactivated chase behavior for entity {entity}");
        }
    }
}
```

### Step 4: Update Bootstrap Integration

Modify your game bootstrap to initialize the AI system:

```csharp
using UnityEngine;
using EnemyAI.Plugins;
using EnemyAI.Plugins.Patrol;
using EnemyAI.Plugins.Flee;

/// <summary>
/// Game bootstrap that initializes all AI plugins
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [Header("AI Settings")]
    [Tooltip("Enable modular AI system")]
    public bool enableModularAI = true;
    
    [Tooltip("AI plugins to initialize")]
    public string[] aiPluginsToLoad = { "Patrol", "Flee" };

    private void Start()
    {
        if (enableModularAI)
        {
            InitializeAISystem();
        }
    }

    private void InitializeAISystem()
    {
        Debug.Log("[GameBootstrap] Initializing Modular AI System...");

        // Clear any existing registrations (useful for play mode testing)
        AISystemRegistry.Clear();

        // Initialize core AI plugins
        foreach (var pluginName in aiPluginsToLoad)
        {
            switch (pluginName.ToLower())
            {
                case "patrol":
                    PatrolPlugin.Initialize();
                    break;
                case "flee":
                    FleePlugin.Initialize();
                    break;
                default:
                    Debug.LogWarning($"[GameBootstrap] Unknown AI plugin: {pluginName}");
                    break;
            }
        }

        Debug.Log("[GameBootstrap] Modular AI System initialized successfully");
    }

    private void OnValidate()
    {
        // Ensure we have some default plugins
        if (aiPluginsToLoad == null || aiPluginsToLoad.Length == 0)
        {
            aiPluginsToLoad = new string[] { "Patrol", "Flee" };
        }
    }
}
```

### Step 5: Migration Strategy

#### Immediate Migration (Recommended)

1. **Update EnemyMovementSystem**: Replace the existing system with the new version
2. **Add AIBehaviorManagerSystem**: This will automatically add chase behavior to existing enemies
3. **Test**: Existing enemies should continue working exactly as before

#### Gradual Migration

1. **Keep both systems**: Rename old system to `LegacyEnemyMovementSystem`
2. **Add new system**: Run both systems in parallel
3. **Migrate enemies**: Gradually add `EnemyAIBehavior` components to enemies
4. **Remove legacy**: Once all enemies have AI behavior, remove legacy system

#### Testing Migration

```csharp
/// <summary>
/// Test system to verify migration is working correctly
/// </summary>
public partial struct AIMigrationTestSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Count enemies with and without AI behavior
        var totalEnemies = SystemAPI.QueryBuilder()
            .WithAll<EnemyData>()
            .WithNone<DeadData>()
            .Build()
            .CalculateEntityCount();

        var aiEnemies = SystemAPI.QueryBuilder()
            .WithAll<EnemyData, EnemyAIBehavior>()
            .WithNone<DeadData>()
            .Build()
            .CalculateEntityCount();

        var legacyEnemies = totalEnemies - aiEnemies;

        if (legacyEnemies > 0)
        {
            Debug.Log($"[AIMigration] {aiEnemies} enemies with AI behavior, {legacyEnemies} legacy enemies");
        }
    }
}
```

## Backward Compatibility

The integration maintains full backward compatibility:

1. **Existing Enemies**: Continue to work with chase behavior
2. **Existing Scripts**: No changes needed to enemy spawning or other systems
3. **Performance**: No performance impact for enemies without AI behavior
4. **Gradual Adoption**: Can add AI behavior to enemies incrementally

## Performance Considerations

### Query Optimization

```csharp
// Cache queries for better performance
private EntityQuery _aiBehaviorQuery;
private EntityQuery _legacyEnemyQuery;

public void OnCreate(ref SystemState state)
{
    _aiBehaviorQuery = SystemAPI.QueryBuilder()
        .WithAll<EnemyData, EnemyAIBehavior>()
        .WithNone<DeadData>()
        .Build();
        
    _legacyEnemyQuery = SystemAPI.QueryBuilder()
        .WithAll<EnemyData>()
        .WithNone<DeadData, EnemyAIBehavior>()
        .Build();
}
```

### Burst Compilation

```csharp
// Mark systems with BurstCompile when possible
[BurstCompile]
public partial struct OptimizedAISystem : ISystem
{
    // Only works if no managed components are accessed
}
```

### Memory Management

```csharp
// Use WorldUpdateAllocator for temporary arrays
var entities = query.ToEntityArray(state.WorldUpdateAllocator);
// Automatically cleaned up at end of frame
```

## Debugging and Monitoring

### AI Debug System

```csharp
/// <summary>
/// Debug system for monitoring AI behavior
/// </summary>
public partial struct AIDebugSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        if (!Application.isEditor)
            return;

        foreach (var (behavior, entity) in 
                 SystemAPI.Query<RefRO<EnemyAIBehavior>>()
                          .WithEntityAccess())
        {
            var entityTransform = state.EntityManager.GetComponentObject<Transform>(entity);
            if (entityTransform == null)
                continue;

            // Draw debug info
            var position = entityTransform.position;
            var color = GetBehaviorColor(behavior.ValueRO.BehaviorType);
            
            Debug.DrawRay(position, Vector3.up * 2f, color);
            
            // Draw target line
            if (behavior.ValueRO.TargetEntity != Entity.Null)
            {
                var targetTransform = state.EntityManager.GetComponentObject<Transform>(behavior.ValueRO.TargetEntity);
                if (targetTransform != null)
                {
                    Debug.DrawLine(position, targetTransform.position, color);
                }
            }
            else
            {
                Debug.DrawLine(position, behavior.ValueRO.TargetPosition, color);
            }
        }
    }

    private Color GetBehaviorColor(AIBehaviorType behaviorType)
    {
        return behaviorType switch
        {
            AIBehaviorType.Chase => Color.red,
            AIBehaviorType.Patrol => Color.blue,
            AIBehaviorType.Flee => Color.yellow,
            AIBehaviorType.Guard => Color.green,
            AIBehaviorType.Wander => Color.magenta,
            _ => Color.white
        };
    }
}
```

This integration guide provides a complete path for adopting the modular AI architecture while maintaining compatibility with existing systems and enemies.