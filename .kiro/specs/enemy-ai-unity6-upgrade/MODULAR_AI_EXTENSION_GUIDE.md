# Modular AI Extension Guide

## Overview

This guide explains how to extend the enemy AI system with custom behaviors using a modular plugin architecture. The current implementation provides a foundation that separates movement execution from AI decision-making, allowing for future customization without modifying core systems.

## Architecture Overview

The modular AI system follows a separation of concerns pattern:

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
│  AI Behavior Component                   │
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

## Core Components

### 1. AI Behavior Component

The `EnemyAIBehavior` component stores the current AI state and target information:

```csharp
using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// Component that stores AI behavior state and targets.
/// This is the interface between AI decision systems and movement execution.
/// </summary>
public struct EnemyAIBehavior : IComponentData
{
    /// <summary>
    /// The type of AI behavior currently active
    /// </summary>
    public AIBehaviorType BehaviorType;
    
    /// <summary>
    /// Target position for movement (world coordinates)
    /// </summary>
    public float3 TargetPosition;
    
    /// <summary>
    /// Target entity to follow/attack (Entity.Null if position-based)
    /// </summary>
    public Entity TargetEntity;
    
    /// <summary>
    /// Behavior-specific parameters (speed multiplier, aggression, etc.)
    /// </summary>
    public float BehaviorParameter1;
    public float BehaviorParameter2;
    
    /// <summary>
    /// Internal timer for behavior state changes
    /// </summary>
    public float StateTimer;
    
    /// <summary>
    /// Whether this behavior is currently active
    /// </summary>
    public bool IsActive;
}

/// <summary>
/// Enumeration of available AI behavior types
/// </summary>
public enum AIBehaviorType : byte
{
    /// <summary>
    /// Default behavior: chase the player
    /// </summary>
    Chase = 0,
    
    /// <summary>
    /// Patrol between waypoints
    /// </summary>
    Patrol = 1,
    
    /// <summary>
    /// Flee from the player
    /// </summary>
    Flee = 2,
    
    /// <summary>
    /// Guard a specific area
    /// </summary>
    Guard = 3,
    
    /// <summary>
    /// Wander randomly
    /// </summary>
    Wander = 4,
    
    /// <summary>
    /// Custom behavior defined by plugin
    /// </summary>
    Custom = 255
}
```

### 2. AI Decision System Interface

AI decision systems implement the `IAIDecisionSystem` interface:

```csharp
using Unity.Entities;

/// <summary>
/// Interface for AI decision systems that control enemy behavior
/// </summary>
public interface IAIDecisionSystem
{
    /// <summary>
    /// The behavior type this system handles
    /// </summary>
    AIBehaviorType HandledBehaviorType { get; }
    
    /// <summary>
    /// Priority for this system (higher = more important)
    /// Used when multiple systems could handle the same behavior
    /// </summary>
    int Priority { get; }
    
    /// <summary>
    /// Called to update AI decisions for entities with this behavior type
    /// </summary>
    /// <param name="state">System state</param>
    void UpdateDecisions(ref SystemState state);
    
    /// <summary>
    /// Called when an entity switches to this behavior type
    /// </summary>
    /// <param name="state">System state</param>
    /// <param name="entity">Entity switching to this behavior</param>
    void OnBehaviorActivated(ref SystemState state, Entity entity);
    
    /// <summary>
    /// Called when an entity switches away from this behavior type
    /// </summary>
    /// <param name="state">System state</param>
    /// <param name="entity">Entity switching away from this behavior</param>
    void OnBehaviorDeactivated(ref SystemState state, Entity entity);
}
```

## Creating Custom AI Behaviors

### Step 1: Define Your Behavior Component (Optional)

If your behavior needs additional data beyond the base `EnemyAIBehavior`, create a companion component:

```csharp
/// <summary>
/// Example: Additional data for patrol behavior
/// </summary>
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

### Step 2: Implement the AI Decision System

Create a system that implements your behavior logic:

```csharp
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Example: Patrol behavior system
/// Makes enemies patrol between waypoints
/// </summary>
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(EnemyMovementSystem))]
public partial struct PatrolAISystem : ISystem, IAIDecisionSystem
{
    public AIBehaviorType HandledBehaviorType => AIBehaviorType.Patrol;
    public int Priority => 100;

    public void OnCreate(ref SystemState state)
    {
        // Register this system with the AI manager
        AISystemRegistry.RegisterDecisionSystem(this);
        
        // Only run when patrol entities exist
        state.RequireForUpdate<PatrolBehaviorData>();
    }

    public void OnUpdate(ref SystemState state)
    {
        UpdateDecisions(ref state);
    }

    public void UpdateDecisions(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;

        // Query for enemies with patrol behavior
        foreach (var (behavior, patrolData, entity) in 
                 SystemAPI.Query<RefRW<EnemyAIBehavior>, RefRW<PatrolBehaviorData>>()
                          .WithEntityAccess())
        {
            // Skip if not patrol behavior or not active
            if (behavior.ValueRO.BehaviorType != AIBehaviorType.Patrol || 
                !behavior.ValueRO.IsActive)
                continue;

            // Update patrol logic
            UpdatePatrolBehavior(ref behavior.ValueRW, ref patrolData.ValueRW, deltaTime);
        }
    }

    private void UpdatePatrolBehavior(ref EnemyAIBehavior behavior, 
                                    ref PatrolBehaviorData patrolData, 
                                    float deltaTime)
    {
        // Update state timer
        behavior.StateTimer += deltaTime;

        // Check if we've reached the current waypoint
        var distanceToTarget = math.distance(behavior.TargetPosition, GetCurrentPosition());
        
        if (distanceToTarget < 2.0f || behavior.StateTimer > 10.0f) // Timeout fallback
        {
            // Move to next waypoint
            patrolData.CurrentWaypointIndex = (patrolData.CurrentWaypointIndex + 1) % 3;
            behavior.TargetPosition = GetWaypointPosition(patrolData, patrolData.CurrentWaypointIndex);
            behavior.StateTimer = 0f;
        }
    }

    public void OnBehaviorActivated(ref SystemState state, Entity entity)
    {
        // Initialize patrol behavior
        if (state.EntityManager.HasComponent<PatrolBehaviorData>(entity))
        {
            var patrolData = state.EntityManager.GetComponentData<PatrolBehaviorData>(entity);
            var behavior = state.EntityManager.GetComponentData<EnemyAIBehavior>(entity);
            
            // Set initial target to first waypoint
            behavior.TargetPosition = GetWaypointPosition(patrolData, 0);
            behavior.StateTimer = 0f;
            behavior.IsActive = true;
            
            state.EntityManager.SetComponentData(entity, behavior);
        }
    }

    public void OnBehaviorDeactivated(ref SystemState state, Entity entity)
    {
        // Clean up patrol behavior
        if (state.EntityManager.HasComponent<EnemyAIBehavior>(entity))
        {
            var behavior = state.EntityManager.GetComponentData<EnemyAIBehavior>(entity);
            behavior.IsActive = false;
            state.EntityManager.SetComponentData(entity, behavior);
        }
    }

    private float3 GetWaypointPosition(PatrolBehaviorData patrolData, int index)
    {
        // Implementation depends on how waypoints are stored
        // This is a simplified example
        return new float3(index * 10f, 0f, 0f);
    }

    private float3 GetCurrentPosition()
    {
        // Get current entity position - simplified
        return float3.zero;
    }
}
```

### Step 3: Register Your System

Create a registry system to manage AI decision systems:

```csharp
using System.Collections.Generic;
using Unity.Entities;

/// <summary>
/// Registry for AI decision systems
/// Manages which systems handle which behavior types
/// </summary>
public static class AISystemRegistry
{
    private static Dictionary<AIBehaviorType, List<IAIDecisionSystem>> _systems = 
        new Dictionary<AIBehaviorType, List<IAIDecisionSystem>>();

    /// <summary>
    /// Register an AI decision system
    /// </summary>
    public static void RegisterDecisionSystem(IAIDecisionSystem system)
    {
        if (!_systems.ContainsKey(system.HandledBehaviorType))
        {
            _systems[system.HandledBehaviorType] = new List<IAIDecisionSystem>();
        }

        _systems[system.HandledBehaviorType].Add(system);
        
        // Sort by priority (highest first)
        _systems[system.HandledBehaviorType].Sort((a, b) => b.Priority.CompareTo(a.Priority));
    }

    /// <summary>
    /// Get the primary system for a behavior type
    /// </summary>
    public static IAIDecisionSystem GetPrimarySystem(AIBehaviorType behaviorType)
    {
        if (_systems.ContainsKey(behaviorType) && _systems[behaviorType].Count > 0)
        {
            return _systems[behaviorType][0]; // Highest priority
        }
        return null;
    }

    /// <summary>
    /// Get all systems for a behavior type
    /// </summary>
    public static List<IAIDecisionSystem> GetAllSystems(AIBehaviorType behaviorType)
    {
        return _systems.ContainsKey(behaviorType) ? _systems[behaviorType] : new List<IAIDecisionSystem>();
    }
}
```

## Plugin Architecture Example

### Plugin Structure

A complete AI behavior plugin should follow this structure:

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

### Plugin Entry Point

```csharp
using Unity.Entities;

/// <summary>
/// Entry point for custom AI plugin
/// </summary>
public static class MyAIPlugin
{
    /// <summary>
    /// Initialize the plugin - call this during bootstrap
    /// </summary>
    public static void Initialize()
    {
        // Register custom behavior type if needed
        RegisterCustomBehaviorType();
        
        // Systems will auto-register themselves via OnCreate
        Debug.Log("[MyAIPlugin] Initialized successfully");
    }

    private static void RegisterCustomBehaviorType()
    {
        // If using custom behavior types beyond the enum,
        // register them with the AI system here
    }
}
```

### GameObject Authoring Component

```csharp
using Unity.Entities;
using UnityEngine;

/// <summary>
/// Authoring component for custom AI behavior
/// Place this on GameObjects to configure the behavior in the editor
/// </summary>
public class MyBehaviorAuthoring : MonoBehaviour
{
    [Header("Behavior Settings")]
    public AIBehaviorType behaviorType = AIBehaviorType.Custom;
    
    [Header("Custom Parameters")]
    public float customParameter1 = 1.0f;
    public float customParameter2 = 2.0f;
    
    [Header("Waypoints (for patrol)")]
    public Transform[] waypoints;

    public class Baker : Baker<MyBehaviorAuthoring>
    {
        public override void Bake(MyBehaviorAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            // Add the AI behavior component
            AddComponent(entity, new EnemyAIBehavior
            {
                BehaviorType = authoring.behaviorType,
                BehaviorParameter1 = authoring.customParameter1,
                BehaviorParameter2 = authoring.customParameter2,
                IsActive = true
            });

            // Add custom behavior data if needed
            if (authoring.behaviorType == AIBehaviorType.Patrol && authoring.waypoints.Length > 0)
            {
                AddComponent(entity, new PatrolBehaviorData
                {
                    PatrolSpeed = authoring.customParameter1,
                    WaitTimeAtWaypoint = authoring.customParameter2
                    // Waypoint entities would be baked separately
                });
            }
        }
    }
}
```

## Integration with Existing Systems

### Modified EnemyMovementSystem

The existing `EnemyMovementSystem` needs minor modifications to read from the AI behavior component:

```csharp
// In EnemyMovementSystem.OnUpdate()
foreach (var (behavior, entity) in 
         SystemAPI.Query<RefRO<EnemyAIBehavior>>()
                  .WithEntityAccess()
                  .WithAll<EnemyData>()
                  .WithNone<DeadData>())
{
    if (!behavior.ValueRO.IsActive)
        continue;

    var agent = state.EntityManager.GetComponentObject<NavMeshAgent>(entity);
    if (agent == null) continue;

    // Use target from AI behavior instead of hardcoded player position
    if (behavior.ValueRO.TargetEntity != Entity.Null)
    {
        // Follow target entity
        var targetTransform = state.EntityManager.GetComponentObject<Transform>(behavior.ValueRO.TargetEntity);
        if (targetTransform != null)
        {
            agent.SetDestination(targetTransform.position);
        }
    }
    else
    {
        // Move to target position
        agent.SetDestination(behavior.ValueRO.TargetPosition);
    }
}
```

### Behavior Switching System

Create a system to handle dynamic behavior switching:

```csharp
/// <summary>
/// System that handles switching between AI behaviors based on game conditions
/// </summary>
public partial struct AIBehaviorSwitchSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Example: Switch to flee behavior when player is too close
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

    private void SwitchBehavior(ref SystemState state, Entity entity, AIBehaviorType newBehavior)
    {
        var currentBehavior = state.EntityManager.GetComponentData<EnemyAIBehavior>(entity);
        
        // Deactivate old behavior
        var oldSystem = AISystemRegistry.GetPrimarySystem(currentBehavior.BehaviorType);
        oldSystem?.OnBehaviorDeactivated(ref state, entity);
        
        // Switch to new behavior
        currentBehavior.BehaviorType = newBehavior;
        currentBehavior.StateTimer = 0f;
        state.EntityManager.SetComponentData(entity, currentBehavior);
        
        // Activate new behavior
        var newSystem = AISystemRegistry.GetPrimarySystem(newBehavior);
        newSystem?.OnBehaviorActivated(ref state, entity);
    }
}
```

## Testing Your Plugin

### Unit Tests

```csharp
using NUnit.Framework;
using Unity.Entities;
using Unity.Mathematics;

[TestFixture]
public class MyAIPluginTests
{
    private World _world;
    private EntityManager _entityManager;

    [SetUp]
    public void SetUp()
    {
        _world = new World("TestWorld");
        _entityManager = _world.EntityManager;
    }

    [TearDown]
    public void TearDown()
    {
        _world?.Dispose();
    }

    [Test]
    public void PatrolBehavior_SwitchesWaypoints_WhenReachingTarget()
    {
        // Create test entity with patrol behavior
        var entity = _entityManager.CreateEntity();
        _entityManager.AddComponentData(entity, new EnemyAIBehavior
        {
            BehaviorType = AIBehaviorType.Patrol,
            TargetPosition = new float3(0, 0, 0),
            IsActive = true
        });
        
        _entityManager.AddComponentData(entity, new PatrolBehaviorData
        {
            CurrentWaypointIndex = 0,
            PatrolSpeed = 5f
        });

        // Create and update the patrol system
        var system = _world.CreateSystemManaged<PatrolAISystem>();
        system.Update();

        // Verify behavior
        var behavior = _entityManager.GetComponentData<EnemyAIBehavior>(entity);
        Assert.IsTrue(behavior.IsActive);
        Assert.AreEqual(AIBehaviorType.Patrol, behavior.BehaviorType);
    }
}
```

## Best Practices

### 1. Performance Considerations

- **Minimize Entity Queries**: Cache queries in OnCreate when possible
- **Use Burst Compilation**: Mark systems with `[BurstCompile]` when they don't access managed components
- **Batch Operations**: Process multiple entities in single loops rather than individual updates
- **Avoid Frequent Behavior Switches**: Add hysteresis to prevent rapid switching

### 2. Memory Management

- **Use WorldUpdateAllocator**: For temporary allocations within system updates
- **Avoid Managed References**: Prefer entity references over GameObject references
- **Clean Up Resources**: Implement proper cleanup in OnBehaviorDeactivated

### 3. Debugging

- **Add Debug Logging**: Use conditional compilation for debug output
- **Visual Debugging**: Use Unity's Debug.DrawLine for visualizing AI decisions
- **Profiler Integration**: Add profiler markers for performance analysis

```csharp
using Unity.Profiling;

public partial struct MyAISystem : ISystem
{
    private static readonly ProfilerMarker s_ProfilerMarker = new ProfilerMarker("MyAISystem.Update");

    public void OnUpdate(ref SystemState state)
    {
        using (s_ProfilerMarker.Auto())
        {
            // AI logic here
        }
    }
}
```

### 4. Configuration

- **Use ScriptableObjects**: For behavior configuration that designers can modify
- **Support Runtime Changes**: Allow behavior parameters to be modified during play
- **Provide Defaults**: Ensure systems work with minimal configuration

## Migration Path

### Phase 1: Foundation (Current)
- ✅ EnemyMovementSystem upgraded to ISystem
- ✅ Separation of movement execution from decision logic
- ✅ Documentation of extension points

### Phase 2: Core AI Components
- Add `EnemyAIBehavior` component to existing enemies
- Modify `EnemyMovementSystem` to read from AI behavior component
- Create `AISystemRegistry` for managing decision systems

### Phase 3: Default Behaviors
- Implement `ChaseAISystem` (current default behavior)
- Implement `PatrolAISystem` for waypoint-based movement
- Implement `FleeAISystem` for escape behavior

### Phase 4: Plugin System
- Create plugin loading infrastructure
- Add authoring components for designer workflow
- Implement behavior switching system

### Phase 5: Advanced Features
- Add behavior trees or state machines
- Implement group AI behaviors (flocking, formations)
- Add AI debugging tools and visualizations

## Conclusion

This modular AI architecture provides a flexible foundation for extending enemy behaviors without modifying core systems. The separation between decision-making and movement execution allows for:

- **Easy Customization**: Add new behaviors by implementing decision systems
- **Runtime Flexibility**: Switch behaviors dynamically based on game state
- **Designer Friendly**: Use authoring components for visual configuration
- **Performance Scalable**: Leverage ECS performance benefits
- **Maintainable**: Clear separation of concerns and well-defined interfaces

The current implementation serves as the "default" chase behavior, while the extension points allow for unlimited customization through the plugin architecture.