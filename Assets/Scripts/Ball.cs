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
    [Tooltip("Assign Paddle here or tag your Paddle as 'Paddle'")]
    public Transform paddleTransform;

    private Rigidbody2D rb;
    private bool hasLaunched = false;
    private Vector3 initialWorldPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            Debug.LogError("Ball requires a Rigidbody2D.");

        initialWorldPosition = transform.position;

        // Try auto-find Paddle
        if (paddleTransform == null)
        {
            GameObject paddleObj = GameObject.FindWithTag("Paddle");
            if (paddleObj != null)
                paddleTransform = paddleObj.transform;
        }
    }

    void Start()
    {
        ResetBall();
    }

    public void ResetBall()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogError("Ball.ResetBall: Missing Rigidbody2D.");
                return;
            }
        }

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = true;

        //  Position above paddle
        Vector3 startPos = initialWorldPosition;
        if (paddleTransform != null)
        {
            startPos = paddleTransform.position + new Vector3(0f, fallHeightOffset, 0f);
        }
        else
        {
            GameObject paddleObj = GameObject.FindWithTag("Paddle");
            if (paddleObj != null)
            {
                paddleTransform = paddleObj.transform;
                startPos = paddleTransform.position + new Vector3(0f, fallHeightOffset, 0f);
            }
        }

        transform.position = startPos;
        hasLaunched = false;
        gameObject.SetActive(true);

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
