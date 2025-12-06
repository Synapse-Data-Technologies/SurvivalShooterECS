using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Simple verification script to test InputUtilities functions.
/// This can be attached to a GameObject to run manual tests in the Unity Editor.
/// </summary>
public class InputUtilitiesVerification : MonoBehaviour
{
    [ContextMenu("Test Deadzone Filtering")]
    public void TestDeadzoneFiltering()
    {
        Debug.Log("=== Testing Deadzone Filtering ===");
        
        // Test 1: Input below deadzone should be zeroed
        float2 belowDeadzone = new float2(0.1f, 0.05f);
        float2 result1 = InputUtilities.ApplyDeadzone(belowDeadzone, 0.125f);
        Debug.Log($"Input {belowDeadzone} (magnitude {math.length(belowDeadzone):F3}) -> {result1} (Expected: zero)");
        
        // Test 2: Input above deadzone should pass through
        float2 aboveDeadzone = new float2(0.5f, 0.5f);
        float2 result2 = InputUtilities.ApplyDeadzone(aboveDeadzone, 0.125f);
        Debug.Log($"Input {aboveDeadzone} (magnitude {math.length(aboveDeadzone):F3}) -> {result2} (Expected: non-zero)");
        
        // Test 3: Input at exactly deadzone threshold
        float2 atDeadzone = new float2(0.125f, 0.0f);
        float2 result3 = InputUtilities.ApplyDeadzone(atDeadzone, 0.125f);
        Debug.Log($"Input {atDeadzone} (magnitude {math.length(atDeadzone):F3}) -> {result3} (Expected: non-zero)");
    }
    
    [ContextMenu("Test Input Normalization")]
    public void TestInputNormalization()
    {
        Debug.Log("=== Testing Input Normalization ===");
        
        // Test 1: Input at deadzone edge should map to 0
        float2 atDeadzone = new float2(0.125f, 0.0f);
        float2 result1 = InputUtilities.NormalizeInput(atDeadzone, 0.125f, 0.925f);
        Debug.Log($"Input {atDeadzone} -> {result1} (magnitude {math.length(result1):F3}, Expected: ~0)");
        
        // Test 2: Input at max range should map to 1
        float2 atMax = new float2(0.925f, 0.0f);
        float2 result2 = InputUtilities.NormalizeInput(atMax, 0.125f, 0.925f);
        Debug.Log($"Input {atMax} -> {result2} (magnitude {math.length(result2):F3}, Expected: ~1.0)");
        
        // Test 3: Input at midpoint should map to ~0.5
        float deadzone = 0.125f;
        float maxRange = 0.925f;
        float midpoint = (deadzone + maxRange) / 2f;
        float2 atMid = new float2(midpoint, 0.0f);
        float2 result3 = InputUtilities.NormalizeInput(atMid, deadzone, maxRange);
        Debug.Log($"Input {atMid} -> {result3} (magnitude {math.length(result3):F3}, Expected: ~0.5)");
        
        // Test 4: Input beyond max range should be clamped
        float2 beyondMax = new float2(1.5f, 0.0f);
        float2 result4 = InputUtilities.NormalizeInput(beyondMax, 0.125f, 0.925f);
        Debug.Log($"Input {beyondMax} -> {result4} (magnitude {math.length(result4):F3}, Expected: ~1.0)");
    }
    
    [ContextMenu("Test Stick To Direction")]
    public void TestStickToDirection()
    {
        Debug.Log("=== Testing Stick To Direction ===");
        
        // Test 1: Zero input should return zero
        float2 zero = float2.zero;
        float2 result1 = InputUtilities.StickToDirection(zero);
        Debug.Log($"Input {zero} -> {result1} (Expected: zero)");
        
        // Test 2: Right direction
        float2 right = new float2(0.8f, 0.0f);
        float2 result2 = InputUtilities.StickToDirection(right);
        Debug.Log($"Input {right} -> {result2} (magnitude {math.length(result2):F3}, Expected: (1, 0))");
        
        // Test 3: Diagonal direction
        float2 diagonal = new float2(0.5f, 0.5f);
        float2 result3 = InputUtilities.StickToDirection(diagonal);
        Debug.Log($"Input {diagonal} -> {result3} (magnitude {math.length(result3):F3}, Expected: normalized diagonal)");
        
        // Test 4: Angle calculation
        float angle = InputUtilities.StickToAngle(diagonal);
        Debug.Log($"Input {diagonal} -> angle {angle} radians ({angle * Mathf.Rad2Deg} degrees, Expected: ~45 degrees)");
    }
    
    [ContextMenu("Run All Tests")]
    public void RunAllTests()
    {
        TestDeadzoneFiltering();
        Debug.Log("");
        TestInputNormalization();
        Debug.Log("");
        TestStickToDirection();
    }
}
