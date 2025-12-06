# Design Document

## Overview

This design document outlines the upgrade of legacy Unity ECS systems to Unity 6 and Entities 1.3+ standards with a performance-first approach. The primary objectives are:

1. **Migrate CameraFollowSystem and EnemySpawnSystem** from deprecated SystemBase to modern ISystem interface
2. **Implement hybrid GameObject-Entity architecture** for camera and enemy spawning
3. **Create a performance_lab scene** for isolated testing with high-contrast visuals
4. **Integrate ALINE debug visualizations** for real-time parameter monitoring
5. **Maximize performance** through Burst compilation, optimal data structures, and efficient queries

The design prioritizes performance optimization over visual quality, establishing a foundation for future enhancements.

## Architecture

### System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Unity Default World                       │
├─────────────────────────────────────────────────────────────┤
│  ISystem Implementations (Burst-compiled where possible)     │
│  ┌──────────────────┐  ┌──────────────────┐                │
│  │ CameraFollow     │  │ EnemySpawn       │                │
│  │ System           │  │ System           │                │
│  └────────┬─────────┘  └────────┬─────────┘                │
│           │                     │                            │
│           ▼                     ▼                            │
│  ┌─────────────────────────────────────────┐                │
│  │     Entity Component Data                │                │
│  │  - PlayerData                            │                │
│  │  - HealthData                            │                │
│  │  - PlayerInputData                       │                │
│  │  - EnemySpawnerData                      │                │
│  │  - Managed: Transform, Rigidbody, etc.   │                │
│  └─────────────────────────────────────────┘                │
└─────────────────────────────────────────────────────────────┘
         │                                    │
         ▼                                    ▼
┌──────────────────┐              ┌──────────────────┐
│  PlayerObject    │              │  EnemySpawner    │
│  (MonoBehaviour) │              │  (MonoBehaviour) │
│  - GameObject    │              │  - GameObject    │
│  - Transform     │              │  - Transform     │
│  - Rigidbody     │              │  - Prefab Ref    │
│  - Animator      │              │  - Spawn Config  │
└──────────────────┘              └──────────────────┘
```

### Hybrid Architecture Pattern

The system uses a hybrid approach where:
- **GameObjects** provide visual representation and Unity component functionality
- **Entities** store game state and enable high-performance ECS processing
- **Managed Components** bridge the gap, allowing systems to access GameObject components

### Performance_Lab Scene Architecture

```
performance_lab Scene
├── Environment
│   ├── High-Contrast Floor (bright grid material)
│   ├── Directional Light
│   └── Main Camera
├── Player
│   ├── Player Prefab Instance
│   └── Debug Window (ALINE-based)
├── Enemies
│   ├── Standard Enemy Prefabs
│   └── Debug Enemy Variants (with ALINE visualizations)
├── Spawners
│   ├── EnemySpawner GameObjects (4-8 instances)
│   └── Configurable spawn rates
└── Settings
    └── SurvivalShooterSettings GameObject
```

## Components and Interfaces

### New Component: EnemySpawnerData

```csharp
public struct EnemySpawnerData : IComponentData
{
    public float SpawnTime;      // Time between spawns
    public float CurrentTime;    // Accumulated time since last spawn
    public Entity PrefabEntity;  // Entity prefab to spawn (future optimization)
}
```

### Modified Component: EnemySpawner (MonoBehaviour)

The existing `EnemySpawner` MonoBehaviour will be enhanced to create an entity and register itself:

```csharp
public class EnemySpawner : MonoBehaviour
{
    public GameObject Enemy;
    public float SpawnTime = 3f;
    public Entity Entity;

    private void Start()
    {
        // Create entity and link to this GameObject
        var world = World.DefaultGameObjectInjectionWorld;
        var entityManager = world.EntityManager;
        
        Entity = entityManager.CreateEntity();
        entityManager.AddComponentData(Entity, new EnemySpawnerData 
        { 
            SpawnTime = SpawnTime,
            CurrentTime = 0f
        });
        entityManager.AddComponentObject(Entity, transform);
        entityManager.AddComponentObject(Entity, this);
    }
}
```

### Debug Component: PlayerDebugWindow

```csharp
public class PlayerDebugWindow : MonoBehaviour
{
    private Entity playerEntity;
    
    // Cached values for display
    private float fps;
    private float frameTime;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private int health;
    private Vector3 velocity;
    
    private void Update()
    {
        // Update metrics
        // Render using ALINE Draw.Label3D
    }
}
```

## Data Models

### Entity Composition

**Player Entity:**
- `PlayerData` (tag component)
- `HealthData` (health value)
- `PlayerInputData` (movement and look input)
- `Transform` (managed - GameObject transform)
- `Rigidbody` (managed - physics body)
- `Animator` (managed - animation controller)

**Enemy Spawner Entity:**
- `EnemySpawnerData` (spawn timing and configuration)
- `Transform` (managed - spawn position)
- `EnemySpawner` (managed - MonoBehaviour with prefab reference)

**Future Enemy Entity** (for reference):
- `EnemyData` (tag component)
- `HealthData` (health value)
- `Transform` (managed)
- `Rigidbody` (managed)
- `Animator` (managed)

### Performance Metrics Data

```csharp
public struct PerformanceMetrics
{
    public float FPS;
    public float FrameTimeMs;
    public int PlayerCount;
    public int EnemyCount;
    public int SpawnerCount;
    public float CameraSystemTime;
    public float SpawnSystemTime;
}
```


## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: Camera maintains offset during player movement

*For any* player position change, the camera should maintain the initial offset vector while smoothly interpolating to the target position using the configured smoothing value.

**Validates: Requirements 1.2, 1.3**

### Property 2: Enemies spawn at correct locations when timer expires

*For any* enemy spawner with a configured spawn time, when the accumulated time reaches or exceeds the spawn time, an enemy should be instantiated at the spawner's transform position.

**Validates: Requirements 2.2**

### Property 3: Spawn timer resets after spawning

*For any* spawner that has just spawned an enemy, the spawn timer should be reset to zero immediately after the spawn event.

**Validates: Requirements 2.3**

### Property 4: Spawners operate independently

*For any* set of multiple spawners with different spawn times, each spawner's timer should advance and trigger spawns independently without affecting other spawners' timers or spawn events.

**Validates: Requirements 2.6**

### Property 5: Systems handle null managed components safely

*For any* system accessing managed components (Transform, Rigidbody, MonoBehaviour references), when a managed component is null or destroyed, the system should skip processing that entity without throwing exceptions or crashing.

**Validates: Requirements 4.4, 8.4**

### Property 6: Debug window displays accurate real-time data

*For any* player entity component value (health, movement input, velocity), the debug window should display the current value with at most one frame of latency.

**Validates: Requirements 6.2, 6.4**

### Property 7: Entity counts match actual world state

*For any* point in time, the displayed entity counts for players, enemies, and spawners should exactly match the number of entities with those component types in the world.

**Validates: Requirements 7.2**

### Property 8: Visual indicators appear when performance degrades

*For any* frame where frame time exceeds the configured threshold, a visual indicator should be displayed in the debug window.

**Validates: Requirements 7.6**

## Error Handling

### Null Reference Handling

All systems must implement defensive null checks when accessing managed components:

```csharp
public void OnUpdate(ref SystemState state)
{
    foreach (var (transform, spawnerData) in 
        SystemAPI.Query<Transform, RefRW<EnemySpawnerData>>())
    {
        if (transform == null)
        {
            Debug.LogWarning("[EnemySpawnSystem] Null transform detected, skipping entity");
            continue;
        }
        
        // Safe to use transform
    }
}
```

### Missing Player Handling

Both camera and spawn systems must gracefully handle the absence of a player entity:

```csharp
// Early exit if no player found
var playerQuery = SystemAPI.QueryBuilder()
    .WithAll<PlayerData, Transform>()
    .Build();
    
if (playerQuery.IsEmpty)
{
    return; // No player, skip this frame
}
```

### Destroyed GameObject Handling

When GameObjects are destroyed but entities still exist, systems must detect and handle this:

```csharp
if (transform == null || transform.gameObject == null)
{
    // GameObject was destroyed, clean up entity
    state.EntityManager.DestroyEntity(entity);
    continue;
}
```

### Performance Degradation Handling

When frame time exceeds acceptable thresholds, the debug system should:
1. Display a visual warning
2. Log the frame time spike
3. Optionally reduce spawn rates or other expensive operations

## Testing Strategy

### Unit Testing Approach

Unit tests will focus on specific scenarios and edge cases:

1. **Camera Initialization Tests**
   - Test offset calculation on first frame
   - Test camera behavior when player is destroyed
   - Test camera behavior when no player exists

2. **Spawner Initialization Tests**
   - Test entity creation during MonoBehaviour Start()
   - Test spawner registration with correct components
   - Test multiple spawners in scene

3. **Debug Window Tests**
   - Test debug window attachment to player
   - Test metric calculation accuracy
   - Test visual indicator triggering

4. **Error Condition Tests**
   - Test null transform handling
   - Test missing player handling
   - Test destroyed GameObject handling

### Property-Based Testing Approach

Property-based tests will verify universal behaviors across many inputs using the **Unity Test Framework** with custom property test utilities (since Unity doesn't have a built-in PBT library, we'll create lightweight generators):

1. **Property Test: Camera Offset Maintenance (Property 1)**
   - Generate random player positions and movements
   - Verify offset remains constant across all movements
   - Verify interpolation uses correct smoothing value
   - Run 100+ iterations with varied inputs

2. **Property Test: Spawn Location Accuracy (Property 2)**
   - Generate random spawner positions and spawn times
   - Advance time in random increments
   - Verify enemies spawn at exact spawner positions
   - Run 100+ iterations

3. **Property Test: Timer Reset Consistency (Property 3)**
   - Generate random spawn events
   - Verify timer is always zero immediately after spawn
   - Run 100+ iterations

4. **Property Test: Spawner Independence (Property 4)**
   - Generate multiple spawners with random configurations
   - Verify each spawner's behavior is unaffected by others
   - Run 100+ iterations with 2-10 spawners

5. **Property Test: Null Safety (Property 5)**
   - Generate entities with randomly null managed components
   - Verify no exceptions are thrown
   - Verify systems continue processing other entities
   - Run 100+ iterations

6. **Property Test: Debug Display Accuracy (Property 6)**
   - Generate random player state changes
   - Verify debug window reflects changes within one frame
   - Run 100+ iterations

7. **Property Test: Entity Count Accuracy (Property 7)**
   - Generate random entity creation/destruction events
   - Verify displayed counts always match actual counts
   - Run 100+ iterations

8. **Property Test: Performance Indicator Triggering (Property 8)**
   - Simulate frames with random frame times
   - Verify indicator appears when threshold exceeded
   - Verify indicator disappears when below threshold
   - Run 100+ iterations

### Testing Workflow

1. **Development Phase**: Test in performance_lab scene
   - Isolated testing of individual systems
   - High-contrast visuals for easy debugging
   - Configurable stress testing

2. **Integration Phase**: Test in performance_lab with full system
   - All systems running together
   - Monitor performance metrics
   - Validate no system interactions cause issues

3. **Validation Phase**: Test in original game scene
   - Full game environment
   - Real gameplay scenarios
   - Final performance validation

### Performance Testing

Performance tests will measure:
- Frame time with varying enemy counts (10, 50, 100, 500 enemies)
- System execution time for CameraFollowSystem
- System execution time for EnemySpawnSystem
- Memory allocation per frame
- Burst compilation effectiveness

Target metrics:
- 60 FPS (16.67ms frame time) with 100 enemies
- Camera system < 0.1ms per frame
- Spawn system < 0.2ms per frame
- Zero GC allocations in hot paths


## System Implementations

### CameraFollowSystem (ISystem)

```csharp
[BurstCompile]
public partial struct CameraFollowSystem : ISystem
{
    private bool firstFrame;
    private float3 offset;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        firstFrame = true;
        state.RequireForUpdate<PlayerData>();
    }
    
    public void OnUpdate(ref SystemState state)
    {
        var mainCamera = Camera.main;
        if (mainCamera == null)
            return;
            
        // Query for player with Transform (managed component)
        foreach (var (transform, playerData) in 
            SystemAPI.Query<Transform, PlayerData>())
        {
            if (transform == null)
                continue;
                
            var playerPos = transform.position;
            
            if (firstFrame)
            {
                offset = mainCamera.transform.position - playerPos;
                firstFrame = false;
            }
            
            var settings = SurvivalShooterBootstrap.Settings;
            var smoothing = settings.CamSmoothing;
            var dt = SystemAPI.Time.DeltaTime;
            var targetCamPos = playerPos + (Vector3)offset;
            
            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position, 
                targetCamPos, 
                smoothing * dt);
                
            // Only one player, so break after first
            break;
        }
    }
}
```

**Performance Optimizations:**
- Uses `[BurstCompile]` on struct and OnCreate (OnUpdate can't be Burst due to Camera.main)
- Early exit if no camera
- Breaks after processing first player (only one player in game)
- Minimal managed component access

### EnemySpawnSystem (ISystem)

```csharp
public partial struct EnemySpawnSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemySpawnerData>();
    }
    
    public void OnUpdate(ref SystemState state)
    {
        // Check if player exists and is alive
        var playerQuery = SystemAPI.QueryBuilder()
            .WithAll<PlayerData, HealthData>()
            .Build();
            
        if (playerQuery.IsEmpty)
            return;
            
        var playerHealth = 0;
        foreach (var health in SystemAPI.Query<RefRO<HealthData>>()
            .WithAll<PlayerData>())
        {
            playerHealth = health.ValueRO.Value;
            break;
        }
        
        if (playerHealth <= 0)
            return;
            
        var dt = SystemAPI.Time.DeltaTime;
        
        // Process each spawner
        foreach (var (transform, spawnerMB, spawnerData, entity) in 
            SystemAPI.Query<Transform, EnemySpawner, RefRW<EnemySpawnerData>>()
                .WithEntityAccess())
        {
            if (transform == null || spawnerMB == null)
            {
                Debug.LogWarning($"[EnemySpawnSystem] Null component on entity {entity}");
                continue;
            }
            
            ref var data = ref spawnerData.ValueRW;
            data.CurrentTime += dt;
            
            if (data.CurrentTime >= data.SpawnTime)
            {
                // Spawn enemy
                Object.Instantiate(
                    spawnerMB.Enemy, 
                    transform.position, 
                    quaternion.identity);
                    
                // Reset timer
                data.CurrentTime = 0f;
            }
        }
    }
}
```

**Performance Optimizations:**
- Minimal queries (one for player health, one for spawners)
- Early exits to avoid unnecessary processing
- Direct component access via RefRW for cache efficiency
- No allocations in hot path

### PlayerDebugWindow (MonoBehaviour)

```csharp
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Drawing; // ALINE namespace

public class PlayerDebugWindow : MonoBehaviour
{
    private Entity playerEntity;
    private World world;
    
    // Display settings
    public Vector3 offset = new Vector3(0, 3, 0);
    public float updateInterval = 0.1f;
    private float timeSinceUpdate = 0f;
    
    // Cached metrics
    private float fps;
    private float frameTime;
    private float2 moveInput;
    private float2 lookInput;
    private int health;
    private Vector3 position;
    private int enemyCount;
    private int spawnerCount;
    
    // Performance thresholds
    public float targetFrameTime = 16.67f; // 60 FPS
    
    private void Start()
    {
        world = World.DefaultGameObjectInjectionWorld;
        
        // Find player entity
        var playerObj = GetComponent<PlayerObject>();
        if (playerObj != null)
        {
            playerEntity = playerObj.Entity;
        }
    }
    
    private void Update()
    {
        if (world == null || !world.EntityManager.Exists(playerEntity))
            return;
            
        timeSinceUpdate += Time.deltaTime;
        
        // Update metrics at interval to reduce overhead
        if (timeSinceUpdate >= updateInterval)
        {
            UpdateMetrics();
            timeSinceUpdate = 0f;
        }
        
        // Render debug info using ALINE
        RenderDebugInfo();
    }
    
    private void UpdateMetrics()
    {
        var em = world.EntityManager;
        
        // FPS and frame time
        fps = 1f / Time.deltaTime;
        frameTime = Time.deltaTime * 1000f;
        
        // Player data
        if (em.HasComponent<PlayerInputData>(playerEntity))
        {
            var inputData = em.GetComponentData<PlayerInputData>(playerEntity);
            moveInput = inputData.Move;
            lookInput = inputData.Look;
        }
        
        if (em.HasComponent<HealthData>(playerEntity))
        {
            health = em.GetComponentData<HealthData>(playerEntity).Value;
        }
        
        position = transform.position;
        
        // Entity counts
        enemyCount = CountEntitiesWithComponent<EnemyData>();
        spawnerCount = CountEntitiesWithComponent<EnemySpawnerData>();
    }
    
    private int CountEntitiesWithComponent<T>() where T : struct, IComponentData
    {
        var query = world.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<T>());
        var count = query.CalculateEntityCount();
        query.Dispose();
        return count;
    }
    
    private void RenderDebugInfo()
    {
        using (Draw.WithDuration(updateInterval))
        {
            var worldPos = transform.position + offset;
            
            // Performance indicator color
            var color = frameTime > targetFrameTime ? Color.red : Color.green;
            
            // Build debug text
            var debugText = $"FPS: {fps:F1}\n" +
                          $"Frame: {frameTime:F2}ms\n" +
                          $"Health: {health}\n" +
                          $"Move: ({moveInput.x:F2}, {moveInput.y:F2})\n" +
                          $"Pos: ({position.x:F1}, {position.y:F1}, {position.z:F1})\n" +
                          $"Enemies: {enemyCount}\n" +
                          $"Spawners: {spawnerCount}";
            
            // Draw label at world position
            Draw.Label3D(worldPos, debugText, color, fontSize: 12);
            
            // Draw performance indicator sphere
            if (frameTime > targetFrameTime)
            {
                Draw.WireSphere(worldPos + new Vector3(0, 0.5f, 0), 0.2f, Color.red);
            }
        }
    }
}
```

**ALINE Best Practices:**
- Use `Draw.WithDuration()` to avoid redrawing every frame
- Batch all drawing calls together
- Use appropriate primitive types (Label3D for text, WireSphere for indicators)
- Conditional compilation for production builds (add `#if UNITY_EDITOR` guards)

## Performance_Lab Scene Setup

### Scene Hierarchy

```
performance_lab
├── Environment
│   ├── Floor (Plane with high-contrast grid material)
│   ├── Directional Light
│   └── Walls (optional boundaries)
├── Main Camera
│   └── (Standard camera setup, will be controlled by CameraFollowSystem)
├── Settings
│   └── SurvivalShooterSettings (GameObject with settings component)
├── Player
│   └── Player Prefab Instance (with PlayerDebugWindow attached)
├── Spawners
│   ├── Spawner_1 (EnemySpawner, SpawnTime=3s)
│   ├── Spawner_2 (EnemySpawner, SpawnTime=5s)
│   ├── Spawner_3 (EnemySpawner, SpawnTime=2s)
│   └── Spawner_4 (EnemySpawner, SpawnTime=4s)
└── UI
    └── GameUi (existing UI prefab)
```

### High-Contrast Floor Material

```
Material: PerformanceLabFloor
- Shader: Standard
- Albedo: Bright white (#FFFFFF) with black grid texture
- Metallic: 0
- Smoothness: 0.5
- Tiling: 10x10 (for clear grid)
```

### Debug Enemy Variants

Create enemy prefabs with ALINE visualizations:
- Draw health bar above enemy
- Draw attack range sphere
- Draw path to player
- Color-code by state (idle, chasing, attacking)

## Integration Points

### Bootstrap Integration

The systems will be automatically created by Unity's default world. No manual registration needed due to removing `[DisableAutoCreation]`.

### Settings Integration

Both systems access `SurvivalShooterBootstrap.Settings` for configuration values:
- `CamSmoothing` - camera interpolation speed
- Enemy prefab references
- Spawn timing defaults

### Existing System Compatibility

The upgraded systems must work alongside:
- `PlayerInputSystem` - provides input data
- `PlayerMovementSystem` - moves player
- `PlayerTurningSystem` - rotates player
- `PlayerShootingSystem` - handles shooting

No conflicts expected as camera and spawn systems don't modify player components.

## Build Configuration

### Debug Build

```csharp
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    // Enable debug visualizations
    #define ENABLE_DEBUG_WINDOW
    #define ENABLE_ALINE_DRAWING
#endif
```

### Release Build

- Strip all ALINE drawing calls
- Remove PlayerDebugWindow component
- Disable performance metrics collection
- Ensure zero overhead from debug code

## Migration Path

### Phase 1: Camera System Upgrade
1. Backup existing CameraFollowSystem.cs
2. Implement new ISystem version
3. Test in performance_lab scene
4. Validate in main game scene

### Phase 2: Enemy Spawn System Upgrade
1. Backup existing EnemySpawnSystem.cs
2. Update EnemySpawner MonoBehaviour to create entities
3. Implement new ISystem version
4. Test spawning in performance_lab scene
5. Validate in main game scene

### Phase 3: Debug Infrastructure
1. Create performance_lab scene
2. Implement PlayerDebugWindow
3. Add ALINE visualizations
4. Configure build stripping

### Phase 4: Performance Validation
1. Run performance tests
2. Profile with Unity Profiler
3. Optimize bottlenecks
4. Document performance characteristics

## Future Enhancements

### Enemy Entity Conversion

Currently enemies are spawned as GameObjects. Future optimization:
- Convert enemies to pure ECS entities
- Use entity prefabs instead of GameObject prefabs
- Implement enemy AI as ECS systems
- Expected 10-100x performance improvement for large enemy counts

### Camera System Optimization

Potential improvements:
- Move camera logic to Burst-compiled job
- Use unmanaged camera data structure
- Implement camera shake and effects as ECS systems

### Advanced Debug Features

- Timeline recording and playback
- Performance regression detection
- Automated stress testing
- Heat maps for spawn locations and player movement
