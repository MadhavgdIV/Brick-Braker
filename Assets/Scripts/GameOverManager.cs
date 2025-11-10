using UnityEngine;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI coinsText;

    void Start()
    {
        int finalScore = PlayerPrefs.GetInt("FinalScore", 0);
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        int gameResult = PlayerPrefs.GetInt("GameResult", 0); // 1 = win, 0 = lose

        if (finalScoreText != null) finalScoreText.text = "Score: " + finalScore;
        if (highScoreText != null) highScoreText.text = "High Score: " + highScore;
        if (resultText != null) resultText.text = (gameResult == 1) ? "YOU WIN!" : "GAME OVER";
        if (coinsText != null) coinsText.text = "Coins: " + (EconomyManager.Instance != null ? EconomyManager.Instance.GetCoins() : PlayerPrefs.GetInt("Coins", 0));
    }

    public void OnRestartButton()
    {
        GameManager.Instance?.RestartGame();
    }

    public void OnExitButton()
    {
        GameManager.Instance?.GoToMenu();
    }
}
