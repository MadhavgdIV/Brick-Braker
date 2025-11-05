using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MissZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ball"))
        {
            GameManager.Instance.OnBallMiss();
        }
    }
}
