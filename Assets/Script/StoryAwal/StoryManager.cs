using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class StoryManager : MonoBehaviour
{
    [Header("UI References")]
    public Image storyImage;
    public Image fadePanel;
    public TextMeshProUGUI subtitleText;
    public GameObject skipButton;
    
    [Header("Story Scenes")]
    public Sprite[] storySprites;
    
    [Header("Audio")]
    public AudioSource narratorSource;
    public AudioClip[] narratorClips;
    public AudioSource bgmSource;

    [Header("Sound Effects")]  // ← BARU
    [Tooltip("Audio source untuk sound effect ambient")]
    public AudioSource sfxSource;

    [Tooltip("Sound effect untuk setiap scene (opsional, bisa kosong)")]
    public AudioClip[] sceneSoundEffects;

    [Tooltip("Volume sound effect")]
    [Range(0f, 1f)]
    public float sfxVolume = 0.5f;

    [Tooltip("Loop sound effect selama scene berlangsung")]
    public bool loopSoundEffects = true;
    
    [Header("Subtitles")]
    [Tooltip("Subtitle untuk setiap scene")]
    public SceneSubtitles[] allSceneSubtitles;
    
    [Header("Subtitle Style")]
    public int defaultFontSize = 32;
    public Color defaultTextColor = Color.white;
    public Color defaultOutlineColor = Color.black;
    [Range(0f, 1f)]
    public float subtitleFadeSpeed = 0.3f;
    
    [Header("Zoom Animation")]
    [Tooltip("Aktifkan zoom in animation")]
    public bool enableZoomAnimation = true;
    
    [Tooltip("Scale awal gambar (1.0 = normal, 1.2 = 120%)")]
    [Range(1.0f, 2.0f)]
    public float zoomStartScale = 1.2f;
    
    [Tooltip("Scale akhir gambar (biasanya 1.0 untuk normal)")]
    [Range(1.0f, 2.0f)]
    public float zoomEndScale = 1.0f;
    
    [Tooltip("Durasi zoom animation (detik)")]
    [Range(1f, 30f)]
    public float zoomDuration = 15f;
    
    [Tooltip("Tipe easing untuk zoom")]
    public ZoomEasingType zoomEasingType = ZoomEasingType.EaseInOut;
    
    [Tooltip("Custom zoom settings per scene (opsional)")]
    public SceneZoomSettings[] customZoomPerScene;

    [Header("Timing Settings")]
    public float fadeDuration = 1f;
    public float sceneHoldTime = 1f;

    [Tooltip("Waktu hold khusus untuk scene terakhir sebelum ke menu (detik)")]
    [Range(0f, 5f)]
    public float lastSceneHoldTime = 0.5f; // ← BARU
    
    [Header("Scene to Load")]
    public string nextSceneName = "Level Selection";

    [Header("Camera Shake (Optional)")]
    public bool enableCameraShake = false;
    public float shakeDuration = 0.3f;
    public float shakeMagnitude = 0.1f;

    [Header("Skip Settings")]
    public float skipButtonShowDelay = 3f;
    public float skipButtonFadeDuration = 0.5f;

    private bool skipButtonShown = false;

    [Header("Debug")]
    public bool showTimingDebug = false;
    
    private int currentSceneIndex = 0;
    private bool canSkip = true;
    private Coroutine currentZoomCoroutine;
    private Coroutine currentSubtitleCoroutine;
    private RectTransform imageRectTransform;

    public enum ZoomEasingType
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut,
        EaseInQuad,
        EaseOutQuad
    }
    
    void Start()
    {
        // Get RectTransform for zoom animation
        if (storyImage != null)
        {
            imageRectTransform = storyImage.GetComponent<RectTransform>();
        }
        
        // Setup subtitle text
        if (subtitleText != null)
        {
            subtitleText.fontSize = defaultFontSize;
            subtitleText.color = defaultTextColor;
            subtitleText.outlineColor = defaultOutlineColor;
            subtitleText.text = "";
        }

        // Setup SFX source
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
            sfxSource.loop = loopSoundEffects;
        }

         if (skipButton != null)
        {
            skipButton.SetActive(false);
        }
        
        SetFadeAlpha(1f);
        StartCoroutine(PlayStorySequence());
        StartCoroutine(ShowSkipButtonAfterDelay());
    }
    
    void Update()
    {
        if (canSkip && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            SkipToNextScene();
        }
    }
    
    IEnumerator PlayStorySequence()
    {
        for (int i = 0; i < storySprites.Length; i++)
        {
            currentSceneIndex = i;
            
            bool isLastScene = (i >= storySprites.Length - 1);
            
            yield return StartCoroutine(PlaySingleScene(i, isLastScene));
        }
        
        // Hanya di akhir semua scene, baru fade out dan load
        yield return StartCoroutine(FadeOut());
        LoadNextScene();
    }

    IEnumerator FadeOutBGM(float duration)
    {
        if (bgmSource == null) yield break;
        
        float startVolume = bgmSource.volume;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }
        
        bgmSource.volume = 0f;
        bgmSource.Stop();
    }
    
    IEnumerator PlaySingleScene(int index, bool isLastScene)
    {
        // 1. Set gambar story
        storyImage.sprite = storySprites[index];
        storyImage.color = new Color(1, 1, 1, 0);

        // 2. Reset scale
        if (imageRectTransform != null)
        {
            imageRectTransform.localScale = Vector3.one;
        }
        
        // 3. Fade in gambar
        yield return StartCoroutine(FadeInImage());
        
        // 4. Hitung durasi scene
        float sceneDuration = 0f;
        if (index < narratorClips.Length && narratorClips[index] != null)
        {
            sceneDuration = narratorClips[index].length;
        }
        else
        {
            sceneDuration = 5f;
        }

        // 5. Play SFX
        PlaySceneSoundEffect(index);

        // 6. Zoom animation
        if (enableZoomAnimation && imageRectTransform != null)
        {
            SceneZoomSettings customZoom = GetCustomZoomForScene(index);
            
            if (customZoom != null)
            {
                currentZoomCoroutine = StartCoroutine(ZoomInAnimation(
                    customZoom.startScale,
                    customZoom.endScale,
                    customZoom.duration,
                    customZoom.easingType
                ));
            }
            else
            {
                currentZoomCoroutine = StartCoroutine(ZoomInAnimation(
                    zoomStartScale,
                    zoomEndScale,
                    zoomDuration,
                    zoomEasingType
                ));
            }
        }
        
        // 7. Subtitle
        if (index < allSceneSubtitles.Length && allSceneSubtitles[index].subtitles.Length > 0)
        {
            currentSubtitleCoroutine = StartCoroutine(PlaySubtitlesForScene(index, sceneDuration));
        }
        
        // 8. Narrator
        if (index < narratorClips.Length && narratorClips[index] != null)
        {
            narratorSource.clip = narratorClips[index];
            narratorSource.Play();
            yield return new WaitForSeconds(narratorClips[index].length);
        }
        else
        {
            yield return new WaitForSeconds(sceneDuration);
        }

        // 9. Stop SFX
        StopSceneSoundEffect();
        
        // 10. Stop subtitle
        if (currentSubtitleCoroutine != null)
        {
            StopCoroutine(currentSubtitleCoroutine);
            currentSubtitleCoroutine = null;
        }

        // 11. Stop zoom
        if (currentZoomCoroutine != null)
        {
            StopCoroutine(currentZoomCoroutine);
            currentZoomCoroutine = null;
        }
        
        // 12. Clear subtitle
        if (subtitleText != null)
        {
            subtitleText.text = "";
        }
        
        // 13. Camera shake
        if (enableCameraShake && index == 3)
        {
            yield return StartCoroutine(CameraShake(shakeDuration, shakeMagnitude));
        }
        
        // 14. Hold dan fade out HANYA untuk scene yang bukan terakhir
        if (!isLastScene)
        {
            yield return new WaitForSeconds(sceneHoldTime);
            yield return StartCoroutine(FadeOutImage());
        }
        else
        {
            // Scene terakhir: tunggu sebentar lalu selesai (tidak fade out image)
            yield return new WaitForSeconds(0.5f);
        }
    }

    void PlaySceneSoundEffect(int sceneIndex)
    {
        Debug.Log($"[SFX] === START PlaySceneSoundEffect for scene {sceneIndex} ===");
        
        if (sfxSource == null)
        {
            Debug.LogError("[SFX] ERROR: SFX Source is NULL!");
            return;
        }
        
        Debug.Log($"[SFX] SFX Source OK: {sfxSource.gameObject.name}");
        
        // Stop sound effect sebelumnya
        if (sfxSource.isPlaying)
        {
            Debug.Log("[SFX] Stopping previous SFX");
            sfxSource.Stop();
        }

        // Cek array
        if (sceneSoundEffects == null)
        {
            Debug.LogError("[SFX] Scene Sound Effects array is NULL!");
            return;
        }
        
        if (sceneSoundEffects.Length == 0)
        {
            Debug.LogWarning("[SFX] Scene Sound Effects array is EMPTY!");
            return;
        }
        
        Debug.Log($"[SFX] Array length: {sceneSoundEffects.Length}");
        Debug.Log($"[SFX] Scene index: {sceneIndex}");
        
        // Cek apakah ada sound effect untuk scene ini
        if (sceneIndex >= sceneSoundEffects.Length)
        {
            Debug.LogWarning($"[SFX] Scene index {sceneIndex} is OUT OF RANGE (array length: {sceneSoundEffects.Length})");
            return;
        }
        
        if (sceneSoundEffects[sceneIndex] == null)
        {
            Debug.LogWarning($"[SFX] Sound effect for scene {sceneIndex} is NULL");
            return;
        }
        
        Debug.Log($"[SFX] Clip assigned: {sceneSoundEffects[sceneIndex].name}");
        
        // Assign clip
        sfxSource.clip = sceneSoundEffects[sceneIndex];
        Debug.Log($"[SFX] Clip set to AudioSource");
        
        // Set loop
        sfxSource.loop = loopSoundEffects;
        Debug.Log($"[SFX] Loop set to: {sfxSource.loop}");
        
        // Set volume
        sfxSource.volume = sfxVolume;
        Debug.Log($"[SFX] Volume set to: {sfxSource.volume}");
        
        // Play
        Debug.Log($"[SFX] Calling Play()...");
        sfxSource.Play();
        Debug.Log($"[SFX] Play() executed");
        
        // Check if playing
        Debug.Log($"[SFX] IsPlaying: {sfxSource.isPlaying}");
        Debug.Log($"[SFX] Time: {sfxSource.time}");
        
        if (sfxSource.isPlaying)
        {
            Debug.Log("[SFX] ✅ SUCCESS! SFX is playing!");
        }
        else
        {
            Debug.LogError("[SFX] ❌ FAILED! SFX is NOT playing after Play() call!");
            Debug.LogError($"[SFX] AudioSource enabled: {sfxSource.enabled}");
            Debug.LogError($"[SFX] AudioSource mute: {sfxSource.mute}");
            Debug.LogError($"[SFX] Clip length: {sfxSource.clip.length}");
        }
        
        Debug.Log($"[SFX] === END PlaySceneSoundEffect ===");
    }
    
    void StopSceneSoundEffect()
    {
        if (sfxSource != null && sfxSource.isPlaying)
        {
            // Fade out sound effect (opsional)
            StartCoroutine(FadeOutSFX(0.5f));
        }
    }
    
    IEnumerator FadeOutSFX(float duration)
    {
        if (sfxSource == null) yield break;
        
        float startVolume = sfxSource.volume;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            sfxSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }
        
        sfxSource.Stop();
        sfxSource.volume = sfxVolume; // Reset volume
    }

    IEnumerator ZoomInAnimation(float startScale, float endScale, float duration, ZoomEasingType easing)
    {
        float elapsed = 0f;
        Vector3 startScaleVector = new Vector3(startScale, startScale, 1f);
        Vector3 endScaleVector = new Vector3(endScale, endScale, 1f);
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;
            
            // Apply easing
            float easedTime = ApplyEasing(normalizedTime, easing);
            
            // Lerp scale
            imageRectTransform.localScale = Vector3.Lerp(startScaleVector, endScaleVector, easedTime);
            
            if (showTimingDebug)
            {
                Debug.Log($"Zoom Progress: {normalizedTime:F2} | Scale: {imageRectTransform.localScale.x:F2}");
            }
            
            yield return null;
        }
        
        // Ensure final scale
        imageRectTransform.localScale = endScaleVector;
    }

    float ApplyEasing(float t, ZoomEasingType easing)
    {
        switch (easing)
        {
            case ZoomEasingType.Linear:
                return t;
            
            case ZoomEasingType.EaseIn:
                return t * t;
            
            case ZoomEasingType.EaseOut:
                return t * (2f - t);
            
            case ZoomEasingType.EaseInOut:
                return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
            
            case ZoomEasingType.EaseInQuad:
                return t * t * t;
            
            case ZoomEasingType.EaseOutQuad:
                return 1f - Mathf.Pow(1f - t, 3f);
            
            default:
                return t;
        }
    }
    
    SceneZoomSettings GetCustomZoomForScene(int sceneIndex)
    {
        if (customZoomPerScene != null && sceneIndex < customZoomPerScene.Length)
        {
            SceneZoomSettings settings = customZoomPerScene[sceneIndex];
            if (settings.useCustomZoom)
            {
                return settings;
            }
        }
        return null;
    }
    
    IEnumerator PlaySubtitlesForScene(int sceneIndex, float sceneDuration)
    {
        if (subtitleText == null) yield break;
        
        SceneSubtitles sceneSubtitles = allSceneSubtitles[sceneIndex];
        float elapsedTime = 0f;
        int currentSubtitleIndex = 0;
        
        while (elapsedTime < sceneDuration)
        {
            // Cek apakah ada subtitle yang harus ditampilkan
            if (currentSubtitleIndex < sceneSubtitles.subtitles.Length)
            {
                SubtitleData subtitle = sceneSubtitles.subtitles[currentSubtitleIndex];
                
                // Waktu untuk menampilkan subtitle ini
                if (elapsedTime >= subtitle.startTime && elapsedTime < subtitle.endTime)
                {
                    // Apply custom style jika ada
                    if (subtitle.customFontSize > 0)
                    {
                        subtitleText.fontSize = subtitle.customFontSize;
                    }
                    else
                    {
                        subtitleText.fontSize = defaultFontSize;
                    }
                    
                    if (subtitle.useCustomColor)
                    {
                        subtitleText.color = subtitle.customColor;
                    }
                    else
                    {
                        subtitleText.color = defaultTextColor;
                    }
                    
                    // Tampilkan text
                    subtitleText.text = subtitle.text;
                }
                // Waktu untuk hide subtitle ini
                else if (elapsedTime >= subtitle.endTime)
                {
                    subtitleText.text = "";
                    currentSubtitleIndex++;
                }
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Clear subtitle di akhir
        subtitleText.text = "";
    }
    
    IEnumerator FadeInImage()
    {
        float elapsed = 0f;
        Color startColor = new Color(1, 1, 1, 0);
        Color endColor = new Color(1, 1, 1, 1);
        
        yield return StartCoroutine(FadeIn());
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / fadeDuration;
            storyImage.color = Color.Lerp(startColor, endColor, normalizedTime);
            yield return null;
        }
        
        storyImage.color = endColor;
    }
    
    IEnumerator FadeOutImage()
    {
        float elapsed = 0f;
        Color startColor = new Color(1, 1, 1, 1);
        Color endColor = new Color(1, 1, 1, 0);
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / fadeDuration;
            storyImage.color = Color.Lerp(startColor, endColor, normalizedTime);
            yield return null;
        }
        
        storyImage.color = endColor;
    }
    
    IEnumerator FadeIn()
    {
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / fadeDuration;
            SetFadeAlpha(1f - normalizedTime);
            yield return null;
        }
        
        SetFadeAlpha(0f);
    }
    
    IEnumerator FadeOut()
    {
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / fadeDuration;
            SetFadeAlpha(normalizedTime);
            yield return null;
        }
        
        SetFadeAlpha(1f);
    }
    
    void SetFadeAlpha(float alpha)
    {
        Color color = fadePanel.color;
        color.a = alpha;
        fadePanel.color = color;
    }
    
    IEnumerator CameraShake(float duration, float magnitude)
    {
        Vector3 originalPos = Camera.main.transform.localPosition;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            
            Camera.main.transform.localPosition = new Vector3(x, y, originalPos.z);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        Camera.main.transform.localPosition = originalPos;
    }
    
    void SkipToNextScene()
    {
        StopAllCoroutines();
        narratorSource.Stop();
        
        if (sfxSource != null && sfxSource.isPlaying)
        {
            sfxSource.Stop();
            sfxSource.volume = sfxVolume;
        }
        
        if (skipButton != null)
        {
            skipButton.SetActive(false);
        }
        
        if (subtitleText != null)
        {
            subtitleText.text = "";
        }
        
        LoadNextScene();
    }

    IEnumerator ShowSkipButtonAfterDelay()
    {
        yield return new WaitForSeconds(skipButtonShowDelay);
        
        if (skipButton != null && !skipButtonShown)
        {
            skipButtonShown = true;
            skipButton.SetActive(true);
            
            CanvasGroup canvasGroup = skipButton.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                yield return StartCoroutine(FadeInSkipButton(canvasGroup));
            }
        }
    }

    IEnumerator FadeInSkipButton(CanvasGroup canvasGroup)
    {
        float elapsed = 0f;
        canvasGroup.alpha = 0f;
        
        while (elapsed < skipButtonFadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / skipButtonFadeDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }

    public void OnSkipButtonClicked()
    {
        Debug.Log("[SKIP] Loading LevelSelection");
        SkipToNextScene();
    }
    
    void LoadNextScene()
    {
        if (bgmSource != null)
        {
            bgmSource.volume = 0;
        }
        
        SceneManager.LoadScene(nextSceneName);
    }
}