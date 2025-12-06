using Unity.Entities;
using Unity.Mathematics;

public struct PlayerInputData : IComponentData
{
    public float2 Move;        // Movement input (-1 to 1 on each axis)
    public float2 Look;        // Look input (screen position for mouse, direction for stick)
    public float Shoot;        // Shoot input (0 or 1)
    public bool IsUsingGamepad; // Flag to indicate active input method
}
