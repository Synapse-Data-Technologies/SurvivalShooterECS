using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// Component for storing debug information about player aiming.
/// Used to visualize aim direction and angle in the editor and at runtime.
/// </summary>
public struct AimDebugData : IComponentData
{
    public float2 RawStickInput;      // Raw right stick input before processing
    public float2 AimDirection;       // Processed aim direction vector
    public float AimAngleDegrees;     // Aim angle in degrees (0-360)
    public float AimAngleRadians;     // Aim angle in radians
    public float StickMagnitude;      // Magnitude of stick input (0-1)
    public bool IsUsingGamepad;       // Whether gamepad is active
}
