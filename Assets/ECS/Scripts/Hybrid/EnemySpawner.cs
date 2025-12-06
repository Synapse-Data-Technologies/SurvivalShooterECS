using UnityEngine;
using Unity.Entities;

public class EnemySpawner : MonoBehaviour
{
    public GameObject Enemy;
    public float SpawnTime;
    public Entity Entity;

    private void Start()
    {
        // Get the default world and entity manager
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null)
        {
            Debug.LogError("[EnemySpawner] DefaultGameObjectInjectionWorld is null. Cannot create entity.");
            return;
        }

        var entityManager = world.EntityManager;

        // Create entity for this spawner
        Entity = entityManager.CreateEntity();
        Debug.Log($"[EnemySpawner] Created entity {Entity} for spawner at {transform.position}");

        // Add EnemySpawnerData component with spawn timing
        entityManager.AddComponentData(Entity, new EnemySpawnerData
        {
            SpawnTime = SpawnTime,
            CurrentTime = 0f,
            PrefabEntity = Entity.Null
        });
        Debug.Log($"[EnemySpawner] Added EnemySpawnerData to entity {Entity} with SpawnTime={SpawnTime}");

        // Add Transform as managed component
        entityManager.AddComponentObject(Entity, transform);
        Debug.Log($"[EnemySpawner] Added Transform managed component to entity {Entity}");

        // Add self reference as managed component
        entityManager.AddComponentObject(Entity, this);
        Debug.Log($"[EnemySpawner] Added EnemySpawner managed component to entity {Entity}");

        Debug.Log($"[EnemySpawner] Initialization complete for entity {Entity}");
    }
}
