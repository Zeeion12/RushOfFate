using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FadeManager fadeManager;

    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "LevelSelection"; // SESUAIKAN dengan nama scene game Anda!

    /// <summary>
    /// Dipanggil saat tombol PLAY diklik
    /// </summary>
    public void OnPlayButtonClicked()
    {
        Debug.Log("Play button clicked!");

        if (fadeManager != null)
        {
            // Fade out lalu load scene game
            fadeManager.FadeOutAndLoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("FadeManager not assigned!");
            // Fallback: load langsung tanpa fade
            LoadSceneByName(gameSceneName);
        }
    }

    /// <summary>
    /// Dipanggil saat tombol QUIT diklik
    /// </summary>
    public void OnQuitButtonClicked()
    {
        Debug.Log("Quit button clicked!");

        if (fadeManager != null)
        {
            // Fade out lalu quit
            fadeManager.FadeOutAndQuit();
        }
        else
        {
            // Fallback: quit langsung
            QuitGame();
        }
    }

    /// <summary>
    /// Load scene langsung tanpa fade (fungsi lama - untuk backup)
    /// </summary>
    public void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("LoadSceneByName dipanggil tapi sceneName kosong.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"Scene '{sceneName}' tidak ada di Build Settings!");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Quit game langsung tanpa fade (fungsi lama - untuk backup)
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Keluar dari aplikasi...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}