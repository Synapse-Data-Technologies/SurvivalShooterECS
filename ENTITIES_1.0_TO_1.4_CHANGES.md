# Entities 1.0 → 1.4 API Changes Quick Reference

## Transform Components

### ❌ Old (Entities 1.0)
```csharp
using Unity.Transforms;

// Separate components
Translation translation;
Rotation rotation;
Scale scale;

// Usage
translation.Value = new float3(1, 2, 3);
rotation.Value = quaternion.identity;
scale.Value = 1.0f;
```

### ✅ New (Entities 1.4)
```csharp
using Unity.Transforms;

// Unified component
LocalTransform transform;

// Usage
transform.Position = new float3(1, 2, 3);
transform.Rotation = quaternion.identity;
transform.Scale = 1.0f;
```

## System Patterns

### ❌ Old (SystemBase with Entities.ForEach)
```csharp
public partial class MySystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities
            .WithAll<PlayerData>()
            .ForEach((Entity e, ref Translation trans, in Velocity vel) =>
            {
                trans.Value += vel.Value * SystemAPI.Time.DeltaTime;
            }).ScheduleParallel();
    }
}
```

### ✅ New (ISystem with SystemAPI.Query)
```csharp
public partial struct MySystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (trans, vel) in 
            SystemAPI.Query<RefRW<LocalTransform>, RefRO<Velocity>>()
                .WithAll<PlayerData>())
        {
            trans.ValueRW.Position += vel.ValueRO.Value * SystemAPI.Time.DeltaTime;
        }
    }
}
```

### ⚠️ Still Supported (SystemBase - Hybrid)
```csharp
// SystemBase still works for hybrid GameObject/ECS scenarios
public partial class PlayerTurningSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities
            .WithoutBurst()
            .WithAll<PlayerData>()
            .ForEach((Entity entity, Rigidbody rb, in PlayerInputData input) =>
            {
                // Hybrid: accessing Rigidbody (managed component)
                rb.MoveRotation(/* ... */);
            }).Run();
    }
}
```

## System Management

### ❌ Old
```csharp
var system = world.GetExistingSystem<MySystem>();
var newSystem = world.CreateSystem<MySystem>();
```

### ✅ New
```csharp
var system = world.GetExistingSystemManaged<MySystem>();
var newSystem = world.CreateSystemManaged<MySystem>();
```

## Query Patterns

### ❌ Old (Entities.ForEach)
```csharp
Entities.ForEach((Entity e, ref MyComponent comp) =>
{
    comp.Value += 1;
}).Schedule();
```

### ✅ New (SystemAPI.Query)
```csharp
foreach (var comp in SystemAPI.Query<RefRW<MyComponent>>())
{
    comp.ValueRW.Value += 1;
}
```

### With Entity Access
```csharp
foreach (var (comp, entity) in 
    SystemAPI.Query<RefRW<MyComponent>>().WithEntityAccess())
{
    comp.ValueRW.Value += 1;
    // Can use entity here
}
```

## Component Access

### Read-Only
```csharp
// Old
in MyComponent comp

// New
RefRO<MyComponent> comp
// Access: comp.ValueRO
```

### Read-Write
```csharp
// Old
ref MyComponent comp

// New
RefRW<MyComponent> comp
// Access: comp.ValueRW
```

## Baking (Authoring)

### ❌ Deprecated
```csharp
// ConvertToEntity is fully retired
[RequiresEntityConversion]
public class MyAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, 
        GameObjectConversionSystem conversionSystem)
    {
        // Old conversion
    }
}
```

### ✅ New (Baker Pattern)
```csharp
public class MyAuthoring : MonoBehaviour
{
    public float speed;
}

public class MyBaker : Baker<MyAuthoring>
{
    public override void Bake(MyAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new MyComponent { Speed = authoring.speed });
    }
}
```

## Your Project Status

### Already Using Modern API ✅
- `AimDebugSystem` - Uses `LocalTransform`
- `AimDebugVisualizer` - Uses `GetExistingSystemManaged`

### Using Compatible Hybrid Pattern ✅
- `PlayerInputSystem` - SystemBase (still supported)
- `PlayerTurningSystem` - SystemBase with Rigidbody (hybrid, still supported)

### May Need Checking ⚠️
- `PlayerMovementSystem` - Check if using old Transform components
- `PlayerAnimationSystem` - Check for deprecated APIs

## Migration Priority

### High Priority (Breaking Changes)
1. ✅ `Translation/Rotation/Scale` → `LocalTransform` (if used)
2. ✅ `GetExistingSystem` → `GetExistingSystemManaged` (already done)

### Medium Priority (Performance)
1. Consider `SystemBase` → `ISystem` for pure ECS systems
2. Consider `Entities.ForEach` → `SystemAPI.Query`

### Low Priority (Optional)
1. Update to Baker pattern if using old conversion
2. Optimize queries with `SystemAPI`

## When to Use What

### Use ISystem When:
- Pure ECS (no GameObjects)
- Maximum performance needed
- No managed components (Rigidbody, Transform, etc.)

### Use SystemBase When:
- Hybrid ECS + GameObject
- Need `WithoutBurst()` for managed components
- Easier migration from old code

## Performance Notes

**ISystem Benefits:**
- Struct-based (no GC allocations)
- Better Burst compilation
- Faster scheduling
- ~10-30% performance improvement

**SystemBase Still Good For:**
- Hybrid scenarios
- Rapid prototyping
- When performance difference is negligible
