using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CreditsController : MonoBehaviour
{
    [Header("Credits Settings")]
    public CanvasGroup creditsCanvasGroup;
    
    [Header("Text Canvas Groups")]
    public CanvasGroup productionCanvasGroup;
    public CanvasGroup communityCanvasGroup;
    
    [Header("Fade Timings")]
    public float fadeInDuration = 1.5f;
    public float displayDuration = 2f;
    public float fadeOutDuration = 1.5f;
    public float delayBetweenTexts = 1f;
    
    private void Start()
    {
        if (creditsCanvasGroup != null)
            creditsCanvasGroup.alpha = 0;
        
        if (productionCanvasGroup != null)
            productionCanvasGroup.alpha = 0;
        
        if (communityCanvasGroup != null)
            communityCanvasGroup.alpha = 0;
        
        gameObject.SetActive(false);
    }
    
    public void ShowCredits()
    {
        gameObject.SetActive(true);
        StartCoroutine(CreditsSequence());
    }
    
    private IEnumerator CreditsSequence()
    {
        Debug.Log("=== SHOWING CREDITS ===");
        
        // Fade in background
        yield return StartCoroutine(FadeCanvasGroup(creditsCanvasGroup, 0, 0.8f, 1f));
        
        // Show Production Text
        yield return StartCoroutine(ShowText(productionCanvasGroup, "Production"));
        
        // Delay between texts
        yield return new WaitForSeconds(delayBetweenTexts);
        
        // Show Community Text
        yield return StartCoroutine(ShowText(communityCanvasGroup, "Community"));
        
        // Hold both
        yield return new WaitForSeconds(displayDuration);
        
        // Fade out all
        yield return StartCoroutine(FadeCanvasGroup(creditsCanvasGroup, 0.8f, 0, fadeOutDuration));
        
        Debug.Log("=== CREDITS COMPLETE ===");
        gameObject.SetActive(false);
    }
    
    private IEnumerator ShowText(CanvasGroup canvasGroup, string textName)
    {
        if (canvasGroup == null) yield break;
        
        Debug.Log($"Showing {textName} text");
        
        // Fade in
        yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 0, 1, fadeInDuration));
        
        // Hold
        yield return new WaitForSeconds(displayDuration);
        
        // Fade out
        yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 1, 0, fadeOutDuration));
    }
    
    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null) yield break;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        
        group.alpha = to;
    }
}