using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script untuk navigasi antar scene tanpa mempengaruhi progress level
/// Attach ke GameObject dengan Collider2D (set as Trigger)
/// Ketika player masuk trigger, akan pindah ke scene yang ditentukan
/// TANPA menyimpan progress atau unlock level
///
/// Berguna untuk:
/// - Portal/pintu untuk kembali ke scene sebelumnya
/// - Testing navigation tanpa harus menyelesaikan level
/// - Shortcut antar area
/// - Exit door yang tidak mempengaruhi progress
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LevelNavigation : MonoBehaviour
{
    [Header("Target Scene")]
    [SerializeField] private string targetSceneName = "LevelSelection";
    [Tooltip("Scene tujuan yang akan di-load ketika player masuk trigger.\n" +
             "Contoh: MainMenu, LevelSelection, Stage1, Stage2, dll.")]

    [Header("Navigation Settings")]
    [SerializeField] private float delayBeforeTransition = 0f;
    [Tooltip("Delay sebelum pindah scene (untuk animasi/fade effect)")]

    [SerializeField] private bool showNavigationLog = true;
    [Tooltip("Tampilkan log navigasi di console?")]

    [Header("Optional: Player Freeze")]
    [SerializeField] private bool freezePlayerOnTrigger = false;
    [Tooltip("Freeze player movement saat trigger?")]

    [Header("Visual Settings")]
    [SerializeField] private Color gizmoColor = new Color(0f, 0.5f, 1f, 0.3f); // Blue transparent
    [Tooltip("Warna gizmo di Scene view")]

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

        // Validasi target scene
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError($"{gameObject.name}: Target Scene Name is empty! Please set it in Inspector.");
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
        if (freezePlayerOnTrigger)
        {
            FreezePlayer(other.gameObject);
        }

        // Navigate ke scene
        NavigateToScene();
    }

    /// <summary>
    /// Navigate ke target scene (TANPA menyimpan progress)
    /// </summary>
    void NavigateToScene()
    {
        // Log navigation
        if (showNavigationLog)
        {
            Debug.Log($"🔄 LevelNavigation: Moving to '{targetSceneName}' (No progress saved)");
        }

        // Navigate dengan atau tanpa delay
        if (delayBeforeTransition > 0)
        {
            StartCoroutine(LoadSceneWithDelay());
        }
        else
        {
            LoadScene();
        }
    }

    /// <summary>
    /// Load scene langsung
    /// </summary>
    void LoadScene()
    {
        SceneManager.LoadScene(targetSceneName);
    }

    /// <summary>
    /// Coroutine untuk load scene dengan delay
    /// </summary>
    IEnumerator LoadSceneWithDelay()
    {
        // Tunggu sebentar
        yield return new WaitForSeconds(delayBeforeTransition);

        // Load scene
        LoadScene();
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

        if (showNavigationLog)
        {
            Debug.Log("Player movement frozen");
        }
    }

    /// <summary>
    /// Visualisasi trigger area di editor
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            // Fill color
            Gizmos.color = gizmoColor;
            Gizmos.DrawCube(transform.position + (Vector3)col.offset, col.bounds.size);

            // Wireframe
            Color wireColor = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
            Gizmos.color = wireColor;
            Gizmos.DrawWireCube(transform.position + (Vector3)col.offset, col.bounds.size);

            // Draw label
#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 0.5f,
                $"NAVIGATE TO:\n{targetSceneName}",
                new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = wireColor },
                    fontSize = 11,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                }
            );
#endif
        }
    }

    // ==================== PUBLIC METHODS (Untuk dipanggil dari script lain) ====================

    /// <summary>
    /// Set target scene secara programmatic
    /// </summary>
    public void SetTargetScene(string sceneName)
    {
        targetSceneName = sceneName;
    }

    /// <summary>
    /// Trigger navigasi secara manual (tanpa collision)
    /// </summary>
    public void TriggerNavigation()
    {
        if (!hasTriggered)
        {
            hasTriggered = true;
            NavigateToScene();
        }
    }
}
