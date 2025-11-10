using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Brick : MonoBehaviour
{
    public Sprite[] states;
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
        if (unbreakable) return;

        // Register this breakable brick with the game manager so it can track remaining bricks
        GameManager.Instance?.RegisterBreakable();

        health = states.Length;
        if (health > 0) spriteRenderer.sprite = states[health - 1];
    }

    private void Hit()
    {
        if (unbreakable) return;

        health--;

        if (health <= 0)
        {
            gameObject.SetActive(false);
            GameManager.Instance?.OnBrickHit(points);
        }
        else if (health - 1 >= 0 && health - 1 < states.Length)
        {
            spriteRenderer.sprite = states[health - 1];
        }

        // Play brick hit sound
        AudioManager.Instance?.PlayBrickHit();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            Hit();
        }
    }
}
