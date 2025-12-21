using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.SceneManagement;

public class TimerManager : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("Waktu awal dalam detik untuk New Game")]
    public float startTime = 1800f; // 30 menit = 1800 detik

    [Tooltip("Apakah timer berjalan otomatis saat scene start?")]
    public bool startOnAwake = true;

    [Header("Scene Configuration")]
    [Tooltip("Nama scene level selection (timer akan pause di scene ini)")]
    public string levelSelectionSceneName = "LevelSelection";

    [Tooltip("List nama scene gameplay (Stage1, Stage2, dll) - timer akan running di scene ini")]
    public string[] gameplaySceneNames = { "Stage1", "Stage2", "Stage3", "Stage4", "Stage5" };

    [Header("UI References")]
    [Tooltip("Text untuk display timer (format MM:SS)")]
    public TextMeshProUGUI timerText;

    [Header("Events")]
    [Tooltip("Event dipanggil saat waktu habis")]
    public UnityEvent OnTimeUp;

    [Tooltip("Event dipanggil setiap detik (untuk update UI/audio)")]
    public UnityEvent<float> OnTimeChanged;

    [Header("Debug Mode")]
    [Tooltip("Aktifkan debug mode untuk testing")]
    public bool debugMode = false;

    [Tooltip("Tampilkan log debug di console?")]
    public bool showDebugLogs = false;

    [Header("Debug Controls (Hanya saat Debug Mode aktif)")]
    [Tooltip("Set waktu custom (dalam detik) - Tekan D untuk apply")]
    public float debugSetTime = 300f; // 5 menit

    [Tooltip("Freeze timer (tidak countdown) - Toggle dengan F")]
    public bool debugFreezeTimer = false;

    // PlayerPrefs Keys
    private const string TIMER_SAVE_KEY = "RushOfFate_CurrentTime";
    private const string TIMER_RUNNING_KEY = "RushOfFate_TimerRunning";
    private const string NEW_GAME_KEY = "RushOfFate_NewGame";

    // Private variables
    private float currentTime;
    private bool isRunning = false;
    private bool timeUpTriggered = false;
    private bool isPausedByDeath = false;
    private string currentSceneName;

    // Singleton pattern (agar tidak ada duplicate timer manager)
    private static TimerManager instance;

    // Properties untuk diakses script lain
    public float CurrentTime => currentTime;
    public bool IsRunning => isRunning;
    public float TimeRemaining => Mathf.Max(0, currentTime);
    public bool IsPausedByDeath => isPausedByDeath;

    void Awake()
    {
        // Singleton pattern - hanya satu TimerManager di scene
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // Get current scene name
        currentSceneName = SceneManager.GetActiveScene().name;

        if (showDebugLogs)
        {
            Debug.Log($"[TimerManager] Awake in scene: {currentSceneName}");
        }
    }

    void Start()
    {
        // Check apakah ini New Game
        bool isNewGame = PlayerPrefs.GetInt(NEW_GAME_KEY, 1) == 1;

        if (isNewGame)
        {
            // NEW GAME - Reset timer
            currentTime = startTime;
            timeUpTriggered = false;

            // Save initial time
            SaveTimerState();

            // Clear new game flag
            PlayerPrefs.SetInt(NEW_GAME_KEY, 0);
            PlayerPrefs.Save();

            if (showDebugLogs)
            {
                Debug.Log($"[TimerManager] NEW GAME - Timer set to {FormatTime(currentTime)}");
            }
        }
        else
        {
            // LOAD SAVED TIME
            LoadTimerState();

            if (showDebugLogs)
            {
                Debug.Log($"[TimerManager] Loaded saved time: {FormatTime(currentTime)}");
            }
        }

        // Update UI initial
        UpdateTimerDisplay();

        // Tentukan apakah timer harus running berdasarkan scene
        if (IsGameplayScene(currentSceneName))
        {
            // Di gameplay scene - start timer
            if (startOnAwake)
            {
                StartTimer();
            }
        }
        else
        {
            // Di level selection atau scene lain - pause timer
            StopTimer();

            if (showDebugLogs)
            {
                Debug.Log($"[TimerManager] Timer paused - not in gameplay scene");
            }
        }
    }

    void Update()
    {
        // ========== DEBUG MODE CONTROLS ==========
        if (debugMode)
        {
            HandleDebugInput();
        }

        // ========== TIMER COUNTDOWN ==========
        // Jangan countdown jika:
        // 1. Timer tidak running
        // 2. Paused by death
        // 3. Debug freeze aktif
        if (!isRunning || isPausedByDeath || (debugMode && debugFreezeTimer))
        {
            return;
        }

        // Countdown
        currentTime -= Time.deltaTime;

        // Clamp ke 0
        if (currentTime < 0)
        {
            currentTime = 0;

            // Trigger time up event (hanya sekali)
            if (!timeUpTriggered)
            {
                timeUpTriggered = true;
                OnTimeUp?.Invoke();
                StopTimer();

                if (showDebugLogs)
                {
                    Debug.Log("[TimerManager] ⏰ TIME'S UP!");
                }
            }
        }

        // Update UI setiap frame
        UpdateTimerDisplay();

        // Trigger time changed event
        OnTimeChanged?.Invoke(currentTime);

        // Auto-save setiap 1 detik (untuk prevent data loss)
        if (Time.frameCount % 60 == 0) // Asumsi 60 FPS = 1 detik
        {
            SaveTimerState();
        }
    }

    /// <summary>
    /// Handle debug keyboard input
    /// </summary>
    void HandleDebugInput()
    {
        // D = Set custom time
        if (Input.GetKeyDown(KeyCode.D))
        {
            SetTime(debugSetTime);
            Debug.Log($"[DEBUG] Time set to {FormatTime(debugSetTime)}");
        }

        // F = Toggle freeze timer
        if (Input.GetKeyDown(KeyCode.F))
        {
            debugFreezeTimer = !debugFreezeTimer;
            Debug.Log($"[DEBUG] Timer freeze: {(debugFreezeTimer ? "ON" : "OFF")}");
        }

        // P = Toggle pause/resume
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isRunning)
            {
                StopTimer();
                Debug.Log("[DEBUG] Timer paused manually");
            }
            else
            {
                StartTimer();
                Debug.Log("[DEBUG] Timer resumed manually");
            }
        }

        // PageUp = Add 60 seconds
        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            AddTime(60f);
            Debug.Log("[DEBUG] Added 60 seconds");
        }

        // PageDown = Subtract 60 seconds
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            SubtractTime(60f);
            Debug.Log("[DEBUG] Subtracted 60 seconds");
        }

        // Home = Add 600 seconds (10 minutes)
        if (Input.GetKeyDown(KeyCode.Home))
        {
            AddTime(600f);
            Debug.Log("[DEBUG] Added 10 minutes");
        }

        // End = Subtract 600 seconds (10 minutes)
        if (Input.GetKeyDown(KeyCode.End))
        {
            SubtractTime(600f);
            Debug.Log("[DEBUG] Subtracted 10 minutes");
        }
    }

    /// <summary>
    /// Pause timer saat player mati (dipanggil oleh PlayerHealth)
    /// </summary>
    public void PauseOnDeath()
    {
        if (isPausedByDeath) return; // Already paused

        isPausedByDeath = true;
        SaveTimerState(); // Save before pausing

        if (showDebugLogs)
        {
            Debug.Log("[TimerManager] ⏸ Timer paused - Player died");
        }
    }

    /// <summary>
    /// Resume timer saat player respawn (dipanggil oleh PlayerHealth)
    /// </summary>
    public void ResumeOnRespawn()
    {
        if (!isPausedByDeath) return; // Not paused by death

        isPausedByDeath = false;

        if (showDebugLogs)
        {
            Debug.Log("[TimerManager] ▶ Timer resumed - Player respawned");
        }
    }

    /// <summary>
    /// Save timer state ke PlayerPrefs
    /// </summary>
    void SaveTimerState()
    {
        PlayerPrefs.SetFloat(TIMER_SAVE_KEY, currentTime);
        PlayerPrefs.SetInt(TIMER_RUNNING_KEY, isRunning ? 1 : 0);
        PlayerPrefs.Save();

        if (showDebugLogs && Time.frameCount % 300 == 0) // Log setiap 5 detik
        {
            Debug.Log($"[TimerManager] 💾 Saved: {FormatTime(currentTime)}");
        }
    }

    /// <summary>
    /// Load timer state dari PlayerPrefs
    /// </summary>
    void LoadTimerState()
    {
        currentTime = PlayerPrefs.GetFloat(TIMER_SAVE_KEY, startTime);
        bool wasRunning = PlayerPrefs.GetInt(TIMER_RUNNING_KEY, 0) == 1;

        // Clamp time
        currentTime = Mathf.Max(0, currentTime);

        if (showDebugLogs)
        {
            Debug.Log($"[TimerManager] 📂 Loaded: {FormatTime(currentTime)} (Was running: {wasRunning})");
        }
    }

    /// <summary>
    /// Check apakah scene adalah gameplay scene
    /// </summary>
    bool IsGameplayScene(string sceneName)
    {
        foreach (string gameplayScene in gameplaySceneNames)
        {
            if (sceneName == gameplayScene)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Reset timer untuk New Game (dipanggil dari Main Menu)
    /// </summary>
    public static void PrepareNewGame()
    {
        PlayerPrefs.SetInt(NEW_GAME_KEY, 1);
        PlayerPrefs.Save();

        Debug.Log("[TimerManager] 🎮 Prepared for NEW GAME");
    }

    /// <summary>
    /// Tambah waktu ke timer (dipanggil oleh ItemPickup)
    /// </summary>
    public void AddTime(float seconds)
    {
        currentTime += seconds;

        // Clamp ke max time jika perlu (opsional)
        // currentTime = Mathf.Min(currentTime, maxTime);

        UpdateTimerDisplay();
        SaveTimerState(); // Save after adding time

        if (showDebugLogs)
        {
            Debug.Log($"[TimerManager] ➕ Added {seconds} seconds. Current time: {FormatTime(currentTime)}");
        }
    }

    /// <summary>
    /// Kurangi waktu dari timer
    /// </summary>
    public void SubtractTime(float seconds)
    {
        currentTime -= seconds;
        currentTime = Mathf.Max(0, currentTime);

        UpdateTimerDisplay();
        SaveTimerState(); // Save after subtracting time

        if (showDebugLogs)
        {
            Debug.Log($"[TimerManager] ➖ Subtracted {seconds} seconds. Current time: {FormatTime(currentTime)}");
        }
    }

    /// <summary>
    /// Start/resume timer
    /// </summary>
    public void StartTimer()
    {
        isRunning = true;

        if (showDebugLogs)
        {
            Debug.Log("[TimerManager] Timer started");
        }
    }

    /// <summary>
    /// Pause timer
    /// </summary>
    public void StopTimer()
    {
        isRunning = false;

        if (showDebugLogs)
        {
            Debug.Log("[TimerManager] Timer stopped");
        }
    }

    /// <summary>
    /// Reset timer ke waktu awal
    /// </summary>
    public void ResetTimer()
    {
        currentTime = startTime;
        timeUpTriggered = false;
        UpdateTimerDisplay();

        if (showDebugLogs)
        {
            Debug.Log("[TimerManager] Timer reset");
        }
    }

    /// <summary>
    /// Set waktu secara manual
    /// </summary>
    public void SetTime(float seconds)
    {
        currentTime = seconds;
        currentTime = Mathf.Max(0, currentTime); // Prevent negative time
        timeUpTriggered = false; // Reset time up flag

        UpdateTimerDisplay();
        SaveTimerState(); // Save after setting time

        if (showDebugLogs)
        {
            Debug.Log($"[TimerManager] ⏱ Time set to {FormatTime(currentTime)}");
        }
    }

    /// <summary>
    /// Update UI text dengan format MM:SS
    /// </summary>
    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            timerText.text = FormatTime(currentTime);
        }
    }

    /// <summary>
    /// Format waktu ke string MM:SS
    /// </summary>
    string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);

        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    /// <summary>
    /// Check apakah waktu sudah habis
    /// </summary>
    public bool IsTimeUp()
    {
        return currentTime <= 0;
    }
}