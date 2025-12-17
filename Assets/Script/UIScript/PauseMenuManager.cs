using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Manager untuk Pause Menu system di Rush of Fate
/// Handle pause/resume game dan navigation ke main menu
/// </summary>
public class PauseMenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pauseMenuCanvas;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Input")]
    [SerializeField] private InputActionReference pauseAction;

    [Header("Scene Navigation")]
    [SerializeField] private string levelSelectionSceneName = "LevelSelection";
    [Tooltip("Scene yang akan di-load saat klik Main Menu (biasanya LevelSelection)")]

    [Header("References")]
    [SerializeField] private FadeManager fadeManager;

    [Header("Audio (Optional - Commented for now)")]
    // [SerializeField] private AudioSource buttonClickSFX;
    // [SerializeField] private AudioSource pauseOpenSFX;
    // [SerializeField] private AudioSource pauseCloseSFX;

    // State
    private bool isPaused = false;

    void Start()
    {
        // Validasi references
        ValidateReferences();

        // Auto-find FadeManager jika belum di-assign
        if (fadeManager == null)
        {
            fadeManager = FindFirstObjectByType<FadeManager>();
            if (fadeManager == null)
            {
                Debug.LogWarning("FadeManager not found! Scene transition will happen without fade.");
            }
        }

        // Setup button listeners
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeButtonClicked);
        else
            Debug.LogError("Resume Button not assigned!");

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        else
            Debug.LogError("Main Menu Button not assigned!");

        // Hide pause menu at start
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);
        else
            Debug.LogError("Pause Menu Canvas not assigned!");

        // Initial state: game is running
        Time.timeScale = 1f;
        isPaused = false;
    }

    void OnEnable()
    {
        // Enable pause input action
        if (pauseAction != null)
        {
            pauseAction.action.Enable();
            pauseAction.action.performed += OnPauseInput;
        }
        else
        {
            Debug.LogError("Pause Action not assigned!");
        }
    }

    void OnDisable()
    {
        // Disable pause input action
        if (pauseAction != null)
        {
            pauseAction.action.Disable();
            pauseAction.action.performed -= OnPauseInput;
        }

        // Cleanup button listeners
        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(OnResumeButtonClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(OnMainMenuButtonClicked);
    }

    /// <summary>
    /// Dipanggil saat player menekan tombol Pause (ESC)
    /// </summary>
    void OnPauseInput(InputAction.CallbackContext context)
    {
        TogglePause();
    }

    /// <summary>
    /// Toggle between pause and resume
    /// </summary>
    void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    /// <summary>
    /// Pause the game
    /// </summary>
    void PauseGame()
    {
        isPaused = true;

        // Show pause menu
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(true);

        // Freeze game time
        Time.timeScale = 0f;

        // TODO: Play pause open sound effect
        // if (pauseOpenSFX != null)
        //     pauseOpenSFX.Play();

        Debug.Log("Game Paused");
    }

    /// <summary>
    /// Resume the game
    /// </summary>
    void ResumeGame()
    {
        isPaused = false;

        // Hide pause menu
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);

        // Unfreeze game time
        Time.timeScale = 1f;

        // TODO: Play pause close sound effect
        // if (pauseCloseSFX != null)
        //     pauseCloseSFX.Play();

        Debug.Log("Game Resumed");
    }

    /// <summary>
    /// Dipanggil saat Resume button diklik
    /// </summary>
    void OnResumeButtonClicked()
    {
        // TODO: Play button click sound
        // if (buttonClickSFX != null)
        //     buttonClickSFX.Play();

        ResumeGame();
    }

    /// <summary>
    /// Dipanggil saat Main Menu button diklik
    /// </summary>
    void OnMainMenuButtonClicked()
    {
        // TODO: Play button click sound
        // if (buttonClickSFX != null)
        //     buttonClickSFX.Play();

        Debug.Log("Returning to Level Selection...");

        // IMPORTANT: Unfreeze time sebelum load scene baru
        // (Fade effect butuh time berjalan!)
        Time.timeScale = 1f;

        // Load scene dengan fade
        if (fadeManager != null)
        {
            fadeManager.FadeOutAndLoadScene(levelSelectionSceneName);
        }
        else
        {
            // Fallback: load langsung tanpa fade
            UnityEngine.SceneManagement.SceneManager.LoadScene(levelSelectionSceneName);
        }
    }

    /// <summary>
    /// Validasi semua references yang dibutuhkan
    /// </summary>
    void ValidateReferences()
    {
        if (pauseMenuCanvas == null)
            Debug.LogError($"{gameObject.name}: Pause Menu Canvas is not assigned!");

        if (resumeButton == null)
            Debug.LogError($"{gameObject.name}: Resume Button is not assigned!");

        if (mainMenuButton == null)
            Debug.LogError($"{gameObject.name}: Main Menu Button is not assigned!");

        if (pauseAction == null)
            Debug.LogError($"{gameObject.name}: Pause Action is not assigned!");

        if (string.IsNullOrEmpty(levelSelectionSceneName))
            Debug.LogError($"{gameObject.name}: Level Selection Scene Name is empty!");
    }

    /// <summary>
    /// Public method untuk pause dari script lain (opsional)
    /// </summary>
    public void Pause()
    {
        if (!isPaused)
            PauseGame();
    }

    /// <summary>
    /// Public method untuk resume dari script lain (opsional)
    /// </summary>
    public void Resume()
    {
        if (isPaused)
            ResumeGame();
    }

    /// <summary>
    /// Check apakah game sedang paused (untuk script lain)
    /// </summary>
    public bool IsPaused()
    {
        return isPaused;
    }
}