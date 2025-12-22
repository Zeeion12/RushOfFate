using UnityEngine;

public class BanditArcherHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 6;
    [SerializeField] private float invincibilityDuration = 0.2f;
    [SerializeField] private float knockbackForce = 3f;

    // State
    private int currentHealth;
    private bool isInvincible = false;
    private bool isDead = false;

    // Components
    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;

    // ✅ NEW: Reference untuk DropManager
    private DropManager dropManager;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // ✅ NEW: Get DropManager component (harus ditambahkan ke enemy prefab)
        dropManager = GetComponent<DropManager>();

        if (dropManager == null)
        {
            Debug.LogWarning($"{gameObject.name}: DropManager component not found! Items will not drop.");
        }
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

            // Apply knockback
            Vector2 knockbackDirection = ((Vector2)transform.position - attackerPosition).normalized;
            ApplyKnockback(knockbackDirection);

            // Start invincibility
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    void ApplyKnockback(Vector2 direction)
    {
        if (rb != null)
        {
            // Apply knockback force
            rb.linearVelocity = new Vector2(direction.x * knockbackForce, rb.linearVelocity.y);

            // Stop knockback setelah durasi invincibility
            StartCoroutine(StopKnockbackAfterDelay());
        }
    }

    System.Collections.IEnumerator StopKnockbackAfterDelay()
    {
        yield return new WaitForSeconds(invincibilityDuration);

        // Enemy bisa gerak lagi setelah knockback
        // Velocity akan di-handle oleh BanditAI script
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        currentHealth = 0;

        Debug.Log($"{gameObject.name} died!");

        // ✅ NEW: Try drop item sebelum animasi death
        if (dropManager != null)
        {
            dropManager.TryDropItem();
        }

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
        BanditAI aiScript = GetComponent<BanditAI>();
        if (aiScript != null)
        {
            aiScript.enabled = false;
        }

        BanditAttack attackScript = GetComponent<BanditAttack>();
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