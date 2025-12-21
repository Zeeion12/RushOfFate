using UnityEngine;

public class DropManager : MonoBehaviour
{
    [Header("Drop Settings")]
    [Tooltip("Persentase chance untuk drop item (0-100)")]
    [Range(0f, 100f)]
    public float dropChance = 50f;

    [Header("Item Prefabs")]
    [Tooltip("Prefab untuk item health")]
    public GameObject healthItemPrefab;

    [Tooltip("Prefab untuk item time")]
    public GameObject timeItemPrefab;

    [Header("Drop Configuration")]
    [Tooltip("Persentase chance untuk drop health item (sisanya adalah time item)")]
    [Range(0f, 100f)]
    public float healthItemChance = 50f;

    [Header("Spawn Settings")]
    [Tooltip("Offset posisi spawn item dari posisi enemy (agar tidak tertimpa ground)")]
    public Vector2 spawnOffset = new Vector2(0f, 0.5f);

    [Header("Debug")]
    [Tooltip("Tampilkan log debug di console?")]
    public bool showDebugLogs = true;

    /// <summary>
    /// Method untuk dipanggil saat enemy mati
    /// Akan me-roll chance dan spawn item jika berhasil
    /// </summary>
    public void TryDropItem()
    {
        // Roll untuk chance drop item
        float dropRoll = Random.Range(0f, 100f);

        if (dropRoll <= dropChance)
        {
            // Berhasil drop, sekarang tentukan item apa
            SpawnRandomItem();
        }
        else
        {
            if (showDebugLogs)
            {
                Debug.Log($"[DropManager] No item dropped (Roll: {dropRoll:F1}%, Chance: {dropChance}%)");
            }
        }
    }

    /// <summary>
    /// Spawn item secara random (Health atau Time)
    /// </summary>
    void SpawnRandomItem()
    {
        // Roll untuk tentukan item type
        float itemTypeRoll = Random.Range(0f, 100f);
        GameObject itemToSpawn = null;
        string itemName = "";

        if (itemTypeRoll <= healthItemChance)
        {
            // Spawn health item
            itemToSpawn = healthItemPrefab;
            itemName = "Health";
        }
        else
        {
            // Spawn time item
            itemToSpawn = timeItemPrefab;
            itemName = "Time";
        }

        // Validasi prefab
        if (itemToSpawn != null)
        {
            // Hitung posisi spawn dengan offset
            Vector3 spawnPosition = transform.position + (Vector3)spawnOffset;

            // Spawn item
            GameObject droppedItem = Instantiate(itemToSpawn, spawnPosition, Quaternion.identity);

            if (showDebugLogs)
            {
                Debug.Log($"[DropManager] Dropped {itemName} item at position {spawnPosition}");
            }
        }
        else
        {
            Debug.LogWarning($"[DropManager] {itemName} item prefab is not assigned!");
        }
    }

    /// <summary>
    /// Method alternatif untuk force spawn item tertentu (untuk testing)
    /// </summary>
    public void ForceDropHealth()
    {
        if (healthItemPrefab != null)
        {
            Vector3 spawnPosition = transform.position + (Vector3)spawnOffset;
            Instantiate(healthItemPrefab, spawnPosition, Quaternion.identity);
            Debug.Log("[DropManager] Forced health item drop");
        }
    }

    /// <summary>
    /// Method alternatif untuk force spawn time item (untuk testing)
    /// </summary>
    public void ForceDropTime()
    {
        if (timeItemPrefab != null)
        {
            Vector3 spawnPosition = transform.position + (Vector3)spawnOffset;
            Instantiate(timeItemPrefab, spawnPosition, Quaternion.identity);
            Debug.Log("[DropManager] Forced time item drop");
        }
    }

    // Gizmo untuk visualisasi spawn position di editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 spawnPos = transform.position + (Vector3)spawnOffset;
        Gizmos.DrawWireSphere(spawnPos, 0.3f);
        Gizmos.DrawLine(transform.position, spawnPos);
    }
}