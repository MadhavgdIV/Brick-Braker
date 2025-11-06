using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Elements (Optional in Main Scene)")]
    public TextMeshProUGUI gameOverText; // Assign if you want in-game UI

    private int score = 0;
    private bool isGameOver = false;

    [Header("Scene Names")]
    public string mainGameSceneName = "Game";       // <-- Updated to your main game scene
    public string gameOverSceneName = "GameOverScene";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);
    }

    public void OnBrickHit(Brick brick)
    {
        if (!brick.unbreakable)
        {
            score += brick.points;
            Debug.Log($"Brick hit! +{brick.points} points. Total score: {score}");
        }
    }

    public void OnBallMiss()
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log("Ball missed! Game over!");

        if (gameOverText != null)
            ShowGameOverUI();

        PlayerPrefs.SetInt("FinalScore", score);
        SceneManager.LoadScene(gameOverSceneName);
    }

    private void ShowGameOverUI()
    {
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = $"GAME OVER!\nScore: {score}";
        }
    }

    public void ResetGame()
    {
        score = 0;
        isGameOver = false;
        Time.timeScale = 1f;

        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);
    }

    public int GetScore() => score;
}
