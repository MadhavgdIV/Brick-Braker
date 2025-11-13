using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DayBasedMusicManager : MonoBehaviour
{
    [Header("Audio Sources (optional)")]
    [Tooltip("Primary audio source. If null, one will be created.")]
    public AudioSource audioSourceA;
    [Tooltip("Secondary audio source used for crossfades. If null, one will be created.")]
    public AudioSource audioSourceB;

    [Header("Per-day clips (index 0 = Sunday, 1 = Monday ... 6 = Saturday)")]
    public AudioClip[] dayClips = new AudioClip[7];

    [Header("Crossfade / playback settings")]
    [Tooltip("Seconds to crossfade between tracks (0 = instant switch)")]
    public float crossfadeDuration = 2f;
    [Tooltip("Whether to loop the played clip")]
    public bool loop = true;
    [Tooltip("Global music volume (0..1)")]
    [Range(0f,1f)] public float volume = 1f;

    [Header("Testing / override")]
    public bool forceDayEnabled = false;
    public DayOfWeek forceDay = DayOfWeek.Monday;

    // internal
    private AudioSource _active;
    private AudioSource _inactive;
    private Coroutine _fadeCoroutine;
    private DayOfWeek? _appliedDay = null;

    private void Awake()
    {
        SetupAudioSources();
        ApplyForToday(); // initial apply
    }

    private void SetupAudioSources()
    {
        // Create or use provided sources (both on this GameObject for easy management)
        if (audioSourceA == null)
        {
            audioSourceA = gameObject.AddComponent<AudioSource>();
            audioSourceA.playOnAwake = false;
        }
        if (audioSourceB == null)
        {
            audioSourceB = gameObject.AddComponent<AudioSource>();
            audioSourceB.playOnAwake = false;
        }

        // Make sure both have same general settings
        audioSourceA.loop = loop;
        audioSourceB.loop = loop;
        audioSourceA.volume = 0f; // start silent
        audioSourceB.volume = 0f;
        audioSourceA.spatialBlend = 0f;
        audioSourceB.spatialBlend = 0f;

        _active = audioSourceA;
        _inactive = audioSourceB;
    }

    public void ApplyForToday()
    {
        DayOfWeek day = forceDayEnabled ? forceDay : DateTime.Now.DayOfWeek;
        if (_appliedDay.HasValue && _appliedDay.Value == day) return;
        _appliedDay = day;
        PlayClipForDay(day);
    }


    public void PlayClipForDay(DayOfWeek day)
    {
        int idx = (int)day;
        AudioClip clip = (dayClips != null && idx >= 0 && idx < dayClips.Length) ? dayClips[idx] : null;
        if (clip == null)
        {
            Debug.Log($"DayBasedMusicManager: No clip assigned for {day}.");
            return;
        }

        // If the same clip already playing on active, do nothing
        if (_active.clip == clip && _active.isPlaying) return;

        // Stop previous fade if any
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(CrossfadeToClip(clip, crossfadeDuration));
    }

    private IEnumerator CrossfadeToClip(AudioClip clip, float duration)
    {
        // Prepare inactive
        _inactive.clip = clip;
        _inactive.loop = loop;
        _inactive.Play();
        float targetVol = volume;
        float t = 0f;

        // if duration == 0 instantly swap
        if (duration <= 0f)
        {
            _inactive.volume = targetVol;
            _active.volume = 0f;
            _active.Stop();
            SwapActiveInactive();
            _fadeCoroutine = null;
            yield break;
        }

        // ramp volumes: active -> 0 ; inactive -> targetVol
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            _inactive.volume = Mathf.Lerp(0f, targetVol, k);
            _active.volume = Mathf.Lerp(targetVol, 0f, k);
            yield return null;
        }

        // finalize
        _inactive.volume = targetVol;
        _active.volume = 0f;
        _active.Stop();
        SwapActiveInactive();
        _fadeCoroutine = null;
    }

    private void SwapActiveInactive()
    {
        var tmp = _active;
        _active = _inactive;
        _inactive = tmp;
    }

    public void RefreshSettings()
    {
        audioSourceA.loop = loop;
        audioSourceB.loop = loop;
        _active.volume = volume;
        _inactive.volume = 0f;
    }

    // Optional: expose API to manually set day (useful for menu testing)
    public void SetForcedDay(DayOfWeek day)
    {
        forceDay = day;
        forceDayEnabled = true;
        ApplyForToday();
    }
}
