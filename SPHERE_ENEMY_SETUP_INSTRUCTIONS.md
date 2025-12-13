# Sphere Enemy Setup Instructions

## Overview
I've created a new modular sphere enemy type for your Unity project with the following features:
- Simple sphere geometry (easy to kill for testing)
- Modular AI system with multiple behavior modes
- Debug UI overlay showing AI version and current mode
- Easy to spawn and configure for testing

## Files Created

### Core Scripts (Assets/Scripts/Enemy/)
- `SphereEnemyAI.cs` - Main AI controller with state machine
- `SphereEnemyHealth.cs` - Health management and death handling
- `SphereEnemyDebugUI.cs` - Floating debug UI display

### Utility Scripts (Assets/Scripts/)
- `SphereEnemyDemo.cs` - Easy creation and testing
- `SphereEnemySpawner.cs` - Automatic enemy spawning
- `SphereEnemyTester.cs` - Interactive testing controls

### Editor Scripts (Assets/Scripts/Editor/)
- `SphereEnemyMenuItems.cs` - Unity menu items for creation
- `SphereEnemyEditor.cs` - Custom inspector tools

### Documentation
- `Assets/Scripts/Enemy/README_SphereEnemy.md` - Detailed documentation

## Quick Setup

### Method 1: Using the Demo Script
1. Create an empty GameObject in your scene
2. Add the `SphereEnemyDemo` component to it
3. Use the context menu (right-click component) to:
   - "Create Demo Sphere Enemy" - Creates one enemy
   - "Create Multiple Demo Enemies" - Creates several enemies
   - "Clear All Demo Enemies" - Removes all demo enemies

### Method 2: Manual Creation
1. Create a Sphere primitive GameObject
2. Set tag to "Enemy"
3. Add these components:
   - Rigidbody
   - NavMeshAgent
   - AudioSource
   - SphereEnemyAI
   - SphereEnemyHealth
   - SphereEnemyDebugUI

### Method 3: Runtime Creation (Play Mode)
1. Add `SphereEnemyTester` to any GameObject
2. In play mode, press Space to spawn enemies
3. Press C to clear enemies
4. Press D to toggle debug UI

## Features

### AI Behavior Modes
- **Idle**: Stationary, waiting for player
- **Patrol**: Random movement within radius
- **Chase**: Pursues player when detected
- **Attack**: Attacks when in range
- **Retreat**: Temporarily moves away from player

### Debug UI
- Shows AI version (configurable string)
- Shows current AI mode with color coding:
  - Idle: Gray
  - Patrol: Green
  - Chase: Orange
  - Attack: Red
  - Retreat: Cyan

### Configuration Options
- Detection range (default: 10 units)
- Attack range (default: 2 units)
- Movement speed (default: 3 units/sec)
- Patrol radius (default: 5 units)
- Health (default: 50 HP - easy to kill)

## Testing Your AI Functions

The sphere enemy is designed to be an ideal test target:

1. **Low Health**: Only 50 HP for quick elimination
2. **Visual Feedback**: Red color and floating debug UI
3. **Modular Design**: Easy to modify AI behavior
4. **Real-time Debug Info**: See AI state changes instantly

## Requirements

- Player GameObject with "Player" tag
- NavMesh baked in scene for enemy movement
- Main Camera for debug UI positioning

## Usage Examples

```csharp
// Get AI component and change version
SphereEnemyAI ai = enemy.GetComponent<SphereEnemyAI>();
ai.SetAIVersion("MyCustomAI_v2.1");

// Force AI mode change
ai.SetAIMode(SphereEnemyAI.AIMode.Chase);

// Check if enemy is dead
SphereEnemyHealth health = enemy.GetComponent<SphereEnemyHealth>();
if (health.IsDead()) {
    // Enemy is dead
}

// Toggle debug UI
SphereEnemyDebugUI debugUI = enemy.GetComponent<SphereEnemyDebugUI>();
debugUI.ToggleDebugUI();
```

## Next Steps

1. Ensure you have a NavMesh baked in your scene
2. Make sure you have a Player GameObject with "Player" tag
3. Use the SphereEnemyDemo script to create your first test enemy
4. Experiment with different AI versions and configurations
5. Integrate with your existing enemy management systems

The sphere enemy system is now ready for testing your modular AI functions!