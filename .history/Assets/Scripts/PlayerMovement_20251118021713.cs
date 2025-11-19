using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private float moveX;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        moveX = Input.GetAxisRaw("Horizontal");

        // Flip
        if (moveX > 0) sr.flipX = false;
        else if (moveX < 0) sr.flipX = true;

        // Animaciones
        anim.SetFloat("Speed", Mathf.Abs(moveX));
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveX * speed, rb.linearVelocity.y);
    }
}
