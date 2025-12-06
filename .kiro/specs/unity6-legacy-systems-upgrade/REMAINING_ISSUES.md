# Remaining Issues for Performance Lab

## Issue 1: Floor Not Visible ❌

**Problem:** The Floor GameObject has no MeshRenderer component, so it's invisible.

**Manual Fix Required:**
1. Select the **Floor** GameObject in Perf_Lab scene
2. Click **Add Component** → **Mesh Renderer**
3. Assign a material:
   - Option A: Use existing material (Assets/Materials/WallMaterial.mat or PlanksMaterial.mat)
   - Option B: Create new high-contrast material with white color

**Why Manual:** Unity MCP tools have limitations with component manipulation on existing GameObjects.

---

## Issue 2: Enemies Not Moving ❌

**Problem:** Enemies spawn but stand still at spawn points.

**Root Cause:** Enemy AI systems still use deprecated Unity ECS patterns:
- `EnemyMovementSystem` - Uses SystemBase + ForEach (needs ISystem upgrade)
- `EnemyAttackSystem` - Likely also needs upgrading
- Other enemy behavior systems

**These systems were NOT part of the current spec** (unity6-legacy-systems-upgrade), which focused on:
- ✅ CameraFollowSystem (completed)
- ✅ EnemySpawnSystem (completed)  
- ✅ Performance lab scene (completed)

**Solution Options:**

### Option A: Create New Spec for Enemy AI Upgrade
Create a new spec: `enemy-ai-unity6-upgrade` with requirements for:
1. Upgrade EnemyMovementSystem to ISystem
2. Upgrade EnemyAttackSystem to ISystem
3. Implement modular AI plugin architecture (as you mentioned)
4. Test in performance lab

### Option B: Quick Temporary Fix
Add a simple MonoBehaviour script to enemies that moves them toward player:
```csharp
public class TempEnemyAI : MonoBehaviour
{
    public float speed = 3f;
    private Transform player;
    
    void Start()
    {
        player = FindObjectOfType<PlayerObject>()?.transform;
    }
    
    void Update()
    {
        if (player != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, 
                player.position, 
                speed * Time.deltaTime);
        }
    }
}
```

### Option C: Continue Current Workflow
1. Complete current spec tasks
2. Test what IS working (spawning, camera, performance metrics)
3. Create enemy AI spec as next phase

---

## Recommendation

I recommend **Option A** - create a proper spec for enemy AI upgrade. This follows the same workflow we used for the camera/spawn systems and ensures:
- Proper requirements gathering
- Design with correctness properties
- Structured implementation plan
- Property-based testing

The performance lab can still be used to test:
- ✅ Player movement
- ✅ Player shooting
- ✅ Camera following
- ✅ Enemy spawning
- ✅ Performance metrics
- ✅ Debug visualization

Enemy movement would be the next feature to add.

---

## Current Task Status

**Task 5: Upgrade EnemySpawnSystem** ✅ COMPLETE
- Enemies spawn correctly at timed intervals
- Spawn system uses modern ISystem interface
- All requirements met

**Task 7: Create performance_lab scene** ✅ COMPLETE  
- Scene created with minimal setup
- Debug tools added
- Ready for testing

**Next Steps:**
1. Fix floor visibility (manual - 2 minutes)
2. Decide on enemy AI approach
3. Continue with remaining tasks or create new spec
