using UnityEngine;

/// <summary>
/// Manager untuk mendeteksi build baru dan auto-reset progress
/// Sistem ini akan reset semua data ketika versi build berubah
/// </summary>
public class BuildVersionManager : MonoBehaviour
{
    private static BuildVersionManager instance;
    public static BuildVersionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<BuildVersionManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("BuildVersionManager");
                    instance = go.AddComponent<BuildVersionManager>();
                }
            }
            return instance;
        }
    }

    // Key untuk menyimpan versi build terakhir
    private const string BUILD_VERSION_KEY = "LastBuildVersion";
    private const string FIRST_RUN_KEY = "IsFirstRun";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            CheckBuildVersion();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Cek apakah ini build baru atau pertama kali run
    /// Jika iya, reset semua progress
    /// </summary>
    void CheckBuildVersion()
    {
        // Get versi build dari Unity
        string currentVersion = Application.version;
        string lastVersion = PlayerPrefs.GetString(BUILD_VERSION_KEY, "");
        bool isFirstRun = PlayerPrefs.GetInt(FIRST_RUN_KEY, 1) == 1;

        Debug.Log($"[BuildVersion] Current: {currentVersion}, Last: {lastVersion}, First Run: {isFirstRun}");

#if !UNITY_EDITOR
        // HANYA DI BUILD (bukan di Editor)
        // Reset jika:
        // 1. Pertama kali run (lastVersion kosong)
        // 2. Versi berbeda dengan versi terakhir
        if (string.IsNullOrEmpty(lastVersion) || lastVersion != currentVersion || isFirstRun)
        {
            Debug.Log("[BuildVersion] New build detected! Resetting all progress...");
            ResetAllGameData();

            // Simpan versi build yang baru
            PlayerPrefs.SetString(BUILD_VERSION_KEY, currentVersion);
            PlayerPrefs.SetInt(FIRST_RUN_KEY, 0);
            PlayerPrefs.Save();

            Debug.Log($"[BuildVersion] Progress reset complete! New version saved: {currentVersion}");
        }
        else
        {
            Debug.Log("[BuildVersion] Same build version, keeping progress.");
        }
#else
        // Di Editor, hanya log
        Debug.Log("[BuildVersion] Running in Editor - auto-reset disabled");
#endif
    }

    /// <summary>
    /// Reset SEMUA data game (progress, inventory, dll)
    /// </summary>
    void ResetAllGameData()
    {
        // Simpan versi sebelum delete all (agar tidak hilang)
        string currentVersion = Application.version;

        // NUCLEAR RESET: Hapus semua PlayerPrefs
        PlayerPrefs.DeleteAll();

        // Restore versi build
        PlayerPrefs.SetString(BUILD_VERSION_KEY, currentVersion);
        PlayerPrefs.SetInt(FIRST_RUN_KEY, 0);
        PlayerPrefs.Save();

        Debug.Log("[BuildVersion] All game data has been reset!");

        // Reinitialize managers (jika sudah ada)
        if (LevelProgressManager.Instance != null)
        {
            // Force re-initialize progress (unlock Tutorial)
            LevelProgressManager.Instance.ResetAllProgress();
        }

        if (InventoryManager.Instance != null)
        {
            // Clear inventory
            InventoryManager.Instance.NuclearResetAllData();
        }
    }

    /// <summary>
    /// Manual reset - bisa dipanggil dari tombol/cheat code
    /// </summary>
    public void ManualResetAllData()
    {
        Debug.Log("[BuildVersion] Manual reset triggered!");
        ResetAllGameData();
    }

#if UNITY_EDITOR
    [ContextMenu("Debug: Force Reset (Simulate New Build)")]
    void DebugForceReset()
    {
        PlayerPrefs.DeleteKey(BUILD_VERSION_KEY);
        PlayerPrefs.DeleteKey(FIRST_RUN_KEY);
        PlayerPrefs.Save();
        Debug.Log("[BuildVersion] Cleared version keys. Next run will trigger auto-reset.");
    }

    [ContextMenu("Debug: Show Current Version")]
    void DebugShowVersion()
    {
        Debug.Log($"Current Version: {Application.version}");
        Debug.Log($"Last Saved Version: {PlayerPrefs.GetString(BUILD_VERSION_KEY, "NONE")}");
        Debug.Log($"Is First Run: {PlayerPrefs.GetInt(FIRST_RUN_KEY, 1) == 1}");
    }
#endif
}
