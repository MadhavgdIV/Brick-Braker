using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Paddle : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 direction;

    public float moveSpeed = 15f;
    public float maxBounceAngle = 60f; // Smaller angle for smoother gameplay

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    void Start()
    {
        ResetPaddle();
    }

    public void ResetPaddle()
    {
        rb.velocity = Vector2.zero;
        transform.position = new Vector2(0f, transform.position.y);
    }

    void Update()
    {
        // Direct movement (easier and more responsive)
        float moveInput = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            moveInput = -1f;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            moveInput = 1f;

        direction = new Vector2(moveInput, 0f);
    }

    void FixedUpdate()
    {
        // Direct velocity movement (not physics-based)
        rb.velocity = direction * moveSpeed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Ball"))
            return;

        Rigidbody2D ballRb = collision.rigidbody;
        Collider2D paddleCollider = collision.otherCollider;

        // How far from the center did the ball hit (-1 to 1)
        float distanceFromCenter = (ballRb.position.x - paddleCollider.bounds.center.x) / (paddleCollider.bounds.size.x / 2);
        distanceFromCenter = Mathf.Clamp(distanceFromCenter, -1f, 1f);

        // Calculate the new bounce angle
        float bounceAngle = distanceFromCenter * maxBounceAngle;

        // Convert to a direction vector
        float radians = bounceAngle * Mathf.Deg2Rad;
        Vector2 newDirection = new Vector2(Mathf.Sin(radians), 1f).normalized;

        // Keep the same speed
        float ballSpeed = ballRb.velocity.magnitude;
        ballRb.velocity = newDirection * ballSpeed;

        AudioManager.Instance?.PlayPaddleHit();
    }
}