using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sliding toggle switch: animates the handle left / right and toggles AudioManager sound state.
/// Persists state to PlayerPrefs under key "sound_on" (1 = on, 0 = off).
/// Attach to the ToggleBackground GameObject (the clickable area).
/// This version auto-finds references when possible and provides helpful logs.
/// </summary>
[RequireComponent(typeof(Button))]
public class SlidingToggle : MonoBehaviour
{
    [Header("References (assign in Inspector or let script auto-find)")]
    public RectTransform handle;              // the small knob that slides
    public RectTransform handleParent;        // usually the ToggleBackground rect (optional fallback)
    public Image backgroundImage;             // background image to tint on/off

    [Header("Appearance")]
    public Vector2 handleOnAnchoredPos = new Vector2(36f, 0f);
    public Vector2 handleOffAnchoredPos = new Vector2(-36f, 0f);
    public float animationDuration = 0.12f;

    public Color colorOn = new Color(0.22f, 0.78f, 0.34f);
    public Color colorOff = new Color(0.4f, 0.4f, 0.4f);

    // runtime
    private bool isOn = true;
    private Coroutine animRoutine;

    private const string PREF_KEY = "sound_on";

    void Awake()
    {
        // find parent rect if not given
        if (handleParent == null)
            handleParent = GetComponent<RectTransform>();

        // find background image if not assigned
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        // try to auto-find a handle if none assigned
        if (handle == null)
        {
            // heuristic 1: child named "Handle"
            Transform t = transform.Find("Handle");
            if (t != null && t is RectTransform)
            {
                handle = t as RectTransform;
            }
            else
            {
                // heuristic 2: first child with RectTransform + Image
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform c = transform.GetChild(i);
                    if (c is RectTransform && c.GetComponent<Image>() != null)
                    {
                        handle = c as RectTransform;
                        break;
                    }
                }
            }
        }

        if (handle == null)
        {
            Debug.LogError($"SlidingToggle on '{gameObject.name}' requires a handle RectTransform assigned (child named 'Handle' or assign in Inspector).");
        }

        // wire button click to Toggle()
        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => Toggle());
        }
    }

    void Start()
    {
        // load saved state (default true)
        isOn = PlayerPrefs.GetInt(PREF_KEY, 1) == 1;

        // ensure AudioManager exists (best practice: place one in Menu scene)
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManager not found. Add it to Menu scene for persistent audio control.");
        }
        else
        {
            // apply state through public API
            AudioManager.Instance.SetSoundState(isOn);
        }

        // set visuals instantly (safe even if handle/background are missing)
        ApplyVisualsInstant();
    }

    /// <summary>
    /// Toggle app state and animate UI.
    /// </summary>
    public void Toggle()
    {
        isOn = !isOn;

        // save preference
        PlayerPrefs.SetInt(PREF_KEY, isOn ? 1 : 0);
        PlayerPrefs.Save();

        // notify AudioManager safely
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSoundState(isOn);
        }

        // animate handle
        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(AnimateHandle(isOn));
    }

    private IEnumerator AnimateHandle(bool toOn)
    {
        if (handle == null)
        {
            // nothing to animate; still update background color if possible
            if (backgroundImage != null)
                backgroundImage.color = toOn ? colorOn : colorOff;
            yield break;
        }

        Vector2 start = handle.anchoredPosition;
        Vector2 target = toOn ? handleOnAnchoredPos : handleOffAnchoredPos;
        float elapsed = 0f;

        // change color immediately
        if (backgroundImage != null)
            backgroundImage.color = toOn ? colorOn : colorOff;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            handle.anchoredPosition = Vector2.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        handle.anchoredPosition = target;
    }

    private void ApplyVisualsInstant()
    {
        if (handle != null)
            handle.anchoredPosition = isOn ? handleOnAnchoredPos : handleOffAnchoredPos;

        if (backgroundImage != null)
            backgroundImage.color = isOn ? colorOn : colorOff;
    }

    // Public getter
    public bool IsOn() => isOn;
}
