using UnityEngine;

public class GolemAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 2.5f; // Slower attack speed
    [SerializeField] private float attackDelay = 0.3f; // Slightly longer delay for heavy punch
    [SerializeField] private int attackDamage = 3; // Higher damage than Bandit/Canine
    [SerializeField] private float attackRange = 1.5f;

    [Header("Punch Attack Hitbox")]
    [SerializeField] private Vector2 attackBoxSize = new Vector2(1.8f, 1.5f); // Larger hitbox for Golem
    [SerializeField] private float attackBoxOffset = 1f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Audio")]
    [SerializeField] private AudioSource attackAudioSource;
    [SerializeField] private AudioClip punchSound;
    [SerializeField][Range(0f, 1f)] private float attackVolume = 1f;

    // Components
    private Animator animator;
    private GolemHealth healthScript;

    // Attack state
    private float lastAttackTime = -999f;
    private bool isAttacking = false;
    private Transform player;
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        healthScript = GetComponent<GolemHealth>();

        // Get or create audio source
        if (attackAudioSource == null)
            attackAudioSource = GetComponent<AudioSource>();

        if (attackAudioSource == null)
            attackAudioSource = gameObject.AddComponent<AudioSource>();

        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerMovement = playerObj.GetComponent<PlayerMovement>();
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }
    }

    void Update()
    {
        // Don't attack if dead
        if (healthScript != null && !healthScript.IsAlive()) return;

        // Check if player is in range and can attack
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

        // Play punch sound
        if (attackAudioSource != null && punchSound != null)
        {
            attackAudioSource.PlayOneShot(punchSound, attackVolume);
        }

        // Deal damage after delay
        StartCoroutine(DealDamageAfterDelay());
    }

    System.Collections.IEnumerator DealDamageAfterDelay()
    {
        // Wait for middle of animation (heavy punch)
        yield return new WaitForSeconds(attackDelay);

        // Check for player in attack range
        Vector2 attackPosition = GetAttackPosition();
        Collider2D[] hits = Physics2D.OverlapBoxAll(attackPosition, attackBoxSize, 0f, playerLayer);

        foreach (Collider2D hit in hits)
        {
            // Check invincibility before dealing damage
            if (playerMovement != null && playerMovement.IsInvincible())
            {
                Debug.Log($"Golem attack blocked - Player is invincible (rolling)!");
                continue;
            }

            // Check PlayerHealth invincibility (i-frames after damage)
            if (playerHealth != null && playerHealth.IsInvincible)
            {
                Debug.Log($"Golem attack blocked - Player has i-frames!");
                continue;
            }

            Debug.Log($"Golem (Punch) hit player: {hit.name}");

            // Actually damage the player!
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"Golem dealt {attackDamage} damage to player!");
            }
            else
            {
                Debug.LogWarning("PlayerHealth component not found on player!");
            }
        }

        // Wait for rest of animation to finish
        yield return new WaitForSeconds(0.4f);

        isAttacking = false;
    }

    Vector2 GetAttackPosition()
    {
        // Detect facing direction based on scale
        bool facingRight = transform.localScale.x > 0;
        float direction = facingRight ? 1f : -1f;
        Vector2 offset = new Vector2(attackBoxOffset * direction, 0f);
        return (Vector2)transform.position + offset;
    }

    void OnDrawGizmosSelected()
    {
        // Visualize attack range (circle)
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Visualize punch attack hitbox
        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f); // Gray for punch
        Vector2 attackPos = GetAttackPosition();
        Gizmos.DrawCube(attackPos, attackBoxSize);

        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        Gizmos.DrawWireCube(attackPos, attackBoxSize);
    }
}
