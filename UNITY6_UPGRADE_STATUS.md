# Unity 6 Upgrade Status - DOTS Baseline Alignment

## ✅ Step 1: Unity 6 Installed
- **Unity Version**: 6.x (6000.x)
- **Status**: Complete

## ✅ Step 2: DOTS Packages Aligned

**Manifest Updated to Unity 6 Baseline:**
```json
"com.unity.entities": "1.4.3"
"com.unity.collections": "2.6.3"
"com.unity.entities.graphics": "1.4.16"
```

**What This Fixes:**
- ✅ UI Toolkit MultiColumnListView API gaps
- ✅ Property system dependencies (CreatePropertyAttribute)
- ✅ Codegen compatibility
- ✅ All DOTS .gen.cs files will regenerate correctly

## 🔄 Step 3: Clean Reimport (REQUIRED)

**You MUST do this now:**

1. **Close Unity completely**
2. **Delete the Library folder**
   - Navigate to your project folder
   - Delete the entire `Library/` directory
3. **Reopen Unity**
4. **Wait 5-10 minutes** for full DOTS codegen regeneration

**Why:** This forces all DOTS-generated `.gen.cs` files to recompile against the new Unity 6 property system.

## 📋 Step 4: Validate Core Runtime (After Reimport)

Once Unity finishes reimporting, check:

### Compilation
- [ ] Zero compile errors
- [ ] All systems compile successfully

### ECS Systems
- [ ] PlayerInputSystem works
- [ ] PlayerMovementSystem works
- [ ] PlayerTurningSystem works
- [ ] PlayerShootingSystem works
- [ ] PlayerAnimationSystem works

### Input
- [ ] Keyboard/mouse input works
- [ ] Xbox controller input works
- [ ] Input mode switching works

### Rendering
- [ ] Game renders correctly
- [ ] No visual glitches

## 🔧 Step 5: Fix API Breakages (Entities 1.0 → 1.4)

### Known Changes in Entities 1.4:

#### 1. SystemBase → ISystem (Optional but Recommended)
**Old (SystemBase):**
```csharp
public partial class MySystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref MyComponent comp) => {
            // ...
        }).Schedule();
    }
}
```

**New (ISystem - Better Performance):**
```csharp
public partial struct MySystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var comp in SystemAPI.Query<RefRW<MyComponent>>())
        {
            // ...
        }
    }
}
```

#### 2. Entities.ForEach → SystemAPI.Query
**Old:**
```csharp
Entities.ForEach((Entity e, ref Translation trans) => {
    trans.Value += new float3(1, 0, 0);
}).Schedule();
```

**New:**
```csharp
foreach (var (trans, entity) in SystemAPI.Query<RefRW<LocalTransform>>().WithEntityAccess())
{
    trans.ValueRW.Position += new float3(1, 0, 0);
}
```

#### 3. LocalTransform (Already Using This)
- ✅ Your debug system already uses `LocalTransform`
- ✅ No changes needed here

### Your Current Systems Status:

**Already Compatible:**
- ✅ `PlayerInputSystem` - Uses SystemBase (still supported)
- ✅ `AimDebugSystem` - Uses SystemBase + LocalTransform
- ✅ `InputUtilities` - Pure utility class, no changes needed

**May Need Updates:**
- ⚠️ `PlayerMovementSystem` - Check if using old Transform components
- ⚠️ `PlayerTurningSystem` - Uses hybrid Rigidbody (should still work)
- ⚠️ `PlayerAnimationSystem` - Check for deprecated APIs

## 📊 Current Status

**Status**: ⏳ Waiting for clean reimport

**Next Actions:**
1. Close Unity
2. Delete `Library/` folder
3. Reopen Unity
4. Wait for reimport (5-10 minutes)
5. Report any compilation errors

## 🎯 Expected Outcome

After clean reimport:
- ✅ Zero compilation errors
- ✅ All DOTS systems working
- ✅ Modern Entities 1.4 API
- ✅ Better performance
- ✅ Debug visualization working
- ✅ Xbox controller support functional

## 📝 Locked Engine Stack (For Future Reference)

**Baseline for all development:**
- **Unity**: 6.x (6000.x)
- **Entities**: 1.4.3
- **Collections**: 2.6.3
- **Entities Graphics**: 1.4.16
- **Burst**: Auto-resolved by Unity 6
- **Input System**: 1.14.2

**Policy**: No DOTS package upgrades without validation.

---

## 🚨 If You See Errors After Reimport

**Tell me:**
1. The exact error messages
2. Which files have errors
3. I'll fix them immediately

**Common fixes I'll apply:**
- Update any `Translation/Rotation/Scale` → `LocalTransform`
- Update any deprecated system APIs
- Fix any hybrid component access patterns
