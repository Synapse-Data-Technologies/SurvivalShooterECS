# Unity 6 Upgrade - Next Steps

## ✅ What's Done

1. **Unity 6 Installed** - Project opened in Unity 6
2. **DOTS Packages Updated** - Manifest aligned to Unity 6 baseline:
   - Entities: 1.4.3
   - Collections: 2.6.3
   - Entities Graphics: 1.4.16

## 🔄 What You Need to Do NOW

### Step 1: Clean Reimport (CRITICAL)

**Option A: Use the Script**
1. Close Unity completely
2. Run `fix_ecs_upgrade.bat`
3. Reopen Unity 6
4. Wait 5-10 minutes

**Option B: Manual**
1. Close Unity completely
2. Delete `Library/` folder from project directory
3. Delete `Temp/` folder from project directory
4. Reopen Unity 6
5. Wait 5-10 minutes for full reimport

**Why This is Critical:**
- Forces DOTS codegen to regenerate all `.gen.cs` files
- Ensures compatibility with Unity 6 property system
- Fixes the Collections/CreateProperty errors

### Step 2: Check for Errors

After reimport completes, check Unity Console:

**If you see errors:**
- Copy the error messages
- Tell me which files have errors
- I'll fix them immediately

**If no errors:**
- ✅ Proceed to Step 3

### Step 3: Test Core Functionality

Test these in Play mode:

- [ ] Game starts without errors
- [ ] Player moves with WASD
- [ ] Player aims with mouse
- [ ] Player shoots with mouse button
- [ ] Xbox controller connects
- [ ] Player moves with left stick
- [ ] Player aims with right stick
- [ ] Player shoots with right trigger
- [ ] Input mode switching works

### Step 4: Enable Debug Visualization

1. Create empty GameObject in scene
2. Add `AimDebugVisualizer` component
3. Play with Xbox controller
4. Verify debug UI shows aim angle

## 📚 Documentation Created

1. **UNITY6_UPGRADE_STATUS.md** - Complete upgrade status and validation checklist
2. **ENTITIES_1.0_TO_1.4_CHANGES.md** - API changes reference
3. **fix_ecs_upgrade.bat** - Clean reimport script
4. **This file** - Quick next steps

## 🎯 Expected Timeline

- **Clean Reimport**: 5-10 minutes
- **Fix Errors** (if any): 15-30 minutes
- **Testing**: 15 minutes
- **Total**: ~30-60 minutes

## 🚨 Troubleshooting

### If Reimport Takes Forever
- Check Unity Hub isn't downloading other packages
- Check disk space (need ~5GB free)
- Check antivirus isn't blocking Unity

### If You See Compilation Errors
Common errors and fixes:

**Error: `Translation` not found**
- Fix: Replace with `LocalTransform.Position`

**Error: `Rotation` not found**
- Fix: Replace with `LocalTransform.Rotation`

**Error: `GetExistingSystem` not found**
- Fix: Already fixed in AimDebugVisualizer

**Error: Hybrid component access**
- Fix: Ensure `WithoutBurst()` is used

### If Game Doesn't Work After Upgrade
1. Check Console for runtime errors
2. Verify all systems are enabled
3. Check input bindings still exist
4. Test with keyboard/mouse first, then controller

## 💡 What I'll Do After Reimport

Once you tell me the reimport is complete:

1. **If there are errors**: I'll fix them file by file
2. **If no errors**: I'll help you test and verify everything works
3. **Then**: We can finish the Xbox controller implementation
4. **Finally**: Enable the aim debug visualization

## 📊 Current Status

**Status**: ⏳ Waiting for you to do clean reimport

**Your Action**: 
1. Close Unity
2. Run `fix_ecs_upgrade.bat` (or manually delete Library/Temp)
3. Reopen Unity 6
4. Wait for reimport
5. Tell me the result (errors or success)

---

**Ready?** Close Unity and run the clean reimport script!
