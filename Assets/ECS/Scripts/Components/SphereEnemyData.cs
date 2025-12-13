using Unity.Entities;

/// <summary>
/// Component data for sphere enemies with modular AI
/// </summary>
public struct SphereEnemyData : IComponentData
{
    public float DetectionRange;
    public float AttackRange;
    public float MoveSpeed;
    public float PatrolRadius;
    public AIMode CurrentMode;
    public float LastModeChangeTime;
    public int AIVersionHash; // Hash of AI version string for performance
    
    public enum AIMode : byte
    {
        Idle = 0,
        Patrol = 1,
        Chase = 2,
        Attack = 3,
        Retreat = 4
    }
}

/// <summary>
/// Component for sphere enemy patrol behavior
/// </summary>
public struct SphereEnemyPatrolData : IComponentData
{
    public Unity.Mathematics.float3 StartPosition;
    public Unity.Mathematics.float3 PatrolTarget;
}

/// <summary>
/// Component for sphere enemy debug information
/// </summary>
public struct SphereEnemyDebugData : IComponentData
{
    public bool ShowDebugUI;
    public Unity.Mathematics.float3 UIOffset;
}