using Unity.Entities;
using UnityEngine;

[DisableAutoCreation]
public partial class PlayerTurningSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var mainCamera = Camera.main;
        if (mainCamera == null)
            return;

        var camRayLen = SurvivalShooterBootstrap.Settings.CamRayLen;
        var floor = LayerMask.GetMask("Floor");

        Entities.WithoutBurst().WithAll<PlayerData>().WithNone<DeadData>().ForEach(
            (Entity entity, Rigidbody rigidBody, in PlayerInputData input) =>
            {
                Vector3 lookDirection;

                if (input.IsUsingGamepad)
                {
                    // Gamepad mode: Look is a direction vector
                    // Only rotate if there's meaningful input (not zero vector)
                    if (input.Look.x != 0 || input.Look.y != 0)
                    {
                        // Convert 2D direction to 3D (Y axis is up, so we use X and Z for horizontal plane)
                        lookDirection = new Vector3(input.Look.x, 0f, input.Look.y);
                        lookDirection.Normalize();
                        
                        var newRot = Quaternion.LookRotation(lookDirection);
                        rigidBody.MoveRotation(newRot);
                    }
                    // If input is zero, maintain current rotation (do nothing)
                }
                else
                {
                    // Mouse mode: Look is screen position
                    var mousePos = new Vector3(input.Look.x, input.Look.y, 0);
                    var camRay = mainCamera.ScreenPointToRay(mousePos);
                    RaycastHit floorHit;
                    if (Physics.Raycast(camRay, out floorHit, camRayLen, floor))
                    {
                        var position = rigidBody.gameObject.transform.position;
                        var playerToMouse = floorHit.point - new Vector3(position.x, position.y, position.z);
                        playerToMouse.y = 0f;
                        var newRot = Quaternion.LookRotation(playerToMouse);
                        rigidBody.MoveRotation(newRot);
                    }
                }
            }).Run();
    }
}
