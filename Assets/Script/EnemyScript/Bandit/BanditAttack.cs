using UnityEngine;

public class BanditAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackDelay = 0.2f;
    [SerializeField] private int attackDamage = 2;
    [SerializeField] private float attackRange = 1.5f;

    [Header("Whip Attack Hitbox")]
    [SerializeField] private Vector2 attackBoxSize = new Vector2(2f, 1f); // Whip has longer horizontal range
    [SerializeField] private float attackBoxOffset = 1f; // Further offset for whip reach
    [SerializeField] private LayerMask playerLayer;

    // Components
    private Animator animator;
    private BanditHealth healthScript;

    // Attack state
    private float lastAttackTime = -999f;
    private bool isAttacking = false;
    private Transform player;
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth; // NEW: Reference to PlayerHealth

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        healthScript = GetComponent<BanditHealth>();

        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerMovement = playerObj.GetComponent<PlayerMovement>();
            playerHealth = playerObj.GetComponent<PlayerHealth>(); // NEW: Get PlayerHealth
        }
    }

    void Update()
    {
        // Jangan attack kalau sudah mati
        if (healthScript != null && !healthScript.IsAlive()) return;

        // Check jika player dalam range dan bisa attack
        if (player != null && CanAttack())
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer <= attackRange)
            {
                PerformAttack();
            }
        }
    }

    bool CanAttack()
    {
        return !isAttacking && Time.time >= lastAttackTime + attackCooldown;
    }

    void PerformAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Trigger attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Deal damage after delay
        StartCoroutine(DealDamageAfterDelay());
    }

    System.Collections.IEnumerator DealDamageAfterDelay()
    {
        // Wait untuk tengah animasi (whip swing)
        yield return new WaitForSeconds(attackDelay);

        // Check for player in attack range
        Vector2 attackPosition = GetAttackPosition();
        Collider2D[] hits = Physics2D.OverlapBoxAll(attackPosition, attackBoxSize, 0f, playerLayer);

        foreach (Collider2D hit in hits)
        {
            // Check invincibility sebelum deal damage
            if (playerMovement != null && playerMovement.IsInvincible())
            {
                Debug.Log($"Bandit attack blocked - Player is invincible (rolling)!");
                continue;
            }

            // NEW: Check PlayerHealth invincibility (i-frames setelah damage)
            if (playerHealth != null && playerHealth.IsInvincible)
            {
                Debug.Log($"Bandit attack blocked - Player has i-frames!");
                continue;
            }

            Debug.Log($"Bandit (Whip) hit player: {hit.name}");

            // NEW: Actually damage the player!
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"Bandit dealt {attackDamage} damage to player!");
            }
            else
            {
                Debug.LogWarning("PlayerHealth component not found on player!");
            }
        }

        // Wait sisa animasi selesai
        yield return new WaitForSeconds(0.3f);

        isAttacking = false;
    }

    Vector2 GetAttackPosition()
    {
        // Deteksi arah hadap berdasarkan scale
        bool facingRight = transform.localScale.x > 0;
        float direction = facingRight ? 1f : -1f;
        Vector2 offset = new Vector2(attackBoxOffset * direction, 0f);
        return (Vector2)transform.position + offset;
    }

    void OnDrawGizmosSelected()
    {
        // Visualisasi attack range (circle)
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Visualisasi whip attack hitbox (horizontal rectangle)
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // Orange for whip
        Vector2 attackPos = GetAttackPosition();
        Gizmos.DrawCube(attackPos, attackBoxSize);

        Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
        Gizmos.DrawWireCube(attackPos, attackBoxSize);
    }
}