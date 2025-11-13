using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Clips")]
    public AudioClip hitBrickClip;
    public AudioClip hitPaddleClip;
    public AudioClip ballMissClip;
    public AudioClip backgroundMusicClip;

    // public config: default to on
    [Header("Settings")]
    public bool soundOn = true;

    // internal audio sources (keep private)
    private AudioSource sfxSource;
    private AudioSource musicSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Create audio sources
        sfxSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.clip = backgroundMusicClip;

        // Ensure music respects saved/default soundOn state
        musicSource.mute = !soundOn;
        if (backgroundMusicClip != null)
            musicSource.Play();
    }

    // Play one-shot SFX (respects soundOn)
    public void PlaySFX(AudioClip clip)
    {
        if (!soundOn || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayBrickHit() => PlaySFX(hitBrickClip);
    public void PlayPaddleHit() => PlaySFX(hitPaddleClip);
    public void PlayBallMiss() => PlaySFX(ballMissClip);

    /// <summary>
    /// Toggle sound globally (legacy helper).
    /// </summary>
    public void ToggleSound()
    {
        SetSoundState(!soundOn);
    }

    /// <summary>
    /// Public API: set global sound on/off.
    /// This encapsulates internal audio sources so other scripts don't touch them directly.
    /// </summary>
    public void SetSoundState(bool on)
    {
        soundOn = on;

        if (musicSource != null)
            musicSource.mute = !soundOn;
    }

    /// <summary>
    /// Public read-only accessor for other scripts.
    /// </summary>
    public bool IsSoundOn() => soundOn;
}
