using UnityEngine;

/// <summary>
/// Singleton manager untuk menyimpan dan mengambil progress level pemain
/// Menggunakan PlayerPrefs untuk simplicity (26 hari deadline!)
/// </summary>
public class LevelProgressManager : MonoBehaviour
{
    // Singleton instance
    private static LevelProgressManager instance;
    public static LevelProgressManager Instance
    {
        get
        {
            if (instance == null)
            {
                // Cari di scene
                instance = FindFirstObjectByType<LevelProgressManager>();

                // Jika tidak ada, buat GameObject baru
                if (instance == null)
                {
                    GameObject go = new GameObject("LevelProgressManager");
                    instance = go.AddComponent<LevelProgressManager>();
                }
            }
            return instance;
        }
    }

    // PlayerPrefs keys
    private const string LEVEL_PROGRESS_KEY = "LevelProgress_";

    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // PENTING: Tutorial Stage selalu unlocked di awal
        if (!HasProgress())
        {
            InitializeProgress();
        }
    }

    /// <summary>
    /// Cek apakah sudah pernah ada progress (first time play)
    /// </summary>
    bool HasProgress()
    {
        return PlayerPrefs.HasKey(LEVEL_PROGRESS_KEY + "TutorialStage");
    }

    /// <summary>
    /// Initialize progress pertama kali (Tutorial unlocked)
    /// </summary>
    void InitializeProgress()
    {
        Debug.Log("First time play - unlocking Tutorial Stage");
        UnlockLevel("TutorialStage");
    }

    /// <summary>
    /// Unlock level tertentu
    /// </summary>
    public void UnlockLevel(string levelName)
    {
        PlayerPrefs.SetInt(LEVEL_PROGRESS_KEY + levelName, 1);
        PlayerPrefs.Save();
        Debug.Log($"Level unlocked: {levelName}");
    }

    /// <summary>
    /// Cek apakah level sudah unlocked
    /// </summary>
    public bool IsLevelUnlocked(string levelName)
    {
        return PlayerPrefs.GetInt(LEVEL_PROGRESS_KEY + levelName, 0) == 1;
    }

    /// <summary>
    /// Panggil ini saat player menyelesaikan level
    /// Otomatis unlock level berikutnya berdasarkan progression
    /// </summary>
    public void CompleteLevel(string completedLevelName)
    {
        Debug.Log($"Level completed: {completedLevelName}");

        // Mapping level ke level berikutnya
        string nextLevel = GetNextLevel(completedLevelName);

        if (!string.IsNullOrEmpty(nextLevel))
        {
            UnlockLevel(nextLevel);
            Debug.Log($"Next level unlocked: {nextLevel}");
        }
        else
        {
            Debug.Log("All levels completed!");
        }
    }

    /// <summary>
    /// Get nama level berikutnya berdasarkan progression
    /// </summary>
    string GetNextLevel(string currentLevel)
    {
        switch (currentLevel)
        {
            case "TutorialStage":
                return "Stage1";
            case "Stage1":
                return "Stage2";
            case "Stage2":
                return "Stage3";
            case "Stage3":
                return "Stage4";
            case "Stage4":
                return "Stage5";
            case "Stage5":
                return null; // Sudah level terakhir
            default:
                Debug.LogWarning($"Unknown level: {currentLevel}");
                return null;
        }
    }

    /// <summary>
    /// DEBUGGING: Reset semua progress (untuk testing)
    /// </summary>
    [ContextMenu("Reset All Progress")]
    public void ResetAllProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        InitializeProgress();
        Debug.Log("All progress reset! Tutorial unlocked.");
    }

    /// <summary>
    /// DEBUGGING: Unlock semua level (untuk testing)
    /// </summary>
    [ContextMenu("Unlock All Levels")]
    public void UnlockAllLevels()
    {
        UnlockLevel("TutorialStage");
        UnlockLevel("Stage1");
        UnlockLevel("Stage2");
        UnlockLevel("Stage3");
        UnlockLevel("Stage4");
        UnlockLevel("Stage5");
        Debug.Log("All levels unlocked!");
    }
}