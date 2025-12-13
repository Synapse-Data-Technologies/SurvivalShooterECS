using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Hybrid GameObject component for sphere enemies in ECS
/// Creates and manages the ECS entity for a sphere enemy GameObject
/// </summary>
public class SphereEnemyObject : MonoBehaviour
{
    [Header("AI Configuration")]
    public string aiVersion = "1.0";
    public SphereEnemyData.AIMode startingMode = SphereEnemyData.AIMode.Patrol;
    
    [Header("AI Settings")]
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float moveSpeed = 3f;
    public float patrolRadius = 5f;
    
    [Header("Debug Settings")]
    public bool showDebugUI = true;
    public Vector3 uiOffset = new Vector3(0, 2f, 0);
    
    public Entity Entity { get; private set; }
    
    private void Start()
    {
        // Set up the GameObject
        SetupGameObject();
        
        // Create ECS entity
        CreateECSEntity();
        
        Debug.Log($"[SphereEnemyObject] Created sphere enemy: {gameObject.name} with entity {Entity}");
    }
    
    private void SetupGameObject()
    {
        // Ensure proper tag
        if (!gameObject.CompareTag("Enemy"))
        {
            gameObject.tag = "Enemy";
        }
        
        // Ensure required components exist
        if (GetComponent<Rigidbody>() == null)
        {
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.linearDamping = 1f;
            rb.angularDamping = 5f;
        }
        
        if (GetComponent<NavMeshAgent>() == null)
        {
            var nav = gameObject.AddComponent<NavMeshAgent>();
            nav.speed = moveSpeed;
            nav.stoppingDistance = 1.5f;
            nav.acceleration = 8f;
        }
        
        if (GetComponent<AudioSource>() == null)
        {
            var audio = gameObject.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.volume = 0.5f;
        }
        
        if (GetComponent<SphereCollider>() == null)
        {
            var collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = 0.5f;
            collider.isTrigger = false; // Keep as solid collider to avoid EnemyAttacker issues
        }
        
        // Remove any EnemyAttacker component that might cause issues
        var enemyAttacker = GetComponent<EnemyAttacker>();
        if (enemyAttacker != null)
        {
            Debug.LogWarning("[SphereEnemyObject] Removing EnemyAttacker component - sphere enemies use their own attack system");
            DestroyImmediate(enemyAttacker);
        }
        
        // Set visual appearance
        var renderer = GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            renderer.material.color = Color.red;
        }
        
        // Disable any old MonoBehaviour enemy scripts
        DisableOldScripts();
    }
    
    private void DisableOldScripts()
    {
        // Disable old non-ECS scripts if they exist
        var oldScripts = new System.Type[]
        {
            typeof(EnemyMovement),
            typeof(EnemyAttack), 
            typeof(EnemyHealth),
            typeof(SphereEnemyAI),
            typeof(SphereEnemyHealth),
            typeof(SphereEnemyDebugUI)
        };
        
        foreach (var scriptType in oldScripts)
        {
            var component = GetComponent(scriptType) as MonoBehaviour;
            if (component != null)
            {
                Debug.LogWarning($"[SphereEnemyObject] Disabling old {scriptType.Name} script - using ECS instead");
                component.enabled = false;
            }
        }
    }
    
    private void CreateECSEntity()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null)
        {
            Debug.LogError("[SphereEnemyObject] No default world found!");
            return;
        }
        
        var entityManager = world.EntityManager;
        var settings = SurvivalShooterBootstrap.Settings;
        
        // Create entity
        Entity = entityManager.CreateEntity();
        
        // Add base enemy components
        entityManager.AddComponentData(Entity, new EnemyData());
        entityManager.AddComponentData(Entity, new HealthData { Value = 50 }); // Lower health for easy testing
        
        // Add sphere enemy specific components
        entityManager.AddComponentData(Entity, new SphereEnemyData
        {
            DetectionRange = detectionRange,
            AttackRange = attackRange,
            MoveSpeed = moveSpeed,
            PatrolRadius = patrolRadius,
            CurrentMode = startingMode,
            LastModeChangeTime = 0f,
            AIVersionHash = aiVersion.GetHashCode()
        });
        
        entityManager.AddComponentData(Entity, new SphereEnemyPatrolData
        {
            StartPosition = transform.position,
            PatrolTarget = transform.position
        });
        
        entityManager.AddComponentData(Entity, new SphereEnemyDebugData
        {
            ShowDebugUI = showDebugUI,
            UIOffset = uiOffset
        });
        
        // Add managed components for hybrid GameObject integration
        entityManager.AddComponentObject(Entity, transform);
        
        var navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent != null)
            entityManager.AddComponentObject(Entity, navMeshAgent);
        
        var animator = GetComponent<Animator>();
        if (animator != null)
            entityManager.AddComponentObject(Entity, animator);
        
        var audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
            entityManager.AddComponentObject(Entity, audioSource);
        
        var collider = GetComponent<Collider>();
        if (collider != null)
            entityManager.AddComponentObject(Entity, collider);
        
        var rigidbody = GetComponent<Rigidbody>();
        if (rigidbody != null)
            entityManager.AddComponentObject(Entity, rigidbody);
        
        // Add self reference for debug UI access
        entityManager.AddComponentObject(Entity, this);
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
    
    // Public methods for external control (called by systems or debug tools)
    public void SetAIMode(SphereEnemyData.AIMode newMode)
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world != null && world.EntityManager.Exists(Entity))
        {
            var sphereData = world.EntityManager.GetComponentData<SphereEnemyData>(Entity);
            sphereData.CurrentMode = newMode;
            sphereData.LastModeChangeTime = Time.time;
            world.EntityManager.SetComponentData(Entity, sphereData);
        }
    }
    
    public void SetAIVersion(string version)
    {
        aiVersion = version;
        var world = World.DefaultGameObjectInjectionWorld;
        if (world != null && world.EntityManager.Exists(Entity))
        {
            var sphereData = world.EntityManager.GetComponentData<SphereEnemyData>(Entity);
            sphereData.AIVersionHash = version.GetHashCode();
            world.EntityManager.SetComponentData(Entity, sphereData);
        }
    }
    
    public SphereEnemyData.AIMode GetCurrentMode()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world != null && world.EntityManager.Exists(Entity))
        {
            var sphereData = world.EntityManager.GetComponentData<SphereEnemyData>(Entity);
            return sphereData.CurrentMode;
        }
        return SphereEnemyData.AIMode.Idle;
    }
}