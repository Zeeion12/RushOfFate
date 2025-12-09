using UnityEngine;

public class FlyingEnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float invincibilityDuration = 0.2f;
    [SerializeField] private float knockbackForce = 3f;

    // State
    private int currentHealth;
    private bool isInvincible = false;
    private bool isDead = false;

    // Components
    private Animator animator;
    private Collider2D col;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();
        col = GetComponent<Collider2D>();
    }

    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        // Tidak bisa kena damage kalau invincible atau sudah mati
        if (isInvincible || isDead) return;

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");

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

            // Apply knockback (flying enemy ringan, knockback lebih smooth)
            Vector2 knockbackDirection = ((Vector2)transform.position - attackerPosition).normalized;
            ApplyKnockback(knockbackDirection);

            // Start invincibility
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    void ApplyKnockback(Vector2 direction)
    {
        // Knockback untuk flying enemy: push away smoothly
        // Karena kinematic, pakai transform langsung
        StartCoroutine(KnockbackCoroutine(direction));
    }

    System.Collections.IEnumerator KnockbackCoroutine(Vector2 direction)
    {
        float elapsed = 0f;
        Vector2 startPos = transform.position;
        Vector2 targetPos = startPos + direction * knockbackForce;

        while (elapsed < invincibilityDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / invincibilityDuration;

            // Smooth knockback dengan ease-out
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);
            transform.position = Vector2.Lerp(startPos, targetPos, smoothT);

            yield return null;
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        currentHealth = 0;

        Debug.Log($"{gameObject.name} died!");

        // Trigger death animation
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        // Disable collider agar tidak bisa diserang lagi
        if (col != null)
        {
            col.enabled = false;
        }

        // Disable scripts
        FlyingEnemyAI aiScript = GetComponent<FlyingEnemyAI>();
        if (aiScript != null)
        {
            aiScript.enabled = false;
        }

        FlyingEnemyAttack attackScript = GetComponent<FlyingEnemyAttack>();
        if (attackScript != null)
        {
            attackScript.enabled = false;
        }

        // Wait untuk death animation selesai, lalu disable GameObject
        StartCoroutine(DisableAfterDeath());
    }

    System.Collections.IEnumerator DisableAfterDeath()
    {
        // Sesuaikan durasi dengan panjang animasi death (default 1 detik)
        yield return new WaitForSeconds(1f);

        // Disable GameObject
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
}