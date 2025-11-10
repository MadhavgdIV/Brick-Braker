// MissZone.cs
using UnityEngine;

public class MissZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Ball")) return;

        // Notify GameManager first so it can find the ball (if it uses FindObjectOfType)
        GameManager.Instance?.OnBallMiss();

        // Now deactivate the ball (optional)
        other.gameObject.SetActive(false);
    }
}
