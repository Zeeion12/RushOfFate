using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    [Header("Arrow Settings")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 3f; // Destroy arrow setelah X detik jika tidak hit anything

    [Header("Knockback (Optional)")]
    [SerializeField] private bool applyKnockback = false; // Set true jika mau knockback
    [SerializeField] private float knockbackForce = 2f;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask groundLayer;

    // Private variables
    private Vector2 velocity;
    private bool hasHit = false;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Auto-destroy setelah lifetime
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (!hasHit)
        {
            // Rotate arrow to face movement direction
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void FixedUpdate()
    {
        if (!hasHit && rb != null)
        {
            // Move arrow dengan constant velocity
            rb.linearVelocity = velocity;
        }
    }

    /// <summary>
    /// Launch arrow dengan direction dan speed tertentu
    /// Dipanggil dari BanditArcherAttack saat spawn
    /// </summary>
    public void Launch(Vector2 direction, float speed)
    {
        velocity = direction.normalized * speed;

        // Rotate arrow to face direction immediately
        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Debug.Log($"Arrow launched with velocity: {velocity}");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return; // Prevent multiple hits

        // Check if hit player
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            PlayerMovement playerMovement = collision.GetComponent<PlayerMovement>();

            // Check player invincibility (roll)
            if (playerMovement != null && playerMovement.IsInvincible())
            {
                Debug.Log("Arrow blocked - Player is rolling!");
                return; // Don't hit, arrow continues flying
            }

            // Check player i-frames
            if (playerHealth != null && playerHealth.IsInvincible)
            {
                Debug.Log("Arrow blocked - Player has i-frames!");
                return; // Don't hit, arrow continues flying
            }

            // Deal damage
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Arrow hit player! Dealt {damage} damage.");

                // Apply knockback (optional, commented by default)
                /*
                if (applyKnockback)
                {
                    Vector2 knockbackDirection = velocity.normalized;
                    // TODO: Add knockback to PlayerMovement
                    // playerMovement.ApplyKnockback(knockbackDirection * knockbackForce);
                }
                */
            }

            // Arrow hits player and destroys
            HitTarget();
        }
        // Check if hit ground/wall
        else if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            Debug.Log("Arrow hit ground/wall!");
            HitTarget();
        }
    }

    void HitTarget()
    {
        hasHit = true;

        // Stop movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Optional: Play impact effect
        // Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);

        // Optional: Play impact sound
        // AudioManager.Instance.PlaySound("ArrowHit");

        // Destroy arrow
        Destroy(gameObject, 0.1f); // Small delay untuk visual feedback
    }

    void OnDrawGizmos()
    {
        // Visualize velocity direction
        if (Application.isPlaying && !hasHit)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, velocity.normalized * 0.5f);
        }
    }
}