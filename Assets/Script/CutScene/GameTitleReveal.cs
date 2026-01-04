using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameTitleReveal : MonoBehaviour
{
    [Header("Title Settings")]
    public CanvasGroup titleCanvasGroup;
    public Text titleText; // Atau TextMeshProUGUI jika pakai TMP
    
    [Header("Animation Timings")]
    public float fadeInDuration = 2f;
    public float displayDuration = 3f;
    public float fadeOutDuration = 2f;
    
    [Header("Position Animation (Optional)")]
    public bool useSlideAnimation = true;
    public float slideDistance = 100f; // Slide dari bawah
    
    [Header("Audio (Optional)")]
    public AudioClip titleRevealSound; // Whoosh atau dramatic sound
    
    private Vector3 originalPosition;
    private RectTransform rectTransform;
    
    void Start()
    {
        // Pastikan title mulai hidden
        if (titleCanvasGroup != null)
        {
            titleCanvasGroup.alpha = 0;
        }
        
        // Simpan posisi original
        if (titleText != null)
        {
            rectTransform = titleText.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                originalPosition = rectTransform.anchoredPosition;
            }
        }
        
        // Hide di awal
        gameObject.SetActive(false);
    }
    
    public void ShowTitle()
    {
        gameObject.SetActive(true);
        StartCoroutine(TitleRevealSequence());
    }
    
    IEnumerator TitleRevealSequence()
    {
        Debug.Log("=== TITLE REVEAL START ===");
        
        // Setup starting position
        if (useSlideAnimation && rectTransform != null)
        {
            rectTransform.anchoredPosition = originalPosition + Vector3.down * slideDistance;
        }
        
        // Fade In + Slide
        yield return StartCoroutine(FadeIn());
        
        // Display/Hold
        yield return new WaitForSeconds(displayDuration);
        
        // Fade Out
        yield return StartCoroutine(FadeOut());
        
        Debug.Log("=== TITLE REVEAL END ===");
        
        // Hide object
        gameObject.SetActive(false);
    }
    
    IEnumerator FadeIn()
    {
        float elapsed = 0f;
        Vector3 startPos = useSlideAnimation && rectTransform != null ? 
            rectTransform.anchoredPosition : Vector3.zero;
        
        // Play sound
        if (titleRevealSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(titleRevealSound);
        }
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            
            // Ease out cubic untuk smooth
            t = 1f - Mathf.Pow(1f - t, 3f);
            
            // Fade alpha
            if (titleCanvasGroup != null)
            {
                titleCanvasGroup.alpha = Mathf.Lerp(0, 1, t);
            }
            
            // Slide position
            if (useSlideAnimation && rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector3.Lerp(startPos, originalPosition, t);
            }
            
            yield return null;
        }
        
        // Ensure final values
        if (titleCanvasGroup != null)
        {
            titleCanvasGroup.alpha = 1;
        }
        
        if (useSlideAnimation && rectTransform != null)
        {
            rectTransform.anchoredPosition = originalPosition;
        }
    }
    
    IEnumerator FadeOut()
    {
        float elapsed = 0f;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            
            // Fade alpha
            if (titleCanvasGroup != null)
            {
                titleCanvasGroup.alpha = Mathf.Lerp(1, 0, t);
            }
            
            yield return null;
        }
        
        if (titleCanvasGroup != null)
        {
            titleCanvasGroup.alpha = 0;
        }
    }
    
    // Helper untuk CutsceneController
    public float GetTotalDuration()
    {
        return fadeInDuration + displayDuration + fadeOutDuration;
    }
}