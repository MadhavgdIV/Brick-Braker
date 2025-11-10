using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    [Header("Speeds")]
    public float speed = 8f;            // Speed after launch
    public float startDelay = 1f;       // Delay before dropping
    public float fallSpeed = 10f;       // Speed while falling before launch

    [Header("Respawn")]
    [Tooltip("If assigned, the ball will be positioned relative to this transform on ResetBall/Retry. If null, uses the initial position recorded at Start.")]
    public Transform paddleTransform;

    private Rigidbody2D rb;
    private bool hasLaunched = false;

    // store the very first position the ball had when the scene started
    private Vector3 initialStartPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // record the very first position so retries return here if paddleTransform not used
        initialStartPosition = transform.position;

        ResetBall();
    }

    // ---------------------------------------
    //  NORMAL STARTUP RESET (called at begin)
    // ---------------------------------------
    public void ResetBall()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        // If paddleTransform is assigned, place relative to it; otherwise use initial recorded start pos
        if (paddleTransform != null)
            transform.position = paddleTransform.position + Vector3.up * 0.6f;
        else
            transform.position = initialStartPosition;

        hasLaunched = false;
        gameObject.SetActive(true);

        CancelInvoke();
        Invoke(nameof(DropBall), startDelay);
    }

    // ---------------------------------------
    //  RETRY RESET (called by GameManager)
    // ---------------------------------------
    public void ResetBallForRetry()
    {
        // stop motion
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        hasLaunched = false;

        // Place ball at the *initial* start position (or above paddle if assigned)
        if (paddleTransform != null)
            transform.position = paddleTransform.position + Vector3.up * 0.6f;
        else
            transform.position = initialStartPosition;

        gameObject.SetActive(true);

        // schedule drop after startDelay
        CancelInvoke();
        Invoke(nameof(DropBall), startDelay);

        Debug.Log("[Ball] ResetBallForRetry: repositioned to " + transform.position + " will drop in " + startDelay + "s");
    }

    // ---------------------------------------
    //  INITIAL DROP (pre-launch fall)
    // ---------------------------------------
    private void DropBall()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb == null) return;

        hasLaunched = false;
        rb.velocity = new Vector2(0f, -Mathf.Abs(fallSpeed));
    }

    private void FixedUpdate()
    {
        // Maintain constant speed after launch
        if (hasLaunched && rb != null && rb.velocity.magnitude > 0.01f)
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
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.velocity = direction * speed;

            AudioManager.Instance?.PlayPaddleHit();
            return;
        }

        // Normal paddle hit sound during gameplay
        if (hasLaunched && collision.gameObject.CompareTag("Paddle"))
        {
            AudioManager.Instance?.PlayPaddleHit();
        }
    }

    /// <summary>
    /// Optional helper to immediately force the ball to drop (skip delay).
    /// </summary>
    public void ForceDropNow()
    {
        CancelInvoke();
        DropBall();
    }
}
