using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script untuk button level individual
/// Handle visual locked/unlocked dan load scene dengan fade
/// </summary>
[RequireComponent(typeof(Button))]
public class LevelButton : MonoBehaviour
{
    [Header("Level Configuration")]
    [SerializeField] private string levelSceneName = "Stage1";
    [Tooltip("Nama level untuk checking unlock status (harus sama dengan di LevelProgressManager)")]

    [Header("Visual Settings")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Grayscale

    [Header("References")]
    [SerializeField] private FadeManager fadeManager;

    private Button button;
    private Color originalColor;
    private bool isUnlocked;

    void Start()
    {
        button = GetComponent<Button>();

        // Save original color
        if (buttonImage != null)
        {
            originalColor = buttonImage.color;
        }
        else
        {
            // Fallback: gunakan image dari button itu sendiri
            buttonImage = GetComponent<Image>();
            if (buttonImage != null)
            {
                originalColor = buttonImage.color;
            }
        }

        // Auto-find FadeManager jika belum di-assign
        if (fadeManager == null)
        {
            fadeManager = FindFirstObjectByType<FadeManager>();
            if (fadeManager == null)
            {
                Debug.LogWarning($"FadeManager not found! {gameObject.name} will load scene directly without fade.");
            }
        }

        // Setup button
        UpdateButtonState();
        button.onClick.AddListener(OnButtonClicked);
    }

    /// <summary>
    /// Update visual dan interactability berdasarkan unlock status
    /// </summary>
    void UpdateButtonState()
    {
        // Cek unlock status dari LevelProgressManager
        isUnlocked = LevelProgressManager.Instance.IsLevelUnlocked(levelSceneName);

        if (isUnlocked)
        {
            // UNLOCKED STATE
            button.interactable = true;
            if (buttonImage != null)
            {
                buttonImage.color = originalColor;
            }
        }
        else
        {
            // LOCKED STATE
            button.interactable = false;
            if (buttonImage != null)
            {
                buttonImage.color = lockedColor; // Grayscale
            }
        }

        Debug.Log($"{levelSceneName} - Unlocked: {isUnlocked}");
    }

    /// <summary>
    /// Dipanggil saat button diklik
    /// </summary>
    void OnButtonClicked()
    {
        if (!isUnlocked)
        {
            Debug.LogWarning($"{levelSceneName} is locked!");
            return;
        }

        Debug.Log($"Loading level: {levelSceneName}");

        // Load dengan fade jika ada FadeManager
        if (fadeManager != null)
        {
            fadeManager.FadeOutAndLoadScene(levelSceneName);
        }
        else
        {
            // Fallback: load langsung
            UnityEngine.SceneManagement.SceneManager.LoadScene(levelSceneName);
        }
    }

    /// <summary>
    /// Public method untuk refresh button state (misal setelah unlock level baru)
    /// </summary>
    public void RefreshButtonState()
    {
        UpdateButtonState();
    }
}