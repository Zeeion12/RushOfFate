using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FadeManager fadeManager;

    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "LevelSelection";

    // ✅ EXISTING METHOD - TETAP SAMA
    public void OnPlayButtonClicked()
    {
        Debug.Log("Play button clicked!");

        // ✅ TAMBAHKAN INI DI AWAL METHOD:
        // Prepare timer untuk new game
        TimerManager.PrepareNewGame();

        if (fadeManager != null)
        {
            fadeManager.FadeOutAndLoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("FadeManager not assigned!");
            LoadSceneByName(gameSceneName);
        }
    }

    // ✅ OPSIONAL: Tambahkan method Continue jika mau fitur Continue Game
    public void OnContinueButtonClicked()
    {
        Debug.Log("Continue button clicked!");

        // TIDAK call PrepareNewGame() - langsung load saja
        // Timer akan load dari save

        if (fadeManager != null)
        {
            fadeManager.FadeOutAndLoadScene(gameSceneName);
        }
        else
        {
            LoadSceneByName(gameSceneName);
        }
    }

    // ✅ EXISTING METHODS - TETAP SAMA, TIDAK DIUBAH
    public void OnQuitButtonClicked()
    {
        Debug.Log("Quit button clicked!");

        if (fadeManager != null)
        {
            fadeManager.FadeOutAndQuit();
        }
        else
        {
            QuitGame();
        }
    }

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

    public void QuitGame()
    {
        Debug.Log("Keluar dari aplikasi...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}