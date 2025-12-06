using Unity.Mathematics;

/// <summary>
/// Utility functions for processing gamepad input with deadzone filtering,
/// normalization, and coordinate transformations.
/// </summary>
public static class InputUtilities
{
    /// <summary>
    /// Validates and sanitizes input values to handle NaN and infinite values.
    /// Invalid values are replaced with zero to prevent propagation of errors.
    /// </summary>
    /// <param name="input">The input vector to validate</param>
    /// <returns>A valid input vector with NaN and infinite values replaced by zero</returns>
    public static float2 SanitizeInput(float2 input)
    {
        // Check for NaN or infinite values in either component
        if (!math.isfinite(input.x) || !math.isfinite(input.y))
        {
            return float2.zero;
        }
        
        return input;
    }
    
    /// <summary>
    /// Validates and sanitizes a scalar input value to handle NaN and infinite values.
    /// Invalid values are replaced with zero to prevent propagation of errors.
    /// </summary>
    /// <param name="input">The input value to validate</param>
    /// <returns>A valid input value with NaN and infinite values replaced by zero</returns>
    public static float SanitizeInput(float input)
    {
        // Check for NaN or infinite values
        if (!math.isfinite(input))
        {
            return 0f;
        }
        
        return input;
    }
    
    /// <summary>
    /// Applies deadzone filtering to an analog input value.
    /// Values below the threshold are zeroed to prevent stick drift.
    /// </summary>
    /// <param name="input">The raw input vector from an analog stick</param>
    /// <param name="deadzone">The minimum threshold below which input is ignored (default: 0.125)</param>
    /// <returns>The filtered input vector, or zero if below threshold</returns>
    public static float2 ApplyDeadzone(float2 input, float deadzone = 0.125f)
    {
        // Sanitize input first to handle invalid values
        input = SanitizeInput(input);
        
        float magnitude = math.length(input);
        
        // If magnitude is below deadzone, return zero
        if (magnitude < deadzone)
        {
            return float2.zero;
        }
        
        // Return the original input (normalization happens separately)
        return input;
    }
    
    /// <summary>
    /// Normalizes analog stick input from [deadzone, max] range to [0, 1] range.
    /// This ensures smooth transitions from the deadzone edge to maximum displacement.
    /// </summary>
    /// <param name="input">The input vector that has passed deadzone filtering</param>
    /// <param name="deadzone">The inner deadzone threshold (default: 0.125)</param>
    /// <param name="maxRange">The outer deadzone threshold (default: 0.925)</param>
    /// <returns>The normalized input vector with magnitude mapped to [0, 1]</returns>
    public static float2 NormalizeInput(float2 input, float deadzone = 0.125f, float maxRange = 0.925f)
    {
        // Sanitize input first to handle invalid values
        input = SanitizeInput(input);
        
        float magnitude = math.length(input);
        
        // If input is zero or below deadzone, return zero
        if (magnitude < deadzone)
        {
            return float2.zero;
        }
        
        // Clamp magnitude to maxRange
        magnitude = math.min(magnitude, maxRange);
        
        // Normalize the direction
        float2 direction = math.normalize(input);
        
        // Map [deadzone, maxRange] to [0, 1]
        float normalizedMagnitude = (magnitude - deadzone) / (maxRange - deadzone);
        
        // Return direction scaled by normalized magnitude
        return direction * normalizedMagnitude;
    }
    
    /// <summary>
    /// Converts an analog stick input vector to a direction vector suitable for aiming.
    /// The input angle is preserved, and the output is a unit direction vector.
    /// </summary>
    /// <param name="stickInput">The analog stick input vector</param>
    /// <returns>A unit direction vector, or zero if input magnitude is zero</returns>
    public static float2 StickToDirection(float2 stickInput)
    {
        // Sanitize input first to handle invalid values
        stickInput = SanitizeInput(stickInput);
        
        float magnitude = math.length(stickInput);
        
        // If stick is not being moved, return zero
        if (magnitude < 0.001f)
        {
            return float2.zero;
        }
        
        // Return normalized direction
        return math.normalize(stickInput);
    }
    
    /// <summary>
    /// Converts an analog stick input to an angle in radians.
    /// The angle is measured counter-clockwise from the positive X axis.
    /// </summary>
    /// <param name="stickInput">The analog stick input vector</param>
    /// <returns>The angle in radians, or 0 if input magnitude is zero</returns>
    public static float StickToAngle(float2 stickInput)
    {
        // Sanitize input first to handle invalid values
        stickInput = SanitizeInput(stickInput);
        
        float magnitude = math.length(stickInput);
        
        // If stick is not being moved, return zero angle
        if (magnitude < 0.001f)
        {
            return 0f;
        }
        
        // Calculate angle using atan2 (returns angle in radians)
        return math.atan2(stickInput.y, stickInput.x);
    }
}
