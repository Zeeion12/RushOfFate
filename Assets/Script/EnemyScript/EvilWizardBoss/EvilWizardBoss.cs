using UnityEngine;
using System.Collections;

public class EvilWizardBoss : MonoBehaviour
{
    [Header("Boss Stats")]
    [SerializeField] private int maxHealth = 500;
    [SerializeField] private int currentHealth;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float chaseRange = 8f;
    [SerializeField] private float attackRange = 2f;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private int attack1Damage = 15;
    [SerializeField] private int attack2Damage = 25;
    [SerializeField] private float attack1HitboxWidth = 2f;
    [SerializeField] private float attack2HitboxWidth = 3f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Boss Phases")]
    [SerializeField] private int phase2HealthThreshold = 250; // 50% HP (akan auto-calculate ke 50% dari maxHealth)
    [SerializeField] private int phase3HealthThreshold = 100; // 20% HP

    [Header("Phase 2 Modifiers (50% HP)")]
    [SerializeField] private float phase2SpeedMultiplier = 1.3f;
    [SerializeField] private float phase2AttackCooldownMultiplier = 0.7f;
    [SerializeField] private bool phase2EnableComboAttacks = false;

    [Header("Phase 3 Modifiers (20% HP - Enrage)")]
    [SerializeField] private float phase3SpeedMultiplier = 1.5f;
    [SerializeField] private float phase3AttackCooldownMultiplier = 0.5f;
    [SerializeField] private bool phase3EnableComboAttacks = true;
    [SerializeField] private Color phase3TintColor = new Color(1f, 0.5f, 0.5f); // Red tint

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private BossHealthBar healthBar;

    // Components
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // State Management
    private bool isAttacking = false;
    private bool isDead = false;
    private bool isHit = false;
    private float attackTimer = 0f;

    // Boss AI States
    private enum BossState { Idle, Chase, Attack, Hit, Death }
    private BossState currentState = BossState.Idle;

    // Phase Management
    public enum BossPhase { Phase1, Phase2, Phase3 }
    private BossPhase currentPhase = BossPhase.Phase1;

    // Base stats (untuk calculate phase modifiers)
    private float baseSpeed;
    private float baseAttackCooldown;

    void Start()
    {
        // Get Components
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Initialize
        currentHealth = maxHealth;

        // Store base stats
        baseSpeed = moveSpeed;
        baseAttackCooldown = attackCooldown;

        // Auto-calculate phase thresholds based on percentage
        phase2HealthThreshold = Mathf.RoundToInt(maxHealth * 0.5f); // 50% HP
        phase3HealthThreshold = Mathf.RoundToInt(maxHealth * 0.2f); // 20% HP

        // Find player if not assigned
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        // Setup Health Bar
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.ShowHealthBar();
        }

        // Find attack point if not set
        if (attackPoint == null)
        {
            attackPoint = transform.Find("AttackPoint");
            if (attackPoint == null)
                Debug.LogWarning("AttackPoint not found! Create an empty child GameObject named 'AttackPoint'");
        }
    }

    void Update()
    {
        if (isDead || player == null) return;

        attackTimer += Time.deltaTime;

        UpdateBossState();
        HandleBossAI();
    }

    void UpdateBossState()
    {
        if (isHit || isAttacking) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange && attackTimer >= attackCooldown)
        {
            currentState = BossState.Attack;
        }
        else if (distanceToPlayer <= chaseRange)
        {
            currentState = BossState.Chase;
        }
        else
        {
            currentState = BossState.Idle;
        }
    }

    void HandleBossAI()
    {
        switch (currentState)
        {
            case BossState.Idle:
                animator.SetBool("isRunning", false);
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                break;

            case BossState.Chase:
                ChasePlayer();
                break;

            case BossState.Attack:
                PerformAttack();
                break;
        }
    }

    void ChasePlayer()
    {
        animator.SetBool("isRunning", true);

        // Calculate direction
        Vector2 direction = (player.position - transform.position).normalized;

        // Flip sprite
        if (direction.x > 0)
            spriteRenderer.flipX = false;
        else if (direction.x < 0)
            spriteRenderer.flipX = true;

        // Move towards player (speed based on current phase)
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
    }

    void PerformAttack()
    {
        if (isAttacking) return;

        StartCoroutine(AttackSequence());
    }

    IEnumerator AttackSequence()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("isRunning", false);

        // Face player
        FacePlayer();

        // Determine attack pattern based on phase
        bool shouldCombo = (currentPhase == BossPhase.Phase3 && phase3EnableComboAttacks) ||
                          (currentPhase == BossPhase.Phase2 && phase2EnableComboAttacks);

        if (shouldCombo && Random.value > 0.5f)
        {
            // Combo attack: Attack1 -> Attack2
            yield return StartCoroutine(PerformSingleAttack(1));
            yield return new WaitForSeconds(0.2f);
            yield return StartCoroutine(PerformSingleAttack(2));
        }
        else
        {
            // Single attack (60% Attack1, 40% Attack2 for variety)
            int attackType = Random.value > 0.4f ? 1 : 2;
            yield return StartCoroutine(PerformSingleAttack(attackType));
        }

        isAttacking = false;
        attackTimer = 0f;
    }

    IEnumerator PerformSingleAttack(int attackType)
    {
        // Trigger animation
        animator.SetInteger("AttackType", attackType);
        animator.SetTrigger("Attack");

        // Wait for attack animation to reach hit frame (adjust timing based on your animation)
        yield return new WaitForSeconds(0.4f);

        // Deal damage
        if (attackType == 1)
            DealDamage(attack1Damage, attack1HitboxWidth);
        else
            DealDamage(attack2Damage, attack2HitboxWidth);

        // Wait for animation to finish
        yield return new WaitForSeconds(0.6f);
    }

    void DealDamage(int damage, float hitboxWidth)
    {
        if (attackPoint == null) return;

        // Create hitbox
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(
            attackPoint.position,
            hitboxWidth,
            playerLayer
        );

        foreach (Collider2D playerCollider in hitPlayers)
        {
            PlayerHealth playerHealth = playerCollider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Boss dealt {damage} damage to player!");
            }
        }
    }

    void FacePlayer()
    {
        if (player.position.x > transform.position.x)
            spriteRenderer.flipX = false;
        else
            spriteRenderer.flipX = true;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        // Update health bar
        if (healthBar != null)
            healthBar.SetHealth(currentHealth);

        // Check for phase transitions
        CheckPhaseTransition();

        // Check death
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // Take hit reaction
        StartCoroutine(TakeHitSequence());
    }

    void CheckPhaseTransition()
    {
        // Phase 3 transition (20% HP - Enrage)
        if (currentPhase != BossPhase.Phase3 && currentHealth <= phase3HealthThreshold)
        {
            EnterPhase(BossPhase.Phase3);
        }
        // Phase 2 transition (50% HP)
        else if (currentPhase == BossPhase.Phase1 && currentHealth <= phase2HealthThreshold)
        {
            EnterPhase(BossPhase.Phase2);
        }
    }

    IEnumerator TakeHitSequence()
    {
        isHit = true;
        animator.SetTrigger("TakeHit");
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(0.3f);

        isHit = false;
    }

    void EnterPhase(BossPhase newPhase)
    {
        if (currentPhase == newPhase) return;

        currentPhase = newPhase;

        // Update animator speed based on phase (optional - makes animations faster)
        if (animator != null)
        {
            switch (newPhase)
            {
                case BossPhase.Phase2:
                    animator.speed = 1.1f; // Slightly faster animations
                    break;
                case BossPhase.Phase3:
                    animator.speed = 1.25f; // Even faster animations for enrage
                    break;
            }
        }

        switch (newPhase)
        {
            case BossPhase.Phase2:
                Debug.Log("Boss entered Phase 2! (50% HP)");
                // Apply Phase 2 modifiers
                moveSpeed = baseSpeed * phase2SpeedMultiplier;
                attackCooldown = baseAttackCooldown * phase2AttackCooldownMultiplier;
                // Visual feedback
                StartCoroutine(PhaseTransitionEffect(Color.yellow));
                break;

            case BossPhase.Phase3:
                Debug.Log("Boss entered Phase 3 - ENRAGE MODE! (20% HP)");
                // Apply Phase 3 modifiers
                moveSpeed = baseSpeed * phase3SpeedMultiplier;
                attackCooldown = baseAttackCooldown * phase3AttackCooldownMultiplier;
                // Visual feedback
                StartCoroutine(PhaseTransitionEffect(phase3TintColor));
                // Keep the red tint for phase 3
                spriteRenderer.color = phase3TintColor;
                break;
        }
    }

    IEnumerator PhaseTransitionEffect(Color flashColor)
    {
        // Flash effect
        for (int i = 0; i < 4; i++)
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }

        // For Phase 3, keep the tint
        if (currentPhase == BossPhase.Phase3)
        {
            spriteRenderer.color = phase3TintColor;
        }
    }

    void Die()
    {
        isDead = true;
        animator.SetTrigger("Death");
        rb.linearVelocity = Vector2.zero;

        // Disable collider
        GetComponent<Collider2D>().enabled = false;

        // Hide health bar
        if (healthBar != null)
            healthBar.HideHealthBar();

        // Notify game manager
        BossFightManager.Instance?.OnBossDefeated();

        Debug.Log("Boss defeated!");

        // Destroy after animation
        Destroy(gameObject, 2f);
    }

    // Public method untuk debugging atau external calls
    public BossPhase GetCurrentPhase()
    {
        return currentPhase;
    }

    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }

    // Gizmos for debugging
    void OnDrawGizmosSelected()
    {
        // Chase range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Attack hitbox
        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attack1HitboxWidth);
        }
    }
}