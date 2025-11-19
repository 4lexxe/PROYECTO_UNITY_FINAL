using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 7f;

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
        // Movimiento horizontal
        moveX = Input.GetAxisRaw("Horizontal");

        // Flip
        if (moveX > 0) sr.flipX = false;
        else if (moveX < 0) sr.flipX = true;

        // Salto sin checkear suelo
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetBool("isJumping", true);
        }

        // Animaciones de Idle/Run
        anim.SetFloat("Speed", Mathf.Abs(moveX));
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveX * speed, rb.linearVelocity.y);
    }
}
