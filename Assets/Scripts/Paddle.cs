using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Paddle : MonoBehaviour
{
    private Rigidbody2D rb;          // Rigidbody for movement
    private Vector2 direction;       // Direction of paddle movement

    public float speed = 30f;        // How fast the paddle moves
    public float maxBounceAngle = 75f; // Maximum angle the ball can bounce off

    void Awake()
    {
        // Get the Rigidbody2D component attached to this object
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Reset the paddle to the starting position
        ResetPaddle();
    }

    // Reset the paddle to the center horizontally
    public void ResetPaddle()
    {
        rb.velocity = Vector2.zero;
        transform.position = new Vector2(0f, transform.position.y);
    }

    void Update()
    {
        // Check input every frame
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            direction = Vector2.left;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            direction = Vector2.right;
        }
        else
        {
            direction = Vector2.zero; // No movement
        }
    }

    void FixedUpdate()
    {
        // Move the paddle using physics
        if (direction != Vector2.zero)
        {
            rb.AddForce(direction * speed);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Only react if the ball hits the paddle
        if (!collision.gameObject.CompareTag("Ball"))
            return;

        Rigidbody2D ballRb = collision.rigidbody;        // Get ball's Rigidbody
        Collider2D paddleCollider = collision.otherCollider; // Get paddle collider

        // Calculate how far from the center the ball hit
        float distanceFromCenter = ballRb.transform.position.x - paddleCollider.bounds.center.x;

        // Calculate the bounce angle based on hit position
        float normalizedDistance = distanceFromCenter / (paddleCollider.bounds.size.x / 2);
        float bounceAngle = normalizedDistance * maxBounceAngle;

        // Convert angle to a direction vector
        Vector2 newDirection = Quaternion.Euler(0, 0, bounceAngle) * Vector2.up;

        // Keep the ball speed constant
        float speedMagnitude = ballRb.velocity.magnitude;
        ballRb.velocity = newDirection.normalized * speedMagnitude;

        // Play paddle hit sound (if AudioManager exists)
        AudioManager.Instance?.PlayPaddleHit();
    }
}
