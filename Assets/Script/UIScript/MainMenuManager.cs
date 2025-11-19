using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class MainMenuManager : MonoBehaviour
{
    public void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("LoadSceneByName dipanggil tapi sceneName kosong.");
            return;
        }

        // (Opsional) cek apakah scene ada di Build Settings sebelum load
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"Scene '{sceneName}' tidak ada di Build Settings!");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    // Pilihan: fungsi quit (tanpa parameter) bila perlu
    public void QuitGame()
    {
        Debug.Log("Keluar dari aplikasi...");
        Application.Quit();
    }
}
