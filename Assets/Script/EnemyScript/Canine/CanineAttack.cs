using UnityEngine;

public class CanineAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackDelay = 0.2f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackRange = 1.5f;

    [Header("Attack Hitbox")]
    [SerializeField] private Vector2 attackBoxSize = new Vector2(1.5f, 1f);
    [SerializeField] private float attackBoxOffset = 0.5f;
    [SerializeField] private LayerMask playerLayer;

    // Components
    private Animator animator;
    private CanineHealth healthScript;

    // Attack state
    private float lastAttackTime = -999f;
    private bool isAttacking = false;
    private Transform player;
    private PlayerMovement playerMovement; // NEW: untuk check invincibility

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        healthScript = GetComponent<CanineHealth>();

        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerMovement = playerObj.GetComponent<PlayerMovement>(); // NEW: get PlayerMovement
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
        // Wait untuk tengah animasi
        yield return new WaitForSeconds(attackDelay);

        // Check for player in attack range
        Vector2 attackPosition = GetAttackPosition();
        Collider2D[] hits = Physics2D.OverlapBoxAll(attackPosition, attackBoxSize, 0f, playerLayer);

        foreach (Collider2D hit in hits)
        {
            // Check invincibility dari PlayerMovement (roll)
            if (playerMovement != null && playerMovement.IsInvincible())
            {
                Debug.Log($"Enemy attack blocked - Player is invincible (rolling)!");
                continue;
            }

            // NEW: Check invincibility dari PlayerHealth (damage i-frames)
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                if (playerHealth.IsInvincible || playerHealth.IsDead)
                {
                    Debug.Log("Enemy attack blocked - Player has damage invincibility!");
                    continue;
                }

                // Deal damage!
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"Enemy hit player! Damage: {attackDamage}");
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
        // Visualisasi attack range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Visualisasi attack hitbox
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Vector2 attackPos = GetAttackPosition();
        Gizmos.DrawCube(attackPos, attackBoxSize);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackPos, attackBoxSize);
    }
}