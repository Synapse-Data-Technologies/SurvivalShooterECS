using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

#if ALINE_AVAILABLE
using Drawing;
#endif

/// <summary>
/// Displays real-time debug information for the player including performance metrics,
/// input state, health, and entity counts. Uses ALINE for 3D visualization.
/// </summary>
public class PlayerDebugWindow : MonoBehaviour
{
    [Header("Display Settings")]
    public Vector3 offset = new Vector3(0, 3, 0);
    public float updateInterval = 0.1f;
    
    [Header("Performance Thresholds")]
    public float targetFrameTime = 16.67f; // 60 FPS target
    
    private Entity playerEntity;
    private World world;
    private float timeSinceUpdate = 0f;
    
    // Cached metrics
    private float fps;
    private float frameTime;
    private float2 moveInput;
    private float2 lookInput;
    private int health;
    private Vector3 position;
    private int enemyCount;
    private int spawnerCount;
    private int playerCount;
    
    private void Start()
    {
        world = World.DefaultGameObjectInjectionWorld;
        
        // Find player entity
        var playerObj = GetComponent<PlayerObject>();
        if (playerObj != null)
        {
            playerEntity = playerObj.Entity;
            Debug.Log("[PlayerDebugWindow] Attached to player entity");
        }
        else
        {
            Debug.LogWarning("[PlayerDebugWindow] PlayerObject component not found!");
        }
    }
    
    private void Update()
    {
        if (world == null || !world.EntityManager.Exists(playerEntity))
            return;
            
        timeSinceUpdate += Time.deltaTime;
        
        // Update metrics at interval to reduce overhead
        if (timeSinceUpdate >= updateInterval)
        {
            UpdateMetrics();
            timeSinceUpdate = 0f;
        }
        
        // Render debug info
        RenderDebugInfo();
    }
    
    private void UpdateMetrics()
    {
        var em = world.EntityManager;
        
        // FPS and frame time
        fps = 1f / Time.deltaTime;
        frameTime = Time.deltaTime * 1000f;
        
        // Player data
        if (em.HasComponent<PlayerInputData>(playerEntity))
        {
            var inputData = em.GetComponentData<PlayerInputData>(playerEntity);
            moveInput = inputData.Move;
            lookInput = inputData.Look;
        }
        
        if (em.HasComponent<HealthData>(playerEntity))
        {
            health = em.GetComponentData<HealthData>(playerEntity).Value;
        }
        
        position = transform.position;
        
        // Entity counts
        playerCount = CountEntitiesWithComponent<PlayerData>();
        enemyCount = CountEntitiesWithComponent<EnemyData>();
        spawnerCount = CountEntitiesWithComponent<EnemySpawnerData>();
    }
    
    private int CountEntitiesWithComponent<T>() where T : struct, IComponentData
    {
        var query = world.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<T>());
        var count = query.CalculateEntityCount();
        query.Dispose();
        return count;
    }
    
    private void RenderDebugInfo()
    {
#if ALINE_AVAILABLE
        using (Draw.WithDuration(updateInterval))
        {
            var worldPos = transform.position + offset;
            
            // Performance indicator color
            var color = frameTime > targetFrameTime ? Color.red : Color.green;
            
            // Build debug text
            var debugText = $"=== PERFORMANCE ===\n" +
                          $"FPS: {fps:F1}\n" +
                          $"Frame: {frameTime:F2}ms\n" +
                          $"\n=== PLAYER ===\n" +
                          $"Health: {health}\n" +
                          $"Move: ({moveInput.x:F2}, {moveInput.y:F2})\n" +
                          $"Pos: ({position.x:F1}, {position.y:F1}, {position.z:F1})\n" +
                          $"\n=== ENTITIES ===\n" +
                          $"Players: {playerCount}\n" +
                          $"Enemies: {enemyCount}\n" +
                          $"Spawners: {spawnerCount}";
            
            // Draw label at world position
            Draw.Label3D(worldPos, debugText, color, fontSize: 12);
            
            // Draw performance indicator sphere when frame time exceeds threshold
            if (frameTime > targetFrameTime)
            {
                Draw.WireSphere(worldPos + new Vector3(0, 0.5f, 0), 0.2f, Color.red);
            }
        }
#else
        // Fallback: Draw using Unity's Gizmos (only visible in Scene view)
        Debug.DrawLine(transform.position, transform.position + offset, 
            frameTime > targetFrameTime ? Color.red : Color.green);
#endif
    }
    
    private void OnDrawGizmos()
    {
        // Draw a small sphere to show where the debug window is attached
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + offset, 0.1f);
    }
}
