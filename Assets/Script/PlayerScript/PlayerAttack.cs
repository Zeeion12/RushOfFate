using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 0.2f;
    [SerializeField] private float attackDelay = 0.1f;

    [Header("Attack Hitbox")]
    [SerializeField] private Vector2 attackBoxSize = new Vector2(1.5f, 1f);
    [SerializeField] private float attackBoxOffset = 0.8f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference attackAction;

    // Components
    private Animator animator;
    private PlayerMovement movementScript;

    // Attack state
    private float lastAttackTime = -999f;
    private bool isAttacking = false;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        movementScript = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        if (attackAction != null)
        {
            attackAction.action.Enable();
            attackAction.action.performed += OnAttackPerformed;
        }
    }

    private void OnDisable()
    {
        if (attackAction != null)
        {
            attackAction.action.Disable();
            attackAction.action.performed -= OnAttackPerformed;
        }
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (CanAttack())
        {
            PerformAttack();
        }
    }

    private bool CanAttack()
    {
        // Tidak bisa attack jika:
        // - Sedang attack
        // - Masih dalam cooldown
        // - Sedang rolling (NEW!)
        return !isAttacking
            && Time.time >= lastAttackTime + attackCooldown
            && !movementScript.IsRolling(); // Check rolling state
    }

    private void PerformAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Stop movement saat attack
        if (movementScript != null)
        {
            movementScript.SetCanMove(false);
        }

        // Trigger attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Deal damage after delay
        StartCoroutine(DealDamageAfterDelay());
    }

    private System.Collections.IEnumerator DealDamageAfterDelay()
    {
        // Wait untuk tengah animasi
        yield return new WaitForSeconds(attackDelay);

        // Check for enemies in attack range
        Vector2 attackPosition = GetAttackPosition();
        Collider2D[] hits = Physics2D.OverlapBoxAll(attackPosition, attackBoxSize, 0f, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            // Try to damage different enemy types
            bool enemyHit = false;

            // Check for Canine
            CanineHealth canineHealth = hit.GetComponent<CanineHealth>();
            if (canineHealth != null)
            {
                canineHealth.TakeDamage(attackDamage, transform.position);
                Debug.Log($"Player hit Canine: {hit.name}! Damage: {attackDamage}");
                enemyHit = true;
            }

            // Check for Bandit (Whip)
            BanditHealth banditHealth = hit.GetComponent<BanditHealth>();
            if (banditHealth != null)
            {
                banditHealth.TakeDamage(attackDamage, transform.position);
                Debug.Log($"Player hit Bandit: {hit.name}! Damage: {attackDamage}");
                enemyHit = true;
            }

        }

        // Wait sisa animasi selesai
        yield return new WaitForSeconds(attackCooldown - attackDelay);

        // Re-enable movement
        if (movementScript != null)
        {
            movementScript.SetCanMove(true);
        }

        isAttacking = false;
    }

    private Vector2 GetAttackPosition()
    {
        // Get facing direction dari PlayerMovement
        bool facingRight = movementScript != null ? movementScript.IsFacingRight() : true;
        float direction = facingRight ? 1f : -1f;
        Vector2 offset = new Vector2(attackBoxOffset * direction, 0f);
        return (Vector2)transform.position + offset;
    }

    private void OnDrawGizmosSelected()
    {
        // Visualisasi attack hitbox
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Vector2 attackPos = GetAttackPosition();
        Gizmos.DrawCube(attackPos, attackBoxSize);

        // Outline
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(attackPos, attackBoxSize);
    }
}