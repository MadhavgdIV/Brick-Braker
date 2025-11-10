using System.Linq; // <- required for Any()
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI titleText;

    [Header("Scene Names")]
    public string gameSceneName = "Game";
    public string gameOverSceneName = "GameOver";

    private void Start() => UpdateUI();
    private void OnEnable() => UpdateUI();

    private void UpdateUI()
    {
        if (coinsText != null)
        {
            int coins = EconomyManager.Instance != null ? EconomyManager.Instance.GetCoins() : PlayerPrefs.GetInt("Coins", 0);
            coinsText.text = "Coins: " + coins;
        }

        if (highScoreText != null)
        {
            highScoreText.text = "High Score: " + PlayerPrefs.GetInt("HighScore", 0);
        }
    }

    public void OnPlayButton()
    {
        Debug.Log("[Menu] OnPlayButton called. gameSceneName='" + gameSceneName + "'");

        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("[Menu] gameSceneName is empty!");
            return;
        }

#if UNITY_EDITOR
        bool inBuild = UnityEditor.EditorBuildSettings.scenes
            .Any(s => s.path.EndsWith("/" + gameSceneName + ".unity") || s.path.Contains("/" + gameSceneName + ".unity"));
        Debug.Log("[Menu] Scene in BuildSettings: " + inBuild);
#endif

        // Hold LeftShift while clicking to bypass economy checks and force-load the scene (debug)
        if (Input.GetKey(KeyCode.LeftShift))
        {
            Debug.Log("[Menu] LeftShift held: forcing scene load for debug.");
            // Clear any previously set entry-fee flag so a clean session starts
            EconomyManager.Instance?.ClearEntryFeePaid();
            SceneManager.LoadScene(gameSceneName);
            return;
        }

        if (EconomyManager.Instance == null)
        {
            Debug.LogWarning("[Menu] EconomyManager missing. To test scene load, hold LeftShift and press Play button.");
            return;
        }

        int fee = EconomyManager.Instance.entryFee;
        int currentCoins = EconomyManager.Instance.GetCoins();
        Debug.Log("[Menu] EconomyManager found. Coins = " + currentCoins + " EntryFee = " + fee);

        // ENTRY FEE GUARD: only spend once per match
        if (!EconomyManager.Instance.IsEntryFeePaid())
        {
            if (!EconomyManager.Instance.SpendCoins(fee))
            {
                Debug.LogWarning("[Menu] Not enough coins to start the game.");
                return;
            }
            EconomyManager.Instance.MarkEntryFeePaid();
            Debug.Log("[Menu] Entry fee charged and flag marked.");
        }
        else
        {
            Debug.Log("[Menu] Entry fee already paid for this session, skipping SpendCoins.");
        }

        // Default attempts
        PlayerPrefs.SetInt("Game_Attempts", EconomyManager.Instance.attemptsDefault);
        PlayerPrefs.Save();

        Debug.Log("[Menu] Loading scene '" + gameSceneName + "' now.");
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnCheatCoinsButton()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.CheatAddCoins(1000);
            UpdateUI();
        }
        else Debug.LogWarning("[Menu] EconomyManager not found.");
    }

    public void OnResetEconomyButton()
    {
        if (EconomyManager.Instance != null) EconomyManager.Instance.ResetEconomyToDefaults();
        PlayerPrefs.DeleteKey("HighScore");
        PlayerPrefs.Save();
        UpdateUI();
        Debug.Log("[Menu] Economy and high score reset.");
    }

    public void OnQuitButton()
    {
        Debug.Log("Quit Game pressed.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
