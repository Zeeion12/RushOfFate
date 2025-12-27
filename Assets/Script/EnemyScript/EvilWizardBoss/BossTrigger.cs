using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool triggerOnce = true; // Boss hanya spawn sekali
    [SerializeField] private string playerTag = "Player";

    private bool hasTriggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if player entered trigger and hasn't triggered yet
        if (other.CompareTag(playerTag) && (!hasTriggered || !triggerOnce))
        {
            hasTriggered = true;

            // Start boss fight
            if (BossFightManager.Instance != null)
            {
                BossFightManager.Instance.StartBossFight();
                Debug.Log("Boss fight triggered!");
            }
            else
            {
                Debug.LogError("BossFightManager.Instance is null! Make sure BossFightManager exists in scene.");
            }

            // Disable trigger collider after use (optional)
            if (triggerOnce)
            {
                GetComponent<Collider2D>().enabled = false;
            }
        }
    }

    // Visual gizmo di Scene view
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // Red transparent
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            Gizmos.DrawCube(transform.position, boxCollider.size);
        }
    }
}
