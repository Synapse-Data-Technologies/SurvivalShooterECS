using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Modern ISystem implementation for camera following behavior.
/// Smoothly follows the player while maintaining a fixed offset.
/// Runs in SimulationSystemGroup.
/// </summary>
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct CameraFollowSystem : ISystem
{
    private bool firstFrame;
    private float3 offset;

    public void OnCreate(ref SystemState state)
    {
        firstFrame = true;
        offset = float3.zero;
        
        // Only run when a player entity exists
        state.RequireForUpdate<PlayerData>();
        
        Debug.Log("[CameraFollowSystem] System created and initialized");
    }

    // Note: OnUpdate cannot be Burst compiled due to Camera.main access
    public void OnUpdate(ref SystemState state)
    {
        // Early exit if Camera.main is null
        var mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("[CameraFollowSystem] Camera.main is null");
            return;
        }

        // Query for player entities with PlayerData component
        var query = SystemAPI.QueryBuilder()
            .WithAll<PlayerData>()
            .Build();

        var entityArray = query.ToEntityArray(state.WorldUpdateAllocator);
        
        if (entityArray.Length == 0)
        {
            Debug.LogWarning("[CameraFollowSystem] No player entities found with PlayerData");
            return;
        }

        // Process each player entity (should only be one)
        foreach (var entity in entityArray)
        {
            // Get the Transform managed component
            if (!state.EntityManager.HasComponent<Transform>(entity))
            {
                Debug.LogWarning($"[CameraFollowSystem] Player entity {entity} does not have Transform component");
                continue;
            }

            var transform = state.EntityManager.GetComponentObject<Transform>(entity);
            
            // Null safety check for Transform
            if (transform == null)
            {
                Debug.LogWarning("[CameraFollowSystem] Player Transform is null, skipping camera update");
                continue;
            }

            var playerPos = transform.position;

            // Calculate offset on first frame
            if (firstFrame)
            {
                offset = mainCamera.transform.position - playerPos;
                firstFrame = false;
                Debug.Log($"[CameraFollowSystem] Calculated initial offset: {offset}");
            }

            // Get smoothing value from settings
            var settings = SurvivalShooterBootstrap.Settings;
            var smoothing = settings.CamSmoothing;
            var dt = SystemAPI.Time.DeltaTime;
            
            // Calculate target camera position maintaining offset
            var targetCamPos = playerPos + (Vector3)offset;
            
            // Smoothly interpolate camera position
            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position, 
                targetCamPos, 
                smoothing * dt);

            // Only one player exists, so break after processing
            break;
        }
    }
}
