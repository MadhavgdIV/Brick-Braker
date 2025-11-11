using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Paddle : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 direction;

    [Header("Movement")]
    public float moveSpeed = 15f;
    public float maxBounceAngle = 60f; // Smaller angle for smoother gameplay

    // remember original spawn position so ResetPaddle restores exactly
    private Vector3 initialWorldPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        // store the original position when the object is created/loaded
        initialWorldPosition = transform.position;
    }

    void Start()
    {
        ResetPaddle();
    }

    /// <summary>
    /// Resets the paddle to its original spawn position and stops movement.
    /// </summary>
    public void ResetPaddle()
    {
        if (rb != null)
            rb.velocity = Vector2.zero;

        // restore exactly to the stored initial position
        transform.position = initialWorldPosition;
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
        if (rb != null)
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
