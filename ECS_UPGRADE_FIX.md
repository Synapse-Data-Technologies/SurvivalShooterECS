# ECS Upgrade Fix - Collections Package Error

## Problem

The error `CreateProperty could not be found` is caused by a version incompatibility between:
- Unity 2021.3.45f2
- ECS 1.0.16
- Collections 2.1.4

This is a known issue with certain package combinations.

## Solution Applied

I've changed the ECS version to **1.0.10** which is the most stable version for Unity 2021.3.

**Updated manifest.json:**
```json
"com.unity.entities": "1.0.10"
```

## Steps to Fix

### Option 1: Let Unity Resolve (Recommended)

1. **Save your work** in Unity
2. Unity should automatically detect the manifest change
3. Wait for it to re-download packages (2-3 minutes)
4. Let it recompile

### Option 2: Clean Reimport (If Option 1 doesn't work)

1. **Close Unity completely**
2. Run the `fix_ecs_upgrade.bat` script (or manually delete Library and Temp folders)
3. **Reopen Unity**
4. Wait for packages to download (3-5 minutes)
5. Wait for compilation

### Option 3: Manual Clean (If you don't want to use the script)

1. **Close Unity**
2. Delete these folders:
   - `Library/`
   - `Temp/`
3. **Reopen Unity**
4. Wait for reimport

## What Changed

**From:** ECS 0.51.1-preview.21 (old preview)
**To:** ECS 1.0.10 (stable, Unity 2021.3 compatible)

## ECS 1.0.10 Features

Even though it's not the absolute latest (1.0.16), ECS 1.0.10 still gives you:
- ✅ `LocalTransform` component (modern API)
- ✅ `GetExistingSystemManaged` / `CreateSystemManaged`
- ✅ Stable, production-ready
- ✅ All the features we need for the debug system
- ✅ Better performance than 0.51

## After Unity Finishes

Once Unity finishes downloading and compiling, you may still see some errors in your existing game code. These will be in files like:
- `PlayerMovementSystem.cs`
- `PlayerAnimationSystem.cs`
- Any other systems using old transform components

**Tell me what errors you see** and I'll fix them immediately.

## If This Still Doesn't Work

If you still get the Collections error after trying the above:

### Nuclear Option: Complete Reset
1. Close Unity
2. Delete: `Library/`, `Temp/`, `Packages/` folders
3. Reopen Unity
4. Unity will re-download everything fresh

### Rollback Option: Go Back to 0.51
If the upgrade is causing too many issues, we can rollback:
1. Change manifest.json back to: `"com.unity.entities": "0.51.1-preview.21"`
2. Delete Library folder
3. Reopen Unity
4. Use the old ECS API (I'll update the debug system to work with it)

## Current Status

**Manifest updated to ECS 1.0.10**

**Next step**: 
- If Unity is open, it should auto-detect the change
- If not, follow Option 2 or 3 above
- Then tell me what happens!
