# Unity ECS Upgrade Guide: 0.51 → 1.0.16

## Current State
- **Unity Version**: 2021.3.45f2
- **Current ECS**: 0.51.1-preview.21
- **Target ECS**: 1.0.16 (Entities 1.0 - stable release)

## Why Upgrade?

### Benefits
1. **Stable Release**: ECS 1.0 is production-ready (no longer preview)
2. **Better Performance**: Improved job scheduling and Burst compilation
3. **Modern API**: `LocalTransform` component for transforms
4. **Better Tooling**: Improved Entity Inspector and debugging
5. **Bug Fixes**: Many stability improvements

### Breaking Changes
The upgrade from 0.51 to 1.0 has significant API changes:
- `Translation`, `Rotation`, `Scale` → `LocalTransform`
- `GetExistingSystem` → `GetExistingSystemManaged`
- `CreateSystem` → `CreateSystemManaged`
- Hybrid components (GameObject/ECS) work differently

## Upgrade Steps

### Step 1: Backup Your Project
1. Close Unity
2. Make a full backup of your project folder
3. Commit to version control if using Git

### Step 2: Update Package Manifest

The manifest will be updated automatically, but here's what changes:

**Before:**
```json
"com.unity.entities": "0.51.1-preview.21"
```

**After:**
```json
"com.unity.entities": "1.0.16"
```

### Step 3: Let Unity Resolve Dependencies

Unity will automatically download:
- `com.unity.entities` 1.0.16
- `com.unity.entities.graphics` (for rendering)
- `com.unity.burst` (updated version)
- `com.unity.collections` (updated version)
- `com.unity.mathematics` (updated version)

### Step 4: Fix Compilation Errors

After the upgrade, you'll need to update your code:

#### Transform Components
**Old (0.51):**
```csharp
using Unity.Transforms;

// Separate components
Translation translation;
Rotation rotation;
Scale scale;
```

**New (1.0):**
```csharp
using Unity.Transforms;

// Unified component
LocalTransform transform;
Vector3 position = transform.Position;
Quaternion rotation = transform.Rotation;
float scale = transform.Scale;
```

#### System Management
**Old (0.51):**
```csharp
var system = world.GetExistingSystem<MySystem>();
var newSystem = world.CreateSystem<MySystem>();
```

**New (1.0):**
```csharp
var system = world.GetExistingSystemManaged<MySystem>();
var newSystem = world.CreateSystemManaged<MySystem>();
```

#### Entities.ForEach
**Old (0.51):**
```csharp
Entities.ForEach((Entity e, ref Translation trans) => {
    trans.Value += new float3(1, 0, 0);
}).Schedule();
```

**New (1.0):**
```csharp
Entities.ForEach((Entity e, ref LocalTransform trans) => {
    trans.Position += new float3(1, 0, 0);
}).Schedule();
```

### Step 5: Update Hybrid Components

If you're using GameObjects with ECS (like this project does):

**Old (0.51):**
```csharp
// Direct GameObject access in ForEach
Entities.ForEach((Entity e, Rigidbody rb) => {
    // ...
}).WithoutBurst().Run();
```

**New (1.0):**
```csharp
// Use GetComponentObject for managed components
Entities.WithoutBurst().ForEach((Entity e) => {
    var rb = EntityManager.GetComponentObject<Rigidbody>(e);
    // ...
}).Run();
```

## Files That Need Updates

Based on your project, these files will need changes:

### 1. PlayerInputSystem.cs
- No changes needed (doesn't use transforms)

### 2. PlayerTurningSystem.cs
- Already uses `Rigidbody` directly (hybrid approach)
- May need to update to use `GetComponentObject` pattern

### 3. PlayerMovementSystem.cs
- Likely uses transforms
- Update to `LocalTransform`

### 4. AimDebugSystem.cs (new file)
- Already written for ECS 1.0
- Will work after upgrade

### 5. AimDebugVisualizer.cs (new file)
- Already uses correct API
- Will work after upgrade

## Automated Migration

Unity provides some automatic migration:
1. **API Updater**: Runs automatically on first compile
2. **Manual Fixes**: Some changes require manual intervention

## Testing After Upgrade

1. **Compile**: Fix all compilation errors
2. **Play Mode**: Test basic gameplay
3. **Input**: Verify controller input still works
4. **Movement**: Check player movement
5. **Aiming**: Test aiming system
6. **Shooting**: Verify shooting works

## Rollback Plan

If the upgrade fails:
1. Close Unity
2. Restore from backup
3. Or revert `Packages/manifest.json` to old version
4. Delete `Library` folder
5. Reopen Unity

## Expected Timeline

- **Package Download**: 2-5 minutes
- **Initial Compile**: 5-10 minutes
- **Fix Errors**: 30-60 minutes
- **Testing**: 15-30 minutes

**Total**: ~1-2 hours

## Need Help?

If you encounter issues:
1. Check Unity Console for specific errors
2. Refer to [Unity ECS 1.0 Migration Guide](https://docs.unity3d.com/Packages/com.unity.entities@1.0/manual/upgrade-guide.html)
3. Ask me to fix specific compilation errors

## Ready to Upgrade?

Say "yes" and I'll:
1. Update the package manifest
2. Wait for Unity to download packages
3. Fix all compilation errors in your code
4. Test the systems
