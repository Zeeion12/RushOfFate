using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance;
    
    [Header("Fade Settings")]
    public float fadeDuration = 1f;
    public Color fadeColor = Color.black;
    
    [Header("Audio")]
    public AudioClip transitionSound;
    
    private Canvas fadeCanvas;
    private Image fadeImage;
    private bool isFading = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateFadeCanvas();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Recreate canvas setiap scene load untuk avoid missing reference
        if (fadeCanvas == null || fadeImage == null)
        {
            CreateFadeCanvas();
        }
    }
    
    private void CreateFadeCanvas()
    {
        // Destroy old canvas if exists
        if (fadeCanvas != null)
        {
            Destroy(fadeCanvas.gameObject);
        }
        
        // Create new canvas
        GameObject canvasObj = new GameObject("FadeCanvas");
        canvasObj.transform.SetParent(transform);
        
        fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9999; // Highest priority
        
        // Add Canvas Scaler
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Add Graphic Raycaster
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create fade image
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        
        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
        fadeImage.raycastTarget = false;
        
        // Stretch to full screen
        RectTransform rectTransform = fadeImage.rectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        
        Debug.Log("Fade Canvas created");
    }
    
    public void FadeToScene(int sceneIndex)
    {
        if (!isFading)
        {
            StartCoroutine(FadeAndLoadScene(sceneIndex));
        }
    }
    
    public void FadeToScene(string sceneName)
    {
        if (!isFading)
        {
            StartCoroutine(FadeAndLoadSceneByName(sceneName));
        }
    }
    
    private IEnumerator FadeAndLoadScene(int sceneIndex)
    {
        isFading = true;
        
        // Fade out
        yield return StartCoroutine(FadeOut());
        
        // Load scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // Recreate canvas after scene load
        yield return new WaitForEndOfFrame();
        CreateFadeCanvas();
        
        // Set to black
        if (fadeImage != null)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1);
        }
        
        // Fade in
        yield return StartCoroutine(FadeIn());
        
        isFading = false;
    }
    
    private IEnumerator FadeAndLoadSceneByName(string sceneName)
    {
        isFading = true;
        
        yield return StartCoroutine(FadeOut());
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        yield return new WaitForEndOfFrame();
        CreateFadeCanvas();
        
        if (fadeImage != null)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1);
        }
        
        yield return StartCoroutine(FadeIn());
        
        isFading = false;
    }
    
    public IEnumerator FadeOut()
    {
        if (fadeImage == null)
        {
            CreateFadeCanvas();
        }
        
        float elapsed = 0f;
        
        if (transitionSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(transitionSound);
        }
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0, 1, elapsed / fadeDuration);
            
            if (fadeImage != null)
            {
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            }
            
            yield return null;
        }
        
        if (fadeImage != null)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1);
        }
    }
    
    public IEnumerator FadeIn()
    {
        if (fadeImage == null)
        {
            CreateFadeCanvas();
            if (fadeImage != null)
            {
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1);
            }
        }
        
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1, 0, elapsed / fadeDuration);
            
            if (fadeImage != null)
            {
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            }
            
            yield return null;
        }
        
        if (fadeImage != null)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
        }
    }
    
    public void StartFadeOut()
    {
        if (!isFading)
        {
            StartCoroutine(FadeOut());
        }
    }
    
    public void StartFadeIn()
    {
        if (!isFading)
        {
            StartCoroutine(FadeIn());
        }
    }
}