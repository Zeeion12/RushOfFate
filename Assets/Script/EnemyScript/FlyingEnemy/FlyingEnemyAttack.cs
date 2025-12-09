using UnityEngine;

public class FlyingEnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private int contactDamage = 1;
    [SerializeField] private float attackRange = 3f;

    [Header("Dive Attack Settings")]
    [SerializeField] private float diveWindupTime = 0.5f; // Waktu persiapan sebelum dive
    [SerializeField] private float diveSpeed = 8f;
    [SerializeField] private float diveDistance = 5f;
    [SerializeField] private int diveDamage = 1;

    [Header("Contact Damage Settings")]
    [SerializeField] private float contactDamageCooldown = 1f; // Cooldown antar contact damage

    [Header("References")]
    [SerializeField] private LayerMask playerLayer;

    [Header("Gizmo Settings")]
    [SerializeField] private Vector2 gizmoOffset = Vector2.zero;


    // Components
    private Animator animator;
    private FlyingEnemyHealth healthScript;
    private FlyingEnemyAI aiScript;

    // Attack state
    private float lastAttackTime = -999f;
    private float lastContactDamageTime = -999f;
    private bool isAttacking = false;
    private bool isDiving = false;
    private Transform player;
    private PlayerMovement playerMovement;

    // Dive attack variables
    private Vector2 diveStartPosition;
    private Vector2 diveDirection;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        healthScript = GetComponent<FlyingEnemyHealth>();
        aiScript = GetComponent<FlyingEnemyAI>();

        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerMovement = playerObj.GetComponent<PlayerMovement>();
        }
    }

    void Update()
    {
        // Jangan attack kalau sudah mati atau sedang attack
        if (healthScript != null && !healthScript.IsAlive()) return;
        if (isAttacking || isDiving) return;

        // Check jika player dalam range dan bisa attack
        if (player != null && CanAttack())
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer <= attackRange)
            {
                PerformDiveAttack();
            }
        }
    }

    bool CanAttack()
    {
        return !isAttacking && Time.time >= lastAttackTime + attackCooldown;
    }

    void PerformDiveAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Disable AI movement saat attack
        if (aiScript != null)
        {
            aiScript.enabled = false;
        }

        // Trigger attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Start dive attack sequence
        StartCoroutine(DiveAttackSequence());
    }

    System.Collections.IEnumerator DiveAttackSequence()
    {
        // PHASE 1: WINDUP (persiapan)
        diveStartPosition = transform.position;

        // Calculate initial dive direction toward player
        if (player != null)
        {
            diveDirection = (player.position - transform.position).normalized;
        }
        else
        {
            // Fallback: dive down kalau player hilang
            diveDirection = Vector2.down;
        }

        // Wait for windup animation
        yield return new WaitForSeconds(diveWindupTime);

        // PHASE 2: DIVE (terjun)
        isDiving = true;
        float diveTimer = 0f;
        float diveDuration = diveDistance / diveSpeed;

        while (diveTimer < diveDuration)
        {
            diveTimer += Time.deltaTime;

            // ✅ RECALCULATE direction setiap frame untuk track player yang bergerak
            if (player != null)
            {
                // Update direction tapi smooth (blend dengan direction lama)
                Vector2 newDirection = (player.position - transform.position).normalized;
                diveDirection = Vector2.Lerp(diveDirection, newDirection, Time.deltaTime * 3f);
            }

            // Move in dive direction
            transform.position += (Vector3)diveDirection * diveSpeed * Time.deltaTime;

            // Check for player hit during dive
            CheckDiveHit();

            yield return null;
        }

        isDiving = false;

        // PHASE 3: RECOVERY (kembali ke posisi hover)
        yield return new WaitForSeconds(0.3f);

        // Return to start position smoothly
        float returnTimer = 0f;
        float returnDuration = 0.5f;
        Vector2 currentPos = transform.position;

        while (returnTimer < returnDuration)
        {
            returnTimer += Time.deltaTime;
            float t = returnTimer / returnDuration;

            transform.position = Vector2.Lerp(currentPos, diveStartPosition, t);
            yield return null;
        }

        // Re-enable AI
        if (aiScript != null)
        {
            aiScript.enabled = true;
        }

        isAttacking = false;
    }

    void CheckDiveHit()
    {
        // Check for player collision during dive
        Collider2D hitPlayer = Physics2D.OverlapCircle(transform.position, 0.5f, playerLayer);

        if (hitPlayer != null && hitPlayer.CompareTag("Player"))
        {
            // Check invincibility
            if (playerMovement != null && playerMovement.IsInvincible())
            {
                Debug.Log("Dive attack blocked - Player is invincible!");
                return;
            }

            // Deal dive damage
            Debug.Log($"Dive attack hit player for {diveDamage} damage!");

            // ✅ FIXED: Use correct method signature (1 parameter)
            PlayerHealth playerHealth = hitPlayer.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(diveDamage); // Only 1 parameter!
                Debug.Log($"✅ Damage successfully applied to PlayerHealth!");
            }
            else
            {
                Debug.LogWarning($"❌ PlayerHealth component NOT FOUND on {hitPlayer.name}!");
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        // Contact damage saat menyentuh player
        if (collision.gameObject.CompareTag("Player") && !isDiving)
        {
            // Check cooldown untuk contact damage
            if (Time.time < lastContactDamageTime + contactDamageCooldown)
                return;

            // Check invincibility
            if (playerMovement != null && playerMovement.IsInvincible())
            {
                Debug.Log("Contact damage blocked - Player is invincible!");
                return;
            }

            lastContactDamageTime = Time.time;
            Debug.Log($"Contact damage hit player for {contactDamage} damage!");

            // ✅ FIXED: Use correct method signature (1 parameter)
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage); // Only 1 parameter!
                Debug.Log($"✅ Contact damage successfully applied to PlayerHealth!");
            }
            else
            {
                Debug.LogWarning($"❌ PlayerHealth component NOT FOUND on {collision.gameObject.name}!");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Pusat gizmo dengan offset
        Vector2 gizmoCenter = (Vector2)transform.position + gizmoOffset;

        // Visualisasi attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gizmoCenter, attackRange);

        // Visualisasi dive distance
        if (Application.isPlaying && isDiving)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(
                gizmoCenter,
                gizmoCenter + (Vector2)diveDirection * diveDistance
            );
        }

        // Opsional: bantu alignment dengan sumbu
        Gizmos.color = Color.green;
        Gizmos.DrawLine(gizmoCenter, gizmoCenter + Vector2.up * 0.5f);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(gizmoCenter, gizmoCenter + Vector2.right * 0.5f);
    }

}