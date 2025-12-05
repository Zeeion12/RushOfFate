using System.Collections;
using UnityEngine;

/// <summary>
/// Script untuk GameObject "LevelFinish" di setiap stage
/// Trigger ini menandakan pemain telah menyelesaikan level
/// dan akan membuka level berikutnya + kembali ke LevelSelection
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LevelFinish : MonoBehaviour
{
    [Header("Level Configuration")]
    [SerializeField] private string currentLevelName = "TutorialStage";
    [Tooltip("Nama level ini (harus sama dengan nama scene):\n" +
             "- TutorialStage\n" +
             "- Stage1\n" +
             "- Stage2\n" +
             "- Stage3\n" +
             "- Stage4\n" +
             "- Stage5")]

    [Header("Scene Navigation")]
    [SerializeField] private string levelSelectionSceneName = "LevelSelection";
    [Tooltip("Scene yang akan di-load setelah level selesai (biasanya LevelSelection)")]

    [Header("Timing Settings")]
    [SerializeField] private float delayBeforeTransition = 1.5f;
    [Tooltip("Delay sebelum pindah scene (untuk animasi victory, dll)")]

    [SerializeField] private bool showCompletionMessage = true;
    [Tooltip("Tampilkan log completion message?")]

    [Header("References")]
    [SerializeField] private FadeManager fadeManager;

    [Header("Optional: Player Freeze")]
    [SerializeField] private bool freezePlayerOnFinish = true;
    [Tooltip("Freeze player movement saat finish?")]

    // State
    private bool hasTriggered = false;

    void Start()
    {
        // Validasi collider
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError($"{gameObject.name}: Missing Collider2D component!");
            return;
        }

        if (!col.isTrigger)
        {
            Debug.LogWarning($"{gameObject.name}: Collider2D is not set as Trigger! Auto-fixing...");
            col.isTrigger = true;
        }

        // Auto-find FadeManager jika belum di-assign
        if (fadeManager == null)
        {
            fadeManager = FindFirstObjectByType<FadeManager>();
            if (fadeManager == null)
            {
                Debug.LogWarning($"{gameObject.name}: FadeManager not found! Will load scene directly without fade.");
            }
        }

        // Validasi nama level
        if (string.IsNullOrEmpty(currentLevelName))
        {
            Debug.LogError($"{gameObject.name}: Current Level Name is empty! Please set it in Inspector.");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Cek apakah yang masuk adalah player
        if (!other.CompareTag("Player"))
            return;

        // Cegah trigger multiple kali
        if (hasTriggered)
            return;

        hasTriggered = true;

        // Freeze player jika diaktifkan
        if (freezePlayerOnFinish)
        {
            FreezePlayer(other.gameObject);
        }

        // Proses completion
        CompleteLevel();
    }

    /// <summary>
    /// Proses level completion:
    /// 1. Simpan progress
    /// 2. Unlock level berikutnya
    /// 3. Kembali ke LevelSelection
    /// </summary>
    void CompleteLevel()
    {
        // Log completion
        if (showCompletionMessage)
        {
            Debug.Log($"🎉 LEVEL COMPLETED: {currentLevelName}");
        }

        // Simpan progress dan unlock level berikutnya
        LevelProgressManager.Instance.CompleteLevel(currentLevelName);

        // Kembali ke LevelSelection setelah delay
        StartCoroutine(ReturnToLevelSelection());
    }

    /// <summary>
    /// Coroutine untuk kembali ke LevelSelection dengan delay
    /// </summary>
    IEnumerator ReturnToLevelSelection()
    {
        // Tunggu sebentar (untuk victory animation, sound effect, dll)
        yield return new WaitForSeconds(delayBeforeTransition);

        // Log navigation
        Debug.Log($"Returning to {levelSelectionSceneName}...");

        // Load scene dengan fade atau langsung
        if (fadeManager != null)
        {
            fadeManager.FadeOutAndLoadScene(levelSelectionSceneName);
        }
        else
        {
            // Fallback: load langsung
            UnityEngine.SceneManagement.SceneManager.LoadScene(levelSelectionSceneName);
        }
    }

    /// <summary>
    /// Freeze player movement (opsional)
    /// </summary>
    void FreezePlayer(GameObject player)
    {
        // Disable PlayerMovement script
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        // Stop Rigidbody2D velocity
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        Debug.Log("Player movement frozen");
    }

    /// <summary>
    /// Visualisasi trigger area di editor
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f); // Green transparent
            Gizmos.DrawCube(transform.position + (Vector3)col.offset, col.bounds.size);

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + (Vector3)col.offset, col.bounds.size);

            // Draw label
#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 0.5f,
                $"FINISH: {currentLevelName}",
                new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = Color.green },
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
                }
            );
#endif
        }
    }
}