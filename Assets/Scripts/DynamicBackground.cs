using System;
using System.Collections;
using UnityEngine;


[ExecuteAlways]
public class DynamicBackground : MonoBehaviour
{
    public enum Mode { SpriteRenderer, CameraColor }

    [Header("Mode")]
    public Mode mode = Mode.SpriteRenderer;

    [Header("Common (optional)")]
    [Tooltip("Enable to force a day for testing. Choose a day below.")]
    public bool forceDayEnabled = false;
    public DayOfWeek forceDay = DayOfWeek.Monday;

    [Header("Sprite Renderer Mode")]
    [Tooltip("SpriteRenderer used as the background (should be behind game objects).")]
    public SpriteRenderer backgroundRenderer;

    [Tooltip("Optional secondary renderer used for crossfade. If empty the script will create a child GameObject with a SpriteRenderer.")]
    public SpriteRenderer secondaryRenderer;

    [Tooltip("Sprites mapped to DayOfWeek. Index 0 = Sunday ... 6 = Saturday. Leave null if you don't want to change that day.")]
    public Sprite[] daySprites = new Sprite[7];

    [Tooltip("Crossfade duration in seconds when switching sprites (0 = immediate).")]
    public float spriteCrossfadeDuration = 0.5f;

    [Header("Camera Color Mode")]
    [Tooltip("Camera to change background color for (if using CameraColor mode).")]
    public Camera targetCamera;

    [Tooltip("Colors mapped to DayOfWeek. Index 0 = Sunday ... 6 = Saturday. Leave transparent/black if not used.")]
    public Color[] dayColors = new Color[7];

    [Tooltip("Smoothly interpolate camera color (0 = immediate).")]
    public float cameraColorLerpDuration = 0.5f;

    // internal
    private DayOfWeek currentAppliedDay = (DayOfWeek)(-1);
    private Coroutine fadeCoroutine;

    private void Reset()
    {
        // initialize arrays to length 7 if not already
        if (daySprites == null || daySprites.Length != 7) daySprites = new Sprite[7];
        if (dayColors == null || dayColors.Length != 7) dayColors = new Color[7];
    }

    private void Awake()
    {
        // Ensure arrays are present
        Reset();

        // If sprite mode and no secondary renderer, try create one for crossfade
        if (mode == Mode.SpriteRenderer && backgroundRenderer != null && secondaryRenderer == null)
        {
            CreateSecondaryRenderer();
        }
    }

    private void OnEnable()
    {
        ApplyBackgroundForToday();
    }

#if UNITY_EDITOR
    private void Update()
    {
        // In editor, allow changes to take effect immediately when editing values
        if (!Application.isPlaying)
        {
            ApplyBackgroundForToday();
        }
    }
#endif

  
    public void ApplyBackgroundForToday()
    {
        DayOfWeek day = forceDayEnabled ? forceDay : DateTime.Now.DayOfWeek;

        // If same day already applied, do nothing
        if (day == currentAppliedDay) return;

        currentAppliedDay = day;

        if (mode == Mode.SpriteRenderer)
            ApplySpriteForDay(day);
        else
            ApplyCameraColorForDay(day);
    }

    private void ApplySpriteForDay(DayOfWeek day)
    {
        if (backgroundRenderer == null)
        {
            Debug.LogWarning("DynamicBackground: backgroundRenderer is not assigned.");
            return;
        }

        int idx = (int)day;
        Sprite newSprite = (daySprites != null && idx >= 0 && idx < daySprites.Length) ? daySprites[idx] : null;

        // If null, we simply do nothing (preserve existing), but you could set to default if desired
        if (newSprite == null)
        {
            Debug.Log($"DynamicBackground: No sprite assigned for {day}. Background left unchanged.");
            return;
        }

        // If no crossfade => immediate
        if (spriteCrossfadeDuration <= 0f || secondaryRenderer == null)
        {
            backgroundRenderer.sprite = newSprite;
            return;
        }

        // Crossfade: ensure secondary renderer exists
        if (secondaryRenderer == null) CreateSecondaryRenderer();

        // Stop any running fade and start new
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(CrossfadeSprites(backgroundRenderer, secondaryRenderer, newSprite, spriteCrossfadeDuration));
    }

    private IEnumerator CrossfadeSprites(SpriteRenderer primary, SpriteRenderer secondary, Sprite targetSprite, float duration)
    {
        // secondary shows current primary sprite initially
        secondary.sprite = primary.sprite;
        secondary.enabled = true;
        primary.enabled = true;

        // prepare primary with the target sprite but invisible
        primary.sprite = targetSprite;
        Color pCol = primary.color;
        Color sCol = secondary.color;

        // ensure alpha starts 0 for primary and 1 for secondary
        primary.color = new Color(pCol.r, pCol.g, pCol.b, 0f);
        secondary.color = new Color(sCol.r, sCol.g, sCol.b, 1f);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            primary.color = new Color(pCol.r, pCol.g, pCol.b, k);
            secondary.color = new Color(sCol.r, sCol.g, sCol.b, 1f - k);
            yield return null;
        }

        // finalize
        primary.color = new Color(pCol.r, pCol.g, pCol.b, 1f);
        secondary.sprite = null;
        secondary.enabled = false;
        secondary.color = sCol;

        fadeCoroutine = null;
    }

    private void ApplyCameraColorForDay(DayOfWeek day)
    {
        if (targetCamera == null)
        {
            Debug.LogWarning("DynamicBackground: targetCamera is not assigned.");
            return;
        }

        int idx = (int)day;
        Color target = (dayColors != null && idx >= 0 && idx < dayColors.Length) ? dayColors[idx] : targetCamera.backgroundColor;

        if (cameraColorLerpDuration <= 0f)
        {
            targetCamera.backgroundColor = target;
            return;
        }

        // animate color
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(LerpCameraColor(targetCamera, targetCamera.backgroundColor, target, cameraColorLerpDuration));
    }

    private IEnumerator LerpCameraColor(Camera cam, Color from, Color to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cam.backgroundColor = Color.Lerp(from, to, Mathf.Clamp01(t / duration));
            yield return null;
        }
        cam.backgroundColor = to;
        fadeCoroutine = null;
    }

    private void CreateSecondaryRenderer()
    {
        if (backgroundRenderer == null) return;

        GameObject go = new GameObject("BG_Secondary");
        go.hideFlags = HideFlags.DontSaveInBuild; // optional
        go.transform.SetParent(backgroundRenderer.transform.parent, false);
        go.transform.SetSiblingIndex(backgroundRenderer.transform.GetSiblingIndex()); // keep order

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerID = backgroundRenderer.sortingLayerID;
        sr.sortingOrder = backgroundRenderer.sortingOrder - 1; // behind by default; adjust if needed
        sr.sprite = null;
        sr.enabled = false;
        secondaryRenderer = sr;
    }

    public void RefreshNow()
    {
        ApplyBackgroundForToday();
    }
}
