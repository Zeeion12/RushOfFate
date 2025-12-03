using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    [Header("Respawn Settings")]
    [SerializeField] private float respawnInvincibilityDuration = 2f; // I-frames setelah respawn

    private Vector2 checkpointPosition; // Rename untuk lebih jelas
    private PlayerHealth playerHealth;

    void Start()
    {
        // Set initial checkpoint = spawn position
        checkpointPosition = transform.position;

        // Get PlayerHealth component
        playerHealth = GetComponent<PlayerHealth>();

        Debug.Log($"Initial checkpoint set at: {checkpointPosition}");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Update checkpoint saat sentuh checkpoint trigger
        if (collision.gameObject.CompareTag("Checkpoint"))
        {
            checkpointPosition = transform.position;
            Debug.Log($"Checkpoint updated at: {checkpointPosition}");

            // Optional: Visual/audio feedback untuk checkpoint activated
            // PlayCheckpointEffect();
        }

        // Trap handling - trigger death instead of instant teleport
        if (collision.gameObject.CompareTag("Trap"))
        {
            if (playerHealth != null && !playerHealth.IsDead)
            {
                // Kill player instantly (fall damage / trap damage)
                playerHealth.TakeDamage(playerHealth.MaxHealth);
                Debug.Log("Player hit trap! Triggering death...");
            }
        }
    }

    /// <summary>
    /// Respawn player ke checkpoint terakhir
    /// </summary>
    public void RespawnPlayer()
    {
        // Teleport ke checkpoint
        transform.position = checkpointPosition;

        Debug.Log($"Player respawned at checkpoint: {checkpointPosition}");

        // Optional: Respawn visual effect
        // PlayRespawnEffect();
    }

    /// <summary>
    /// Get current checkpoint position (untuk debugging atau UI)
    /// </summary>
    public Vector2 GetCheckpointPosition()
    {
        return checkpointPosition;
    }

    /// <summary>
    /// Manually set checkpoint (untuk scripted events)
    /// </summary>
    public void SetCheckpoint(Vector2 newPosition)
    {
        checkpointPosition = newPosition;
        Debug.Log($"Checkpoint manually set at: {checkpointPosition}");
    }
}