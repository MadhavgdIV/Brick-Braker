using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Clips")]
    public AudioClip hitBrickClip;
    public AudioClip hitPaddleClip;
    public AudioClip ballMissClip;
    public AudioClip backgroundMusicClip;

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
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void PlayBrickHit() => PlaySFX(hitBrickClip);
    public void PlayPaddleHit() => PlaySFX(hitPaddleClip);
    public void PlayBallMiss() => PlaySFX(ballMissClip);
}
