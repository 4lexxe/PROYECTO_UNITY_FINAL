using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 8f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private bool isGrounded;
    private bool jumpPressed;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public float groundCheckDistance = 0.2f;
    public float airControlFactor = 0.7f;

    private float moveX;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        moveX = Input.GetAxisRaw("Horizontal");
        if (moveX > 0) sr.flipX = false;
        else if (moveX < 0) sr.flipX = true;
        if (Input.GetButtonDown("Jump")) jumpPressed = true;
        Vector3 gcPos = groundCheck != null ? groundCheck.position : transform.position;
        var hit = Physics2D.Raycast(gcPos, Vector2.down, groundCheckDistance, groundLayer);
        if (hit.collider == null)
        {
            isGrounded = Physics2D.OverlapCircle(gcPos, groundCheckRadius, groundLayer);
        }
        else
        {
            isGrounded = true;
        }
        anim.SetFloat("Speed", Mathf.Abs(moveX));
        anim.SetBool("isJumping", !isGrounded);
    }

    void FixedUpdate()
    {
        float targetSpeed = isGrounded ? speed : speed * airControlFactor;
        rb.linearVelocity = new Vector2(moveX * targetSpeed, rb.linearVelocity.y);
        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
        jumpPressed = false;
        rb.angularVelocity = 0f;
        transform.rotation = Quaternion.identity;
    }
}
