using System.IO;
using UnityEngine;
using TMPro;


public class GameOverManager : MonoBehaviour
{
    [Header("UI - assign in Inspector")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI resultText;      // "YOU WIN!" or "GAME OVER"
    public TextMeshProUGUI coinsText;       // show current coins
    public TextMeshProUGUI streakBonusText; // optional: separate field to show "+X bonus"

    void Start()
    {
        // Ensure EconomyManager exists. If not found, create one so coin data can be read.
        if (EconomyManager.Instance == null)
        {
            Debug.LogWarning("EconomyManager instance not found in scene. Creating a temporary EconomyManager GameObject.");
            GameObject go = new GameObject("EconomyManager");
            go.AddComponent<EconomyManager>();
            // EconomyManager Awake() will run and load/save file as needed
        }

        // Read values (score/highscore/game result come from PlayerPrefs in existing flow)
        int finalScore = PlayerPrefs.GetInt("FinalScore", 0);
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        int gameResult = PlayerPrefs.GetInt("GameResult", 0); // 1 = win, 0 = lose

        // Coins: try EconomyManager (preferred). If still null, fallback to 0.
        int coins = 0;
        if (EconomyManager.Instance != null)
        {
            coins = EconomyManager.Instance.GetCoins();
        }
        else
        {
            Debug.LogWarning("EconomyManager still null after attempted creation. Showing 0 coins.");
        }

        // Last streak bonus (saved by GameManager before loading this scene)
        int lastStreakBonus = PlayerPrefs.GetInt("LastStreakBonus", 0);

        // Safely populate UI elements (guard each one)
        if (finalScoreText != null)
            finalScoreText.text = "Score: " + finalScore;
        else
            Debug.LogWarning("GameOverManager: finalScoreText is not assigned in the Inspector.");

        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore;
        else
            Debug.LogWarning("GameOverManager: highScoreText is not assigned in the Inspector.");

        if (resultText != null)
            resultText.text = (gameResult == 1) ? "YOU WIN!" : "GAME OVER";
        else
            Debug.LogWarning("GameOverManager: resultText is not assigned in the Inspector.");

        if (coinsText != null)
            coinsText.text = "Coins: " + coins;
        else
            Debug.LogWarning("GameOverManager: coinsText is not assigned in the Inspector.");

        // Show the streak bonus if the player won and a bonus exists
        if (gameResult == 1 && lastStreakBonus > 0)
        {
            string bonusText = $"+{lastStreakBonus} bonus!";
            if (streakBonusText != null)
            {
                streakBonusText.text = bonusText;
            }
            else
            {
                // fallback: append to the result text if no separate field assigned
                if (resultText != null)
                    resultText.text = resultText.text + " (" + bonusText + ")";
            }
        }
        else
        {
            // clear the streakBonusText if assigned
            if (streakBonusText != null)
                streakBonusText.text = "";
        }
    }

    // Optional UI hooks
    public void OnRestartButton()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
        else
        {
            Debug.LogWarning("GameManager not found when trying to restart — loading Game scene directly.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        }
    }

    public void OnExitButton()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.GoToMenu();
        else
        {
            Debug.LogWarning("GameManager not found when trying to exit — loading Menu scene directly.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }
    }
}
