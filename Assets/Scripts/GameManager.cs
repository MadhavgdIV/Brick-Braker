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
    // Brick / Score Management
    // -------------------------------
    // Called by Brick.ResetBrick() for each breakable brick
    public void RegisterBreakable()
    {
        // Only register during gameplay scenes
        remainingBreakableBricks++;
        //Debug.Log("Registered breakable brick. Remaining: " + remainingBreakableBricks);
    }

    // Called by Brick when it is destroyed (previously OnBrickHit)
    public void OnBrickHit(int points)
    {
        if (isGameOver) return;

        // Add points
        score += points;
        Debug.Log("Brick destroyed! +" + points + " points. Total: " + score);

        // Decrement remaining breakable bricks and check for win
        remainingBreakableBricks--;
        //Debug.Log("Breakable bricks left: " + remainingBreakableBricks);
        if (remainingBreakableBricks <= 0)
        {
            OnAllBricksCleared();
        }
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
        // Save final score for GameOver scene
        PlayerPrefs.SetInt("FinalScore", score);

        // Save result: 1 = win, 0 = lose
        PlayerPrefs.SetInt("GameResult", didWin ? 1 : 0);

        // Update high score if needed
        int prevHigh = PlayerPrefs.GetInt("HighScore", 0);
        if (score > prevHigh)
        {
            PlayerPrefs.SetInt("HighScore", score);
            Debug.Log("New high score: " + score);
        }

        // Make sure prefs are written
        PlayerPrefs.Save();

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
        remainingBreakableBricks = 0;
    }
}
