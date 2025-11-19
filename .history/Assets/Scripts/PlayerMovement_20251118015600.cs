using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 8f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private Collider2D col;

    public Transform cameraTransform;
    public float cameraSmoothTime = 0.2f;
    public float cameraLookAheadDistance = 1.0f;
    public Vector2 cameraOffset = Vector2.zero;

    private float lastX;
    private float lookAheadX;
    private Vector3 camVelocity;
    private bool jumpPressed;
    private bool isGrounded;
    public LayerMask groundLayer;

    private float moveX;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
        lastX = transform.position.x;
    }

    void Update()
    {
        moveX = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump")) jumpPressed = true;

        if (moveX > 0)
        {
            if (sr != null) sr.flipX = false;
            else
            {
                var s = transform.localScale;
                s.x = Mathf.Abs(s.x);
                transform.localScale = s;
            }
        }
        else if (moveX < 0)
        {
            if (sr != null) sr.flipX = true;
            else
            {
                var s = transform.localScale;
                s.x = -Mathf.Abs(s.x);
                transform.localScale = s;
            }
        }

        if (col != null) isGrounded = col.IsTouchingLayers(groundLayer);
        else isGrounded = Mathf.Abs(rb.linearVelocity.y) < 0.01f;
        anim.SetFloat("Speed", Mathf.Abs(moveX));
        anim.SetBool("isJumping", !isGrounded);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveX * speed, rb.linearVelocity.y);
        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
        jumpPressed = false;
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;
        float x = transform.position.x;
        float dx = x - lastX;
        lastX = x;
        if (Mathf.Abs(dx) > 0.001f) lookAheadX = Mathf.Sign(dx) * cameraLookAheadDistance;
        else lookAheadX = Mathf.Lerp(lookAheadX, 0f, Time.deltaTime * 3f);
        Vector3 desired = new Vector3(x + lookAheadX + cameraOffset.x, cameraTransform.position.y + cameraOffset.y, cameraTransform.position.z);
        cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, desired, ref camVelocity, cameraSmoothTime);
    }
}
