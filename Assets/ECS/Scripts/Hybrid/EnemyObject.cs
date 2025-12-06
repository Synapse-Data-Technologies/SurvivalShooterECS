using Unity.Entities;
using UnityEngine;
using UnityEngine.AI;

public class EnemyObject : MonoBehaviour
{
    public Entity Entity;

    private void Start()
    {
        // Create entity at runtime for hybrid GameObject
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null)
        {
            Debug.LogError("[EnemyObject] No default world found!");
            return;
        }

        var entityManager = world.EntityManager;
        var settings = SurvivalShooterBootstrap.Settings;

        // Disable old MonoBehaviour scripts if they exist (legacy non-ECS scripts)
        var oldMovement = GetComponent<EnemyMovement>();
        if (oldMovement != null)
        {
            Debug.LogWarning("[EnemyObject] Disabling old EnemyMovement MonoBehaviour script - using ECS system instead");
            oldMovement.enabled = false;
        }
        
        var oldAttack = GetComponent<EnemyAttack>();
        if (oldAttack != null)
        {
            Debug.LogWarning("[EnemyObject] Disabling old EnemyAttack MonoBehaviour script - using ECS system instead");
            oldAttack.enabled = false;
        }
        
        var oldHealth = GetComponent<EnemyHealth>();
        if (oldHealth != null)
        {
            Debug.LogWarning("[EnemyObject] Disabling old EnemyHealth MonoBehaviour script - using ECS system instead");
            oldHealth.enabled = false;
        }
        
        // Get GameObject components
        var navMeshAgent = GetComponent<NavMeshAgent>();
        var animator = GetComponent<Animator>();
        var audioSource = GetComponent<AudioSource>();
        var capsuleCollider = GetComponent<CapsuleCollider>();

        // Create entity and add components
        Entity = entityManager.CreateEntity();
        entityManager.AddComponentData(Entity, new EnemyData());
        entityManager.AddComponentData(Entity, new HealthData { Value = settings.StartingEnemyHealth });
        
        // Add managed components for hybrid GameObject integration
        entityManager.AddComponentObject(Entity, transform);
        
        if (navMeshAgent != null)
            entityManager.AddComponentObject(Entity, navMeshAgent);
        
        if (animator != null)
            entityManager.AddComponentObject(Entity, animator);
        
        if (audioSource != null)
            entityManager.AddComponentObject(Entity, audioSource);
        
        if (capsuleCollider != null)
            entityManager.AddComponentObject(Entity, capsuleCollider);

        Debug.Log($"[EnemyObject] Created enemy entity: {Entity}");
        Debug.Log($"[EnemyObject] Has Transform: {entityManager.HasComponent<Transform>(Entity)}");
        Debug.Log($"[EnemyObject] Has NavMeshAgent: {entityManager.HasComponent<NavMeshAgent>(Entity)}");
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
