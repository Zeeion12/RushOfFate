using UnityEngine;

public class CanineHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 2;
    [SerializeField] private float invincibilityDuration = 0.2f;
    [SerializeField] private float knockbackForce = 5f; // ✅ Naikin jadi 5
    [SerializeField] private float knockbackDuration = 0.2f; // ✅ BARU - durasi knockback

    // State
    private int currentHealth;
    private bool isInvincible = false;
    private bool isDead = false;
    private bool isKnockedBack = false; // ✅ BARU - flag knockback

    // Components
    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;
    private CanineAI aiScript; // ✅ BARU - reference AI

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        aiScript = GetComponent<CanineAI>(); // ✅ BARU
    }

    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        if (isInvincible || isDead) return;

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");

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

            Vector2 knockbackDirection = ((Vector2)transform.position - attackerPosition).normalized;
            ApplyKnockback(knockbackDirection);

            StartCoroutine(InvincibilityCoroutine());
        }
    }

    void ApplyKnockback(Vector2 direction)
    {
        if (rb != null)
        {
            // ✅ Disable AI sementara biar ga override velocity
            if (aiScript != null)
            {
                aiScript.enabled = false;
            }

            isKnockedBack = true;

            // Apply knockback force
            rb.linearVelocity = new Vector2(direction.x * knockbackForce, rb.linearVelocity.y);

            StartCoroutine(KnockbackCoroutine());
        }
    }

    // ✅ GANTI dari StopKnockbackAfterDelay jadi KnockbackCoroutine
    System.Collections.IEnumerator KnockbackCoroutine()
    {
        yield return new WaitForSeconds(knockbackDuration);

        // ✅ Re-enable AI setelah knockback selesai
        if (aiScript != null)
        {
            aiScript.enabled = true;
        }

        isKnockedBack = false;
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        currentHealth = 0;

        Debug.Log($"{gameObject.name} died!");

        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (col != null)
        {
            col.enabled = false;
        }

        if (aiScript != null)
        {
            aiScript.enabled = false;
        }

        CanineAttack attackScript = GetComponent<CanineAttack>();
        if (attackScript != null)
        {
            attackScript.enabled = false;
        }

        StartCoroutine(DisableAfterDeath());
    }

    System.Collections.IEnumerator DisableAfterDeath()
    {
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }

    System.Collections.IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    // Public getters
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public bool IsAlive() => !isDead && currentHealth > 0;
    public bool IsInvincible() => isInvincible;
    public bool IsKnockedBack() => isKnockedBack; // ✅ BARU
}