# Design Document

## Overview

This design document outlines the upgrade of enemy AI systems to Unity 6 and Entities 1.3+ standards. The primary objectives are:

1. **Migrate EnemyMovementSystem** from deprecated SystemBase to modern ISystem interface
2. **Maintain existing behavior** - enemies chase player using NavMeshAgent
3. **Ensure compatibility** with hybrid GameObject-Entity architecture
4. **Prepare for future modular AI** - separate movement from decision-making
5. **Validate in performance_lab** - test with varying enemy counts

The design prioritizes getting enemies moving correctly while laying groundwork for future AI customization.

## Architecture

### Current System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Unity Default World                       │
├─────────────────────────────────────────────────────────────┤
│  Enemy Systems                                               │
│  ┌──────────────────┐  ┌──────────────────┐                │
│  │ EnemyMovement    │  │ EnemyAttack      │                │
│  │ System (OLD)     │  │ System (OK)      │                │
│  └────────┬─────────┘  └────────┬─────────┘                │
│           │                     │                            │
│  ┌──────────────────┐  ┌──────────────────┐                │
│  │ EnemyHealth      │  │ EnemyDeath       │                │
│  │ System (OK)      │  │ System (OLD)     │                │
│  └────────┬─────────┘  └────────┬─────────┘                │
│           │                     │                            │
│           ▼                     ▼                            │
│  ┌─────────────────────────────────────────┐                │
│  │     Entity Component Data                │                │
│  │  - EnemyData (tag)                       │                │
│  │  - HealthData                            │                │
│  │  - EnemyAttackData                       │                │
│  │  - DamagedData                           │                │
│  │  - DeadData                              │                │
│  │  - Managed: Transform, NavMeshAgent      │                │
│  └─────────────────────────────────────────┘                │
└─────────────────────────────────────────────────────────────┘
         │                                    
         ▼                                    
┌──────────────────┐              
│  Enemy GameObject│              
│  (MonoBehaviour) │              
│  - Transform     │              
│  - NavMeshAgent  │              
│  - Animator      │              
│  - AudioSource   │              
│  - Collider      │              
└──────────────────┘              
```

### Future Modular AI Architecture (Foundation)

```
┌─────────────────────────────────────────┐
│  EnemyMovementSystem (ISystem)          │
│  ┌─────────────────────────────────┐    │
│  │  Movement Execution             │    │
│  │  - Reads target position        │    │
│  │  - Updates NavMeshAgent         │    │
│  └─────────────────────────────────┘    │
└─────────────────────────────────────────┘
                  ▲
                  │ (reads)
┌─────────────────────────────────────────┐
│  AI Decision Component (Future)          │
│  - Chase behavior                        │
│  - Patrol behavior                       │
│  - Flee behavior                         │
│  - Custom plugin behaviors               │
└─────────────────────────────────────────┘
```

## Components and Interfaces

### Existing Components (No Changes)

```csharp
public struct EnemyData : IComponentData { }

public struct HealthData : IComponentData
{
    public int Value;
}

public struct EnemyAttackData : IComponentData
{
    public Entity Source;
    public Entity Target;
    public int Damage;
    public float Frequency;
    public float Timer;
}

public struct DamagedData : IComponentData
{
    public int Damage;
    public Vector3 HitPoint;
}

public struct DeadData : IComponentData { }
```

### Future AI Component (Not Implemented Yet)

```csharp
// Future: Modular AI behavior component
public struct EnemyAIBehavior : IComponentData
{
    public AIBehaviorType BehaviorType; // Chase, Patrol, Flee, Custom
    public float3 TargetPosition;
    public Entity TargetEntity;
}

public enum AIBehaviorType
{
    Chase,    // Default: chase player
    Patrol,   // Future: patrol waypoints
    Flee,     // Future: run away from player
    Custom    // Future: plugin-defined behavior
}
```

## Data Models

### Enemy Entity Composition

**Enemy Entity:**
- `EnemyData` (tag component)
- `HealthData` (current health)
- `EnemyAttackData` (attack parameters - added when in range)
- `Transform` (managed - position/rotation)
- `NavMeshAgent` (managed - pathfinding)
- `Animator` (managed - animations)
- `AudioSource` (managed - sound effects)
- `CapsuleCollider` (managed - physics)

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: Enemies move toward player when alive

*For any* enemy entity with positive health and a player entity with positive health, the enemy's NavMeshAgent destination should be set to the player's current position.

**Validates: Requirements 1.1, 1.2**

### Property 2: Enemies stop when player dies

*For any* enemy entity, when the player's health reaches zero or below, the enemy's NavMeshAgent should be disabled.

**Validates: Requirements 1.3**

### Property 3: Dead enemies stop moving

*For any* enemy entity with the DeadData component, the enemy's NavMeshAgent should be disabled.

**Validates: Requirements 1.4**

### Property 4: Systems handle null managed components safely

*For any* system accessing managed components (Transform, NavMeshAgent), when a managed component is null or destroyed, the system should skip processing that entity without throwing exceptions.

**Validates: Requirements 4.3, 7.4**

## Error Handling

### Null Reference Handling

All systems must implement defensive null checks when accessing managed components:

```csharp
public void OnUpdate(ref SystemState state)
{
    var entityArray = query.ToEntityArray(state.WorldUpdateAllocator);
    
    foreach (var entity in entityArray)
    {
        if (!state.EntityManager.HasComponent<NavMeshAgent>(entity))
            continue;
            
        var agent = state.EntityManager.GetComponentObject<NavMeshAgent>(entity);
        
        if (agent == null)
        {
            Debug.LogWarning("[EnemyMovementSystem] Null NavMeshAgent, skipping");
            continue;
        }
        
        // Safe to use agent
    }
}
```

### Missing Player Handling

The movement system must gracefully handle the absence of a player:

```csharp
var playerQuery = SystemAPI.QueryBuilder()
    .WithAll<PlayerData, HealthData>()
    .Build();
    
if (playerQuery.IsEmpty)
    return; // No player, skip this frame
```

## Testing Strategy

### Unit Testing Approach

Unit tests will focus on specific scenarios:

1. **Movement Tests**
   - Test enemy moves toward player when both alive
   - Test enemy stops when player dies
   - Test enemy stops when enemy dies
   - Test null NavMeshAgent handling

2. **Integration Tests**
   - Test multiple enemies moving simultaneously
   - Test enemies navigating around obstacles
   - Test attack system triggers when in range

### Property-Based Testing Approach

Property-based tests will verify universal behaviors using the **Unity Test Framework**:

1. **Property Test: Enemy Movement Toward Player (Property 1)**
   - Generate random enemy and player positions
   - Verify NavMeshAgent destination is set to player position
   - Run 100+ iterations

2. **Property Test: Movement Stops on Player Death (Property 2)**
   - Generate random scenarios with dead player
   - Verify all enemy NavMeshAgents are disabled
   - Run 100+ iterations

3. **Property Test: Dead Enemy Stops (Property 3)**
   - Generate random dead enemy scenarios
   - Verify NavMeshAgent is disabled for dead enemies
   - Run 100+ iterations

4. **Property Test: Null Safety (Property 4)**
   - Generate entities with randomly null managed components
   - Verify no exceptions are thrown
   - Run 100+ iterations

### Performance Testing

Test in performance_lab scene with:
- 10 enemies - baseline performance
- 50 enemies - moderate stress test
- 100 enemies - high stress test
- 500 enemies - extreme stress test

Target metrics:
- 60 FPS (16.67ms frame time) with 100 enemies
- EnemyMovementSystem < 0.5ms per frame
- Zero GC allocations in hot paths

## System Implementations

### EnemyMovementSystem (ISystem) - NEW

```csharp
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
        // Only run when enemies exist
        state.RequireForUpdate<EnemyData>();
    }

    public void OnUpdate(ref SystemState state)
    {
        // Find player and check if alive
        var playerQuery = SystemAPI.QueryBuilder()
            .WithAll<PlayerData, HealthData, Transform>()
            .Build();

        if (playerQuery.IsEmpty)
            return;

        // Get player position and health
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

        // Query for all living enemies
        var enemyQuery = SystemAPI.QueryBuilder()
            .WithAll<EnemyData, HealthData>()
            .WithNone<DeadData>()
            .Build();

        var enemyEntities = enemyQuery.ToEntityArray(state.WorldUpdateAllocator);

        // Process each enemy
        foreach (var entity in enemyEntities)
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

            // Move toward player if both are alive
            if (enemyHealth.Value > 0 && playerAlive)
            {
                agent.SetDestination(playerPosition);
            }
            else
            {
                // Stop moving if player or enemy is dead
                agent.enabled = false;
            }
        }
    }
}
```

**Performance Optimizations:**
- Uses QueryBuilder for efficient entity queries
- Early exits to avoid unnecessary processing
- Minimal managed component access
- No allocations in hot path (uses WorldUpdateAllocator)

### EnemyDeathSystem - EVALUATE FOR UPGRADE

The current EnemyDeathSystem uses `WithStructuralChanges()` and `ForEach`, which are deprecated. Consider upgrading to ISystem pattern similar to EnemyMovementSystem.

## Modular AI Plugin Architecture

### Overview

The enemy AI system is designed with modularity in mind, separating movement execution from AI decision-making. This architecture allows developers to create custom AI behaviors without modifying core systems.

### Architecture Pattern

```
┌─────────────────────────────────────────┐
│  AI Decision System (Plugin)            │
│  - Analyzes game state                   │
│  - Makes behavioral decisions            │
│  - Sets target positions/actions         │
└─────────────────┬───────────────────────┘
                  │ (writes to)
                  ▼
┌─────────────────────────────────────────┐
│  EnemyAIBehavior Component               │
│  - Stores current behavior state         │
│  - Holds target position/entity          │
│  - Contains behavior parameters          │
└─────────────────┬───────────────────────┘
                  │ (read by)
                  ▼
┌─────────────────────────────────────────┐
│  EnemyMovementSystem (Core)             │
│  - Reads target from behavior component │
│  - Executes movement via NavMeshAgent   │
│  - Handles pathfinding and obstacles    │
└─────────────────────────────────────────┘
```

### Extension Points

#### 1. AI Behavior Component

The `EnemyAIBehavior` component serves as the interface between decision systems and movement execution:

```csharp
public struct EnemyAIBehavior : IComponentData
{
    public AIBehaviorType BehaviorType;     // Chase, Patrol, Flee, etc.
    public float3 TargetPosition;           // World position target
    public Entity TargetEntity;            // Entity to follow
    public float BehaviorParameter1;       // Speed multiplier, aggression, etc.
    public float BehaviorParameter2;       // Behavior-specific parameters
    public float StateTimer;               // Internal timing
    public bool IsActive;                  // Whether behavior is active
}

public enum AIBehaviorType : byte
{
    Chase = 0,      // Default: chase player
    Patrol = 1,     // Patrol waypoints
    Flee = 2,       // Run from player
    Guard = 3,      // Guard area
    Wander = 4,     // Random movement
    Custom = 255    // Plugin-defined
}
```

#### 2. AI Decision System Interface

Custom AI behaviors implement the `IAIDecisionSystem` interface:

```csharp
public interface IAIDecisionSystem
{
    AIBehaviorType HandledBehaviorType { get; }
    int Priority { get; }
    void UpdateDecisions(ref SystemState state);
    void OnBehaviorActivated(ref SystemState state, Entity entity);
    void OnBehaviorDeactivated(ref SystemState state, Entity entity);
}
```

#### 3. System Registry

The `AISystemRegistry` manages which systems handle which behavior types:

```csharp
public static class AISystemRegistry
{
    public static void RegisterDecisionSystem(IAIDecisionSystem system);
    public static IAIDecisionSystem GetPrimarySystem(AIBehaviorType behaviorType);
    public static List<IAIDecisionSystem> GetAllSystems(AIBehaviorType behaviorType);
}
```

### Creating Custom AI Behaviors

#### Step 1: Define Behavior Data (Optional)

```csharp
public struct PatrolBehaviorData : IComponentData
{
    public Entity WaypointEntity1;
    public Entity WaypointEntity2;
    public Entity WaypointEntity3;
    public int CurrentWaypointIndex;
    public float PatrolSpeed;
    public float WaitTimeAtWaypoint;
}
```

#### Step 2: Implement Decision System

```csharp
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(EnemyMovementSystem))]
public partial struct PatrolAISystem : ISystem, IAIDecisionSystem
{
    public AIBehaviorType HandledBehaviorType => AIBehaviorType.Patrol;
    public int Priority => 100;

    public void OnCreate(ref SystemState state)
    {
        AISystemRegistry.RegisterDecisionSystem(this);
        state.RequireForUpdate<PatrolBehaviorData>();
    }

    public void UpdateDecisions(ref SystemState state)
    {
        foreach (var (behavior, patrolData, entity) in 
                 SystemAPI.Query<RefRW<EnemyAIBehavior>, RefRW<PatrolBehaviorData>>()
                          .WithEntityAccess())
        {
            if (behavior.ValueRO.BehaviorType != AIBehaviorType.Patrol || 
                !behavior.ValueRO.IsActive)
                continue;

            UpdatePatrolLogic(ref behavior.ValueRW, ref patrolData.ValueRW);
        }
    }

    // Implementation details...
}
```

#### Step 3: Create Authoring Component

```csharp
public class PatrolBehaviorAuthoring : MonoBehaviour
{
    public Transform[] waypoints;
    public float patrolSpeed = 3.5f;
    public float waitTime = 2.0f;

    public class Baker : Baker<PatrolBehaviorAuthoring>
    {
        public override void Bake(PatrolBehaviorAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent(entity, new EnemyAIBehavior
            {
                BehaviorType = AIBehaviorType.Patrol,
                BehaviorParameter1 = authoring.patrolSpeed,
                BehaviorParameter2 = authoring.waitTime,
                IsActive = true
            });

            AddComponent(entity, new PatrolBehaviorData
            {
                PatrolSpeed = authoring.patrolSpeed,
                WaitTimeAtWaypoint = authoring.waitTime
            });
        }
    }
}
```

### Plugin Architecture

#### Plugin Structure

```
MyAIPlugin/
├── Components/
│   ├── MyBehaviorData.cs          # Custom behavior data
│   └── MyBehaviorSettings.cs      # Configuration settings
├── Systems/
│   ├── MyAIDecisionSystem.cs      # Main decision logic
│   └── MyBehaviorInitSystem.cs    # Initialization logic
├── Authoring/
│   └── MyBehaviorAuthoring.cs     # GameObject authoring component
└── MyAIPlugin.cs                  # Plugin entry point
```

#### Plugin Entry Point

```csharp
public static class MyAIPlugin
{
    public static void Initialize()
    {
        RegisterCustomBehaviorType();
        Debug.Log("[MyAIPlugin] Initialized successfully");
    }
}
```

### Integration with Core Systems

#### Modified EnemyMovementSystem

The movement system reads from AI behavior components instead of hardcoded player targeting:

```csharp
// In EnemyMovementSystem.OnUpdate()
foreach (var (behavior, entity) in 
         SystemAPI.Query<RefRO<EnemyAIBehavior>>()
                  .WithEntityAccess()
                  .WithAll<EnemyData>()
                  .WithNone<DeadData>())
{
    if (!behavior.ValueRO.IsActive) continue;

    var agent = state.EntityManager.GetComponentObject<NavMeshAgent>(entity);
    if (agent == null) continue;

    // Use target from AI behavior
    if (behavior.ValueRO.TargetEntity != Entity.Null)
    {
        // Follow target entity
        var targetTransform = state.EntityManager.GetComponentObject<Transform>(behavior.ValueRO.TargetEntity);
        if (targetTransform != null)
            agent.SetDestination(targetTransform.position);
    }
    else
    {
        // Move to target position
        agent.SetDestination(behavior.ValueRO.TargetPosition);
    }
}
```

#### Behavior Switching System

```csharp
public partial struct AIBehaviorSwitchSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (behavior, health, entity) in 
                 SystemAPI.Query<RefRW<EnemyAIBehavior>, RefRO<HealthData>>()
                          .WithEntityAccess())
        {
            // Switch to flee if health is low
            if (health.ValueRO.Value < 20 && behavior.ValueRO.BehaviorType != AIBehaviorType.Flee)
            {
                SwitchBehavior(ref state, entity, AIBehaviorType.Flee);
            }
            // Switch back to chase if health is restored
            else if (health.ValueRO.Value >= 50 && behavior.ValueRO.BehaviorType == AIBehaviorType.Flee)
            {
                SwitchBehavior(ref state, entity, AIBehaviorType.Chase);
            }
        }
    }
}
```

### Best Practices

#### Performance Considerations
- **Minimize Entity Queries**: Cache queries in OnCreate when possible
- **Use Burst Compilation**: Mark systems with `[BurstCompile]` when they don't access managed components
- **Batch Operations**: Process multiple entities in single loops
- **Avoid Frequent Behavior Switches**: Add hysteresis to prevent rapid switching

#### Memory Management
- **Use WorldUpdateAllocator**: For temporary allocations within system updates
- **Avoid Managed References**: Prefer entity references over GameObject references
- **Clean Up Resources**: Implement proper cleanup in OnBehaviorDeactivated

#### Debugging
- **Add Debug Logging**: Use conditional compilation for debug output
- **Visual Debugging**: Use Unity's Debug.DrawLine for visualizing AI decisions
- **Profiler Integration**: Add profiler markers for performance analysis

### Migration Path

1. **Phase 1** (Current): Upgrade EnemyMovementSystem to ISystem
2. **Phase 2** (Next): Add EnemyAIBehavior component to existing enemies
3. **Phase 3** (Future): Implement default behavior systems (Chase, Patrol, Flee)
4. **Phase 4** (Future): Create plugin loading infrastructure
5. **Phase 5** (Advanced): Add behavior trees, group AI, debugging tools

### Example Behaviors

#### Chase Behavior (Default)
```csharp
public partial struct ChaseAISystem : ISystem, IAIDecisionSystem
{
    public AIBehaviorType HandledBehaviorType => AIBehaviorType.Chase;
    
    public void UpdateDecisions(ref SystemState state)
    {
        // Find player and set as target for all chase-behavior enemies
        var playerQuery = SystemAPI.QueryBuilder().WithAll<PlayerData>().Build();
        if (playerQuery.IsEmpty) return;
        
        var playerEntity = playerQuery.GetSingletonEntity();
        
        foreach (var (behavior, entity) in 
                 SystemAPI.Query<RefRW<EnemyAIBehavior>>()
                          .WithEntityAccess())
        {
            if (behavior.ValueRO.BehaviorType == AIBehaviorType.Chase)
            {
                behavior.ValueRW.TargetEntity = playerEntity;
            }
        }
    }
}
```

#### Flee Behavior
```csharp
public partial struct FleeAISystem : ISystem, IAIDecisionSystem
{
    public AIBehaviorType HandledBehaviorType => AIBehaviorType.Flee;
    
    public void UpdateDecisions(ref SystemState state)
    {
        // Calculate flee positions away from player
        var playerQuery = SystemAPI.QueryBuilder().WithAll<PlayerData, Transform>().Build();
        if (playerQuery.IsEmpty) return;
        
        var playerTransform = playerQuery.GetSingleton<Transform>();
        var playerPos = playerTransform.position;
        
        foreach (var (behavior, transform, entity) in 
                 SystemAPI.Query<RefRW<EnemyAIBehavior>, RefRO<Transform>>()
                          .WithEntityAccess())
        {
            if (behavior.ValueRO.BehaviorType == AIBehaviorType.Flee)
            {
                var enemyPos = transform.ValueRO.position;
                var fleeDirection = (enemyPos - playerPos).normalized;
                var fleeTarget = enemyPos + fleeDirection * 10f; // Flee 10 units away
                
                behavior.ValueRW.TargetPosition = fleeTarget;
                behavior.ValueRW.TargetEntity = Entity.Null;
            }
        }
    }
}
```

This modular architecture provides a solid foundation for extending enemy AI behaviors while maintaining performance and code organization. The separation of concerns allows for easy testing, debugging, and customization without affecting core movement systems.

## Integration Points

### Bootstrap Integration

Systems will be automatically created by Unity's default world. No manual registration needed.

### Settings Integration

Systems access `SurvivalShooterBootstrap.Settings` for configuration values.

### Existing System Compatibility

The upgraded systems must work alongside:
- `EnemySpawnSystem` - spawns enemies (already upgraded)
- `EnemyAttackSystem` - handles attacks (already modern)
- `EnemyHealthSystem` - processes damage (already modern)
- `EnemyDeathSystem` - handles death (needs evaluation)

## Performance Considerations

### NavMeshAgent Performance

NavMeshAgent is a managed component and cannot be Burst-compiled. Performance optimizations:
- Minimize NavMeshAgent access frequency
- Consider caching player position updates
- Use NavMeshAgent.updatePosition/updateRotation flags wisely

### Future Optimizations

For extreme enemy counts (1000+), consider:
- Custom pathfinding using ECS jobs
- Simplified movement without NavMeshAgent
- LOD system for distant enemies
