using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PlayerObject : MonoBehaviour
{
    public Transform GunPivot;
    public Entity Entity;

    private Rigidbody rb;
    private Animator animator;

    private void Start()
    {
        // Get GameObject components
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // Create entity at runtime for hybrid GameObject
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null)
        {
            Debug.LogError("[PlayerObject] No default world found!");
            return;
        }

        var entityManager = world.EntityManager;
        var settings = SurvivalShooterBootstrap.Settings;

        // Create entity and add components
        Entity = entityManager.CreateEntity();
        entityManager.AddComponentData(Entity, new PlayerData());
        entityManager.AddComponentData(Entity, new HealthData { Value = settings.StartingPlayerHealth });
        entityManager.AddComponentData(Entity, new PlayerInputData { Move = new float2(0, 0) });
        
        // Add managed components for hybrid GameObject integration
        entityManager.AddComponentObject(Entity, transform);
        entityManager.AddComponentObject(Entity, rb);
        entityManager.AddComponentObject(Entity, animator);

        Debug.Log($"[PlayerObject] Created player entity: {Entity}");
        Debug.Log($"[PlayerObject] Has Transform: {entityManager.HasComponent<Transform>(Entity)}");
        Debug.Log($"[PlayerObject] Has Rigidbody: {entityManager.HasComponent<Rigidbody>(Entity)}");
        Debug.Log($"[PlayerObject] Has PlayerData: {entityManager.HasComponent<PlayerData>(Entity)}");
    }

    private void FixedUpdate()
    {
        // Handle movement in hybrid mode
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null || !world.EntityManager.Exists(Entity))
            return;

        var entityManager = world.EntityManager;
        
        // Check if player is dead
        if (entityManager.HasComponent<DeadData>(Entity))
            return;

        // Get input data from entity
        if (!entityManager.HasComponent<PlayerInputData>(Entity))
            return;

        var inputData = entityManager.GetComponentData<PlayerInputData>(Entity);
        var health = entityManager.GetComponentData<HealthData>(Entity);

        // Movement
        if (rb != null && health.Value > 0)
        {
            var speed = SurvivalShooterBootstrap.Settings.PlayerMoveSpeed;
            var move = inputData.Move;
            var movement = new Vector3(move.x, 0, move.y);
            movement = movement.normalized * speed * Time.fixedDeltaTime;
            rb.MovePosition(transform.position + movement);
        }

        // Animation
        if (animator != null)
        {
            var isWalking = inputData.Move.x != 0f || inputData.Move.y != 0f;
            animator.SetBool("IsWalking", isWalking);
        }

        // Turning (aiming)
        if (rb != null && health.Value > 0)
        {
            Vector3 lookDirection;

            if (inputData.IsUsingGamepad)
            {
                // Gamepad mode: Look is a direction vector
                if (inputData.Look.x != 0 || inputData.Look.y != 0)
                {
                    lookDirection = new Vector3(inputData.Look.x, 0f, inputData.Look.y);
                    lookDirection.Normalize();
                    var newRot = Quaternion.LookRotation(lookDirection);
                    rb.MoveRotation(newRot);
                }
            }
            else
            {
                // Mouse mode: Look is screen position
                var mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    var mousePos = new Vector3(inputData.Look.x, inputData.Look.y, 0);
                    var camRay = mainCamera.ScreenPointToRay(mousePos);
                    RaycastHit floorHit;
                    var camRayLen = SurvivalShooterBootstrap.Settings.CamRayLen;
                    var floor = LayerMask.GetMask("Floor");
                    
                    if (Physics.Raycast(camRay, out floorHit, camRayLen, floor))
                    {
                        var playerToMouse = floorHit.point - transform.position;
                        playerToMouse.y = 0f;
                        var newRot = Quaternion.LookRotation(playerToMouse);
                        rb.MoveRotation(newRot);
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        // Clean up entity when GameObject is destroyed
        var world = World.DefaultGameObjectInjectionWorld;
        if (world != null && world.EntityManager.Exists(Entity))
        {
            world.EntityManager.DestroyEntity(Entity);
        }
    }
}
