using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Scene Manager untuk Mountain Parallax Scene
/// Menggunakan PlayerMovement.cs biasa (manual control)
/// </summary>
public class MountainParallaxSceneManager : MonoBehaviour
{
    [Header("Scene References")]
    public PlayerMovement playerMovement; // ✅ Pakai PlayerMovement biasa
    public CameraZoomController cameraController;
    public AmbientSoundController ambientSound;
    
    [Header("Scene Timeline")]
    [Tooltip("Delay sebelum scene mulai")]
    public float startDelay = 1f;
    [Tooltip("Durasi scene parallax (player bisa explore)")]
    public float parallaxDuration = 15f;
    [Tooltip("Auto load scene berikutnya setelah duration")]
    public bool autoLoadNextScene = false;
    [Tooltip("Nama scene berikutnya")]
    public string nextSceneName = "NextScene";
    
    [Header("Camera Settings")]
    [Tooltip("Trigger zoom out saat scene start")]
    public bool startWithZoom = true;
    
    [Header("Fade Settings")]
    public CanvasGroup fadeCanvas;
    public float fadeInDuration = 1f;
    public float fadeOutDuration = 1f;
    
    [Header("Scene End Trigger")]
    [Tooltip("Trigger area untuk end scene (optional)")]
    public GameObject endSceneTrigger;
    
    [Header("Events")]
    public UnityEngine.Events.UnityEvent onSceneStart;
    public UnityEngine.Events.UnityEvent onSceneEnd;
    
    private bool sceneStarted = false;
    private bool sceneEnded = false;
    private float sceneTimer = 0f;
    
    void Start()
    {
        // Ensure time scale is normal
        Time.timeScale = 1f;
        
        // Start scene sequence
        StartCoroutine(SceneSequence());
    }
    
    void Update()
    {
        // Timer untuk auto-end scene (jika enabled)
        if (sceneStarted && !sceneEnded && autoLoadNextScene)
        {
            sceneTimer += Time.deltaTime;
            
            if (sceneTimer >= parallaxDuration)
            {
                TriggerEndScene();
            }
        }
    }
    
    IEnumerator SceneSequence()
    {
        // Disable player movement saat fade in
        if (playerMovement != null)
        {
            playerMovement.SetCanMove(false);
        }
        
        // Initial fade in
        if (fadeCanvas != null)
        {
            yield return StartCoroutine(FadeIn());
        }
        
        // Wait for start delay
        yield return new WaitForSeconds(startDelay);
        
        // Trigger scene start event
        onSceneStart?.Invoke();
        sceneStarted = true;
        
        // Start camera zoom out (optional)
        if (startWithZoom && cameraController != null)
        {
            cameraController.StartZoomOut();
        }
        
        // Enable player movement
        if (playerMovement != null)
        {
            playerMovement.SetCanMove(true);
        }
        
        // Player sekarang bisa explore dengan manual control!
    }
    
    /// <summary>
    /// Trigger end scene (called by timer or trigger area)
    /// </summary>
    public void TriggerEndScene()
    {
        if (!sceneEnded)
        {
            StartCoroutine(EndScene());
        }
    }
    
    IEnumerator EndScene()
    {
        if (sceneEnded) yield break;
        sceneEnded = true;
        
        // Trigger scene end event
        onSceneEnd?.Invoke();
        
        // Stop player movement
        if (playerMovement != null)
        {
            playerMovement.SetCanMove(false);
        }
        
        // Fade out
        if (fadeCanvas != null)
        {
            yield return StartCoroutine(FadeOut());
        }
        
        // Load next scene
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
    
    IEnumerator FadeIn()
    {
        float timer = 0f;
        fadeCanvas.alpha = 1f;
        fadeCanvas.blocksRaycasts = true;
        
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(1f, 0f, timer / fadeInDuration);
            yield return null;
        }
        
        fadeCanvas.alpha = 0f;
        fadeCanvas.blocksRaycasts = false;
    }
    
    IEnumerator FadeOut()
    {
        float timer = 0f;
        fadeCanvas.alpha = 0f;
        fadeCanvas.blocksRaycasts = true;
        
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(0f, 1f, timer / fadeOutDuration);
            yield return null;
        }
        
        fadeCanvas.alpha = 1f;
    }
    
    #region Public Control Methods
    
    /// <summary>
    /// Restart current scene
    /// </summary>
    public void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    /// <summary>
    /// Load specific scene with fade
    /// </summary>
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }
    
    IEnumerator LoadSceneAsync(string sceneName)
    {
        // Stop player
        if (playerMovement != null)
        {
            playerMovement.SetCanMove(false);
        }
        
        // Fade out
        if (fadeCanvas != null)
        {
            yield return StartCoroutine(FadeOut());
        }
        
        // Load scene
        SceneManager.LoadScene(sceneName);
    }
    
    /// <summary>
    /// Pause scene
    /// </summary>
    public void PauseScene()
    {
        if (playerMovement != null)
        {
            playerMovement.SetCanMove(false);
        }
        
        Time.timeScale = 0f;
    }
    
    /// <summary>
    /// Resume scene
    /// </summary>
    public void ResumeScene()
    {
        if (playerMovement != null)
        {
            playerMovement.SetCanMove(true);
        }
        
        Time.timeScale = 1f;
    }
    
    #endregion
    
    #region Trigger Handler
    
    /// <summary>
    /// Called oleh trigger area untuk end scene
    /// </summary>
    public void OnPlayerReachEnd()
    {
        TriggerEndScene();
    }
    
    #endregion
}