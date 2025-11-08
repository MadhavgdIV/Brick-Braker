using UnityEngine;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public TextMeshProUGUI finalScoreText;

    void Start()
    {
        int finalScore = PlayerPrefs.GetInt("FinalScore", 0);
        if (finalScoreText != null)
            finalScoreText.text = "GAME OVER\nScore: " + finalScore;
    }

    public void OnRestartButton()
    {
        GameManager.Instance.RestartGame();
    }

    public void OnExitButton()
    {
        GameManager.Instance.GoToMenu();
    }
}
