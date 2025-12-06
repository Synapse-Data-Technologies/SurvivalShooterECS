using Unity.Collections;
using Unity.Entities;
using UnityEngine;

/// <summary>
/// Modern ISystem implementation for enemy death handling.
/// Processes dead enemies by playing death animations, sounds, and updating score.
/// </summary>
public partial struct EnemyDeathSystem : ISystem
{
    private int score;
    private static readonly int DeadHash = Animator.StringToHash("Dead");

    public void OnCreate(ref SystemState state)
    {
        // Only run when dead enemies exist
        state.RequireForUpdate<DeadData>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var gameUi = SurvivalShooterBootstrap.Settings.GameUi;
        var scorePerDeath = SurvivalShooterBootstrap.Settings.ScorePerDeath;

        // Query for all dead enemies
        var deadEnemyQuery = SystemAPI.QueryBuilder()
            .WithAll<EnemyData, DeadData>()
            .Build();

        var deadEnemies = deadEnemyQuery.ToEntityArray(state.WorldUpdateAllocator);

        foreach (var entity in deadEnemies)
        {
            // Null safety checks for managed components
            if (!state.EntityManager.HasComponent<CapsuleCollider>(entity) ||
                !state.EntityManager.HasComponent<Animator>(entity) ||
                !state.EntityManager.HasComponent<AudioSource>(entity))
            {
                Debug.LogWarning($"[EnemyDeathSystem] Dead enemy {entity} missing required components");
                continue;
            }

            var collider = state.EntityManager.GetComponentObject<CapsuleCollider>(entity);
            var animator = state.EntityManager.GetComponentObject<Animator>(entity);
            var audio = state.EntityManager.GetComponentObject<AudioSource>(entity);

            if (collider == null || animator == null || audio == null)
            {
                Debug.LogWarning($"[EnemyDeathSystem] Null managed components on dead enemy {entity}");
                continue;
            }

            // Handle death effects
            collider.isTrigger = true;
            animator.SetTrigger(DeadHash);
            audio.clip = SurvivalShooterBootstrap.Settings.EnemyDeathClip;
            audio.Play();

            // Get EnemyObject and destroy entity
            var enemyObject = collider.gameObject.GetComponent<EnemyObject>();
            if (enemyObject != null)
            {
                state.EntityManager.DestroyEntity(enemyObject.Entity);
            }
            else
            {
                Debug.LogWarning($"[EnemyDeathSystem] No EnemyObject found on dead enemy {entity}");
                // Fallback: destroy the entity directly
                state.EntityManager.DestroyEntity(entity);
            }

            // Update score
            score += scorePerDeath;
            gameUi.OnEnemyKilled(score);
        }
    }
}
