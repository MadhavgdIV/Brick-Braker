using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    [Header("Motion Settings")]
    public float speed = 8f;            // Speed after launch
    public float startDelay = 1f;       // Delay before falling
    public float fallSpeed = 10f;       // Speed while falling before launch
    public float fallHeightOffset = 1.5f; // How far above paddle the ball spawns

    [Header("Optional References")]
    [Tooltip("If you prefer to assign the paddle explicitly, drop it here. Otherwise the Ball will find the active object tagged 'Paddle'.")]
    public Transform paddleTransform; // optional — may be null, we will try to find current paddle each reset

    private Rigidbody2D rb;
    private bool hasLaunched = false;
    private Vector3 initialWorldPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            Debug.LogError("Ball requires a Rigidbody2D.");

        // cache the ball's initial world position (in case paddle isn't found)
        initialWorldPosition = transform.position;
    }

    void Start()
    {
        // ensure the ball starts in the reset state
        ResetBall();
    }

    /// <summary>
    /// Reset ball to spawn position above the current paddle (if available) or the initial saved position.
    /// This always searches for a live Paddle object so new paddle instances are respected.
    /// </summary>
    public void ResetBall()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogError("Ball.ResetBall: Rigidbody2D missing; cannot reset.");
                return;
            }
        }

        // stop movement
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = true;

        // find the current paddle each time (do not rely on a stale cached transform)
        Transform currentPaddle = paddleTransform;
        if (currentPaddle == null)
        {
            GameObject paddleObj = GameObject.FindWithTag("Paddle");
            if (paddleObj != null)
                currentPaddle = paddleObj.transform;
        }

        // Determine spawn position
        Vector3 spawnPos = initialWorldPosition;
        if (currentPaddle != null)
        {
            // spawn directly above the paddle using the paddle's current Y (so paddle reset is reflected)
            spawnPos = currentPaddle.position + new Vector3(0f, fallHeightOffset, 0f);
        }

        transform.position = spawnPos;
        hasLaunched = false;
        gameObject.SetActive(true);

        // ensure any prior invoke canceled then schedule drop
        CancelInvoke();
        Invoke(nameof(DropBall), startDelay);
    }

    void DropBall()
    {
        // Ball falls straight down
        if (rb == null) return;
        rb.velocity = new Vector2(0f, -Mathf.Abs(fallSpeed));
    }

    void FixedUpdate()
    {
        // Maintain constant speed after launch
        if (hasLaunched && rb != null && rb.velocity.magnitude > 0)
        {
            rb.velocity = rb.velocity.normalized * speed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasLaunched && collision.gameObject.CompareTag("Paddle"))
        {
            hasLaunched = true;

            float x = Random.Range(-0.5f, 0.5f);
            Vector2 direction = new Vector2(x, 1f).normalized;
            rb.velocity = direction * speed;

            AudioManager.Instance?.PlayPaddleHit();
            return;
        }

        if (hasLaunched && collision.gameObject.CompareTag("Paddle"))
        {
            AudioManager.Instance?.PlayPaddleHit();
        }
    }
}
