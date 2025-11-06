using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public TextMeshProUGUI finalScoreText;
    public string mainGameSceneName = "Game"; // <-- Updated to your main game scene

    private void Start()
    {
        int finalScore = PlayerPrefs.GetInt("FinalScore", 0);
        if (finalScoreText != null)
            finalScoreText.text = $"GAME OVER!\nScore: {finalScore}";
    }

    public void OnRestartButton()
    {
        Debug.Log("Restart button clicked!"); // should appear in console

        if (GameManager.Instance != null)
            GameManager.Instance.ResetGame();

        SceneManager.LoadScene(mainGameSceneName);
    }
}
        