using Unity.Entities;
using UnityEngine;

/// <summary>
/// Debug script to diagnose player control issues.
/// Attach to any GameObject in the scene.
/// </summary>
public class PlayerDebug : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool showDebugInfo = true;
    public KeyCode debugKey = KeyCode.F3;

    private void Update()
    {
        if (Input.GetKeyDown(debugKey))
        {
            DebugPlayerState();
        }
    }

    private void OnGUI()
    {
        if (!showDebugInfo)
            return;

        GUILayout.BeginArea(new Rect(10, Screen.height - 200, 400, 190));
        GUILayout.BeginVertical("box");

        GUILayout.Label("<b>Player Debug (Press F3 for details)</b>", new GUIStyle(GUI.skin.label) { richText = true });

        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null)
        {
            GUILayout.Label("<color=red>ERROR: No default world!</color>", new GUIStyle(GUI.skin.label) { richText = true });
            GUILayout.EndVertical();
            GUILayout.EndArea();
            return;
        }

        // Check for player entities
        var query = world.EntityManager.CreateEntityQuery(typeof(PlayerData));
        int playerCount = query.CalculateEntityCount();
        
        if (playerCount == 0)
        {
            GUILayout.Label("<color=red>ERROR: No player entities found!</color>", new GUIStyle(GUI.skin.label) { richText = true });
        }
        else
        {
            GUILayout.Label($"<color=green>Player entities: {playerCount}</color>", new GUIStyle(GUI.skin.label) { richText = true });

            // Get player input data
            var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
            if (entities.Length > 0)
            {
                var entity = entities[0];
                if (world.EntityManager.HasComponent<PlayerInputData>(entity))
                {
                    var inputData = world.EntityManager.GetComponentData<PlayerInputData>(entity);
                    GUILayout.Label($"Move: ({inputData.Move.x:F2}, {inputData.Move.y:F2})");
                    GUILayout.Label($"Look: ({inputData.Look.x:F2}, {inputData.Look.y:F2})");
                    GUILayout.Label($"Shoot: {inputData.Shoot:F2}");
                    GUILayout.Label($"Using Gamepad: {inputData.IsUsingGamepad}");
                }
                else
                {
                    GUILayout.Label("<color=yellow>No PlayerInputData component</color>", new GUIStyle(GUI.skin.label) { richText = true });
                }
            }
            entities.Dispose();
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void DebugPlayerState()
    {
        Debug.Log("=== PLAYER DEBUG INFO ===");

        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null)
        {
            Debug.LogError("No default world found!");
            return;
        }

        // Check player entities
        var query = world.EntityManager.CreateEntityQuery(typeof(PlayerData));
        int playerCount = query.CalculateEntityCount();
        Debug.Log($"Player entities found: {playerCount}");

        if (playerCount == 0)
        {
            Debug.LogError("No player entities! Check if PlayerObject has PlayerObjectBaker and is in the scene.");
            
            // Check for PlayerObject in scene
            var playerObjects = FindObjectsOfType<PlayerObject>();
            Debug.Log($"PlayerObject MonoBehaviours in scene: {playerObjects.Length}");
            foreach (var po in playerObjects)
            {
                Debug.Log($"  - PlayerObject: {po.gameObject.name}, Entity: {po.Entity}");
            }
            return;
        }

        // Check player components
        var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
        foreach (var entity in entities)
        {
            Debug.Log($"Player Entity: {entity}");
            Debug.Log($"  Has PlayerData: {world.EntityManager.HasComponent<PlayerData>(entity)}");
            Debug.Log($"  Has PlayerInputData: {world.EntityManager.HasComponent<PlayerInputData>(entity)}");
            Debug.Log($"  Has HealthData: {world.EntityManager.HasComponent<HealthData>(entity)}");

            if (world.EntityManager.HasComponent<PlayerInputData>(entity))
            {
                var inputData = world.EntityManager.GetComponentData<PlayerInputData>(entity);
                Debug.Log($"  Input - Move: {inputData.Move}, Look: {inputData.Look}, Shoot: {inputData.Shoot}");
            }
        }
        entities.Dispose();

        // Check systems
        Debug.Log("=== SYSTEM STATUS ===");
        var runFixed = FindObjectOfType<RunFixedUpdateSystems>();
        if (runFixed == null)
        {
            Debug.LogError("RunFixedUpdateSystems not found in scene!");
        }
        else
        {
            Debug.Log("RunFixedUpdateSystems found");
        }

        Debug.Log("=== END DEBUG INFO ===");
    }
}
