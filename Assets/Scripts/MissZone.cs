using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MissZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ball"))
        {
            // Stop the ball
            other.gameObject.SetActive(false);

            // Play miss sound (optional)
            AudioManager.Instance?.PlayBallMiss();

            // Tell GameManager
            GameManager.Instance?.OnBallMiss();
        }
    }
}
