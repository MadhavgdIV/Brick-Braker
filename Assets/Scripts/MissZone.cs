using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MissZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ball"))
        {
            // Play miss sound (optional) — keep as-is in AudioManager
            AudioManager.Instance?.PlayBallMiss();

            // Tell GameManager and pass the ball GameObject (so it can decide what to do)
            GameManager.Instance?.OnBallMiss(other.gameObject);
        }
    }
}
