using UnityEngine;

public class BanditArcherAI : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("References")]
    [SerializeField] private Transform player;

    // Private variables
    private bool movingRight = true; // Default facing direction
    private bool playerDetected = false;

    void Start()
    {
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
            playerDetected = true;
            FacePlayer();
        }
        else
        {
            playerDetected = false;
        }
    }

    void FacePlayer()
    {
        if (player == null) return;

        // Calculate direction to player
        float directionToPlayer = Mathf.Sign(player.position.x - transform.position.x);

        // Flip if needed
        bool shouldFaceRight = directionToPlayer > 0;
        
        if (shouldFaceRight != movingRight)
        {
            Flip();
        }
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

    // Public getter for attack script
    public bool IsPlayerDetected() => playerDetected;
    public Transform GetPlayerTransform() => player;

    // Visualize detection range in editor
    void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Line to player if detected
        if (Application.isPlaying && player != null && playerDetected)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}