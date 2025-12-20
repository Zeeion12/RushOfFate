using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private float invincibilityDuration = 1f;

    [Header("Death Settings")]
    [SerializeField] private float deathDelay = 2f;

    [Header("Respawn Settings")]
    [SerializeField] private float respawnInvincibilityDuration = 2f;

    [Header("Events")]
    public UnityEvent<int> OnHealthChanged;
    public UnityEvent OnDeath;
    public UnityEvent OnRespawn;

    // State
    private int currentHealth;
    private bool isInvincible = false;
    private bool isDead = false;

    // Components
    private Animator animator;
    private PlayerMovement movementScript;
    private SpriteRenderer spriteRenderer;

    // ✅ NEW: Reference ke TimerManager
    private TimerManager timerManager;

    // Properties
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsInvincible => isInvincible;
    public bool IsDead => isDead;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();
        movementScript = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // ✅ NEW: Get TimerManager reference
        timerManager = FindObjectOfType<TimerManager>();

        if (timerManager == null)
        {
            Debug.LogWarning("[PlayerHealth] TimerManager not found in scene!");
        }

        OnHealthChanged?.Invoke(currentHealth);
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible || isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"Player took {damage} damage. Health: {currentHealth}/{maxHealth}");
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
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
        OnHealthChanged?.Invoke(currentHealth);
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Player died!");

        // ✅ NEW: Pause timer saat player mati
        if (timerManager != null)
        {
            timerManager.PauseOnDeath();
        }

        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        if (movementScript != null)
        {
            movementScript.enabled = false;
        }

        OnDeath?.Invoke();
        StartCoroutine(HandleDeath());
    }

    System.Collections.IEnumerator HandleDeath()
    {
        yield return new WaitForSeconds(deathDelay);

        CheckpointManager checkpoint = GetComponent<CheckpointManager>();

        if (checkpoint != null)
        {
            currentHealth = maxHealth;
            isDead = false;
            OnHealthChanged?.Invoke(currentHealth);
            checkpoint.RespawnPlayer();

            if (movementScript != null)
            {
                movementScript.enabled = true;
            }

            if (animator != null)
            {
                animator.Play("Idle");
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }

            StartCoroutine(RespawnInvincibilityCoroutine(respawnInvincibilityDuration));
            OnRespawn?.Invoke();

            // ✅ NEW: Resume timer setelah respawn
            if (timerManager != null)
            {
                timerManager.ResumeOnRespawn();
            }

            Debug.Log("Player respawned at checkpoint!");
        }
        else
        {
            Debug.LogWarning("GAME OVER - No CheckpointManager found!");
        }
    }

    System.Collections.IEnumerator RespawnInvincibilityCoroutine(float duration)
    {
        isInvincible = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        isInvincible = false;
    }
}