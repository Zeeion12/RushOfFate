using UnityEngine;

public class ManaCounterUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMana playerMana;

    private Animator animator;

    void Start()
    {
        // Get animator
        animator = GetComponent<Animator>();

        // Auto-find PlayerMana jika tidak di-assign
        if (playerMana == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerMana = player.GetComponent<PlayerMana>();
            }
        }

        // Subscribe to mana change event
        if (playerMana != null)
        {
            playerMana.OnManaChanged.AddListener(UpdateManaDisplay);

            // Set initial display
            UpdateManaDisplay(playerMana.CurrentMana);
        }
        else
        {
            Debug.LogError("PlayerMana not found! Assign manually in Inspector.");
        }
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (playerMana != null)
        {
            playerMana.OnManaChanged.RemoveListener(UpdateManaDisplay);
        }
    }

    void UpdateManaDisplay(int currentMana)
    {
        if (animator != null)
        {
            // FIXED: Gunakan SetFloat karena Blend Tree parameter adalah Float
            animator.SetFloat("CurrentMana", currentMana);
        }
    }
}