using UnityEngine;

public class SphereEnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int startingHealth = 50; // Lower health for easy testing
    public int currentHealth;
    public int scoreValue = 15;
    public AudioClip deathClip;
    
    [Header("Death Effects")]
    public float sinkSpeed = 2.5f;
    public GameObject deathParticles;
    
    private AudioSource enemyAudio;
    private SphereCollider sphereCollider;
    private Renderer enemyRenderer;
    private bool isDead;
    private bool isSinking;
    private Color originalColor;
    
    void Awake()
    {
        enemyAudio = GetComponent<AudioSource>();
        sphereCollider = GetComponent<SphereCollider>();
        enemyRenderer = GetComponent<Renderer>();
        
        currentHealth = startingHealth;
        
        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;
        }
    }
    
    void Update()
    {
        if (isSinking)
        {
            transform.Translate(-Vector3.up * sinkSpeed * Time.deltaTime);
        }
    }
    
    public void TakeDamage(int amount, Vector3 hitPoint)
    {
        if (isDead)
            return;
        
        // Play hit sound
        if (enemyAudio != null)
        {
            enemyAudio.Play();
        }
        
        currentHealth -= amount;
        
        // Visual feedback - flash red
        if (enemyRenderer != null)
        {
            StartCoroutine(FlashRed());
        }
        
        // Create hit particles at hit point
        if (deathParticles != null)
        {
            GameObject particles = Instantiate(deathParticles, hitPoint, Quaternion.identity);
            Destroy(particles, 2f);
        }
        
        if (currentHealth <= 0)
        {
            Death();
        }
    }
    
    private System.Collections.IEnumerator FlashRed()
    {
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            enemyRenderer.material.color = originalColor;
        }
    }
    
    void Death()
    {
        isDead = true;
        
        // Disable collider for attacks but keep as trigger for cleanup
        if (sphereCollider != null)
        {
            sphereCollider.isTrigger = true;
        }
        
        // Disable AI components
        SphereEnemyAI ai = GetComponent<SphereEnemyAI>();
        if (ai != null)
        {
            ai.enabled = false;
        }
        
        // Play death sound
        if (enemyAudio != null && deathClip != null)
        {
            enemyAudio.clip = deathClip;
            enemyAudio.Play();
        }
        
        // Start sinking
        StartSinking();
    }
    
    public void StartSinking()
    {
        // Disable NavMesh agent
        UnityEngine.AI.NavMeshAgent nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (nav != null)
        {
            nav.enabled = false;
        }
        
        // Make rigidbody kinematic
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        
        isSinking = true;
        
        // Add score (uncomment when ScoreManager is available)
        // ScoreManager.score += scoreValue;
        
        // Destroy after sinking
        Destroy(gameObject, 3f);
    }
    
    // Public getter for other scripts
    public bool IsDead()
    {
        return isDead;
    }
}