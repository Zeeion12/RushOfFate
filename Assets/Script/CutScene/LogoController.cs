using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogoController : MonoBehaviour
{
    [Header("Logo Settings")]
    public CanvasGroup logoCanvasGroup;
    public float fadeInDuration = 1.5f;
    public float displayDuration = 2f;
    public float fadeOutDuration = 1f;

    void Start()
    {
        if (logoCanvasGroup != null)
        {
            logoCanvasGroup.alpha = 0;
        }
        
        StartCoroutine(LogoSequence());
    }

    IEnumerator LogoSequence()
    {
        // Fade in logo
        yield return StartCoroutine(FadeIn());
        
        // Hold
        yield return new WaitForSeconds(displayDuration);
        
        // Fade out logo
        yield return StartCoroutine(FadeOut());
        
        // Use transition to load next scene
        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.FadeToScene(1); // LoadingScene
        }
        else
        {
            SceneManager.LoadScene(1);
        }
    }

    IEnumerator FadeIn()
    {
        float elapsedTime = 0;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            logoCanvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeInDuration);
            yield return null;
        }
        
        logoCanvasGroup.alpha = 1;
    }

    IEnumerator FadeOut()
    {
        float elapsedTime = 0;
        
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            logoCanvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeOutDuration);
            yield return null;
        }
        
        logoCanvasGroup.alpha = 0;
    }
}