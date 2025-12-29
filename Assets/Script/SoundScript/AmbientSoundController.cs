using UnityEngine;
using System.Collections;

public class AmbientSoundController : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource windAmbience;
    public AudioSource rainAmbience;
    public AudioSource thunderSound;
    
    [Header("Wind Settings")]
    public AudioClip windClip;
    [Range(0f, 1f)]
    public float windVolume = 0.2f;
    public bool playWindOnStart = true;
    
    [Header("Rain Settings")]
    public AudioClip rainClip;
    [Range(0f, 1f)]
    public float rainVolume = 0.3f;
    public bool playRainOnStart = false;
    
    [Header("Thunder Settings")]
    public AudioClip[] thunderClips;
    public float minThunderInterval = 3f;
    public float maxThunderInterval = 8f;
    [Range(0f, 1f)]
    public float thunderVolume = 0.6f;
    
    [Header("Fade Settings")]
    public float fadeInDuration = 2f;
    public float fadeOutDuration = 2f;
    
    void Start()
    {
        // Setup wind ambience
        if (windAmbience != null && windClip != null)
        {
            windAmbience.clip = windClip;
            windAmbience.loop = true;
            windAmbience.volume = 0f;
            
            if (playWindOnStart)
            {
                StartCoroutine(FadeInAudio(windAmbience, windVolume, fadeInDuration));
            }
        }
        
        // Setup rain ambience
        if (rainAmbience != null && rainClip != null)
        {
            rainAmbience.clip = rainClip;
            rainAmbience.loop = true;
            rainAmbience.volume = 0f;
            
            if (playRainOnStart)
            {
                StartCoroutine(FadeInAudio(rainAmbience, rainVolume, fadeInDuration));
            }
        }
        
        // Start thunder routine
        if (thunderSound != null && thunderClips.Length > 0)
        {
            StartCoroutine(ThunderRoutine());
        }
    }
    
    IEnumerator ThunderRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minThunderInterval, maxThunderInterval);
            yield return new WaitForSeconds(waitTime);
            
            // Play random thunder sound
            AudioClip clip = thunderClips[Random.Range(0, thunderClips.Length)];
            thunderSound.PlayOneShot(clip, Random.Range(thunderVolume * 0.7f, thunderVolume));
        }
    }
    
    IEnumerator FadeInAudio(AudioSource source, float targetVolume, float duration)
    {
        source.Play();
        float timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, targetVolume, timer / duration);
            yield return null;
        }
        
        source.volume = targetVolume;
    }
    
    IEnumerator FadeOutAudio(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }
        
        source.volume = 0f;
        source.Stop();
    }
    
    // Public methods
    public void PlayWind()
    {
        if (windAmbience != null)
        {
            StartCoroutine(FadeInAudio(windAmbience, windVolume, fadeInDuration));
        }
    }
    
    public void StopWind()
    {
        if (windAmbience != null)
        {
            StartCoroutine(FadeOutAudio(windAmbience, fadeOutDuration));
        }
    }
    
    public void PlayRain()
    {
        if (rainAmbience != null)
        {
            StartCoroutine(FadeInAudio(rainAmbience, rainVolume, fadeInDuration));
        }
    }
    
    public void StopRain()
    {
        if (rainAmbience != null)
        {
            StartCoroutine(FadeOutAudio(rainAmbience, fadeOutDuration));
        }
    }
}