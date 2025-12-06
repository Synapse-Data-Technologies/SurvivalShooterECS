# Performance Lab Scene Setup Guide

## Overview
The Perf_Lab scene is a simplified copy of the Game scene designed for performance testing and debugging. It removes visual clutter and adds debug tools.

## Manual Setup Steps

### 1. Clean Up Scene (Delete These GameObjects)
- [ ] **Environment** - Delete entire hierarchy (walls, toys, decorations)
- [ ] **Lights** - Delete Scene Lighting, Light Probe Group, Reflection Probe
- [ ] **BackgroundMusic** - Delete
- [ ] **Launcher** - Delete  
- [ ] **EventSystem** - Delete
- [ ] **Decorative Floor** - Delete the rotated floor with planks (keep the simple Floor at 0,0,0)

### 2. Keep These GameObjects
- [x] **Main Camera** - Already positioned at (1, 15, -22) with 30° rotation
- [x] **Floor** - Simple floor plane at (0,0,0) with scale (100,100,1)
- [x] **Settings** - Contains SurvivalShooterSettings
- [x] **HUDCanvas** - UI with GameUi component
- [x] **RunFixedUpdateSystems** - Manages player systems
- [x] **ZombunnySpawnPoint** - At (-20.5, 0, 12.5)
- [x] **ZombearSpawnPoint** - At (22.5, 0, 15)
- [x] **HellephantSpawnPoint** - At (0, 0, 32)

### 3. Add PlayerDebugWindow Component
1. Wait for the Player to spawn in the scene (it spawns at runtime)
2. Select the **Player** GameObject in the hierarchy
3. Click **Add Component**
4. Search for **PlayerDebugWindow**
5. Configure settings:
   - Offset: (0, 3, 0)
   - Update Interval: 0.1
   - Target Frame Time: 16.67

### 4. Configure Spawn Points
Each spawn point needs an **EnemySpawner** component:

**ZombunnySpawnPoint:**
- Add Component: EnemySpawner
- Enemy: Zombunny prefab (Assets/ECS/Prefabs/Zombunny.prefab)
- Spawn Time: 3.0

**ZombearSpawnPoint:**
- Add Component: EnemySpawner
- Enemy: Zombear prefab (Assets/ECS/Prefabs/Zombear.prefab)
- Spawn Time: 5.0

**HellephantSpawnPoint:**
- Add Component: EnemySpawner
- Enemy: Hellephant prefab (Assets/ECS/Prefabs/Hellephant.prefab)
- Spawn Time: 2.0

### 5. Optional: High-Contrast Floor Material
For better visibility during testing:
1. Select the **Floor** GameObject
2. In Inspector, find the **Mesh Renderer** component
3. Assign the **PerformanceLabFloor** material (Assets/Materials/PerformanceLabFloor.mat)
   - Or create a new material with bright white color and grid texture

## What the Debug Window Shows

The PlayerDebugWindow displays:

**Performance Metrics:**
- FPS (frames per second)
- Frame time in milliseconds
- Red indicator when frame time exceeds 16.67ms (60 FPS threshold)

**Player State:**
- Health value
- Movement input (x, y)
- Position in world space

**Entity Counts:**
- Number of player entities (should be 1)
- Number of enemy entities
- Number of spawner entities

## Testing Workflow

1. **Start the scene** - Player spawns automatically
2. **Move around** - WASD keys
3. **Shoot enemies** - Left mouse button
4. **Watch debug window** - Floats 3 units above player
5. **Monitor performance** - Red sphere appears when FPS drops below 60

## Performance Targets

- **60 FPS** (16.67ms frame time) with 100 enemies
- **Camera system** < 0.1ms per frame
- **Spawn system** < 0.2ms per frame
- **Zero GC allocations** in hot paths

## Notes

- The debug window uses ALINE if available, otherwise falls back to Unity Gizmos
- All debug code is wrapped in conditional compilation for zero overhead in release builds
- The scene is a 1:1 functional copy of Game scene, just with less visual clutter
