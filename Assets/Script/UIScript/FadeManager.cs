using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeManager : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;

    private void Start()
    {
        // Pastikan raycast dimatikan di awal
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// Fade out lalu load scene
    /// </summary>
    public void FadeOutAndLoadScene(string sceneName)
    {
        StartCoroutine(FadeOutThenLoadScene(sceneName));
    }

    /// <summary>
    /// Fade out lalu quit game
    /// </summary>
    public void FadeOutAndQuit()
    {
        StartCoroutine(FadeOutThenQuit());
    }

    /// <summary>
    /// Coroutine: Fade to black lalu load scene
    /// </summary>
    private IEnumerator FadeOutThenLoadScene(string sceneName)
    {
        // Block input saat fade
        fadeCanvasGroup.blocksRaycasts = true;

        // Fade to black
        yield return StartCoroutine(FadeCoroutine(0f, 1f));

        // Setelah fade selesai, load scene
        Debug.Log($"Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Coroutine: Fade to black lalu quit
    /// </summary>
    private IEnumerator FadeOutThenQuit()
    {
        // Block input saat fade
        fadeCanvasGroup.blocksRaycasts = true;

        // Fade to black
        yield return StartCoroutine(FadeCoroutine(0f, 1f));

        // Setelah fade selesai, quit
        Debug.Log("Quitting game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    /// <summary>
    /// Fade animation coroutine
    /// </summary>
    private IEnumerator FadeCoroutine(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            fadeCanvasGroup.alpha = newAlpha;
            yield return null;
        }

        fadeCanvasGroup.alpha = endAlpha;
        Debug.Log($"Fade complete! Alpha: {endAlpha}");
    }

    /// <summary>
    /// Fade in (untuk scene baru) - OPSIONAL
    /// </summary>
    public void FadeIn()
    {
        StartCoroutine(FadeInCoroutine());
    }

    private IEnumerator FadeInCoroutine()
    {
        // Fade dari hitam ke transparan
        yield return StartCoroutine(FadeCoroutine(1f, 0f));

        // Unblock input setelah fade in
        fadeCanvasGroup.blocksRaycasts = false;
    }
}