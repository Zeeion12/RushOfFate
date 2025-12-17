using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private float invincibilityDuration = 1f; // Setelah terkena damage

    [Header("Death Settings")]
    [SerializeField] private float deathDelay = 2f; // Delay sebelum respawn/game over

    [Header("Respawn Settings")]
    [SerializeField] private float respawnInvincibilityDuration = 2f; // I-frames setelah respawn

    [Header("Events")]
    public UnityEvent<int> OnHealthChanged; // Event untuk update UI
    public UnityEvent OnDeath;
    public UnityEvent OnRespawn; // Event saat respawn

    // State
    private int currentHealth;
    private bool isInvincible = false;
    private bool isDead = false;

    // Components
    private Animator animator;
    private PlayerMovement movementScript;
    private SpriteRenderer spriteRenderer;

    // Properties (untuk diakses script lain)
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsInvincible => isInvincible;
    public bool IsDead => isDead;

    void Start()
    {
        // Initialize
        currentHealth = maxHealth;

        // Get components
        animator = GetComponentInChildren<Animator>();
        movementScript = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Trigger initial UI update
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void TakeDamage(int damage)
    {
        // Prevent damage jika invincible atau sudah mati
        if (isInvincible || isDead) return;

        // Kurangi health
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"Player took {damage} damage. Health: {currentHealth}/{maxHealth}");

        // Trigger UI update
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Trigger hurt animation
            if (animator != null)
            {
                animator.SetTrigger("Hurt");
            }
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"Player healed {amount}. Health: {currentHealth}/{maxHealth}");

        // Trigger UI update
        OnHealthChanged?.Invoke(currentHealth);
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        Debug.Log("Player died!");

        // Trigger death animation
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        // Disable movement
        if (movementScript != null)
        {
            movementScript.enabled = false;
        }

        // Trigger death event
        OnDeath?.Invoke();

        // Handle death (respawn, game over, etc.)
        StartCoroutine(HandleDeath());
    }

    System.Collections.IEnumerator HandleDeath()
    {
        // Wait for death animation to play
        yield return new WaitForSeconds(deathDelay);

        // Try to respawn using checkpoint system
        CheckpointManager checkpoint = GetComponent<CheckpointManager>();

        if (checkpoint != null)
        {
            // === RESPAWN SEQUENCE ===

            // 1. Restore health
            currentHealth = maxHealth;
            isDead = false;

            // 2. Update UI
            OnHealthChanged?.Invoke(currentHealth);

            // 3. Teleport to checkpoint
            checkpoint.RespawnPlayer();

            // 4. Re-enable movement
            if (movementScript != null)
            {
                movementScript.enabled = true;
            }

            // 5. FORCE IDLE STATE (Keluar dari Death state)
            if (animator != null)
            {
                animator.Play("Idle");

                Debug.Log("Forced to Idle state after respawn!");
            }

            // 6. Ensure sprite visible
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }

            // 7. Give temporary invincibility setelah respawn
            StartCoroutine(RespawnInvincibilityCoroutine(respawnInvincibilityDuration));

            // 8. Trigger respawn event
            OnRespawn?.Invoke();

            Debug.Log("Player respawned at checkpoint!");
        }
        else
        {
            // === NO CHECKPOINT = GAME OVER ===
            Debug.LogWarning("GAME OVER - No CheckpointManager found!");

            // Reload current scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }
    }

    /// <summary>
    /// I-frames setelah respawn agar player tidak langsung mati lagi
    /// </summary>
    System.Collections.IEnumerator RespawnInvincibilityCoroutine(float duration)
    {
        isInvincible = true;

        // Visual feedback: slower blinking
        float blinkInterval = 0.15f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
            elapsedTime += blinkInterval;
        }

        // Ensure sprite visible
        spriteRenderer.enabled = true;
        isInvincible = false;

        Debug.Log("Respawn invincibility ended!");
    }
}