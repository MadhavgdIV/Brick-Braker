using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Brick : MonoBehaviour
{
    [Header("Brick Settings")]
    public Sprite[] states = new Sprite[0];
    public int points = 100;
    public bool unbreakable;

    private SpriteRenderer spriteRenderer;
    private int health;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        ResetBrick();
    }

    public void ResetBrick()
    {
        gameObject.SetActive(true);

        if (unbreakable)
        {
            // Optional: Give unbreakable bricks a unique sprite if you want.
            return;
        }

        health = states.Length;

        if (health > 0)
        {
            spriteRenderer.sprite = states[health - 1];
        }
        else
        {
            Debug.LogWarning($"Brick '{name}' has no sprite states assigned!");
        }
    }

    private void Hit()
    {
        if (unbreakable)
            return;

        health--;

        if (health <= 0)
        {
            gameObject.SetActive(false);
        }
        else if (health - 1 >= 0 && health - 1 < states.Length)
        {
            spriteRenderer.sprite = states[health - 1];
        }

        GameManager.Instance.OnBrickHit(this);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Ball")
        {
            Hit();
        }
    }
}
