using UnityEngine;
using System.Collections;

public class StageMusicManager : MonoBehaviour
{
    public static StageMusicManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource stageMusic;
    [SerializeField] private AudioSource bossMusic;

    [Header("Music Clips")]
    [SerializeField] private AudioClip stageMusicClip;
    [SerializeField] private AudioClip bossMusicClip;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float stageMusicVolume = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float bossMusicVolume = 0.7f;

    [Header("Fade Settings")]
    [SerializeField] private float fadeOutDuration = 2f;
    [SerializeField] private float fadeInDuration = 2f;

    private bool isTransitioning = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Setup audio sources if not assigned
        SetupAudioSources();

        // Play stage music at start
        PlayStageMusic();
    }

    void SetupAudioSources()
    {
        // Get or create audio sources
        AudioSource[] sources = GetComponents<AudioSource>();

        if (sources.Length >= 2)
        {
            stageMusic = sources[0];
            bossMusic = sources[1];
        }
        else if (sources.Length == 1)
        {
            stageMusic = sources[0];
            bossMusic = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            stageMusic = gameObject.AddComponent<AudioSource>();
            bossMusic = gameObject.AddComponent<AudioSource>();
        }

        // Configure audio sources
        stageMusic.loop = true;
        stageMusic.playOnAwake = false;
        bossMusic.loop = true;
        bossMusic.playOnAwake = false;
    }

    public void PlayStageMusic(bool immediate = false)
    {
        if (stageMusic == null || stageMusicClip == null) return;

        if (immediate)
        {
            stageMusic.clip = stageMusicClip;
            stageMusic.volume = stageMusicVolume;
            stageMusic.Play();
        }
        else
        {
            StartCoroutine(FadeInMusic(stageMusic, stageMusicClip, stageMusicVolume, fadeInDuration));
        }
    }

    public void PlayBossMusic()
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionToBossMusic());
    }

    public void ReturnToStageMusic()
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionToStageMusic());
    }

    IEnumerator TransitionToBossMusic()
    {
        isTransitioning = true;

        // Fade out stage music
        if (stageMusic.isPlaying)
        {
            yield return StartCoroutine(FadeOutMusic(stageMusic, fadeOutDuration));
        }

        // Fade in boss music
        if (bossMusicClip != null)
        {
            yield return StartCoroutine(FadeInMusic(bossMusic, bossMusicClip, bossMusicVolume, fadeInDuration));
        }

        isTransitioning = false;
    }

    IEnumerator TransitionToStageMusic()
    {
        isTransitioning = true;

        // Fade out boss music
        if (bossMusic.isPlaying)
        {
            yield return StartCoroutine(FadeOutMusic(bossMusic, fadeOutDuration));
        }

        // Fade in stage music
        if (stageMusicClip != null)
        {
            yield return StartCoroutine(FadeInMusic(stageMusic, stageMusicClip, stageMusicVolume, fadeInDuration));
        }

        isTransitioning = false;
    }

    IEnumerator FadeInMusic(AudioSource source, AudioClip clip, float targetVolume, float duration)
    {
        if (source.clip != clip)
        {
            source.clip = clip;
        }

        source.volume = 0f;
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

    IEnumerator FadeOutMusic(AudioSource source, float duration)
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

    public void StopAllMusic(bool immediate = false)
    {
        if (immediate)
        {
            stageMusic.Stop();
            bossMusic.Stop();
        }
        else
        {
            StartCoroutine(FadeOutMusic(stageMusic, fadeOutDuration));
            StartCoroutine(FadeOutMusic(bossMusic, fadeOutDuration));
        }
    }

    public bool IsStageMusicPlaying()
    {
        return stageMusic != null && stageMusic.isPlaying;
    }

    public bool IsBossMusicPlaying()
    {
        return bossMusic != null && bossMusic.isPlaying;
    }
}
