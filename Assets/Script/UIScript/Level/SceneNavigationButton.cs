using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Generic button untuk navigasi scene (Back to Main Menu, Merchant, dll)
/// Reusable untuk button apapun yang pindah scene
/// </summary>
[RequireComponent(typeof(Button))]
public class SceneNavigationButton : MonoBehaviour
{
    [Header("Scene Configuration")]
    [SerializeField] private string targetSceneName = "MainMenu";
    [Tooltip("Nama scene tujuan (harus ada di Build Settings!)")]

    [Header("References")]
    [SerializeField] private FadeManager fadeManager;

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();

        // Auto-find FadeManager jika belum di-assign
        if (fadeManager == null)
        {
            fadeManager = FindFirstObjectByType<FadeManager>();
            if (fadeManager == null)
            {
                Debug.LogWarning($"FadeManager not found! {gameObject.name} will load scene directly without fade.");
            }
        }

        // Add listener
        button.onClick.AddListener(OnButtonClicked);
    }

    /// <summary>
    /// Dipanggil saat button diklik
    /// </summary>
    void OnButtonClicked()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError($"{gameObject.name}: Target scene name is empty!");
            return;
        }

        Debug.Log($"Navigating to scene: {targetSceneName}");

        // Load dengan fade jika ada FadeManager
        if (fadeManager != null)
        {
            fadeManager.FadeOutAndLoadScene(targetSceneName);
        }
        else
        {
            // Fallback: load langsung
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
        }
    }
}