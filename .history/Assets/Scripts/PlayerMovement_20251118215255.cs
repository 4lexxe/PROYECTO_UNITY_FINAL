using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 8f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private Collider2D col;
    private bool isGrounded;
    private bool jumpPressed;
    private bool jumpLock;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public float groundCheckDistance = 0.2f;
    public float airControlFactor = 0.7f;
    public string groundTag = "Ground";

    private float moveX;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        moveX = Input.GetAxisRaw("Horizontal");
        if (moveX > 0) sr.flipX = false;
        else if (moveX < 0) sr.flipX = true;
        bool jumpDown = Input.GetButtonDown("Jump");
        bool jumpUp = Input.GetButtonUp("Jump");
        Vector3 gcPos = groundCheck != null ? groundCheck.position : transform.position;
        bool useLayer = groundLayer.value != 0;
        if (useLayer)
        {
            var hit = Physics2D.Raycast(gcPos, Vector2.down, groundCheckDistance, groundLayer);
            if (col != null)
            {
                isGrounded = col.IsTouchingLayers(groundLayer);
                if (!isGrounded)
                {
                    isGrounded = hit.collider != null || Physics2D.OverlapCircle(gcPos, groundCheckRadius, groundLayer);
                }
            }
            else
            {
                isGrounded = hit.collider != null || Physics2D.OverlapCircle(gcPos, groundCheckRadius, groundLayer);
            }
        }
        else
        {
            isGrounded = false;
            var hitAny = Physics2D.Raycast(gcPos, Vector2.down, groundCheckDistance);
            if (hitAny.collider != null && (string.IsNullOrEmpty(groundTag) || hitAny.collider.CompareTag(groundTag))) isGrounded = true;
            if (!isGrounded)
            {
                var cols = Physics2D.OverlapCircleAll(gcPos, groundCheckRadius);
                for (int i = 0; i < cols.Length; i++)
                {
                    if (string.IsNullOrEmpty(groundTag) || cols[i].CompareTag(groundTag)) { isGrounded = true; break; }
                }
            }
        }
        if (jumpDown && isGrounded && !jumpLock) jumpPressed = true;
        if (!isGrounded) jumpLock = true;
        if (isGrounded && !Input.GetButton("Jump")) jumpLock = false;
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
