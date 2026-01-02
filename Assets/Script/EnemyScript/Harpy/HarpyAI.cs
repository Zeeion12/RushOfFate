using UnityEngine;

public class HarpyAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private float patrolSpeed = 1.5f; // Flying speed
    [SerializeField] private float patrolDistance = 6f; // Horizontal patrol distance
    [SerializeField] private float hoverHeight = 3f; // Base height for flying
    [SerializeField] private float waveAmplitude = 0.5f; // Height of wave motion
    [SerializeField] private float waveFrequency = 2f; // Speed of wave motion
    [SerializeField] private float idleTime = 1.5f;

    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 2.5f; // Faster when chasing
    [SerializeField] private float detectionRange = 5f; // Can detect from further
    [SerializeField] private float chaseRange = 7f; // Wider chase range

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float stoppingDistance = 1.8f; // Stop distance for attack

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private LayerMask obstacleLayer;

    // Private variables
    private Rigidbody2D rb;
    private Animator animator;
    private HarpyHealth healthScript;
    private Vector2 spawnPosition;
    private float leftBound;
    private float rightBound;
    private bool movingRight = true;
    private float idleTimer = 0f;
    private bool isIdling = false;
    private float waveTimer = 0f; // For wave motion

    // States
    private enum EnemyState { Patrol, Chase, Idle }
    private EnemyState currentState = EnemyState.Patrol;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        healthScript = GetComponent<HarpyHealth>();

        // Set gravity to 0 for flying enemy
        rb.gravityScale = 0f;

        // Get animator from child Renderer object
        animator = GetComponentInChildren<Animator>();

        // Set spawn position and patrol bounds
        spawnPosition = transform.position;
        leftBound = spawnPosition.x - patrolDistance;
        rightBound = spawnPosition.x + patrolDistance;

        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    void Update()
    {
        // Don't do anything if dead
        if (healthScript != null && !healthScript.IsAlive())
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Check if player is in detection range
        if (player != null && CanSeePlayer())
        {
            currentState = EnemyState.Chase;
        }
        else if (currentState == EnemyState.Chase)
        {
            // Return to patrol if player is too far
            if (player == null || Vector2.Distance(transform.position, player.position) > chaseRange)
            {
                currentState = EnemyState.Patrol;
            }
        }

        // State machine
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Chase:
                ChasePlayer();
                break;
            case EnemyState.Idle:
                Idle();
                break;
        }
    }

    void Patrol()
    {
        if (isIdling)
        {
            Idle();
            return;
        }

        // Horizontal movement
        float moveDirection = movingRight ? 1f : -1f;

        // Wave motion for vertical movement
        waveTimer += Time.deltaTime * waveFrequency;
        float waveOffset = Mathf.Sin(waveTimer) * waveAmplitude;
        float targetY = spawnPosition.y + hoverHeight + waveOffset;

        // Apply movement with wave motion
        rb.linearVelocity = new Vector2(
            moveDirection * patrolSpeed,
            (targetY - transform.position.y) * 2f // Smooth vertical movement
        );

        // Set animation - flying animation
        animator.SetBool("isFlying", true);

        // Check patrol bounds
        if (movingRight && transform.position.x >= rightBound)
        {
            Flip();
            StartIdle();
        }
        else if (!movingRight && transform.position.x <= leftBound)
        {
            Flip();
            StartIdle();
        }
    }

    void ChasePlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Stop chasing if in attack range
        if (distanceToPlayer <= stoppingDistance)
        {
            // Hover in place but keep facing player
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isFlying", false);

            // Make sure we're facing the player
            float directionToPlayer = Mathf.Sign(player.position.x - transform.position.x);
            bool shouldFaceRight = directionToPlayer > 0;

            if (shouldFaceRight != movingRight)
            {
                Flip();
            }

            return;
        }

        // Calculate direction to player (both X and Y for diagonal movement)
        Vector2 directionToPlayer2D = (player.position - transform.position).normalized;

        // Flip if needed based on horizontal direction
        float horizontalDirection = Mathf.Sign(directionToPlayer2D.x);
        if ((horizontalDirection > 0 && !movingRight) || (horizontalDirection < 0 && movingRight))
        {
            Flip();
        }

        // Move towards player diagonally
        rb.linearVelocity = directionToPlayer2D * chaseSpeed;

        // Set animation
        animator.SetBool("isFlying", true);
    }

    void Idle()
    {
        // Gentle hover motion when idle
        float hoverOffset = Mathf.Sin(Time.time * waveFrequency * 0.5f) * (waveAmplitude * 0.3f);
        float targetY = transform.position.y + hoverOffset * Time.deltaTime;

        rb.linearVelocity = new Vector2(0, hoverOffset);
        animator.SetBool("isFlying", false);

        idleTimer += Time.deltaTime;
        if (idleTimer >= idleTime)
        {
            isIdling = false;
            idleTimer = 0f;
        }
    }

    void StartIdle()
    {
        isIdling = true;
        idleTimer = 0f;
        rb.linearVelocity = Vector2.zero;
    }

    void Flip()
    {
        movingRight = !movingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Check if player is within detection range
        if (distanceToPlayer <= detectionRange)
        {
            // Line of sight check
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                directionToPlayer,
                detectionRange,
                obstacleLayer
            );

            // If raycast hits nothing or hits the player, we can see them
            if (hit.collider == null || hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    // Visualize detection and patrol range in editor
    void OnDrawGizmosSelected()
    {
        // Patrol bounds
        Vector2 origin = Application.isPlaying ? spawnPosition : (Vector2)transform.position;
        Gizmos.color = Color.yellow;

        // Draw patrol area as a rectangle showing flying zone
        float baseHeight = origin.y + hoverHeight;
        Vector3 topLeft = new Vector3(origin.x - patrolDistance, baseHeight + waveAmplitude, 0);
        Vector3 topRight = new Vector3(origin.x + patrolDistance, baseHeight + waveAmplitude, 0);
        Vector3 bottomLeft = new Vector3(origin.x - patrolDistance, baseHeight - waveAmplitude, 0);
        Vector3 bottomRight = new Vector3(origin.x + patrolDistance, baseHeight - waveAmplitude, 0);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(topLeft, bottomLeft);
        Gizmos.DrawLine(topRight, bottomRight);

        // Detection range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Chase range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // Stopping distance (attack range)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }
}
