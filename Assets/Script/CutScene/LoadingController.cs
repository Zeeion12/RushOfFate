using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // TAMBAHKAN INI

public class LoadingController : MonoBehaviour
{
    [Header("Loading Settings")]
    public float minimumLoadingTime = 2f;
    public Slider loadingBar;
    public Text loadingText;
    
    [Header("Audio Pre-roll")]
    public float footstepPreRollDuration = 2f;
    
    [Header("Animation")]
    public Animator loadingAnimator;
    public float animationSpeed = 2f;
    
    void Start()
    {
        // Set animator speed
        if (loadingAnimator != null)
        {
            loadingAnimator.speed = animationSpeed;
            Debug.Log($"Loading animation speed set to: {animationSpeed}x");
        }
        
        StartCoroutine(LoadNextScene());
    }

    IEnumerator LoadNextScene()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < minimumLoadingTime)
        {
            elapsedTime += Time.deltaTime;
            
            if (loadingBar != null)
            {
                loadingBar.value = elapsedTime / minimumLoadingTime;
            }
            
            if (loadingText != null)
            {
                loadingText.text = "Loading... " + (int)(elapsedTime / minimumLoadingTime * 100) + "%";
            }
            
            yield return null;
        }
        
        if (loadingText != null)
        {
            loadingText.text = "Complete!";
        }
        
        // Footstep pre-roll
        yield return StartCoroutine(FootstepPreRoll());
        
        // GANTI: Gunakan SceneTransition kalau ada, atau load biasa
        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.FadeToScene(2); // CutsceneMain
        }
        else
        {
            // Fallback: Load langsung tanpa fade
            SceneManager.LoadScene(2);
        }
    }
    
    IEnumerator FootstepPreRoll()
    {
        Debug.Log("=== FOOTSTEP PRE-ROLL START ===");
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StartFootsteps();
        }
        
        yield return new WaitForSeconds(footstepPreRollDuration);
        
        Debug.Log("=== FOOTSTEP PRE-ROLL END ===");
    }
}