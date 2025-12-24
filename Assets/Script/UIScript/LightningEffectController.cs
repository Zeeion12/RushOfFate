using UnityEngine;
using System.Collections;

public class LightningEffectController : MonoBehaviour
{
    [Header("Lightning Renderers")]
    [Tooltip("Drag semua lightning sprite renderers ke sini")]
    public SpriteRenderer[] lightningRenderers;
    
    [Header("Timing Settings")]
    public float minInterval = 2f;
    public float maxInterval = 5f;
    public float flashDuration = 0.1f;
    public float fadeOutDuration = 0.3f;
    
    [Header("Intensity")]
    [Range(0f, 1f)]
    public float maxAlpha = 0.8f;
    [Range(1, 5)]
    public int maxSimultaneousStrikes = 3;
    
    [Header("Audio")]
    public AudioSource thunderAudioSource;
    public AudioClip[] thunderClips;
    [Range(0f, 1f)]
    public float thunderVolume = 0.7f;
    public float thunderDelay = 0.1f; // Delay antara flash dan sound
    
    void Start()
    {
        // Initialize all lightning to invisible
        foreach (var renderer in lightningRenderers)
        {
            if (renderer != null)
            {
                Color color = renderer.color;
                color.a = 0f;
                renderer.color = color;
            }
        }
        
        // Start lightning routine
        StartCoroutine(LightningRoutine());
    }
    
    IEnumerator LightningRoutine()
    {
        while (true)
        {
            // Random wait time
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);
            
            // Determine number of strikes (1-3 random)
            int strikeCount = Random.Range(1, maxSimultaneousStrikes + 1);
            
            // Trigger strikes
            for (int i = 0; i < strikeCount; i++)
            {
                // Pick random lightning renderer
                int randomIndex = Random.Range(0, lightningRenderers.Length);
                StartCoroutine(FlashLightning(lightningRenderers[randomIndex]));
                
                // Play thunder sound with delay
                if (thunderAudioSource != null && thunderClips.Length > 0)
                {
                    StartCoroutine(PlayThunderSound());
                }
                
                // Small delay between multiple strikes
                if (i < strikeCount - 1)
                {
                    yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
                }
            }
        }
    }
    
    IEnumerator FlashLightning(SpriteRenderer renderer)
    {
        if (renderer == null) yield break;
        
        // Flash on instantly
        Color color = renderer.color;
        color.a = maxAlpha;
        renderer.color = color;
        
        // Hold flash
        yield return new WaitForSeconds(flashDuration);
        
        // Fade out gradually
        float timer = 0f;
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeOutDuration;
            color.a = Mathf.Lerp(maxAlpha, 0f, t);
            renderer.color = color;
            yield return null;
        }
        
        // Ensure fully transparent
        color.a = 0f;
        renderer.color = color;
    }
    
    IEnumerator PlayThunderSound()
    {
        // Delay untuk simulate speed of light vs sound
        yield return new WaitForSeconds(thunderDelay);
        
        if (thunderAudioSource != null && thunderClips.Length > 0)
        {
            AudioClip clip = thunderClips[Random.Range(0, thunderClips.Length)];
            thunderAudioSource.PlayOneShot(clip, thunderVolume);
        }
    }
    
    // Manual trigger untuk testing atau cutscene
    public void TriggerLightning()
    {
        int randomIndex = Random.Range(0, lightningRenderers.Length);
        StartCoroutine(FlashLightning(lightningRenderers[randomIndex]));
        
        if (thunderAudioSource != null && thunderClips.Length > 0)
        {
            StartCoroutine(PlayThunderSound());
        }
    }
}