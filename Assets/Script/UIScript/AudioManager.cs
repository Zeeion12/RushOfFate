using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource footstepSource;

    [Header("Player SFX")]
    public AudioClip jumpSFX;
    public AudioClip rollSFX;
    public AudioClip landSFX;
    public AudioClip footstepSFX;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ====== DIPANGGIL DARI ANIMATION EVENT ======

    public void PlayJumpSound()
    {
        if (jumpSFX != null)
            sfxSource.PlayOneShot(jumpSFX);
    }

    public void PlayRollSound()
    {
        if (rollSFX != null)
            sfxSource.PlayOneShot(rollSFX);
    }

    public void PlayLandSound()
    {
        if (landSFX != null)
            sfxSource.PlayOneShot(landSFX);
    }

    public void PlayFootstep()
    {
        if (!footstepSource.isPlaying)
        {
            footstepSource.clip = footstepSFX;
            footstepSource.loop = false;
            footstepSource.Play();
        }
    }

    public void StopFootstep()
    {
        if (footstepSource.isPlaying)
        {
            footstepSource.Stop();
        }
    }
}
