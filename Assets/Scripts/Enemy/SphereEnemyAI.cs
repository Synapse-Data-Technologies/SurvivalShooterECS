using UnityEngine;

public class SphereEnemyAI : MonoBehaviour
{
    [Header("AI Configuration")]
    public string aiVersion = "1.0";
    public AIMode currentMode = AIMode.Patrol;
    
    [Header("AI Settings")]
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float moveSpeed = 3f;
    public float patrolRadius = 5f;
    
    public enum AIMode
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Retreat
    }
    
    private Transform player;
    private Vector3 startPosition;
    private Vector3 patrolTarget;
    private float lastModeChange;
    private UnityEngine.AI.NavMeshAgent navAgent;
    private SphereEnemyDebugUI debugUI;
    
    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        debugUI = GetComponent<SphereEnemyDebugUI>();
        startPosition = transform.position;
        
        if (navAgent != null)
        {
            navAgent.speed = moveSpeed;
        }
        
        SetNewPatrolTarget();
        
        // Debug info
        if (player == null)
        {
            Debug.LogWarning($"{gameObject.name}: No Player found with 'Player' tag!");
        }
        
        // Check NavMesh
        CheckNavMeshStatus();
    }
    
    void CheckNavMeshStatus()
    {
        if (navAgent == null) return;
        
        // Wait a frame for NavMesh to initialize
        Invoke(nameof(DelayedNavMeshCheck), 0.1f);
    }
    
    void DelayedNavMeshCheck()
    {
        if (navAgent != null && !navAgent.isOnNavMesh)
        {
            Debug.LogWarning($"{gameObject.name}: Not on NavMesh! Using fallback movement. To fix: Window > AI > Navigation > Bake");
            // Disable NavMeshAgent and use simple movement as fallback
            navAgent.enabled = false;
        }
        else
        {
            Debug.Log($"{gameObject.name}: NavMesh detected, using NavMeshAgent movement");
        }
    }
    
    void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        AIMode previousMode = currentMode;
        
        // AI State Machine
        switch (currentMode)
        {
            case AIMode.Idle:
                HandleIdleMode(distanceToPlayer);
                break;
            case AIMode.Patrol:
                HandlePatrolMode(distanceToPlayer);
                break;
            case AIMode.Chase:
                HandleChaseMode(distanceToPlayer);
                break;
            case AIMode.Attack:
                HandleAttackMode(distanceToPlayer);
                break;
            case AIMode.Retreat:
                HandleRetreatMode(distanceToPlayer);
                break;
        }
        
        // Update debug UI if mode changed
        if (previousMode != currentMode)
        {
            lastModeChange = Time.time;
            if (debugUI != null)
            {
                debugUI.UpdateDebugInfo(aiVersion, currentMode.ToString());
            }
        }
    }
    
    private void MoveTowardsTarget(Vector3 target)
    {
        // Simple movement without NavMesh
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0; // Keep on ground level
        
        if (direction.magnitude > 0.1f)
        {
            // Rotate towards target
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 180f * Time.deltaTime);
            
            // Move forward
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }
    }
    
    private void HandleIdleMode(float distanceToPlayer)
    {
        if (distanceToPlayer <= detectionRange)
        {
            currentMode = AIMode.Chase;
        }
        else if (Time.time - lastModeChange > 2f)
        {
            currentMode = AIMode.Patrol;
        }
    }
    
    private void HandlePatrolMode(float distanceToPlayer)
    {
        if (distanceToPlayer <= detectionRange)
        {
            currentMode = AIMode.Chase;
            return;
        }
        
        if (navAgent != null && navAgent.enabled)
        {
            if (!navAgent.pathPending && navAgent.remainingDistance < 0.5f)
            {
                SetNewPatrolTarget();
            }
            navAgent.SetDestination(patrolTarget);
        }
        else
        {
            // Fallback movement without NavMesh
            MoveTowardsTarget(patrolTarget);
            if (Vector3.Distance(transform.position, patrolTarget) < 1f)
            {
                SetNewPatrolTarget();
            }
        }
    }
    
    private void HandleChaseMode(float distanceToPlayer)
    {
        if (distanceToPlayer > detectionRange * 1.5f)
        {
            currentMode = AIMode.Patrol;
            return;
        }
        
        if (distanceToPlayer <= attackRange)
        {
            currentMode = AIMode.Attack;
            return;
        }
        
        if (navAgent != null && navAgent.enabled)
        {
            navAgent.SetDestination(player.position);
        }
        else if (player != null)
        {
            // Fallback movement without NavMesh
            MoveTowardsTarget(player.position);
        }
    }
    
    private void HandleAttackMode(float distanceToPlayer)
    {
        if (distanceToPlayer > attackRange * 1.2f)
        {
            currentMode = AIMode.Chase;
            return;
        }
        
        // Stop moving when attacking
        if (navAgent != null && navAgent.enabled)
        {
            navAgent.SetDestination(transform.position);
        }
        
        // Simple attack logic - could be expanded
        if (Time.time - lastModeChange > 1f)
        {
            currentMode = AIMode.Retreat;
        }
    }
    
    private void HandleRetreatMode(float distanceToPlayer)
    {
        if (distanceToPlayer > detectionRange)
        {
            currentMode = AIMode.Patrol;
            return;
        }
        
        // Move away from player
        Vector3 retreatDirection = (transform.position - player.position).normalized;
        Vector3 retreatTarget = transform.position + retreatDirection * 5f;
        
        if (navAgent != null && navAgent.enabled)
        {
            navAgent.SetDestination(retreatTarget);
        }
        else
        {
            // Fallback movement without NavMesh
            MoveTowardsTarget(retreatTarget);
        }
        
        if (Time.time - lastModeChange > 3f)
        {
            currentMode = AIMode.Patrol;
        }
    }
    
    private void SetNewPatrolTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += startPosition;
        randomDirection.y = startPosition.y; // Keep same height
        
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, 1))
        {
            patrolTarget = hit.position;
        }
        else
        {
            patrolTarget = randomDirection; // Use the random position even without NavMesh
        }
    }
    
    // Public methods for external control/testing
    public void SetAIMode(AIMode newMode)
    {
        currentMode = newMode;
        lastModeChange = Time.time;
    }
    
    public void SetAIVersion(string version)
    {
        aiVersion = version;
        if (debugUI != null)
        {
            debugUI.UpdateDebugInfo(aiVersion, currentMode.ToString());
        }
    }
}