using UnityEngine;

public class BanditArcherAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackDelay = 0.5f; // Time to spawn arrow after animation starts (charge time)

    [Header("Arrow Settings")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform arrowSpawnPoint; // Posisi spawn arrow (ujung bow)
    [SerializeField] private float arrowSpeed = 10f;

    // Components
    private Animator animator;
    private BanditArcherAI aiScript;
    private BanditArcherHealth healthScript;

    // Attack state
    private float lastAttackTime = -999f;
    private bool isAttacking = false;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        aiScript = GetComponent<BanditArcherAI>();
        healthScript = GetComponent<BanditArcherHealth>();

        // Validation
        if (arrowPrefab == null)
        {
            Debug.LogError($"Arrow Prefab not assigned on {gameObject.name}!");
        }

        if (arrowSpawnPoint == null)
        {
            Debug.LogWarning($"Arrow Spawn Point not assigned on {gameObject.name}! Using archer position as fallback.");
        }
    }

    void Update()
    {
        // Jangan attack kalau sudah mati
        if (healthScript != null && !healthScript.IsAlive()) return;

        // Check jika player detected dan bisa attack
        if (aiScript != null && aiScript.IsPlayerDetected() && CanAttack())
        {
            PerformAttack();
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

        // Trigger attack animation (charge + shoot)
        if (animator != null)
        {
            animator.SetTrigger("BowAttack");
        }

        // Spawn arrow after delay (match with animation release frame)
        StartCoroutine(SpawnArrowAfterDelay());
    }

    System.Collections.IEnumerator SpawnArrowAfterDelay()
    {
        // Wait untuk charge animation (bow draw)
        yield return new WaitForSeconds(attackDelay);

        // Spawn arrow
        SpawnArrow();

        // Wait sisa animation selesai
        float remainingTime = attackCooldown - attackDelay;
        yield return new WaitForSeconds(remainingTime);

        isAttacking = false;
    }

    void SpawnArrow()
    {
        if (arrowPrefab == null) return;

        Transform player = aiScript?.GetPlayerTransform();
        if (player == null) return;

        // Determine spawn position
        Vector3 spawnPosition = arrowSpawnPoint != null ? arrowSpawnPoint.position : transform.position;

        // Calculate direction to player (aim at player's current position)
        Vector2 directionToPlayer = (player.position - spawnPosition).normalized;

        // Instantiate arrow
        GameObject arrow = Instantiate(arrowPrefab, spawnPosition, Quaternion.identity);

        // Get arrow script and set velocity
        ArrowProjectile arrowScript = arrow.GetComponent<ArrowProjectile>();
        if (arrowScript != null)
        {
            arrowScript.Launch(directionToPlayer, arrowSpeed);
        }
        else
        {
            Destroy(arrow);
        }
    }

    // Visualize arrow spawn point in editor
    void OnDrawGizmosSelected()
    {
        if (arrowSpawnPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(arrowSpawnPoint.position, 0.2f);

            // Draw arrow direction preview
            if (Application.isPlaying && aiScript != null)
            {
                Transform player = aiScript.GetPlayerTransform();
                if (player != null)
                {
                    Vector2 direction = (player.position - arrowSpawnPoint.position).normalized;
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawRay(arrowSpawnPoint.position, direction * 3f);
                }
            }
        }
    }
}