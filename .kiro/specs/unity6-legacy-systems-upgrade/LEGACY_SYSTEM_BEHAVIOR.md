# Legacy System Behavior Documentation

This document describes the behavior of the legacy CameraFollowSystem and EnemySpawnSystem implementations for regression testing purposes.

**Date Created:** December 1, 2025  
**Purpose:** Document current system behavior before Unity 6 migration  
**Backup Location:** 
- `Assets/ECS/Scripts/Systems/CameraFollowSystem.cs.backup`
- `Assets/ECS/Scripts/Systems/EnemySpawnSystem.cs.backup`

---

## CameraFollowSystem (Legacy)

### Architecture
- **Base Class:** `SystemBase` (deprecated pattern)
- **Attributes:** `[DisableAutoCreation]` - prevents automatic system registration
- **Query Pattern:** `Entities.WithoutBurst().ForEach()` (deprecated)

### Behavior Description

#### Initialization
1. System starts with `firstFrame = true`
2. `offset` vector is uninitialized until first frame

#### Per-Frame Execution
1. **Query:** Iterates over entities with `Transform` and `PlayerInputData` components
2. **Camera Check:** Verifies `Camera.main` exists, exits early if null
3. **Position Retrieval:** Gets player position from `transform.gameObject.transform.position`
4. **First Frame Logic:**
   - Calculates initial offset: `offset = mainCamera.position - playerPos`
   - Sets `firstFrame = false`
5. **Smooth Following:**
   - Retrieves smoothing value from `SurvivalShooterBootstrap.Settings.CamSmoothing`
   - Calculates target position: `targetCamPos = playerPos + offset`
   - Interpolates camera position using `Vector3.Lerp(current, target, smoothing * deltaTime)`

### Key Characteristics
- **Offset Maintenance:** Camera maintains constant offset calculated on first frame
- **Smooth Interpolation:** Uses lerp with configurable smoothing factor
- **Single Player Assumption:** Processes all entities with player components (assumes one player)
- **No Burst Compilation:** Uses `WithoutBurst()` due to managed component access
- **Managed Components:** Directly accesses Unity Transform component

### Deprecated Patterns Used
1. `[DisableAutoCreation]` attribute
2. `SystemBase` inheritance
3. `Entities.WithoutBurst().ForEach()` query pattern
4. Direct managed component access in ForEach

### Expected Behavior for Regression Testing
- ✓ Camera should follow player smoothly
- ✓ Camera should maintain initial offset throughout gameplay
- ✓ Camera should handle null Camera.main gracefully
- ✓ Camera should stop updating if player entity is destroyed
- ✓ Smoothing should be configurable via SurvivalShooterBootstrap.Settings

---

## EnemySpawnSystem (Legacy)

### Architecture
- **Base Class:** `SystemBase` (deprecated pattern)
- **Query Pattern:** `Entities.WithoutBurst().ForEach()` and `ToComponentArray()` (both deprecated)
- **State Management:** Uses `List<float>` to track spawn timers per spawner

### Behavior Description

#### Initialization (OnCreate)
1. Creates `spawnerQuery` for entities with `EnemySpawner` component
2. Initializes empty `time` list for tracking spawn timers

#### Per-Frame Execution (OnUpdate)

##### Phase 1: Player Health Check
1. **Query:** Iterates over entities with `PlayerData`, `Transform`, and `HealthData`
2. **Data Collection:** 
   - Stores player GameObject reference
   - Stores player health value
3. **Early Exit:** Returns if player is null or health <= 0

##### Phase 2: Spawner Processing
1. **Query Execution:** Calls `spawnerQuery.ToComponentArray<EnemySpawner>()` (deprecated)
2. **Timer Management:**
   - Ensures `time` list has entry for each spawner (grows dynamically)
   - Increments each spawner's timer by deltaTime
3. **Spawn Logic:**
   - When `time[i] >= spawner[i].SpawnTime`:
     - Instantiates enemy prefab at spawner position
     - Resets timer to 0
4. **Position Source:** Uses `spawner[i].transform.position` for spawn location

### Key Characteristics
- **Independent Timers:** Each spawner has its own timer tracked in parallel list
- **Player-Dependent:** Spawning only occurs when player exists and is alive
- **Dynamic Timer List:** Timer list grows as needed to match spawner count
- **GameObject Instantiation:** Uses `Object.Instantiate()` for enemy creation
- **No Burst Compilation:** Uses `WithoutBurst()` due to managed component access

### Deprecated Patterns Used
1. `SystemBase` inheritance
2. `Entities.WithoutBurst().ForEach()` query pattern
3. `ToComponentArray<T>()` method (deprecated, causes performance issues)
4. Manual state management with List<float>
5. Direct managed component access

### Known Issues
- **Performance:** `ToComponentArray()` creates garbage and is inefficient
- **State Synchronization:** Timer list can become desynchronized if spawners are added/removed dynamically
- **Index Coupling:** Timer indices are coupled to spawner array order, fragile if order changes

### Expected Behavior for Regression Testing
- ✓ Enemies should spawn at spawner locations when timer expires
- ✓ Each spawner should operate independently with its own timer
- ✓ Spawning should stop when player health reaches 0
- ✓ Spawning should stop when player entity doesn't exist
- ✓ Timers should reset to 0 after each spawn
- ✓ Multiple spawners should work simultaneously without interference
- ✓ Spawn timing should match configured SpawnTime values

---

## Migration Goals

### CameraFollowSystem Migration
1. Convert from `SystemBase` to `ISystem` struct
2. Remove `[DisableAutoCreation]` attribute
3. Replace `Entities.ForEach()` with `SystemAPI.Query<>()`
4. Implement `OnCreate()` with `RequireForUpdate<PlayerData>()`
5. Add null safety checks for managed components
6. Maintain identical camera following behavior

### EnemySpawnSystem Migration
1. Convert from `SystemBase` to `ISystem` struct
2. Replace `ToComponentArray()` with proper entity queries
3. Move timer state from List to ECS component data (EnemySpawnerData)
4. Replace `Entities.ForEach()` with `SystemAPI.Query<>()`
5. Implement `OnCreate()` with `RequireForUpdate<EnemySpawnerData>()`
6. Add null safety checks for managed components
7. Maintain identical spawning behavior

### Regression Testing Checklist
After migration, verify:
- [ ] Camera follows player with same smoothness
- [ ] Camera maintains offset correctly
- [ ] Camera handles edge cases (null camera, destroyed player)
- [ ] Enemies spawn at correct locations
- [ ] Spawn timers work independently
- [ ] Spawning stops when player dies
- [ ] Multiple spawners work simultaneously
- [ ] No console errors or warnings
- [ ] Performance is equal or better than legacy implementation

---

## Technical Notes

### Why These Systems Were Disabled
The `[DisableAutoCreation]` attribute on CameraFollowSystem suggests it was disabled due to:
- Compatibility issues with Unity 6
- Deprecated API usage causing warnings/errors
- Need for manual system ordering or initialization

### Performance Considerations
Legacy implementations have performance issues:
- **CameraFollowSystem:** Minimal impact, but not Burst-compiled
- **EnemySpawnSystem:** `ToComponentArray()` creates garbage and is O(n) every frame

Expected performance improvements after migration:
- Elimination of garbage collection from `ToComponentArray()`
- Better cache coherency with proper component queries
- Potential for Burst compilation in future iterations
- More efficient entity iteration patterns

### Compatibility Notes
These systems were written for an earlier version of Unity ECS and use patterns that are:
- Deprecated in Unity 6
- Not compatible with Entities 1.3+
- Scheduled for removal in future Unity versions

The migration is necessary to:
- Ensure long-term compatibility
- Improve performance
- Follow Unity's recommended patterns
- Enable future optimizations (Burst, Jobs, etc.)
