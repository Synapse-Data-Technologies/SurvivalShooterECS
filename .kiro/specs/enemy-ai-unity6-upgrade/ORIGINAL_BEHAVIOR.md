# EnemyMovementSystem Original Behavior Documentation

## Purpose
This document captures the original behavior of EnemyMovementSystem.cs before migration to Unity 6 ISystem pattern. This serves as a reference for regression testing to ensure the upgraded system maintains identical behavior.

## Original Implementation Details

### File Location
`Assets/ECS/Scripts/Systems/EnemyMovementSystem.cs`

### Backup Location
`Assets/ECS/Scripts/Systems/EnemyMovementSystem.cs.backup`

### System Type
- **Pattern**: SystemBase (deprecated in Unity 6)
- **Execution**: Main thread with `WithoutBurst()`
- **Update Group**: Default (SimulationSystemGroup)

## Documented Behavior

### 1. Player Detection
**Behavior**: System queries for player entity with PlayerData component
- Queries for entities with `PlayerData`, `Transform`, and `HealthData`
- Captures player GameObject reference and health value
- If no player found, system exits early (returns)

**Expected Result**: 
- Player position is tracked each frame
- Player health status is monitored

### 2. Enemy Movement Logic
**Behavior**: System queries for all living enemies
- Queries for entities with `EnemyData`, `NavMeshAgent`, and `HealthData`
- Excludes entities with `DeadData` component
- For each enemy:
  - If enemy health > 0 AND player health > 0:
    - Sets NavMeshAgent destination to player position
  - Otherwise:
    - Disables NavMeshAgent

**Expected Result**:
- Living enemies chase living player
- Enemies stop when player dies
- Enemies stop when they die

### 3. Component Dependencies
**Required Components**:
- Player: `PlayerData`, `Transform`, `HealthData`
- Enemy: `EnemyData`, `NavMeshAgent`, `HealthData`

**Optional Components**:
- `DeadData` - presence indicates enemy is dead (excluded from movement)

### 4. Edge Cases Handled
1. **No Player**: System returns early if player not found
2. **Dead Player**: Enemies stop moving (agent.enabled = false)
3. **Dead Enemy**: Excluded from query via `WithNone<DeadData>()`
4. **Zero Health**: Treated as dead (agent.enabled = false)

### 5. Performance Characteristics
- **Burst Compilation**: Disabled (`WithoutBurst()`)
- **Job System**: Not used (`.Run()` executes on main thread)
- **Managed Components**: Direct access to Transform and NavMeshAgent
- **Allocations**: Captures GameObject reference each frame

## Regression Test Checklist

After migration, verify the following behaviors remain unchanged:

- [ ] Enemies spawn and immediately begin moving toward player
- [ ] Enemies continuously update path to follow moving player
- [ ] Enemies stop moving when player health reaches 0
- [ ] Enemies stop moving when their own health reaches 0
- [ ] Dead enemies (with DeadData) do not move
- [ ] System handles missing player gracefully (no errors)
- [ ] NavMeshAgent pathfinding works correctly around obstacles
- [ ] Multiple enemies can move simultaneously without interference

## Known Issues in Original Implementation

### Deprecation Warnings
1. **SystemBase**: Deprecated in Unity 6, should use ISystem
2. **Entities.ForEach**: Deprecated pattern, should use SystemAPI queries
3. **WithoutBurst()**: Indicates non-optimal performance path

### Performance Concerns
1. **Main Thread Execution**: No job parallelization
2. **GameObject Capture**: Allocates reference each frame
3. **No Burst Compilation**: Slower than Burst-compiled code

### Functional Limitations
1. **No Null Checks**: Assumes Transform and NavMeshAgent always exist
2. **Single Player**: Only handles first player found
3. **Tight Coupling**: Movement logic directly queries player (not modular)

## Migration Goals

The upgraded system should:
1. ✅ Maintain identical runtime behavior
2. ✅ Use modern ISystem interface
3. ✅ Use SystemAPI for queries
4. ✅ Add null safety checks for managed components
5. ✅ Prepare for future modular AI architecture
6. ✅ Eliminate deprecation warnings

## Testing Validation

### Manual Testing Scenes
1. **Game Scene**: Standard gameplay with single player
2. **performance_lab Scene**: Stress test with multiple enemies (10, 50, 100+)

### Expected Metrics
- 60 FPS with 100 enemies
- EnemyMovementSystem < 0.5ms per frame
- Zero console errors or warnings
- Smooth enemy pathfinding

## Date of Backup
Created: December 1, 2025

## Related Requirements
- Requirements 1.1, 1.2, 1.3, 1.4, 1.5 (Enemy movement behavior)
- Requirements 7.1, 7.2 (No errors or warnings)
