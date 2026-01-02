using UnityEngine;

public class HarpyHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 6; // Medium HP (less than Golem, more than Bandit/Canine)
    [SerializeField] private float invincibilityDuration = 0.2f;
    [SerializeField] private float knockbackForce = 3f; // Light weight, more knockback

    // State
    private int currentHealth;
    private bool isInvincible = false;
    private bool isDead = false;

    // Components
    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;

    // DropManager
    private DropManager dropManager;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        dropManager = GetComponent<DropManager>();

        if (dropManager == null)
        {
            Debug.LogWarning($"{gameObject.name}: DropManager component not found! Items will not drop.");
        }
    }

    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        // Cannot take damage if invincible or dead
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
            // Apply knockback force (flying enemy, so knockback in both X and Y)
            rb.linearVelocity = new Vector2(direction.x * knockbackForce, direction.y * knockbackForce * 0.5f);

            // Stop knockback after invincibility duration
            StartCoroutine(StopKnockbackAfterDelay());
        }
    }

    System.Collections.IEnumerator StopKnockbackAfterDelay()
    {
        yield return new WaitForSeconds(invincibilityDuration);

        // Enemy can move again after knockback
        // Velocity will be handled by HarpyAI script
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        currentHealth = 0;

        Debug.Log($"{gameObject.name} died!");

        // Try drop item before death animation
        if (dropManager != null)
        {
            dropManager.TryDropItem();
        }

        // Trigger death animation
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        // Disable collider so it can't be attacked again
        if (col != null)
        {
            col.enabled = false;
        }

        // Disable scripts
        HarpyAI aiScript = GetComponent<HarpyAI>();
        if (aiScript != null)
        {
            aiScript.enabled = false;
        }

        HarpyAttack attackScript = GetComponent<HarpyAttack>();
        if (attackScript != null)
        {
            attackScript.enabled = false;
        }

        // Wait for death animation to finish, then disable GameObject
        StartCoroutine(DisableAfterDeath());
    }

    System.Collections.IEnumerator DisableAfterDeath()
    {
        // Adjust duration to match death animation length (default 1 second for flying enemy)
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
