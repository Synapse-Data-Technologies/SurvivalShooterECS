# Example Plugin Code

This document contains complete, working example code for implementing a custom AI behavior plugin. This serves as a reference implementation for developers creating their own AI behaviors.

## Core Interface Definitions

### IAIDecisionSystem.cs
```csharp
using Unity.Entities;

namespace EnemyAI.Plugins
{
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
}
```

### EnemyAIBehavior.cs
```csharp
using Unity.Entities;
using Unity.Mathematics;

namespace EnemyAI.Components
{
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
}
```

### AISystemRegistry.cs
```csharp
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using EnemyAI.Components;

namespace EnemyAI.Plugins
{
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
            
            Debug.Log($"[AISystemRegistry] Registered {system.GetType().Name} for {system.HandledBehaviorType} behavior (Priority: {system.Priority})");
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

        /// <summary>
        /// Get all registered behavior types
        /// </summary>
        public static AIBehaviorType[] GetRegisteredBehaviorTypes()
        {
            var types = new AIBehaviorType[_systems.Keys.Count];
            _systems.Keys.CopyTo(types, 0);
            return types;
        }

        /// <summary>
        /// Clear all registered systems (useful for testing)
        /// </summary>
        public static void Clear()
        {
            _systems.Clear();
        }
    }
}
```

## Example Plugin: Patrol Behavior

### PatrolBehaviorData.cs
```csharp
using Unity.Entities;
using Unity.Mathematics;

namespace EnemyAI.Plugins.Patrol
{
    /// <summary>
    /// Additional data for patrol behavior
    /// </summary>
    public struct PatrolBehaviorData : IComponentData
    {
        /// <summary>
        /// Waypoint positions (up to 4 waypoints for simplicity)
        /// </summary>
        public float3 Waypoint1;
        public float3 Waypoint2;
        public float3 Waypoint3;
        public float3 Waypoint4;
        
        /// <summary>
        /// Number of active waypoints (1-4)
        /// </summary>
        public int WaypointCount;
        
        /// <summary>
        /// Current waypoint index
        /// </summary>
        public int CurrentWaypointIndex;
        
        /// <summary>
        /// Speed multiplier for patrol movement
        /// </summary>
        public float PatrolSpeed;
        
        /// <summary>
        /// Time to wait at each waypoint before moving to next
        /// </summary>
        public float WaitTimeAtWaypoint;
        
        /// <summary>
        /// Current wait timer
        /// </summary>
        public float CurrentWaitTime;
        
        /// <summary>
        /// Whether currently waiting at a waypoint
        /// </summary>
        public bool IsWaiting;

        /// <summary>
        /// Get the position of a specific waypoint
        /// </summary>
        public float3 GetWaypoint(int index)
        {
            return index switch
            {
                0 => Waypoint1,
                1 => Waypoint2,
                2 => Waypoint3,
                3 => Waypoint4,
                _ => Waypoint1
            };
        }

        /// <summary>
        /// Set the position of a specific waypoint
        /// </summary>
        public void SetWaypoint(int index, float3 position)
        {
            switch (index)
            {
                case 0: Waypoint1 = position; break;
                case 1: Waypoint2 = position; break;
                case 2: Waypoint3 = position; break;
                case 3: Waypoint4 = position; break;
            }
        }
    }
}
```

### PatrolAISystem.cs
```csharp
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using EnemyAI.Components;
using EnemyAI.Plugins;

namespace EnemyAI.Plugins.Patrol
{
    /// <summary>
    /// AI system that implements patrol behavior
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
                UpdatePatrolBehavior(ref behavior.ValueRW, ref patrolData.ValueRW, deltaTime, entity, ref state);
            }
        }

        private void UpdatePatrolBehavior(ref EnemyAIBehavior behavior, 
                                        ref PatrolBehaviorData patrolData, 
                                        float deltaTime,
                                        Entity entity,
                                        ref SystemState state)
        {
            // Update timers
            behavior.StateTimer += deltaTime;
            
            if (patrolData.IsWaiting)
            {
                patrolData.CurrentWaitTime += deltaTime;
                
                // Check if wait time is over
                if (patrolData.CurrentWaitTime >= patrolData.WaitTimeAtWaypoint)
                {
                    patrolData.IsWaiting = false;
                    patrolData.CurrentWaitTime = 0f;
                    
                    // Move to next waypoint
                    MoveToNextWaypoint(ref behavior, ref patrolData);
                }
            }
            else
            {
                // Check if we've reached the current waypoint
                var currentPos = GetEntityPosition(entity, ref state);
                var targetPos = behavior.TargetPosition;
                var distanceToTarget = math.distance(currentPos, targetPos);
                
                // If close enough to waypoint or timeout, start waiting
                if (distanceToTarget < 2.0f || behavior.StateTimer > 15.0f) // 15s timeout fallback
                {
                    patrolData.IsWaiting = true;
                    patrolData.CurrentWaitTime = 0f;
                    behavior.StateTimer = 0f;
                    
                    // Stop movement while waiting
                    StopMovement(entity, ref state);
                }
            }
        }

        private void MoveToNextWaypoint(ref EnemyAIBehavior behavior, ref PatrolBehaviorData patrolData)
        {
            // Move to next waypoint (circular)
            patrolData.CurrentWaypointIndex = (patrolData.CurrentWaypointIndex + 1) % patrolData.WaypointCount;
            
            // Set new target position
            behavior.TargetPosition = patrolData.GetWaypoint(patrolData.CurrentWaypointIndex);
            behavior.TargetEntity = Entity.Null; // Use position-based targeting
            behavior.StateTimer = 0f;
        }

        private float3 GetEntityPosition(Entity entity, ref SystemState state)
        {
            if (state.EntityManager.HasComponent<Transform>(entity))
            {
                var transform = state.EntityManager.GetComponentObject<Transform>(entity);
                if (transform != null)
                {
                    return transform.position;
                }
            }
            return float3.zero;
        }

        private void StopMovement(Entity entity, ref SystemState state)
        {
            if (state.EntityManager.HasComponent<NavMeshAgent>(entity))
            {
                var agent = state.EntityManager.GetComponentObject<NavMeshAgent>(entity);
                if (agent != null)
                {
                    agent.isStopped = true;
                }
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
                behavior.TargetPosition = patrolData.GetWaypoint(0);
                behavior.TargetEntity = Entity.Null;
                behavior.StateTimer = 0f;
                behavior.IsActive = true;
                
                // Reset patrol state
                patrolData.CurrentWaypointIndex = 0;
                patrolData.IsWaiting = false;
                patrolData.CurrentWaitTime = 0f;
                
                state.EntityManager.SetComponentData(entity, behavior);
                state.EntityManager.SetComponentData(entity, patrolData);
                
                Debug.Log($"[PatrolAISystem] Activated patrol behavior for entity {entity}");
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
                
                Debug.Log($"[PatrolAISystem] Deactivated patrol behavior for entity {entity}");
            }
        }
    }
}
```

### PatrolBehaviorAuthoring.cs
```csharp
using Unity.Entities;
using UnityEngine;
using EnemyAI.Components;

namespace EnemyAI.Plugins.Patrol
{
    /// <summary>
    /// Authoring component for patrol AI behavior
    /// Place this on GameObjects to configure patrol behavior in the editor
    /// </summary>
    public class PatrolBehaviorAuthoring : MonoBehaviour
    {
        [Header("Patrol Settings")]
        [Tooltip("Speed multiplier for patrol movement")]
        public float patrolSpeed = 3.5f;
        
        [Tooltip("Time to wait at each waypoint")]
        public float waitTimeAtWaypoint = 2.0f;
        
        [Header("Waypoints")]
        [Tooltip("Waypoints to patrol between (2-4 waypoints)")]
        public Transform[] waypoints = new Transform[2];

        private void OnValidate()
        {
            // Ensure we have at least 2 waypoints
            if (waypoints.Length < 2)
            {
                System.Array.Resize(ref waypoints, 2);
            }
            
            // Limit to 4 waypoints for simplicity
            if (waypoints.Length > 4)
            {
                System.Array.Resize(ref waypoints, 4);
            }
        }

        private void OnDrawGizmos()
        {
            // Draw waypoints and patrol path
            if (waypoints == null || waypoints.Length < 2) return;

            Gizmos.color = Color.yellow;
            
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;
                
                // Draw waypoint
                Gizmos.DrawWireSphere(waypoints[i].position, 1f);
                
                // Draw path to next waypoint
                int nextIndex = (i + 1) % waypoints.Length;
                if (waypoints[nextIndex] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[nextIndex].position);
                }
            }
        }

        public class Baker : Baker<PatrolBehaviorAuthoring>
        {
            public override void Bake(PatrolBehaviorAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                // Count valid waypoints
                int validWaypointCount = 0;
                for (int i = 0; i < authoring.waypoints.Length; i++)
                {
                    if (authoring.waypoints[i] != null)
                        validWaypointCount++;
                }

                if (validWaypointCount < 2)
                {
                    Debug.LogWarning($"[PatrolBehaviorAuthoring] {authoring.name} needs at least 2 waypoints for patrol behavior");
                    return;
                }

                // Add the AI behavior component
                AddComponent(entity, new EnemyAIBehavior
                {
                    BehaviorType = AIBehaviorType.Patrol,
                    BehaviorParameter1 = authoring.patrolSpeed,
                    BehaviorParameter2 = authoring.waitTimeAtWaypoint,
                    IsActive = true,
                    StateTimer = 0f,
                    TargetEntity = Entity.Null
                });

                // Add patrol-specific data
                var patrolData = new PatrolBehaviorData
                {
                    WaypointCount = validWaypointCount,
                    CurrentWaypointIndex = 0,
                    PatrolSpeed = authoring.patrolSpeed,
                    WaitTimeAtWaypoint = authoring.waitTimeAtWaypoint,
                    IsWaiting = false,
                    CurrentWaitTime = 0f
                };

                // Set waypoint positions
                for (int i = 0; i < validWaypointCount && i < 4; i++)
                {
                    if (authoring.waypoints[i] != null)
                    {
                        patrolData.SetWaypoint(i, authoring.waypoints[i].position);
                    }
                }

                AddComponent(entity, patrolData);
            }
        }
    }
}
```

## Example Plugin: Flee Behavior

### FleeBehaviorData.cs
```csharp
using Unity.Entities;
using Unity.Mathematics;

namespace EnemyAI.Plugins.Flee
{
    /// <summary>
    /// Additional data for flee behavior
    /// </summary>
    public struct FleeBehaviorData : IComponentData
    {
        /// <summary>
        /// Distance to maintain from the threat
        /// </summary>
        public float FleeDistance;
        
        /// <summary>
        /// Speed multiplier when fleeing
        /// </summary>
        public float FleeSpeedMultiplier;
        
        /// <summary>
        /// Entity to flee from (usually the player)
        /// </summary>
        public Entity ThreatEntity;
        
        /// <summary>
        /// Last known threat position
        /// </summary>
        public float3 LastThreatPosition;
        
        /// <summary>
        /// Time since last threat detection
        /// </summary>
        public float TimeSinceLastThreat;
        
        /// <summary>
        /// Maximum time to continue fleeing after losing sight of threat
        /// </summary>
        public float MaxFleeTime;
    }
}
```

### FleeAISystem.cs
```csharp
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using EnemyAI.Components;
using EnemyAI.Plugins;

namespace EnemyAI.Plugins.Flee
{
    /// <summary>
    /// AI system that implements flee behavior
    /// Makes enemies run away from threats (usually the player)
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(EnemyMovementSystem))]
    public partial struct FleeAISystem : ISystem, IAIDecisionSystem
    {
        public AIBehaviorType HandledBehaviorType => AIBehaviorType.Flee;
        public int Priority => 150; // Higher priority than patrol

        public void OnCreate(ref SystemState state)
        {
            AISystemRegistry.RegisterDecisionSystem(this);
            state.RequireForUpdate<FleeBehaviorData>();
        }

        public void OnUpdate(ref SystemState state)
        {
            UpdateDecisions(ref state);
        }

        public void UpdateDecisions(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            // Find the player (primary threat)
            var playerQuery = SystemAPI.QueryBuilder()
                .WithAll<PlayerData, Transform>()
                .Build();

            if (playerQuery.IsEmpty)
                return;

            var playerEntity = playerQuery.GetSingletonEntity();
            var playerTransform = state.EntityManager.GetComponentObject<Transform>(playerEntity);
            if (playerTransform == null)
                return;

            var playerPosition = (float3)playerTransform.position;

            // Update all fleeing entities
            foreach (var (behavior, fleeData, entity) in 
                     SystemAPI.Query<RefRW<EnemyAIBehavior>, RefRW<FleeBehaviorData>>()
                              .WithEntityAccess())
            {
                if (behavior.ValueRO.BehaviorType != AIBehaviorType.Flee || 
                    !behavior.ValueRO.IsActive)
                    continue;

                UpdateFleeBehavior(ref behavior.ValueRW, ref fleeData.ValueRW, 
                                 playerEntity, playerPosition, deltaTime, entity, ref state);
            }
        }

        private void UpdateFleeBehavior(ref EnemyAIBehavior behavior,
                                      ref FleeBehaviorData fleeData,
                                      Entity playerEntity,
                                      float3 playerPosition,
                                      float deltaTime,
                                      Entity entity,
                                      ref SystemState state)
        {
            behavior.StateTimer += deltaTime;
            fleeData.TimeSinceLastThreat += deltaTime;

            var entityPosition = GetEntityPosition(entity, ref state);
            var distanceToPlayer = math.distance(entityPosition, playerPosition);

            // Update threat information
            fleeData.ThreatEntity = playerEntity;
            fleeData.LastThreatPosition = playerPosition;
            fleeData.TimeSinceLastThreat = 0f;

            // Calculate flee direction (away from player)
            var fleeDirection = math.normalize(entityPosition - playerPosition);
            
            // If too close, flee further
            if (distanceToPlayer < fleeData.FleeDistance)
            {
                var fleeTarget = entityPosition + fleeDirection * (fleeData.FleeDistance * 1.5f);
                
                // Make sure flee target is on the ground (simplified)
                fleeTarget.y = entityPosition.y;
                
                behavior.TargetPosition = fleeTarget;
                behavior.TargetEntity = Entity.Null;
            }
            else if (fleeData.TimeSinceLastThreat > fleeData.MaxFleeTime)
            {
                // Stop fleeing after max time
                behavior.IsActive = false;
            }
        }

        private float3 GetEntityPosition(Entity entity, ref SystemState state)
        {
            if (state.EntityManager.HasComponent<Transform>(entity))
            {
                var transform = state.EntityManager.GetComponentObject<Transform>(entity);
                if (transform != null)
                {
                    return transform.position;
                }
            }
            return float3.zero;
        }

        public void OnBehaviorActivated(ref SystemState state, Entity entity)
        {
            if (state.EntityManager.HasComponent<FleeBehaviorData>(entity))
            {
                var fleeData = state.EntityManager.GetComponentData<FleeBehaviorData>(entity);
                var behavior = state.EntityManager.GetComponentData<EnemyAIBehavior>(entity);
                
                behavior.IsActive = true;
                behavior.StateTimer = 0f;
                
                fleeData.TimeSinceLastThreat = 0f;
                
                state.EntityManager.SetComponentData(entity, behavior);
                state.EntityManager.SetComponentData(entity, fleeData);
                
                Debug.Log($"[FleeAISystem] Activated flee behavior for entity {entity}");
            }
        }

        public void OnBehaviorDeactivated(ref SystemState state, Entity entity)
        {
            if (state.EntityManager.HasComponent<EnemyAIBehavior>(entity))
            {
                var behavior = state.EntityManager.GetComponentData<EnemyAIBehavior>(entity);
                behavior.IsActive = false;
                state.EntityManager.SetComponentData(entity, behavior);
                
                Debug.Log($"[FleeAISystem] Deactivated flee behavior for entity {entity}");
            }
        }
    }
}
```

## Plugin Integration Example

### PatrolPlugin.cs
```csharp
using UnityEngine;
using EnemyAI.Plugins;

namespace EnemyAI.Plugins.Patrol
{
    /// <summary>
    /// Entry point for the Patrol AI Plugin
    /// </summary>
    public static class PatrolPlugin
    {
        /// <summary>
        /// Initialize the patrol plugin
        /// Call this during game bootstrap
        /// </summary>
        public static void Initialize()
        {
            Debug.Log("[PatrolPlugin] Initializing Patrol AI Plugin...");
            
            // Systems will auto-register themselves when created by Unity
            // No manual registration needed for ISystem implementations
            
            Debug.Log("[PatrolPlugin] Patrol AI Plugin initialized successfully");
        }
    }
}
```

### Usage in Bootstrap

```csharp
// In your game's bootstrap code
public class GameBootstrap : MonoBehaviour
{
    private void Start()
    {
        // Initialize AI plugins
        EnemyAI.Plugins.Patrol.PatrolPlugin.Initialize();
        EnemyAI.Plugins.Flee.FleePlugin.Initialize();
        
        // Other initialization...
    }
}
```

## Testing Example

### PatrolBehaviorTests.cs
```csharp
using NUnit.Framework;
using Unity.Entities;
using Unity.Mathematics;
using EnemyAI.Components;
using EnemyAI.Plugins.Patrol;

namespace EnemyAI.Tests
{
    [TestFixture]
    public class PatrolBehaviorTests
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
        public void PatrolBehavior_InitializesCorrectly()
        {
            // Create test entity with patrol behavior
            var entity = _entityManager.CreateEntity();
            
            _entityManager.AddComponentData(entity, new EnemyAIBehavior
            {
                BehaviorType = AIBehaviorType.Patrol,
                IsActive = true
            });
            
            var patrolData = new PatrolBehaviorData
            {
                WaypointCount = 3,
                PatrolSpeed = 5f,
                WaitTimeAtWaypoint = 2f
            };
            patrolData.SetWaypoint(0, new float3(0, 0, 0));
            patrolData.SetWaypoint(1, new float3(10, 0, 0));
            patrolData.SetWaypoint(2, new float3(10, 0, 10));
            
            _entityManager.AddComponentData(entity, patrolData);

            // Create and initialize the patrol system
            var system = _world.CreateSystemManaged<PatrolAISystem>();
            var systemState = _world.Unmanaged.ResolveSystemState(system);
            
            system.OnBehaviorActivated(ref systemState, entity);

            // Verify initialization
            var behavior = _entityManager.GetComponentData<EnemyAIBehavior>(entity);
            var updatedPatrolData = _entityManager.GetComponentData<PatrolBehaviorData>(entity);
            
            Assert.IsTrue(behavior.IsActive);
            Assert.AreEqual(AIBehaviorType.Patrol, behavior.BehaviorType);
            Assert.AreEqual(0, updatedPatrolData.CurrentWaypointIndex);
            Assert.IsFalse(updatedPatrolData.IsWaiting);
            Assert.AreEqual(new float3(0, 0, 0), behavior.TargetPosition);
        }

        [Test]
        public void PatrolBehavior_MovesToNextWaypoint()
        {
            // Create test entity
            var entity = _entityManager.CreateEntity();
            
            var behavior = new EnemyAIBehavior
            {
                BehaviorType = AIBehaviorType.Patrol,
                IsActive = true,
                TargetPosition = new float3(0, 0, 0)
            };
            
            var patrolData = new PatrolBehaviorData
            {
                WaypointCount = 2,
                CurrentWaypointIndex = 0,
                IsWaiting = true,
                CurrentWaitTime = 3f, // Exceeded wait time
                WaitTimeAtWaypoint = 2f
            };
            patrolData.SetWaypoint(0, new float3(0, 0, 0));
            patrolData.SetWaypoint(1, new float3(10, 0, 0));
            
            _entityManager.AddComponentData(entity, behavior);
            _entityManager.AddComponentData(entity, patrolData);

            // Create and update the system
            var system = _world.CreateSystemManaged<PatrolAISystem>();
            system.Update();

            // Verify waypoint switch
            var updatedBehavior = _entityManager.GetComponentData<EnemyAIBehavior>(entity);
            var updatedPatrolData = _entityManager.GetComponentData<PatrolBehaviorData>(entity);
            
            Assert.AreEqual(1, updatedPatrolData.CurrentWaypointIndex);
            Assert.AreEqual(new float3(10, 0, 0), updatedBehavior.TargetPosition);
            Assert.IsFalse(updatedPatrolData.IsWaiting);
        }
    }
}
```

This example code provides a complete, working implementation of the modular AI plugin architecture. Developers can use this as a reference for creating their own custom AI behaviors while following the established patterns and interfaces.