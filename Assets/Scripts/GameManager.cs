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
    [Tooltip("Coins awarded to the player when they win a level (base reward).")]
    public int winReward = 200; // base reward on win

    [Header("Attempt Mode")]
    [Tooltip("Maximum attempts (lives) allowed per play session.")]
    public int maxAttempts = 3;
    public TextMeshProUGUI attemptsText; // optional: assign in Game scene HUD

    // runtime state
    private int attemptsRemaining;
    private int score = 0;
    private bool isGameOver = false;
    private int remainingBreakableBricks = 0;

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
            return;
        }

        // Ensure GameDataManager exists early so saves will succeed later.
        // This creates a GameDataManager GameObject if one wasn't placed in the scene.
        GameDataManager.EnsureExists();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Reset per-play state when the Game scene finishes loading.
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

        // Reset paddle first, then ball so ball spawns above the paddle's reset position.
        ResetPaddle();
        ResetBall(); // reset only the ball (helpers below will find ball by tag)
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
        Debug.Log($"Brick destroyed! +{points} points. Total: {score}");

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
    // Called by MissZone (message passes the ball GameObject)
    public void OnBallMiss(GameObject ball)
    {
        if (isGameOver) return;

        attemptsRemaining = Mathf.Max(0, attemptsRemaining - 1);
        UpdateAttemptsUI();

        if (attemptsRemaining > 0)
        {
            Debug.Log($"Ball missed — attempts left: {attemptsRemaining}. Respawning ball and resetting paddle.");

            // IMPORTANT: reset paddle first so the ball will spawn relative to the paddle's reset position
            ResetPaddle();

            if (ball != null)
            {
                // Ensure ball is active and then reset it (ResetBall will look up the paddle and position above it)
                ball.SetActive(true);
                Ball ballComp = ball.GetComponent<Ball>();
                if (ballComp != null)
                    ballComp.ResetBall();
                else
                    Debug.LogWarning("OnBallMiss: Ball does not have Ball component.");
            }
            else
            {
                // fallback: reset both in correct order
                ResetBallAndPaddle();
            }

            return;
        }

        // No attempts left -> final game over (loss)
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

    // Shared finalization logic for win/lose
    private void FinalizeScoreAndLoadGameOver(bool didWin)
    {
        // Award coins and update streaks
        int lastStreakBonus = 0;

        if (didWin)
        {
            if (EconomyManager.Instance != null)
            {
                // Award base win reward
                EconomyManager.Instance.AddCoins(winReward);

                // Award the streak bonus (this increments persisted streak and returns bonus)
                lastStreakBonus = EconomyManager.Instance.OnPlayerWinAndGetBonus();

                Debug.Log($"Win: base reward {winReward} + streak bonus {lastStreakBonus} (streak now {EconomyManager.Instance.GetWinStreak()})");
            }
            else
            {
                Debug.LogWarning($"Player won but EconomyManager is missing; could not award {winReward} or streak bonus.");
            }
        }
        else
        {
            // Player lost -> reset streak
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.OnPlayerLose_ResetStreak();
            }
            else
            {
                Debug.LogWarning("Player lost but EconomyManager is missing; cannot reset streak.");
            }
        }

        // Ensure a GameDataManager exists and save results to JSON
        GameDataManager.EnsureExists(); // safe no-op if already present

        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.SetFinalResults(score, didWin, lastStreakBonus);

            // Debug: read back what was saved and log — helps confirm file wrote correctly
            var saved = GameDataManager.Instance.GetData();
            Debug.Log($"GameData saved -> finalScore: {saved.finalScore}, highScore: {saved.highScore}, didWin: {saved.didWin}, lastStreakBonus: {saved.lastStreakBonus}");
        }
        else
        {
            Debug.LogError("GameDataManager instance not found — results will not be persisted to JSON.");
        }

        // Load GameOver scene
        SceneManager.LoadScene(gameOverSceneName);
    }

    private void ResetBallAndPaddle()
    {
        // reset paddle first
        ResetPaddle();

        // then reset ball
        ResetBall();
    }

    private void ResetBall()
    {
        GameObject ballObj = GameObject.FindWithTag("Ball");
        if (ballObj != null)
        {
            Ball b = ballObj.GetComponent<Ball>();
            if (b != null)
                b.ResetBall();
            else
                Debug.LogWarning("ResetBall: Ball found but no Ball component attached.");
        }
        else
        {
            Debug.LogWarning("ResetBall: No active Ball object found with tag 'Ball'.");
        }
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
        // Loading the Game scene will trigger ResetForNewPlay via OnSceneLoaded
        SceneManager.LoadScene(gameSceneName);
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(menuSceneName);
    }
}
