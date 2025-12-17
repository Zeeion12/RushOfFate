using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    private Vector2 currentCheckpoint;

    void Start()
    {
        // Set initial checkpoint ke posisi spawn
        currentCheckpoint = transform.position;
        Debug.Log($"Initial checkpoint set at: {currentCheckpoint}");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Update checkpoint saat player touch checkpoint object
        if (collision.gameObject.CompareTag("Checkpoint"))
        {
            currentCheckpoint = transform.position;
            Debug.Log($"Checkpoint updated at: {currentCheckpoint}");
        }

        // ONE-HIT KILL TRAP
        if (collision.gameObject.CompareTag("Trap"))
        {
            PlayerHealth playerHealth = GetComponent<PlayerHealth>();
            if (playerHealth != null && !playerHealth.IsDead)
            {
                // Langsung kill player (damage = current HP)
                int overkillDamage = playerHealth.CurrentHealth;
                playerHealth.TakeDamage(overkillDamage);

                Debug.Log($"Player hit one-hit-kill trap! Dealt {overkillDamage} damage.");
            }
        }
    }

    /// <summary>
    /// Teleport player ke checkpoint terakhir (dipanggil dari PlayerHealth saat respawn)
    /// </summary>
    public void RespawnPlayer()
    {
        transform.position = currentCheckpoint;
        Debug.Log($"Player respawned at: {currentCheckpoint}");
    }

    /// <summary>
    /// Get posisi checkpoint saat ini (optional, untuk debugging)
    /// </summary>
    public Vector2 GetCurrentCheckpoint()
    {
        return currentCheckpoint;
    }

    /// <summary>
    /// Manually set checkpoint position (optional, untuk scripting events)
    /// </summary>
    public void SetCheckpoint(Vector2 newCheckpoint)
    {
        currentCheckpoint = newCheckpoint;
        Debug.Log($"Checkpoint manually set at: {newCheckpoint}");
    }
}