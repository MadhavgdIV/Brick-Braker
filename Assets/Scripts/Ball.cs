using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    public float speed = 8f;            // Speed after launch
    public float startDelay = 1f;       // Delay before dropping
    public float fallSpeed = 10f;       // Speed while falling before launch

    private Rigidbody2D rb;
    private bool hasLaunched = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ResetBall();
    }

    public void ResetBall()
    {
        rb.velocity = Vector2.zero;
        transform.position = Vector2.zero;
        hasLaunched = false;
        gameObject.SetActive(true);

        CancelInvoke();
        Invoke(nameof(DropBall), startDelay);
    }

    void DropBall()
    {
        // Make the ball fall straight down at fallSpeed
        rb.velocity = new Vector2(0f, -fallSpeed);
    }

    void FixedUpdate()
    {
        // Keep constant speed after launch
        if (hasLaunched && rb.velocity.magnitude > 0)
        {
            rb.velocity = rb.velocity.normalized * speed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // First paddle hit → launch upward
        if (!hasLaunched && collision.gameObject.CompareTag("Paddle"))
        {
            hasLaunched = true;

            float x = Random.Range(-0.5f, 0.5f);
            Vector2 direction = new Vector2(x, 1f).normalized;
            rb.velocity = direction * speed;

            AudioManager.Instance?.PlayPaddleHit();
            return;
        }

        // Normal gameplay bounce sound
        if (hasLaunched && collision.gameObject.CompareTag("Paddle"))
        {
            AudioManager.Instance?.PlayPaddleHit();
        }
    }
}
