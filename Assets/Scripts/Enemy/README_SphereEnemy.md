# Sphere Enemy System

## Overview
The Sphere Enemy is a simple, modular AI enemy type designed for testing AI functions in the project. It consists of a basic sphere geometry with modular AI components and a debug UI overlay.

## Components

### SphereEnemyAI
The main AI controller that handles enemy behavior through a state machine:

**AI Modes:**
- **Idle**: Enemy is stationary, waiting for player detection
- **Patrol**: Enemy moves randomly within a patrol radius
- **Chase**: Enemy pursues the player when detected
- **Attack**: Enemy attacks when in range
- **Retreat**: Enemy moves away from player temporarily

**Configuration:**
- `aiVersion`: Version string displayed in debug UI
- `detectionRange`: Distance at which enemy detects player (default: 10f)
- `attackRange`: Distance at which enemy attacks (default: 2f)
- `moveSpeed`: Movement speed (default: 3f)
- `patrolRadius`: Radius for patrol behavior (default: 5f)

### SphereEnemyHealth
Handles enemy health, damage, and death:

**Settings:**
- `startingHealth`: Initial health points (default: 50)
- `scoreValue`: Points awarded when killed (default: 15)
- `sinkSpeed`: Speed at which enemy sinks after death (default: 2.5f)

**Features:**
- Visual feedback (red flash) when taking damage
- Death particles and sound effects
- Automatic cleanup after death

### SphereEnemyDebugUI
Creates a floating debug UI above the enemy showing:

**Display Information:**
- AI Version (white text)
- Current AI Mode (color-coded text)
  - Idle: Gray
  - Patrol: Green
  - Chase: Orange
  - Attack: Red
  - Retreat: Cyan

**Configuration:**
- `uiOffset`: Position offset from enemy (default: (0, 2, 0))
- `showDebugUI`: Toggle debug UI visibility

## Usage

### Manual Setup
1. Create a sphere primitive GameObject
2. Add the following components:
   - Rigidbody
   - NavMeshAgent
   - AudioSource
   - SphereEnemyAI
   - SphereEnemyHealth
   - SphereEnemyDebugUI
3. Set the tag to "Enemy"
4. Configure component properties as needed

### Using the Spawner
1. Add the `SphereEnemySpawner` component to an empty GameObject
2. Configure spawn settings:
   - `maxEnemies`: Maximum number of enemies to maintain
   - `spawnRadius`: Radius around spawner to place enemies
   - `spawnInterval`: Time between spawns
3. The spawner will automatically create and manage sphere enemies

### Testing AI Functions
The sphere enemy is designed to be an easy target for testing modular AI:

1. **Low Health**: Only 50 HP for quick elimination
2. **Visual Feedback**: Red color and debug UI for easy identification
3. **Modular AI**: Easy to modify AI behavior and version
4. **Debug Information**: Real-time AI state display

## API Reference

### SphereEnemyAI Methods
- `SetAIMode(AIMode newMode)`: Manually set AI mode
- `SetAIVersion(string version)`: Update AI version string

### SphereEnemyHealth Methods
- `TakeDamage(int amount, Vector3 hitPoint)`: Apply damage
- `IsDead()`: Check if enemy is dead

### SphereEnemyDebugUI Methods
- `UpdateDebugInfo(string version, string mode)`: Update debug display
- `ToggleDebugUI()`: Show/hide debug UI

## Integration Notes

- Requires a Player GameObject with "Player" tag
- Needs NavMesh baked in scene for movement
- Compatible with existing enemy systems
- Can be easily extended with additional AI behaviors

## Troubleshooting

**Enemy not moving:**
- Check if NavMesh is baked in the scene
- Verify NavMeshAgent component is properly configured

**Debug UI not showing:**
- Ensure Camera.main is available
- Check if showDebugUI is enabled

**AI not responding:**
- Verify Player GameObject has "Player" tag
- Check detection range settings