using UnityEngine;

public class HealthCounterUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;

    private Animator animator;

    void Start()
    {
        // Get animator
        animator = GetComponent<Animator>();

        // Auto-find PlayerHealth jika tidak di-assign
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<PlayerHealth>();
            }
        }

        // Subscribe to health change event
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.AddListener(UpdateHealthDisplay);

            // Set initial display
            UpdateHealthDisplay(playerHealth.CurrentHealth);
        }
        else
        {
            Debug.LogError("PlayerHealth not found! Assign manually in Inspector.");
        }
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.RemoveListener(UpdateHealthDisplay);
        }
    }

    void UpdateHealthDisplay(int currentHealth)
    {
        if (animator != null)
        {
            animator.SetFloat("CurrentHealth", currentHealth);
        }
    }
}