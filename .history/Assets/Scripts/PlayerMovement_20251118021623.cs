using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 7f;
    public Transform groundCheck;
    public float groundRadius = 0.1f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private float moveX;
    private bool isGrounded;
    private bool isJumping;

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

        // Flip del sprite
        if (moveX > 0) sr.flipX = false;
        else if (moveX < 0) sr.flipX = true;

        // Chequear si está en el suelo
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        // Presiona salto y está en suelo
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            isJumping = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Si está en el aire
        if (!isGrounded)
        {
            isJumping = true;
        }
        else
        {
            isJumping = false;
        }

        // Animaciones
        anim.SetFloat("Speed", Mathf.Abs(moveX));
        anim.SetBool("isJumping", isJumping);
    }

    void FixedUpdate()
    {
        // Movimiento horizontal físico
        rb.linearVelocity = new Vector2(moveX * speed, rb.linearVelocity.y);
    }
}
