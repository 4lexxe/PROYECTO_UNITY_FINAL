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

        anim.SetFloat("Speed", Mathf.Abs(moveX));
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveX * speed, rb.linearVelocity.y);
    }
}
