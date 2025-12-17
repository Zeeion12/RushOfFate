using UnityEngine;

public class CanineAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float patrolDistance = 5f;
    [SerializeField] private float idleTime = 1.5f;

    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float detectionRange = 4f;
    [SerializeField] private float chaseRange = 5f;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 1.5f; // NEW: Stopping distance untuk attack
    [SerializeField] private float stoppingDistance = 1.2f; // NEW: Jarak minimum sebelum stop chase

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private LayerMask obstacleLayer;

    // Private variables
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 spawnPosition;
    private float leftBound;
    private float rightBound;
    private bool movingRight = true;
    private float idleTimer = 0f;
    private bool isIdling = false;

    // States
    private enum EnemyState { Patrol, Chase, Idle }
    private EnemyState currentState = EnemyState.Patrol;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

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

        // Check if there's ground ahead
        if (!IsGroundAhead())
        {
            Flip();
            StartIdle();
            return;
        }

        // Move enemy
        float moveDirection = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(moveDirection * patrolSpeed, rb.linearVelocity.y);

        // Set animation
        animator.SetBool("isRunning", true);

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

        // NEW: STOP CHASING jika sudah dalam attack range!
        if (distanceToPlayer <= stoppingDistance)
        {
            // Stop movement tapi tetap facing player
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("isRunning", false);

            // Make sure we're facing the player
            float directionToPlayer = Mathf.Sign(player.position.x - transform.position.x);
            bool shouldFaceRight = directionToPlayer > 0;

            if (shouldFaceRight != movingRight)
            {
                Flip();
            }

            return; // Exit early, don't chase further
        }

        // Check if there's ground ahead when chasing
        if (!IsGroundAhead())
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("isRunning", false);
            return;
        }

        // Calculate direction to player
        float directionToPlayer2 = Mathf.Sign(player.position.x - transform.position.x);

        // Flip if needed
        if ((directionToPlayer2 > 0 && !movingRight) || (directionToPlayer2 < 0 && movingRight))
        {
            Flip();
        }

        // Move towards player
        rb.linearVelocity = new Vector2(directionToPlayer2 * chaseSpeed, rb.linearVelocity.y);

        // Set animation
        animator.SetBool("isRunning", true);
    }

    void Idle()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        animator.SetBool("isRunning", false);

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
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    void Flip()
    {
        movingRight = !movingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    bool IsGroundAhead()
    {
        if (groundCheckPoint == null) return true;

        // Raycast down from ground check point
        RaycastHit2D hit = Physics2D.Raycast(
            groundCheckPoint.position,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        return hit.collider != null;
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
        Gizmos.DrawLine(new Vector2(origin.x - patrolDistance, origin.y),
                        new Vector2(origin.x + patrolDistance, origin.y));

        // Detection range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Chase range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // NEW: Stopping distance (attack range)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);

        // Ground check
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(groundCheckPoint.position,
                          groundCheckPoint.position + Vector3.down * groundCheckDistance);
        }
    }
}