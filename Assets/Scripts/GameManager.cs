using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Scene Names")]
    public string menuSceneName = "Menu";
    public string gameSceneName = "Game";
    public string gameOverSceneName = "GameOverScene";

    private int score = 0;
    private bool isGameOver = false;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // -------------------------------
    // Score Management
    // -------------------------------
    public void OnBrickHit(int points)
    {
        if (isGameOver) return;
        score += points;
        Debug.Log("Brick hit! +" + points + " points. Total: " + score);
    }

    public int GetScore() => score;

    // -------------------------------
    // Game Flow
    // -------------------------------
    public void OnBallMiss()
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log("Ball missed! Game Over!");

        // Save score for GameOver scene
        PlayerPrefs.SetInt("FinalScore", score);

        // Load GameOver scene
        SceneManager.LoadScene(gameOverSceneName);
    }

    public void RestartGame()
    {
        ResetGame();
        SceneManager.LoadScene(gameSceneName);
    }

    public void GoToMenu()
    {
        ResetGame();
        SceneManager.LoadScene(menuSceneName);
    }

    private void ResetGame()
    {
        score = 0;
        isGameOver = false;
    }
}
