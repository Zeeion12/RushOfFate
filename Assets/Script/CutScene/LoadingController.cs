using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    [Range(0.5f, 5f)]
    public float animationSpeed = 2f;
    
    void Start()
    {
        // PENTING: Pastikan BGM tetap playing
        if (AudioManager.Instance != null)
        {
            // Check apakah BGM sudah playing
            if (!AudioManager.Instance.bgmSource.isPlaying)
            {
                AudioManager.Instance.PlayBGM(AudioManager.Instance.cutsceneBGM);
            }
            
            Debug.Log($"BGM Status: {(AudioManager.Instance.bgmSource.isPlaying ? "Playing" : "Stopped")}");
        }
        
        // Set loading animation speed
        if (loadingAnimator != null)
        {
            loadingAnimator.speed = animationSpeed;
        }
        
        StartCoroutine(LoadNextScene());
    }

    IEnumerator LoadNextScene()
    {
        float elapsedTime = 0f;
        
        // Loading progress
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
        
        // FOOTSTEP PRE-ROLL (BGM tetap playing)
        yield return StartCoroutine(FootstepPreRoll());
        
        // Load cutscene
        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.FadeToScene(13);
        }
        else
        {
            SceneManager.LoadScene(13);
        }
    }
    
    IEnumerator FootstepPreRoll()
    {
        Debug.Log("=== FOOTSTEP PRE-ROLL START ===");
        
        // PENTING: Start footsteps (BGM tetap jalan)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StartFootsteps();
            
            // Debug check
            Debug.Log($"BGM Playing: {AudioManager.Instance.bgmSource.isPlaying}");
        }
        
        // Wait for pre-roll
        yield return new WaitForSeconds(footstepPreRollDuration);
        
        Debug.Log("=== FOOTSTEP PRE-ROLL END ===");
        // NOTE: Footsteps tetap playing sampai cutscene
    }
}