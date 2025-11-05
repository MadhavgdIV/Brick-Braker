using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private int score;

    private void Awake()
    {
        // Set up the singleton
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

    public void ResetGame()
    {
        score = 0;
        Debug.Log("Game reset.");
    }

    public int GetScore()
    {
        return score;
    }
}
