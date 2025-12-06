using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// System for visualizing aim debug information using Gizmos and on-screen display.
/// Shows the current aim angle, stick input, and direction vector.
/// </summary>
[DisableAutoCreation]
public partial class AimDebugSystem : SystemBase
{
    private bool showDebugGizmos = true;
    private bool showDebugUI = true;

    protected override void OnUpdate()
    {
        // Update debug data from player input
        Entities
            .WithAll<PlayerInputData>()
            .ForEach((Entity entity, ref AimDebugData debugData, in PlayerInputData inputData, in LocalTransform transform) =>
            {
                debugData.IsUsingGamepad = inputData.IsUsingGamepad;
                
                if (inputData.IsUsingGamepad)
                {
                    // Store raw stick input
                    debugData.RawStickInput = inputData.Look;
                    
                    // Calculate aim direction
                    debugData.AimDirection = math.length(inputData.Look) > 0.001f 
                        ? math.normalize(inputData.Look) 
                        : float2.zero;
                    
                    // Calculate angles
                    if (math.length(inputData.Look) > 0.001f)
                    {
                        debugData.AimAngleRadians = math.atan2(inputData.Look.y, inputData.Look.x);
                        debugData.AimAngleDegrees = math.degrees(debugData.AimAngleRadians);
                        
                        // Normalize to 0-360 range
                        if (debugData.AimAngleDegrees < 0)
                            debugData.AimAngleDegrees += 360f;
                    }
                    else
                    {
                        debugData.AimAngleRadians = 0f;
                        debugData.AimAngleDegrees = 0f;
                    }
                    
                    // Calculate stick magnitude
                    debugData.StickMagnitude = math.length(inputData.Look);
                }
            }).Run();
    }

    /// <summary>
    /// Draws debug gizmos in the Scene view showing aim direction and angle.
    /// Call this from a MonoBehaviour's OnDrawGizmos method.
    /// </summary>
    public void DrawDebugGizmos(EntityManager entityManager)
    {
        if (!showDebugGizmos)
            return;

        var query = entityManager.CreateEntityQuery(typeof(AimDebugData), typeof(LocalTransform));
        var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);

        foreach (var entity in entities)
        {
            var debugData = entityManager.GetComponentData<AimDebugData>(entity);
            var transform = entityManager.GetComponentData<LocalTransform>(entity);

            if (!debugData.IsUsingGamepad || debugData.StickMagnitude < 0.001f)
                continue;

            Vector3 position = transform.Position;
            
            // Draw aim direction line
            Vector3 aimDir3D = new Vector3(debugData.AimDirection.x, 0, debugData.AimDirection.y);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(position, position + aimDir3D * 3f);
            
            // Draw aim direction arrow head
            Vector3 arrowEnd = position + aimDir3D * 3f;
            Vector3 perpendicular = Vector3.Cross(aimDir3D, Vector3.up).normalized * 0.3f;
            Gizmos.DrawLine(arrowEnd, arrowEnd - aimDir3D * 0.5f + perpendicular);
            Gizmos.DrawLine(arrowEnd, arrowEnd - aimDir3D * 0.5f - perpendicular);
            
            // Draw stick input circle
            Gizmos.color = Color.yellow;
            DrawCircle(position + Vector3.up * 0.1f, 2f, 32);
            
            // Draw stick position on circle
            Vector3 stickPos = position + Vector3.up * 0.1f + new Vector3(debugData.RawStickInput.x, 0, debugData.RawStickInput.y) * 2f;
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(stickPos, 0.15f);
        }

        entities.Dispose();
    }

    /// <summary>
    /// Renders debug UI showing aim angle and stick information.
    /// Call this from a MonoBehaviour's OnGUI method.
    /// </summary>
    public void DrawDebugUI(EntityManager entityManager)
    {
        if (!showDebugUI)
            return;

        var query = entityManager.CreateEntityQuery(typeof(AimDebugData));
        var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);

        if (entities.Length == 0)
        {
            entities.Dispose();
            return;
        }

        var debugData = entityManager.GetComponentData<AimDebugData>(entities[0]);
        entities.Dispose();

        // Create debug panel
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("<b>Aim Debug Info</b>", new GUIStyle(GUI.skin.label) { richText = true, fontSize = 14 });
        GUILayout.Space(5);
        
        GUILayout.Label($"Input Mode: {(debugData.IsUsingGamepad ? "Gamepad" : "Mouse")}");
        
        if (debugData.IsUsingGamepad)
        {
            GUILayout.Label($"Stick Input: ({debugData.RawStickInput.x:F3}, {debugData.RawStickInput.y:F3})");
            GUILayout.Label($"Stick Magnitude: {debugData.StickMagnitude:F3}");
            GUILayout.Label($"Aim Direction: ({debugData.AimDirection.x:F3}, {debugData.AimDirection.y:F3})");
            GUILayout.Label($"<b>Aim Angle: {debugData.AimAngleDegrees:F1}°</b>", new GUIStyle(GUI.skin.label) { richText = true, fontSize = 12 });
            GUILayout.Label($"Aim Angle (rad): {debugData.AimAngleRadians:F3}");
            
            // Visual angle indicator
            GUILayout.Space(10);
            DrawAngleIndicator(debugData.AimAngleDegrees, debugData.StickMagnitude);
        }
        else
        {
            GUILayout.Label("Switch to gamepad to see aim debug info");
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    /// <summary>
    /// Draws a visual angle indicator showing the current aim direction.
    /// </summary>
    private void DrawAngleIndicator(float angleDegrees, float magnitude)
    {
        if (magnitude < 0.001f)
            return;

        Rect indicatorRect = GUILayoutUtility.GetRect(80, 80);
        Vector2 center = indicatorRect.center;
        float radius = 35f;

        // Draw circle background
        DrawGUICircle(center, radius, Color.gray);

        // Draw angle line
        float angleRad = math.radians(angleDegrees);
        Vector2 direction = new Vector2(math.cos(angleRad), math.sin(angleRad));
        Vector2 endPoint = center + direction * radius * magnitude;

        DrawGUILine(center, endPoint, Color.red, 2f);

        // Draw angle text at center
        GUI.Label(new Rect(center.x - 30, center.y - 10, 60, 20), 
            $"{angleDegrees:F0}°", 
            new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold });
    }

    /// <summary>
    /// Helper method to draw a circle in the Scene view.
    /// </summary>
    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = math.radians(angleStep * i);
            Vector3 newPoint = center + new Vector3(math.cos(angle) * radius, 0, math.sin(angle) * radius);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }

    /// <summary>
    /// Helper method to draw a circle in GUI.
    /// </summary>
    private void DrawGUICircle(Vector2 center, float radius, Color color)
    {
        int segments = 32;
        float angleStep = 360f / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float angle1 = math.radians(angleStep * i);
            float angle2 = math.radians(angleStep * (i + 1));
            
            Vector2 point1 = center + new Vector2(math.cos(angle1), math.sin(angle1)) * radius;
            Vector2 point2 = center + new Vector2(math.cos(angle2), math.sin(angle2)) * radius;
            
            DrawGUILine(point1, point2, color, 1f);
        }
    }

    /// <summary>
    /// Helper method to draw a line in GUI.
    /// </summary>
    private void DrawGUILine(Vector2 start, Vector2 end, Color color, float width)
    {
        Vector2 direction = end - start;
        float length = direction.magnitude;
        float angle = math.degrees(math.atan2(direction.y, direction.x));

        GUIUtility.RotateAroundPivot(angle, start);
        
        Color oldColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(new Rect(start.x, start.y - width / 2, length, width), Texture2D.whiteTexture);
        GUI.color = oldColor;
        
        GUIUtility.RotateAroundPivot(-angle, start);
    }

    public void ToggleGizmos() => showDebugGizmos = !showDebugGizmos;
    public void ToggleUI() => showDebugUI = !showDebugUI;
    public void SetGizmosEnabled(bool enabled) => showDebugGizmos = enabled;
    public void SetUIEnabled(bool enabled) => showDebugUI = enabled;
}
