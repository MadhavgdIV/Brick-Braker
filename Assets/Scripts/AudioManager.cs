using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("SFX")]
    public AudioClip brickHitClip;
    public AudioClip brickBreakClip;
    public AudioClip paddleHitClip;   // <-- new paddle hit clip
    public AudioClip ballMissClip;
    public AudioClip winClip;
    public AudioClip loseClip;

    [Header("Settings")]
    public float sfxVolume = 1f;

    private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null) return;
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.volume = sfxVolume;
        sfxSource.PlayOneShot(clip);
    }

    // Convenience methods
    public void PlayBrickHit() => PlaySfx(brickHitClip);
    public void PlayBrickBreak() => PlaySfx(brickBreakClip);
    public void PlayPaddleHit() => PlaySfx(paddleHitClip); // <-- new method
    public void PlayBallMiss() => PlaySfx(ballMissClip);
    public void PlayWin() => PlaySfx(winClip);
    public void PlayLose() => PlaySfx(loseClip);
}
