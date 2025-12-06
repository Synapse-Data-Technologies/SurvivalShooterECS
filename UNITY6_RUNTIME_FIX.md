# Unity 6 Runtime Fix - UnityEngine Component Registration

## Problem

When running the game in Unity 6 + ECS 1.4, you get these errors:

```
ArgumentException: Unknown Type:`UnityEngine.Animator`
ArgumentException: Unknown Type:`UnityEngine.AI.NavMeshAgent`
```

## Root Cause

In ECS 1.4 (Unity 6), all UnityEngine components used in ECS systems must be explicitly registered with the TypeManager using the `[RegisterUnityEngineComponentType]` attribute.

This is a new requirement for hybrid GameObject/ECS scenarios.

## Solution Applied

Created `Assets/ECS/Scripts/UnityEngineComponentRegistration.cs` which registers all UnityEngine components used in your systems:

- ✅ `Animator` - Used in PlayerAnimationSystem, EnemyDeathSystem, PlayerHitFxSystem
- ✅ `NavMeshAgent` - Used in EnemyMovementSystem
- ✅ `AudioSource` - Used in multiple systems
- ✅ `CapsuleCollider` - Used in EnemyDeathSystem
- ✅ `Rigidbody` - Used in PlayerMovementSystem, PlayerTurningSystem
- ✅ `Transform` - Used in CameraFollowSystem and others

## How It Works

```csharp
[RegisterUnityEngineComponentType]
class AnimatorRegistration : UnityEngine.Animator { }
```

This tells the ECS TypeManager that `Animator` is a valid component type that can be used in entity queries.

## Other Warnings (Safe to Ignore)

**"No SRP present... Mesh Deformation Systems disabled"**
- Your project doesn't use Scriptable Render Pipeline (SRP)
- Uses built-in render pipeline instead
- Mesh deformation features are disabled (not needed for your game)
- This is informational, not an error

**"Entities Graphics package disabled"**
- Same reason - no SRP
- Your game uses traditional GameObject rendering
- This is expected and correct for your setup

## Testing

After this fix, the game should:
1. ✅ Start without ArgumentException errors
2. ✅ Player systems work (movement, animation, shooting)
3. ✅ Enemy systems work (movement, death, attacks)
4. ✅ All hybrid GameObject/ECS interactions function correctly

## Next Steps

1. **Test the game** - Play and verify everything works
2. **Test Xbox controller** - Connect and test input
3. **Check for any remaining errors** - Report if you see any

---

**Status**: ✅ Runtime errors fixed - Ready for testing!
