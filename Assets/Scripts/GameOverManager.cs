using UnityEngine;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI resultText; // "You Win!" or "Game Over"

    void Start()
    {
        int finalScore = PlayerPrefs.GetInt("FinalScore", 0);
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        int gameResult = PlayerPrefs.GetInt("GameResult", 0); // 1 = win, 0 = lose

        if (finalScoreText != null)
            finalScoreText.text = "Score: " + finalScore;

        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore;

        if (resultText != null)
            resultText.text = (gameResult == 1) ? "YOU WIN!" : "GAME OVER";
    }

    public void OnRestartButton()
    {
        // Note: GameManager is a DontDestroyOnLoad singleton; just call its restart
        GameManager.Instance?.RestartGame();
    }

    public void OnExitButton()
    {
        GameManager.Instance?.GoToMenu();
    }
}
