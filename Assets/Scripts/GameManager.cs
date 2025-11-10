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

    // Track breakable bricks
    private int remainingBreakableBricks = 0;

    // Attempts
    private int attemptsLeft = 0;

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

    void Start()
    {
        // read attempts set by the Lobby (default from EconomyManager otherwise)
        attemptsLeft = PlayerPrefs.GetInt("Game_Attempts", EconomyManager.Instance != null ? EconomyManager.Instance.attemptsDefault : 3);
    }

    // -------------------------------
    // Brick / Score Management
    // -------------------------------
    public void RegisterBreakable()
    {
        remainingBreakableBricks++;
    }

    public void OnBrickHit(int points)
    {
        if (isGameOver) return;

        score += points;
        Debug.Log("Brick destroyed! +" + points + " points. Total: " + score);

        remainingBreakableBricks--;
        if (remainingBreakableBricks <= 0)
        {
            OnAllBricksCleared();
        }
    }

    public int GetScore() => score;

    // -------------------------------
    // Game Flow
    // -------------------------------
    // Called when ball misses the paddle
    public void OnBallMiss()
    {
        if (isGameOver) return;

        attemptsLeft--;
        Debug.Log("Ball missed. Attempts left: " + attemptsLeft);

        if (attemptsLeft > 0)
        {
            // Respawn ball / continue round instead of immediate game over
            // You should implement respawn logic for the Ball or call an existing spawn routine.
            RespawnBall();
        }
        else
        {
            isGameOver = true;
            FinalizeScoreAndLoadGameOver(false);
        }
    }

    private void RespawnBall()
    {
        // Example: find Ball and reset position / velocity, or reload a Ball prefab
        Ball ball = FindObjectOfType<Ball>();
        if (ball != null)
        {
            ball.ResetBallForRetry(); // you'll add this method to Ball.cs
        }
        else
        {
            Debug.LogWarning("No Ball found to respawn.");
        }
    }

    private void OnAllBricksCleared()
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log("All bricks cleared! You Win!");

        // award coins for win
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.AddCoins(EconomyManager.Instance.winReward);
        }

        FinalizeScoreAndLoadGameOver(true);
    }

    private void FinalizeScoreAndLoadGameOver(bool didWin)
    {
        PlayerPrefs.SetInt("FinalScore", score);
        PlayerPrefs.SetInt("GameResult", didWin ? 1 : 0);

        // high score handling
        int prevHigh = PlayerPrefs.GetInt("HighScore", 0);
        if (score > prevHigh)
        {
            PlayerPrefs.SetInt("HighScore", score);
            Debug.Log("New high score: " + score);
        }
        PlayerPrefs.Save();

        // Also save economy file now (if applicable)
        EconomyManager.Instance?.SaveEconomy();

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
        remainingBreakableBricks = 0;
        // attemptsLeft will be re-read on Start when the Game scene loads
    }
}
