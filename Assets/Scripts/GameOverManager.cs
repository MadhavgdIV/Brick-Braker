using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public TextMeshProUGUI finalScoreText;
    public string mainGameSceneName = "Game"; // Main game scene
    public string menuSceneName = "Menu";     // Menu scene

    private void Start()
    {
        int finalScore = PlayerPrefs.GetInt("FinalScore", 0);
        if (finalScoreText != null)
            finalScoreText.text = $"GAME OVER!\nScore: {finalScore}";
    }

    public void OnRestartButton()
    {
        Debug.Log("Restart button clicked!");

        if (GameManager.Instance != null)
            GameManager.Instance.ResetGame();

        SceneManager.LoadScene(mainGameSceneName);
    }

    // Add this method for the Exit button
    public void OnExitButton()
    {
        Debug.Log("Exit button clicked!");
        SceneManager.LoadScene(menuSceneName);
    }
}
