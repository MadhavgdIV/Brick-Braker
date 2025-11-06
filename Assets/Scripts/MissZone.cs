using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MissZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ball"))
        {
            // Play miss sound
            AudioManager.Instance?.PlayBallMiss();
            GameManager.Instance.OnBallMiss();
        }
    }
}
