using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Scene")]
    public string gameSceneName = "Game";

    [Header("Economy Settings")]
    [Tooltip("Entry fee deducted before starting the game.")]
    public int entryFee = 100; // ✅ ENTRY FEE IMPLEMENTED HERE
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI entryFeeText;

    [Header("Popup - Not enough coins")]
    public CanvasGroup popupCanvasGroup;      
    public TextMeshProUGUI popupMessageText;  
    public float popupFadeDuration = 0.4f;
    public float popupVisibleDuration = 1.4f;

    [Header("Cheat / Debug")]
    public Button cheatButton;
    public int cheatAmount = 1000;

    private void Start()
    {
        // Check for EconomyManager
        if (EconomyManager.Instance == null)
        {
            Debug.LogError("❌ EconomyManager not found in scene! Add it to the Menu scene.");
            return;
        }

        // Initialize UI
        if (entryFeeText != null)
            entryFeeText.text = $"Entry Fee: {entryFee}";

        UpdateCoinUI();

        // Setup cheat button
        if (cheatButton != null)
            cheatButton.onClick.AddListener(() => {
                EconomyManager.Instance.AddCoins(cheatAmount);
                UpdateCoinUI();
            });

        // Hide popup at start
        if (popupCanvasGroup != null)
            popupCanvasGroup.alpha = 0f;
    }

    public void UpdateCoinUI()
    {
        if (coinText != null && EconomyManager.Instance != null)
            coinText.text = $"Coins: {EconomyManager.Instance.GetCoins()}";
    }

    // Called by the Play Button
    public void OnPlayButton()
    {
        if (EconomyManager.Instance == null)
        {
            Debug.LogError("❌ EconomyManager missing! Cannot check entry fee.");
            return;
        }

        // ✅ Deduct entry fee
        if (EconomyManager.Instance.TryDeduct(entryFee))
        {
            Debug.Log($"✅ Entry Fee of {entryFee} deducted. Starting Game...");
            UpdateCoinUI();
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.Log("⚠️ Not enough coins to start the game.");
            if (popupMessageText != null)
                popupMessageText.text = "Not enough coins!";
            StartCoroutine(ShowPopupCoroutine());
        }
    }

    private IEnumerator ShowPopupCoroutine()
    {
        if (popupCanvasGroup == null) yield break;

        float t = 0f;
        while (t < popupFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            popupCanvasGroup.alpha = Mathf.Clamp01(t / popupFadeDuration);
            yield return null;
        }
        popupCanvasGroup.alpha = 1f;

        yield return new WaitForSecondsRealtime(popupVisibleDuration);

        t = 0f;
        while (t < popupFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            popupCanvasGroup.alpha = 1f - Mathf.Clamp01(t / popupFadeDuration);
            yield return null;
        }
        popupCanvasGroup.alpha = 0f;
    }
}
