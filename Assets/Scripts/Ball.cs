using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    public float speed = 10f;
    public float startDelay = 1f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ResetBall();
    }

    public void ResetBall()
    {
        rb.velocity = Vector2.zero;
        transform.position = Vector2.zero;
        gameObject.SetActive(true);
        CancelInvoke();
        Invoke(nameof(LaunchBall), startDelay);
    }

    void LaunchBall()
    {
        float x = Random.Range(-0.5f, 0.5f);
        float y = -1f;
        Vector2 direction = new Vector2(x, y).normalized;
        rb.velocity = direction * speed;
    }

    void FixedUpdate()
    {
        if (rb.velocity.magnitude > 0)
            rb.velocity = rb.velocity.normalized * speed;
    }
}
