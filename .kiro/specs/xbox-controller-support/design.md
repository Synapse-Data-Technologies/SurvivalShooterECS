# Design Document: Xbox Controller Support

## Overview

This design adds comprehensive Xbox controller support to the Survival Shooter ECS game by enhancing the existing `PlayerInputSystem`. The implementation leverages Unity's Input System to provide dual-stick shooter controls with the left stick for movement, right stick for aiming, and right trigger for shooting. The design maintains backward compatibility with existing keyboard/mouse controls while enabling seamless switching between input methods.

## Architecture

The implementation follows Unity's ECS architecture and builds upon the existing input system:

### Current Architecture
- `PlayerInputSystem`: Captures input from keyboard/mouse and updates `PlayerInputData` components
- `PlayerInputData`: Component storing movement, look, and shoot input values
- Consumer systems: `PlayerMovementSystem`, `PlayerTurningSystem`, `PlayerShootingSystem`, `PlayerAnimationSystem`

### Enhanced Architecture
The design enhances the `PlayerInputSystem` to:
1. Support multiple input schemes (keyboard/mouse and gamepad) simultaneously
2. Implement proper deadzone handling for analog sticks
3. Provide different aiming modes (screen-space for mouse, direction-based for right stick)
4. Normalize analog inputs for consistent behavior

## Components and Interfaces

### PlayerInputData (Enhanced)
```csharp
public struct PlayerInputData : IComponentData
{
    public float2 Move;        // Movement input (-1 to 1 on each axis)
    public float2 Look;        // Look input (screen position for mouse, direction for stick)
    public float Shoot;        // Shoot input (0 or 1)
    public bool IsUsingGamepad; // Flag to indicate active input method
}
```

### Input Actions
The system uses Unity's Input System with the following actions:
- **Move Action**: Composite binding supporting WASD keys and left stick
- **Look Action**: Separate bindings for mouse position and right stick
- **Shoot Action**: Bindings for left mouse button and right trigger

### Input Processing Pipeline
1. **Capture**: Input System captures raw input from devices
2. **Deadzone**: Apply deadzone filtering to analog inputs
3. **Normalization**: Normalize stick inputs from deadzone to maximum
4. **Mode Detection**: Determine active input method (mouse vs gamepad)
5. **Transform**: Convert inputs to appropriate coordinate space
6. **Update**: Write processed values to `PlayerInputData` components

## Data Models

### Deadzone Configuration
```csharp
public struct DeadzoneSettings
{
    public float InnerDeadzone;  // Minimum threshold (default: 0.125)
    public float OuterDeadzone;  // Maximum threshold (default: 0.925)
}
```

### Input Mode
```csharp
public enum InputMode
{
    KeyboardMouse,
    Gamepad
}
```

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system-essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: Movement direction and magnitude
*For any* left stick input vector, the resulting movement direction should match the input direction, and the movement magnitude should be proportional to the stick displacement (after deadzone normalization).
**Validates: Requirements 1.1**

### Property 2: Deadzone filtering for all analog inputs
*For any* analog stick input (movement or aiming) with magnitude below the deadzone threshold, the processed input should be zero.
**Validates: Requirements 1.2, 2.2, 4.1**

### Property 3: Input release stops action
*For any* non-zero input (movement or shooting), when the input transitions to zero, the corresponding action should immediately stop.
**Validates: Requirements 1.4, 3.3**

### Property 4: Aiming direction matches stick angle
*For any* right stick input above the deadzone, the character's facing direction should align with the angle of the stick input vector.
**Validates: Requirements 2.1**

### Property 5: Continuous aiming updates
*For any* sequence of right stick inputs held above the deadzone, the character's facing direction should continuously update to match each input angle.
**Validates: Requirements 2.3**

### Property 6: Independent input processing
*For any* combination of movement and aiming inputs, each input should produce its expected output regardless of the other input's value.
**Validates: Requirements 2.4**

### Property 7: Trigger threshold activation
*For any* right trigger input value above the activation threshold, the system should register a shoot input; for any value below the threshold, no shoot input should be registered.
**Validates: Requirements 3.1, 3.4**

### Property 8: Continuous shooting timing
*For any* held trigger input above threshold, shoot events should occur at intervals matching the weapon's fire rate.
**Validates: Requirements 3.2**

### Property 9: Deadzone normalization
*For any* analog stick input above the deadzone, the normalized output should map the range [deadzone, maximum] to [0, 1] linearly.
**Validates: Requirements 4.2**

### Property 10: Mouse aiming calculation
*For any* mouse screen position, the aiming direction should be calculated as the vector from the character's position to the world position corresponding to the mouse screen position.
**Validates: Requirements 5.1**

## Error Handling

### Input Device Disconnection
- The system should gracefully handle controller disconnection by falling back to keyboard/mouse input
- No exceptions should be thrown when a controller is disconnected mid-game

### Invalid Input Values
- NaN or infinite values from input devices should be clamped to valid ranges
- Deadzone filtering should handle edge cases at exactly the threshold value

### Missing Input Actions
- If Input Actions are not properly configured, the system should log warnings and use default bindings
- The game should remain playable with keyboard/mouse even if gamepad bindings fail

## Testing Strategy

### Unit Testing
The implementation will include unit tests for:
- Deadzone calculation functions (testing boundary values)
- Input normalization functions (testing the mapping from [deadzone, max] to [0, 1])
- Input mode detection logic (testing device switching)
- Angle calculation for stick-to-direction conversion

### Property-Based Testing
The implementation will use property-based testing to verify correctness properties across a wide range of inputs. We will use the **Unity.Testing.PropertyTests** package (or implement a simple property testing framework if not available).

**Configuration:**
- Each property test should run a minimum of 100 iterations
- Tests should generate random inputs covering the full range of valid values
- Each property test must be tagged with a comment referencing its corresponding correctness property

**Test Tagging Format:**
```csharp
// Feature: xbox-controller-support, Property 1: Movement direction and magnitude
[Test]
public void PropertyTest_MovementDirectionAndMagnitude() { ... }
```

**Property Test Coverage:**
- Property 1: Generate random stick inputs, verify movement direction and magnitude
- Property 2: Generate random inputs below deadzone, verify they're zeroed
- Property 3: Generate random non-zero inputs, transition to zero, verify actions stop
- Property 4: Generate random stick angles, verify character rotation matches
- Property 5: Generate sequences of stick inputs, verify continuous rotation updates
- Property 6: Generate random combinations of movement and aiming, verify independence
- Property 7: Generate random trigger values, verify threshold behavior
- Property 8: Simulate held trigger over time, verify shot timing
- Property 9: Generate random inputs above deadzone, verify normalization formula
- Property 10: Generate random mouse positions, verify aiming direction calculation

### Integration Testing
- Test switching between keyboard/mouse and gamepad during gameplay
- Test simultaneous input from multiple devices
- Test controller connection/disconnection scenarios

## Implementation Notes

### Unity Input System Integration
- Use Input Action Assets for binding configuration
- Leverage composite bindings for movement (WASD + left stick)
- Use separate action maps for different control schemes if needed

### Performance Considerations
- Input processing runs in `OnUpdate()` and should be lightweight
- Avoid allocations in the input processing loop
- Use `ScheduleParallel()` for updating multiple player entities

### Compatibility
- Maintain existing keyboard/mouse functionality
- Ensure the system works with Xbox, PlayStation, and generic controllers
- Support both wired and wireless controllers

### Configuration
- Deadzone values should be configurable via Unity's Input System settings
- Trigger activation threshold should be adjustable (default: 0.5)
- Fire rate remains controlled by weapon configuration

## Future Enhancements

### Aim Assist (Optional)
- Implement subtle aim assist for controller users to help target enemies
- Use a cone-based system to slightly pull aim toward nearby enemies
- Make aim assist strength configurable

### Vibration Feedback
- Add controller vibration when taking damage
- Add subtle vibration when shooting
- Implement vibration patterns for different events

### Button Remapping
- Allow players to customize button bindings
- Provide preset configurations for different controller types
- Save custom bindings to player preferences
