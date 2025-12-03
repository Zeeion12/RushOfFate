using UnityEngine;
using UnityEngine.Events;

public class PlayerMana : MonoBehaviour
{
    [Header("Mana Settings")]
    [SerializeField] private int maxMana = 10;
    [SerializeField] private float manaRegenRate = 1f; // Mana regen per detik
    [SerializeField] private float regenDelay = 2f; // Delay sebelum mulai regen setelah consume

    [Header("Events")]
    public UnityEvent<int> OnManaChanged; // Event untuk update UI

    // State
    private int currentMana;
    private float regenTimer = 0f;
    private bool isRegenerating = false;

    // Properties
    public int CurrentMana => currentMana;
    public int MaxMana => maxMana;

    void Start()
    {
        // Initialize mana penuh
        currentMana = maxMana;

        // Trigger initial UI update
        OnManaChanged?.Invoke(currentMana);
    }

    void Update()
    {
        // Handle mana regeneration
        if (currentMana < maxMana)
        {
            if (!isRegenerating)
            {
                // Start regen timer
                regenTimer += Time.deltaTime;

                if (regenTimer >= regenDelay)
                {
                    isRegenerating = true;
                }
            }
            else
            {
                // Regenerate mana
                regenTimer += Time.deltaTime;

                if (regenTimer >= 1f / manaRegenRate)
                {
                    RestoreMana(1);
                    regenTimer = 0f;
                }
            }
        }
    }

    public bool ConsumeMana(int amount)
    {
        // Check jika mana cukup
        if (currentMana < amount)
        {
            Debug.Log($"Not enough mana! Need {amount}, have {currentMana}");
            return false;
        }

        // Consume mana
        currentMana -= amount;
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);

        Debug.Log($"Consumed {amount} mana. Mana: {currentMana}/{maxMana}");

        // Trigger UI update
        OnManaChanged?.Invoke(currentMana);

        // Reset regen timer
        regenTimer = 0f;
        isRegenerating = false;

        return true;
    }

    public void RestoreMana(int amount)
    {
        currentMana += amount;
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);

        // Trigger UI update
        OnManaChanged?.Invoke(currentMana);

        // Stop regen jika sudah penuh
        if (currentMana >= maxMana)
        {
            isRegenerating = false;
            regenTimer = 0f;
        }
    }

    public bool HasEnoughMana(int amount)
    {
        return currentMana >= amount;
    }
}