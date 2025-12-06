using Unity.Entities;
using UnityEngine;

public class RunFixedUpdateSystems : MonoBehaviour
{
    private PlayerInputSystem playerInputSystem;
    private PlayerMovementSystem playerMovementSystem;
    private PlayerTurningSystem playerTurningSystem;
    private PlayerAnimationSystem playerAnimationSystem;
    // Note: CameraFollowSystem is now an ISystem and runs automatically in the default world

    private void Start()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null)
        {
            Debug.LogError("[RunFixedUpdateSystems] Default world not found!");
            return;
        }

        try
        {
            playerInputSystem = world.GetOrCreateSystemManaged<PlayerInputSystem>();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[RunFixedUpdateSystems] Failed to create PlayerInputSystem: {e.Message}");
        }

        try
        {
            playerMovementSystem = world.GetOrCreateSystemManaged<PlayerMovementSystem>();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[RunFixedUpdateSystems] Failed to create PlayerMovementSystem: {e.Message}");
        }

        try
        {
            playerTurningSystem = world.GetOrCreateSystemManaged<PlayerTurningSystem>();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[RunFixedUpdateSystems] Failed to create PlayerTurningSystem: {e.Message}");
        }

        try
        {
            playerAnimationSystem = world.GetOrCreateSystemManaged<PlayerAnimationSystem>();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[RunFixedUpdateSystems] Failed to create PlayerAnimationSystem: {e.Message}");
        }
    }

    private void FixedUpdate()
    {
        if (playerInputSystem != null)
            playerInputSystem.Update();

        if (playerMovementSystem != null)
            playerMovementSystem.Update();

        if (playerTurningSystem != null)
            playerTurningSystem.Update();

        if (playerAnimationSystem != null)
            playerAnimationSystem.Update();
        
        // CameraFollowSystem runs automatically as an ISystem in the default world
    }
}
