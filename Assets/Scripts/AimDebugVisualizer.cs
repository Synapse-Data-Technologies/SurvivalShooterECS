using Unity.Entities;
using UnityEngine;

/// <summary>
/// MonoBehaviour bridge for visualizing aim debug information.
/// Connects the ECS AimDebugSystem to Unity's Gizmo and GUI rendering.
/// Attach this to any GameObject in the scene to enable debug visualization.
/// </summary>
public class AimDebugVisualizer : MonoBehaviour
{
    [Header("Debug Visualization Settings")]
    [Tooltip("Show aim direction gizmos in Scene view")]
    public bool showGizmos = true;
    
    [Tooltip("Show aim debug UI overlay in Game view")]
    public bool showUI = true;
    
    [Header("Keyboard Shortcuts")]
    [Tooltip("Key to toggle gizmo visualization")]
    public KeyCode toggleGizmosKey = KeyCode.F1;
    
    [Tooltip("Key to toggle UI visualization")]
    public KeyCode toggleUIKey = KeyCode.F2;

    private AimDebugSystem debugSystem;
    private EntityManager entityManager;

    private void Start()
    {
        // Get the default world and find the debug system
        var world = World.DefaultGameObjectInjectionWorld;
        if (world != null)
        {
            entityManager = world.EntityManager;
            
            // Get or create the debug system
            debugSystem = world.GetExistingSystemManaged<AimDebugSystem>();
            if (debugSystem == null)
            {
                debugSystem = world.CreateSystemManaged<AimDebugSystem>();
                Debug.Log("[AimDebugVisualizer] Created AimDebugSystem");
            }
            
            // Set initial state
            debugSystem.SetGizmosEnabled(showGizmos);
            debugSystem.SetUIEnabled(showUI);
            
            // Ensure player entities have the debug component
            AddDebugComponentToPlayers();
        }
        else
        {
            Debug.LogWarning("[AimDebugVisualizer] Could not find default world");
        }
    }

    private void Update()
    {
        // Handle keyboard shortcuts
        if (Input.GetKeyDown(toggleGizmosKey))
        {
            showGizmos = !showGizmos;
            if (debugSystem != null)
            {
                debugSystem.SetGizmosEnabled(showGizmos);
                Debug.Log($"[AimDebugVisualizer] Gizmos: {(showGizmos ? "ON" : "OFF")}");
            }
        }

        if (Input.GetKeyDown(toggleUIKey))
        {
            showUI = !showUI;
            if (debugSystem != null)
            {
                debugSystem.SetUIEnabled(showUI);
                Debug.Log($"[AimDebugVisualizer] UI: {(showUI ? "ON" : "OFF")}");
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (debugSystem != null && entityManager != default && showGizmos)
        {
            debugSystem.DrawDebugGizmos(entityManager);
        }
    }

    private void OnGUI()
    {
        if (debugSystem != null && entityManager != default && showUI)
        {
            debugSystem.DrawDebugUI(entityManager);
        }
    }

    /// <summary>
    /// Adds the AimDebugData component to all player entities that have PlayerInputData.
    /// </summary>
    private void AddDebugComponentToPlayers()
    {
        if (entityManager == default)
            return;

        var query = entityManager.CreateEntityQuery(typeof(PlayerInputData));
        var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);

        foreach (var entity in entities)
        {
            if (!entityManager.HasComponent<AimDebugData>(entity))
            {
                entityManager.AddComponentData(entity, new AimDebugData());
                Debug.Log($"[AimDebugVisualizer] Added AimDebugData to player entity");
            }
        }

        entities.Dispose();
    }

    private void OnDestroy()
    {
        // Clean up debug components when visualizer is destroyed
        if (entityManager != default)
        {
            var query = entityManager.CreateEntityQuery(typeof(AimDebugData));
            var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);

            foreach (var entity in entities)
            {
                if (entityManager.Exists(entity))
                {
                    entityManager.RemoveComponent<AimDebugData>(entity);
                }
            }

            entities.Dispose();
        }
    }
}
