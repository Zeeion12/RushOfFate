using System.Collections;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Configuration")]
    [Tooltip("Tipe item yang akan dipickup")]
    public ItemType itemType;

    [Header("Health Item Settings")]
    [Tooltip("Jumlah HP yang akan ditambahkan (hanya untuk ItemType.Health)")]
    public int healthAmount = 1;

    [Header("Time Item Settings")]
    [Tooltip("Pilihan waktu yang akan ditambahkan secara random (hanya untuk ItemType.Time)")]
    public int[] timeOptions = { 10, 15, 20, 30 };

    [Header("Lifetime Settings")]
    [Tooltip("Waktu sebelum item hilang otomatis (dalam detik)")]
    public float itemLifetime = 6f;

    [Header("Visual Feedback (Optional)")]
    [Tooltip("Apakah item berkedip sebelum hilang?")]
    public bool blinkBeforeDestroy = true;
    [Tooltip("Waktu mulai berkedip sebelum hilang (dalam detik)")]
    public float blinkStartTime = 2f;
    [Tooltip("Kecepatan berkedip")]
    public float blinkSpeed = 0.2f;

    private SpriteRenderer spriteRenderer;
    private float timeRemaining;
    private bool isBlinking = false;

    public enum ItemType
    {
        Health,
        Time
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        timeRemaining = itemLifetime;

        // Mulai countdown untuk auto-destroy
        StartCoroutine(ItemLifetimeCountdown());
    }

    void Update()
    {
        timeRemaining -= Time.deltaTime;

        // Mulai berkedip jika waktu tersisa kurang dari blinkStartTime
        if (blinkBeforeDestroy && !isBlinking && timeRemaining <= blinkStartTime)
        {
            StartCoroutine(BlinkEffect());
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Cek apakah yang menyentuh adalah player
        if (other.CompareTag("Player"))
        {
            ApplyItemEffect(other.gameObject);
            Destroy(gameObject);
        }
    }

    void ApplyItemEffect(GameObject player)
    {
        switch (itemType)
        {
            case ItemType.Health:
                ApplyHealthEffect(player);
                break;

            case ItemType.Time:
                ApplyTimeEffect(player);
                break;
        }
    }

    void ApplyHealthEffect(GameObject player)
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.Heal(healthAmount);
            Debug.Log($"[ItemPickup] Player healed for {healthAmount} HP");
        }
        else
        {
            Debug.LogWarning("[ItemPickup] PlayerHealth component not found on player!");
        }
    }

    void ApplyTimeEffect(GameObject player)
    {
        // Cek apakah ada timer manager di scene
        TimerManager timerManager = FindObjectOfType<TimerManager>();

        if (timerManager != null && timeOptions.Length > 0)
        {
            // Pilih waktu random dari array timeOptions
            int randomTime = timeOptions[Random.Range(0, timeOptions.Length)];

            // Tambahkan waktu ke timer
            timerManager.AddTime(randomTime);
            Debug.Log($"[ItemPickup] Added {randomTime} seconds to timer");
        }
        else if (timeOptions.Length == 0)
        {
            Debug.LogWarning("[ItemPickup] timeOptions array is empty!");
        }
        else
        {
            Debug.LogWarning("[ItemPickup] TimerManager not found in scene!");
        }
    }

    IEnumerator ItemLifetimeCountdown()
    {
        yield return new WaitForSeconds(itemLifetime);

        Debug.Log($"[ItemPickup] {itemType} item expired after {itemLifetime} seconds");
        Destroy(gameObject);
    }

    IEnumerator BlinkEffect()
    {
        isBlinking = true;

        while (timeRemaining > 0)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }

            yield return new WaitForSeconds(blinkSpeed);
        }
    }
}