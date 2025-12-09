using UnityEngine;

public class FlyingEnemyAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float patrolDistance = 5f;
    [SerializeField] private float idleTime = 1.5f;

    [Header("Hover Settings")]
    [SerializeField] private float hoverHeight = 3f; // Tinggi terbang dari spawn point
    [SerializeField] private float hoverAmplitude = 0.5f; // Seberapa tinggi naik-turun saat hover
    [SerializeField] private float hoverFrequency = 2f; // Kecepatan naik-turun

    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float chaseRange = 6f;

    [Header("Wall Check")]
    [SerializeField] private Transform wallCheckPoint;
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Gizmo Settings")]
    [SerializeField] private Vector2 gizmoOffset = Vector2.zero; // Offset untuk posisi gizmo
    [SerializeField] private bool showPatrolGizmo = true;
    [SerializeField] private bool showDetectionGizmo = true;
    [SerializeField] private bool showChaseGizmo = true;
    [SerializeField] private bool showWallCheckGizmo = true;
    [SerializeField] private bool showHoverHeightGizmo = true;

    // Private variables
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 spawnPosition;
    private float leftBound;
    private float rightBound;
    private float targetHeight; // Target Y position untuk hover
    private bool movingRight = true;
    private float idleTimer = 0f;
    private bool isIdling = false;
    private float hoverTime = 0f; // Timer untuk sinusoidal hover

    // States
    private enum EnemyState { Patrol, Chase, Idle }
    private EnemyState currentState = EnemyState.Patrol;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Get animator & sprite renderer from child Renderer object
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Set spawn position and patrol bounds
        spawnPosition = transform.position;
        targetHeight = spawnPosition.y + hoverHeight; // Set target hover height
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

        // Validation
        if (rb == null) Debug.LogError($"{gameObject.name}: Rigidbody2D not found!");
        if (animator == null) Debug.LogError($"{gameObject.name}: Animator not found in children!");
        if (spriteRenderer == null) Debug.LogError($"{gameObject.name}: SpriteRenderer not found in children!");
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

        // Check if there's wall ahead
        if (IsWallAhead())
        {
            Flip();
            StartIdle();
            return;
        }

        // Calculate hover offset (sinusoidal up-down movement)
        hoverTime += Time.deltaTime * hoverFrequency;
        float hoverOffset = Mathf.Sin(hoverTime) * hoverAmplitude;
        float currentTargetY = targetHeight + hoverOffset;

        // Move horizontally
        float moveDirection = movingRight ? 1f : -1f;
        float newX = transform.position.x + (moveDirection * patrolSpeed * Time.deltaTime);

        // Move vertically toward hover height with smooth interpolation
        float newY = Mathf.Lerp(transform.position.y, currentTargetY, Time.deltaTime * 2f);

        // Apply movement via transform (karena kinematic)
        transform.position = new Vector2(newX, newY);

        // Set animation (always flying when patrolling)
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

        // Check if there's wall ahead when chasing
        if (IsWallAhead())
        {
            animator.SetBool("isFlying", false);
            return;
        }

        // Calculate direction to player
        Vector2 directionToPlayer = (player.position - transform.position).normalized;

        // Always move toward player (no maintain distance)
        Vector2 targetPosition = (Vector2)transform.position + directionToPlayer * chaseSpeed * Time.deltaTime;
        transform.position = targetPosition;

        // Flip sprite based on direction
        if ((directionToPlayer.x > 0 && !movingRight) || (directionToPlayer.x < 0 && movingRight))
        {
            Flip();
        }

        animator.SetBool("isFlying", true);
    }

    void Idle()
    {
        // Stop horizontal movement, tapi maintain hover height
        hoverTime += Time.deltaTime * hoverFrequency;
        float hoverOffset = Mathf.Sin(hoverTime) * hoverAmplitude;
        float currentTargetY = targetHeight + hoverOffset;

        float newY = Mathf.Lerp(transform.position.y, currentTargetY, Time.deltaTime * 2f);
        transform.position = new Vector2(transform.position.x, newY);

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
    }

    void Flip()
    {
        movingRight = !movingRight;

        // Flip sprite via SpriteRenderer
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !spriteRenderer.flipX;
        }
    }

    bool IsWallAhead()
    {
        if (wallCheckPoint == null) return false;

        // Direction based on movement
        Vector2 direction = movingRight ? Vector2.right : Vector2.left;

        // Raycast to check for walls
        RaycastHit2D hit = Physics2D.Raycast(
            wallCheckPoint.position,
            direction,
            wallCheckDistance,
            obstacleLayer
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

    // Public getter untuk attack script
    public bool IsFacingRight()
    {
        return movingRight;
    }

    // Visualize detection and patrol range in editor
    void OnDrawGizmosSelected()
    {
        // Calculate gizmo center with offset
        Vector2 gizmoCenter = (Vector2)transform.position + gizmoOffset;

        // Patrol bounds
        if (showPatrolGizmo)
        {
            Vector2 origin = Application.isPlaying ? spawnPosition : (Vector2)transform.position;
            origin += gizmoOffset; // Apply offset

            float gizmoHeight = Application.isPlaying ? targetHeight : transform.position.y + hoverHeight;
            gizmoHeight += gizmoOffset.y; // Apply Y offset

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(new Vector2(origin.x - patrolDistance, gizmoHeight),
                           new Vector2(origin.x + patrolDistance, gizmoHeight));

            // Draw vertical lines at bounds
            Gizmos.color = new Color(1f, 1f, 0f, 0.5f); // Semi-transparent yellow
            Gizmos.DrawLine(new Vector2(origin.x - patrolDistance, gizmoHeight - 0.5f),
                           new Vector2(origin.x - patrolDistance, gizmoHeight + 0.5f));
            Gizmos.DrawLine(new Vector2(origin.x + patrolDistance, gizmoHeight - 0.5f),
                           new Vector2(origin.x + patrolDistance, gizmoHeight + 0.5f));
        }

        // Detection range
        if (showDetectionGizmo)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(gizmoCenter, detectionRange);
        }

        // Chase range
        if (showChaseGizmo)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(gizmoCenter, chaseRange);
        }

        // Wall check
        if (showWallCheckGizmo && wallCheckPoint != null)
        {
            Vector2 direction = movingRight ? Vector2.right : Vector2.left;
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(wallCheckPoint.position + (Vector3)gizmoOffset,
                          wallCheckPoint.position + (Vector3)gizmoOffset + (Vector3)direction * wallCheckDistance);

            // Draw sphere at wall check point
            Gizmos.DrawWireSphere(wallCheckPoint.position + (Vector3)gizmoOffset, 0.1f);
        }

        // Hover height indicator
        if (showHoverHeightGizmo)
        {
            Vector2 origin = Application.isPlaying ? spawnPosition : (Vector2)transform.position;
            origin += gizmoOffset; // Apply offset

            float gizmoHeight = Application.isPlaying ? targetHeight : transform.position.y + hoverHeight;
            gizmoHeight += gizmoOffset.y; // Apply Y offset

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(new Vector2(origin.x, gizmoHeight), 0.3f);

            // Draw line from spawn to hover height
            Gizmos.color = new Color(0f, 0f, 1f, 0.3f); // Semi-transparent blue
            Gizmos.DrawLine(new Vector2(origin.x, transform.position.y + gizmoOffset.y),
                            new Vector2(origin.x, gizmoHeight));
        }

        // Draw coordinate axes at gizmo center (helpful untuk alignment)
        Gizmos.color = Color.red;
        Gizmos.DrawLine(gizmoCenter, gizmoCenter + Vector2.right * 0.5f); // X axis
        Gizmos.color = Color.green;
        Gizmos.DrawLine(gizmoCenter, gizmoCenter + Vector2.up * 0.5f); // Y axis
    }
}