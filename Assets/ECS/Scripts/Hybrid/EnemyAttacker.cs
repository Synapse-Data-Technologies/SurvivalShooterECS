using Unity.Entities;
using UnityEngine;

public class EnemyAttacker : MonoBehaviour
{
    private GameObject player;
    private EntityManager entityManager;
    private EntityArchetype enemyAttackArchetype;
    private Entity enemyAttackEntity;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        enemyAttackArchetype = entityManager.CreateArchetype(typeof(EnemyAttackData));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            // Check for EnemyObject first
            var enemyObject = GetComponent<EnemyObject>();
            if (enemyObject != null)
            {
                enemyAttackEntity = entityManager.CreateEntity(enemyAttackArchetype);
                entityManager.SetComponentData(enemyAttackEntity, new EnemyAttackData
                {
                    Timer = 0f,
                    Frequency = SurvivalShooterBootstrap.Settings.TimeBetweenEnemyAttacks,
                    Damage = SurvivalShooterBootstrap.Settings.EnemyAttackDamage,
                    Source = enemyObject.Entity,
                    Target = player.GetComponent<PlayerObject>().Entity
                });
            }
            else
            {
                // Check for SphereEnemyObject as fallback
                var sphereEnemyObject = GetComponent<SphereEnemyObject>();
                if (sphereEnemyObject != null)
                {
                    enemyAttackEntity = entityManager.CreateEntity(enemyAttackArchetype);
                    entityManager.SetComponentData(enemyAttackEntity, new EnemyAttackData
                    {
                        Timer = 0f,
                        Frequency = SurvivalShooterBootstrap.Settings.TimeBetweenEnemyAttacks,
                        Damage = SurvivalShooterBootstrap.Settings.EnemyAttackDamage,
                        Source = sphereEnemyObject.Entity,
                        Target = player.GetComponent<PlayerObject>().Entity
                    });
                }
                else
                {
                    Debug.LogWarning($"[EnemyAttacker] No EnemyObject or SphereEnemyObject found on {gameObject.name}");
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player && entityManager.Exists(enemyAttackEntity))
            entityManager.DestroyEntity(enemyAttackEntity);
    }
}
