using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Scene Names")]
    public string menuSceneName = "Menu";
    public string gameSceneName = "Game";
    public string gameOverSceneName = "GameOverScene";

    [Header("Economy / Rewards")]
    public int winReward = 200;

    [Header("Attempt Mode")]
    public int maxAttempts = 3;
    public TextMeshProUGUI attemptsText; // optional UI to show attempts remaining

    private int attemptsRemaining;
    private int score = 0;
    private bool isGameOver = false;
    private int remainingBreakableBricks = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Only reset per-play state when the Game scene actually finishes loading.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == gameSceneName)
        {
            ResetForNewPlay();
        }
    }

    private void ResetForNewPlay()
    {
        attemptsRemaining = maxAttempts;
        score = 0;
        isGameOver = false;
        remainingBreakableBricks = 0;

        UpdateAttemptsUI();

        // It's safe to reset ball/paddle here because this runs only after Game scene loads.
        ResetBallAndPaddle();
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
    // Game Flow & Attempt handling
    // -------------------------------
    // Called by MissZone (passes the ball GameObject)
    public void OnBallMiss(GameObject ball)
    {
        if (isGameOver) return;

        attemptsRemaining = Mathf.Max(0, attemptsRemaining - 1);
        UpdateAttemptsUI();

        if (attemptsRemaining > 0)
        {
            Debug.Log($"Ball missed — attempts left: {attemptsRemaining}. Respawning ball and resetting paddle.");

            if (ball != null)
            {
                ball.SetActive(true);
                Ball ballComp = ball.GetComponent<Ball>();
                if (ballComp != null)
                    ballComp.ResetBall();
                else
                    Debug.LogWarning("OnBallMiss: Ball does not have Ball component.");
            }
            else
            {
                // If ball wasn't provided or not found, try to reset (should be rare)
                ResetBallAndPaddle();
            }

            ResetPaddle();
            return;
        }

        // No attempts left -> final game over
        isGameOver = true;
        Debug.Log("No attempts remaining. Game Over!");
        FinalizeScoreAndLoadGameOver(false);
    }

    private void OnAllBricksCleared()
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log("All bricks cleared! You Win!");
        FinalizeScoreAndLoadGameOver(true);
    }

    private void FinalizeScoreAndLoadGameOver(bool didWin)
    {
        PlayerPrefs.SetInt("FinalScore", score);
        PlayerPrefs.SetInt("GameResult", didWin ? 1 : 0);

        int prevHigh = PlayerPrefs.GetInt("HighScore", 0);
        if (score > prevHigh)
        {
            PlayerPrefs.SetInt("HighScore", score);
            Debug.Log("New high score: " + score);
        }

        PlayerPrefs.Save();

        if (didWin)
            EconomyManager.Instance?.AddCoins(winReward);

        // Load GameOver scene
        SceneManager.LoadScene(gameOverSceneName);
    }

    private void ResetBallAndPaddle()
    {
        GameObject ballObj = GameObject.FindWithTag("Ball");
        if (ballObj != null)
        {
            Ball b = ballObj.GetComponent<Ball>();
            if (b != null)
                b.ResetBall();
            else
                Debug.LogWarning("ResetBallAndPaddle: Ball found but no Ball component attached.");
        }
        else
        {
            Debug.LogWarning("ResetBallAndPaddle: No active Ball object found with tag 'Ball'.");
        }

        ResetPaddle();
    }

    private void ResetPaddle()
    {
        GameObject paddleObj = GameObject.FindWithTag("Paddle");
        if (paddleObj != null)
        {
            Paddle p = paddleObj.GetComponent<Paddle>();
            if (p != null)
                p.ResetPaddle();
            else
                Debug.LogWarning("ResetPaddle: Paddle found but no Paddle component attached.");
        }
        else
        {
            Debug.LogWarning("ResetPaddle: No active Paddle object found with tag 'Paddle'.");
        }
    }

    private void UpdateAttemptsUI()
    {
        if (attemptsText != null)
            attemptsText.text = $"Attempts: {attemptsRemaining}";
    }

    // Public UI / flow helpers
    public void RestartGame()
    {
        // Just load the Game scene — ResetForNewPlay will execute in OnSceneLoaded.
        SceneManager.LoadScene(gameSceneName);
    }

    public void GoToMenu()
    {
        // Just load the Menu scene — per-play resets will occur next time Game scene loads.
        SceneManager.LoadScene(menuSceneName);
    }
}
