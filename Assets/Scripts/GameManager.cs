using UnityEngine;
using TMPro; // for TextMeshPro UI

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Elements")]
    public TextMeshProUGUI gameOverText; // Assign in Inspector

    private int score;
    private bool isGameOver = false;

    private void Awake()
    {
        // Set up singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void OnBrickHit(Brick brick)
    {
        if (!brick.unbreakable)
        {
            score += brick.points;
            Debug.Log($"Brick hit! +{brick.points} points. Total score: {score}");
        }
        else
        {
            Debug.Log("Hit an unbreakable brick!");
        }
    }

    // 👇 Called when the ball hits the miss zone
    public void OnBallMiss()
    {
        if (isGameOver) return; // prevent duplicate calls
        isGameOver = true;

        Debug.Log("Ball missed! Game over!");
        ShowGameOverUI();
        PauseGame();
    }

    private void ShowGameOverUI()
    {
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = "GAME OVER!";
        }
        else
        {
            Debug.LogWarning("Game Over Text not assigned in Inspector!");
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0f; // ⏸️ stops the game
    }

    public void ResetGame()
    {
        score = 0;
        isGameOver = false;
        Time.timeScale = 1f; // resume
        Debug.Log("Game reset.");
    }

    public int GetScore()
    {
        return score;
    }
}
