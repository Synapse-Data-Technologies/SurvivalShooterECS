# Aim Debug Visualization Setup Guide

## Overview

The aim debug system provides real-time visualization of gamepad aiming input, showing:
- **Aim angle in degrees** (0-360°)
- **Stick input position** (X, Y coordinates)
- **Stick magnitude** (how far the stick is pushed)
- **Aim direction vector**
- **Visual indicators** in both Scene and Game views

## Files Created

1. **AimDebugData.cs** - ECS component storing debug information
2. **AimDebugSystem.cs** - ECS system that processes and visualizes debug data
3. **AimDebugVisualizer.cs** - MonoBehaviour bridge for Unity rendering

## Setup Instructions

### Step 1: Add the Visualizer to Your Scene

1. Open your game scene in Unity
2. Create a new empty GameObject (Right-click in Hierarchy → Create Empty)
3. Rename it to "AimDebugVisualizer"
4. Add the `AimDebugVisualizer` component to it:
   - Select the GameObject
   - Click "Add Component" in the Inspector
   - Search for "AimDebugVisualizer"
   - Click to add it

### Step 2: Configure Settings

In the Inspector for the AimDebugVisualizer component:

- **Show Gizmos**: Enable/disable Scene view visualization (default: ON)
- **Show UI**: Enable/disable Game view UI overlay (default: ON)
- **Toggle Gizmos Key**: Keyboard shortcut to toggle gizmos (default: F1)
- **Toggle UI Key**: Keyboard shortcut to toggle UI (default: F2)

### Step 3: Enable the Debug System

The debug system needs to be added to your ECS world. Add this to your game initialization code:

```csharp
// In your game bootstrap or initialization
var world = World.DefaultGameObjectInjectionWorld;
var debugSystem = world.CreateSystemManaged<AimDebugSystem>();
```

Or the AimDebugVisualizer will automatically create it when it starts.

## Usage

### In Play Mode

1. **Start the game** in Unity Editor
2. **Connect an Xbox controller**
3. **Move the right stick** to aim

You'll see:

#### Game View (On-Screen UI)
- Top-left corner shows a debug panel with:
  - Current input mode (Gamepad/Mouse)
  - Raw stick input coordinates
  - Stick magnitude (0-1)
  - Aim direction vector
  - **Aim angle in degrees** (highlighted)
  - Visual circular indicator showing aim direction

#### Scene View (Gizmos)
- **Red arrow** from player showing aim direction
- **Yellow circle** representing the stick's range
- **Green sphere** showing current stick position on the circle

### Keyboard Shortcuts

- **F1**: Toggle Scene view gizmos on/off
- **F2**: Toggle Game view UI on/off

## Understanding the Display

### Angle Measurement
- **0°**: Aiming right (positive X axis)
- **90°**: Aiming up (positive Y axis)
- **180°**: Aiming left (negative X axis)
- **270°**: Aiming down (negative Y axis)

The angle is measured counter-clockwise from the right direction (standard mathematical convention).

### Stick Magnitude
- **0.0**: Stick centered (no input)
- **0.125**: Deadzone threshold (input starts registering)
- **1.0**: Stick at maximum displacement

### Color Coding (Gizmos)
- **Red**: Aim direction (where you're aiming)
- **Yellow**: Stick input range circle
- **Green**: Current stick position

## Troubleshooting

### Debug UI Not Showing
1. Make sure `showUI` is enabled in the Inspector
2. Check that you're using a gamepad (debug only shows for gamepad input)
3. Verify the AimDebugVisualizer GameObject is active in the scene

### Gizmos Not Showing
1. Make sure `showGizmos` is enabled in the Inspector
2. Check that Gizmos are enabled in the Scene view (button in top-right of Scene view)
3. Verify you're in Play mode

### No Aim Data
1. Ensure your player entity has the `PlayerInputData` component
2. Check that the PlayerInputSystem is running
3. Verify the gamepad is connected and recognized by Unity

## Performance Notes

The debug system is designed for development only:
- Disable it in production builds
- The UI rendering uses immediate mode GUI (OnGUI) which is not optimized
- Gizmo rendering only happens in the editor

## Extending the Debug System

You can modify `AimDebugSystem.cs` to add more debug information:

```csharp
// Example: Add deadzone visualization
debugData.IsInDeadzone = debugData.StickMagnitude < 0.125f;

// Example: Add aim speed tracking
debugData.AimSpeed = math.length(currentAim - previousAim) / deltaTime;
```

Then update the UI rendering in `DrawDebugUI()` to display the new data.
