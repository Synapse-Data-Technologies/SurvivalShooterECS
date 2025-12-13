using UnityEngine;

[System.Serializable]
public class SphereEnemySetup : MonoBehaviour
{
    [Header("Setup Configuration")]
    public bool autoSetup = true;
    
    void Start()
    {
        if (autoSetup)
        {
            SetupSphereEnemy();
        }
    }
    
    [ContextMenu("Setup Sphere Enemy")]
    public void SetupSphereEnemy()
    {
        // Add AI component if not present
        if (GetComponent<SphereEnemyAI>() == null)
        {
            gameObject.AddComponent<SphereEnemyAI>();
        }
        
        // Add Health component if not present
        if (GetComponent<SphereEnemyHealth>() == null)
        {
            gameObject.AddComponent<SphereEnemyHealth>();
        }
        
        // Add Debug UI component if not present
        if (GetComponent<SphereEnemyDebugUI>() == null)
        {
            gameObject.AddComponent<SphereEnemyDebugUI>();
        }
        
        // Configure NavMeshAgent
        UnityEngine.AI.NavMeshAgent navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.speed = 3f;
            navAgent.stoppingDistance = 1.5f;
            navAgent.acceleration = 8f;
        }
        
        // Configure Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.mass = 1f;
            rb.linearDamping = 1f;
            rb.angularDamping = 5f;
            rb.useGravity = true;
            rb.isKinematic = false;
        }
        
        // Configure Collider
        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null)
        {
            col.isTrigger = false;
            col.radius = 0.5f;
        }
        
        // Set material color to distinguish from other objects
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            renderer.material.color = Color.red;
        }
        
        Debug.Log("Sphere Enemy setup completed!");
    }
}