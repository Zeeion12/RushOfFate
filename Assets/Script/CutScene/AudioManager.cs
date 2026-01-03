using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    
    [Header("Background Music")]
    public AudioClip cutsceneBGM;
    
    [Header("Sound Effects")]
    public AudioClip jumpSFX;
    public AudioClip landingSFX;
    public AudioClip footstepSFX; // TAMBAHKAN INI
    
    private bool isPlayingFootsteps = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            PlayBGM(cutsceneBGM);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource.clip != clip)
        {
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
        else if (!bgmSource.isPlaying)
        {
            bgmSource.Play();
        }
    }
    
    public void StopBGM()
    {
        bgmSource.Stop();
    }
    
    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
    
    // TAMBAHKAN METHOD BARU UNTUK FOOTSTEPS LOOP
    public void StartFootsteps()
    {
        if (!isPlayingFootsteps && footstepSFX != null)
        {
            sfxSource.clip = footstepSFX;
            sfxSource.loop = true;
            sfxSource.Play();
            isPlayingFootsteps = true;
        }
    }
    
    public void StopFootsteps()
    {
        if (isPlayingFootsteps)
        {
            sfxSource.loop = false;
            sfxSource.Stop();
            sfxSource.clip = null;
            isPlayingFootsteps = false;
        }
    }
    
    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = volume;
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }

    public void ContinueFootstepsIfPlaying()
    {
        // Jika footsteps sudah playing, terus mainkan
        // Method ini dipanggil saat scene baru load
        if (isPlayingFootsteps && footstepSFX != null)
        {
            if (!sfxSource.isPlaying)
            {
                sfxSource.clip = footstepSFX;
                sfxSource.loop = true;
                sfxSource.Play();
            }
        }
    }
}