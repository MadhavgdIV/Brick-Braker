using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI entryFeeText;
    public Button playButton;
    public Button cheatButton;
    public TextMeshProUGUI attemptsText;

    [Header("Scenes")]
    public string gameSceneName = "Game";

    private int attemptsToUse;

    private void Start()
    {
        if (EconomyManager.Instance == null)
        {
            Debug.LogError("EconomyManager missing in scene. Add it as a GameObject and set as DontDestroyOnLoad.");
            return;
        }

        RefreshUI();

        // Setup button listeners
        playButton.onClick.AddListener(OnPlayPressed);
        cheatButton.onClick.AddListener(OnCheatPressed);

        // initial attempts
        attemptsToUse = EconomyManager.Instance.attemptsDefault;
        UpdateAttemptsText();
    }

    private void RefreshUI()
    {
        coinsText.text = "Coins: " + EconomyManager.Instance.GetCoins();
        entryFeeText.text = "Entry: " + EconomyManager.Instance.entryFee;
    }

    private void UpdateAttemptsText()
    {
        attemptsText.text = "Attempts: " + attemptsToUse;
    }

    public void OnChangeAttempts(int delta)
    {
        attemptsToUse = Mathf.Clamp(attemptsToUse + delta, 1, 10);
        UpdateAttemptsText();
    }

    private void OnPlayPressed()
    {
        int fee = EconomyManager.Instance.entryFee;
        if (EconomyManager.Instance.GetCoins() < fee)
        {
            // not enough coins - show message
            Debug.LogWarning("Not enough coins to enter.");
            // optionally show a popup UI
            return;
        }

        // deduct entry fee
        bool spent = EconomyManager.Instance.SpendCoins(fee);
        if (!spent) return;

        // Pass attempts to GameManager via PlayerPrefs or a GameManager API (we'll use PlayerPrefs for simplicity)
        PlayerPrefs.SetInt("Game_Attempts", attemptsToUse);
        PlayerPrefs.Save();

        RefreshUI();

        // Load the Game scene
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnCheatPressed()
    {
        // cheating adds 1000 coins (as requested)
        EconomyManager.Instance.CheatAddCoins(1000);
        RefreshUI();
    }

    private void OnEnable()
    {
        // refresh on returning to lobby
        if (EconomyManager.Instance != null) RefreshUI();
    }
}
