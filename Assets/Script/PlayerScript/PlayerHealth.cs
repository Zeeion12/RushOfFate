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

    [Header("Audio")]
    [SerializeField] private AudioSource healthAudioSource;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField][Range(0f, 1f)] private float hurtVolume = 1f;
    [SerializeField][Range(0f, 1f)] private float deathVolume = 1f;

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

        // Get or create audio source
        if (healthAudioSource == null)
        {
            AudioSource[] audioSources = GetComponents<AudioSource>();
            // Use the fourth AudioSource if available (first three are for movement and attack)
            if (audioSources.Length >= 4)
            {
                healthAudioSource = audioSources[3];
            }
            else
            {
                healthAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

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
            // Play hurt sound
            if (healthAudioSource != null && hurtSound != null)
            {
                healthAudioSource.PlayOneShot(hurtSound, hurtVolume);
            }

            if (animator != null)
            {
                animator.SetTrigger("Hurt");
            }

            // Start invincibility frames after taking damage
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    System.Collections.IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
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

        // Play death sound
        if (healthAudioSource != null && deathSound != null)
        {
            healthAudioSource.PlayOneShot(deathSound, deathVolume);
        }

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