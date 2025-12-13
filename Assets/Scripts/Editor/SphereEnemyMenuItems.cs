using UnityEngine;
using UnityEditor;

public class SphereEnemyMenuItems
{
    [MenuItem("GameObject/Sphere Enemy/Create Sphere Enemy", false, 10)]
    static void CreateSphereEnemy()
    {
        // Create sphere primitive
        GameObject sphereEnemy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphereEnemy.name = "SphereEnemy";
        sphereEnemy.tag = "Enemy";
        
        // Position it
        sphereEnemy.transform.position = Vector3.up * 0.5f;
        
        // Add required components
        sphereEnemy.AddComponent<Rigidbody>();
        sphereEnemy.AddComponent<UnityEngine.AI.NavMeshAgent>();
        sphereEnemy.AddComponent<AudioSource>();
        
        // Add custom components
        sphereEnemy.AddComponent<SphereEnemyAI>();
        sphereEnemy.AddComponent<SphereEnemyHealth>();
        sphereEnemy.AddComponent<SphereEnemyDebugUI>();
        
        // Configure NavMeshAgent
        UnityEngine.AI.NavMeshAgent navAgent = sphereEnemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.speed = 3f;
            navAgent.stoppingDistance = 1.5f;
            navAgent.acceleration = 8f;
        }
        
        // Configure Rigidbody
        Rigidbody rb = sphereEnemy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.mass = 1f;
            rb.linearDamping = 1f;
            rb.angularDamping = 5f;
        }
        
        // Set material color to red
        Renderer renderer = sphereEnemy.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            renderer.material.color = Color.red;
        }
        
        // Select the created object
        Selection.activeGameObject = sphereEnemy;
        
        Debug.Log("Sphere Enemy created successfully!");
    }
    
    [MenuItem("GameObject/Sphere Enemy/Setup Selected as Sphere Enemy", false, 11)]
    static void SetupSelectedAsSphereEnemy()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogWarning("No GameObject selected!");
            return;
        }
        
        // Add custom components if not present
        if (selected.GetComponent<SphereEnemyAI>() == null)
            selected.AddComponent<SphereEnemyAI>();
            
        if (selected.GetComponent<SphereEnemyHealth>() == null)
            selected.AddComponent<SphereEnemyHealth>();
            
        if (selected.GetComponent<SphereEnemyDebugUI>() == null)
            selected.AddComponent<SphereEnemyDebugUI>();
        
        // Ensure required components
        if (selected.GetComponent<Rigidbody>() == null)
            selected.AddComponent<Rigidbody>();
            
        if (selected.GetComponent<UnityEngine.AI.NavMeshAgent>() == null)
            selected.AddComponent<UnityEngine.AI.NavMeshAgent>();
            
        if (selected.GetComponent<AudioSource>() == null)
            selected.AddComponent<AudioSource>();
        
        // Set tag
        selected.tag = "Enemy";
        
        Debug.Log($"Setup {selected.name} as Sphere Enemy!");
    }
    
    [MenuItem("GameObject/Sphere Enemy/Setup Selected as Sphere Enemy", true)]
    static bool ValidateSetupSelectedAsSphereEnemy()
    {
        return Selection.activeGameObject != null;
    }
}