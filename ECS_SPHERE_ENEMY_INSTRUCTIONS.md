# ECS Sphere Enemy System - Setup Instructions

## Overview
I've created a proper ECS-based sphere enemy system that integrates with your existing Survival Shooter ECS architecture. This system provides:

- **Full ECS Integration**: Uses Unity's Entity Component System
- **Modular AI**: State machine with 5 behavior modes
- **Debug UI**: Floating debug information showing AI version and mode
- **Hybrid Architecture**: Works with existing GameObject-based systems
- **Easy Testing**: Simple spawning and configuration tools

## Files Created

### ECS Components
- `Assets/ECS/Scripts/Components/SphereEnemyData.cs` - Core sphere enemy data
- Contains: `SphereEnemyData`, `SphereEnemyPatrolData`, `SphereEnemyDebugData`

### ECS Systems
- `Assets/ECS/Scripts/Systems/SphereEnemyAISystem.cs` - AI behavior system
- `Assets/ECS/Scripts/Systems/SphereEnemyDebugUISystem.cs` - Debug UI system

### Hybrid Components
- `Assets/ECS/Scripts/Hybrid/SphereEnemyObject.cs` - GameObject-Entity bridge
- `Assets/ECS/Scripts/Hybrid/SphereEnemySpawner.cs` - Automatic spawning
- `Assets/ECS/Scripts/Hybrid/SphereEnemyTester.cs` - Interactive testing

## How It Works

### ECS Architecture
1. **SphereEnemyObject** (MonoBehaviour) creates an ECS Entity
2. **SphereEnemyAISystem** (ISystem) processes AI logic for all sphere enemies
3. **SphereEnemyDebugUISystem** (ISystem) manages debug UI for all sphere enemies
4. Systems run automatically in Unity's default ECS world

### AI Behavior Modes
- **Idle**: Stationary, waiting for player detection
- **Patrol**: Random movement within patrol radius
- **Chase**: Pursues player when detected (10 unit range)
- **Attack**: Attacks when in range (2 unit range)
- **Retreat**: Temporarily moves away from player

## Quick Start

### Method 1: Use the Tester (Recommended)
1. Enter Play Mode
2. Press **Space** to spawn a test sphere enemy
3. Press **C** to clear all sphere enemies
4. Watch the debug UI show AI state changes

### Method 2: Use the Spawner
1. The `SphereEnemySpawner` automatically spawns enemies every 3 seconds
2. Maximum of 5 enemies at once
3. Configurable spawn settings in the inspector

### Method 3: Manual Creation
```csharp
// Create sphere enemy GameObject
var sphereEnemy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
sphereEnemy.tag = "Enemy";

// Add ECS hybrid component
var sphereEnemyObject = sphereEnemy.AddComponent<SphereEnemyObject>();

// Configure AI settings
sphereEnemyObject.aiVersion = "MyAI_v1.0";
sphereEnemyObject.detectionRange = 10f;
sphereEnemyObject.moveSpeed = 3f;
```

## Key Features

### ECS Integration
✅ **Proper ECS Architecture** - Uses Entity Component System correctly
✅ **Hybrid GameObject Support** - Works with existing MonoBehaviour systems
✅ **Automatic System Registration** - ISystem implementations run automatically
✅ **Performance Optimized** - Processes all enemies in batches

### AI Features
✅ **Modular AI System** - Easy to extend and modify
✅ **NavMesh Support** - Uses NavMeshAgent when available
✅ **Fallback Movement** - Works without NavMesh
✅ **Player Detection** - Finds Player entities with correct tag
✅ **State Machine** - Clean AI mode transitions

### Debug Features
✅ **Floating Debug UI** - Shows AI version and current mode
✅ **Color-coded States** - Visual feedback for different AI modes
✅ **Real-time Updates** - Debug info updates as AI changes state
✅ **ECS Entity Count** - Shows number of active sphere enemies

## Configuration

### AI Settings (SphereEnemyObject)
- `aiVersion`: Display string for AI version
- `detectionRange`: Distance to detect player (default: 10f)
- `attackRange`: Distance to attack player (default: 2f)
- `moveSpeed`: Movement speed (default: 3f)
- `patrolRadius`: Patrol area radius (default: 5f)

### Debug Settings
- `showDebugUI`: Enable/disable floating debug UI
- `uiOffset`: Position offset for debug UI (default: (0, 2, 0))

## Integration with Existing Systems

### Player Detection
- Automatically finds Player entities with `PlayerData` component
- Requires Player GameObject to have "Player" tag
- Works with existing PlayerObject hybrid system

### Enemy Management
- Integrates with existing `EnemyData` component system
- Uses existing `HealthData` for health management
- Compatible with existing enemy death systems

### NavMesh Support
- Uses existing NavMeshAgent components when available
- Falls back to simple movement without NavMesh
- Automatically detects NavMesh availability

## Testing Your AI Functions

The sphere enemy system is perfect for testing modular AI:

1. **Easy Spawning**: Press Space in play mode to create test enemies
2. **Low Health**: 50 HP for quick elimination testing
3. **Visual Feedback**: Debug UI shows AI state in real-time
4. **Configurable AI**: Easy to change AI version and behavior
5. **ECS Performance**: Handles multiple enemies efficiently

## Troubleshooting

**Enemies not spawning:**
- Check that SphereEnemyTester component is attached
- Ensure you're in Play Mode
- Look for console errors

**Enemies not moving:**
- Verify Player GameObject exists with "Player" tag
- Check if NavMesh is baked (Window > AI > Navigation)
- System will use fallback movement if NavMesh unavailable

**Debug UI not showing:**
- Ensure `showDebugUI` is enabled on SphereEnemyObject
- Check that Camera.main exists in scene
- Debug UI creates automatically when enemy spawns

**ECS systems not running:**
- Systems run automatically as ISystem implementations
- Check console for any compilation errors
- Verify Unity ECS packages are installed

## Next Steps

1. **Test the System**: Enter Play Mode and press Space to spawn enemies
2. **Configure AI**: Modify AI settings on spawned enemies
3. **Extend Behavior**: Add new AI modes to the state machine
4. **Integrate with Game**: Use the spawner in your game scenes
5. **Performance Testing**: Spawn multiple enemies to test ECS performance

The ECS sphere enemy system is now fully integrated and ready for testing your modular AI functions!