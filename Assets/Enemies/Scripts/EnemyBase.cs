using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBase : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 2f;
    public float stopDistance = 5f;
    public float attackCooldown = 2f;

    protected Rigidbody2D rb;
    protected float attackTimer = 0f;
    protected float initialZ;
    protected SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialZ = transform.position.z;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        Vector2 self2 = new Vector2(transform.position.x, transform.position.y);
        Vector2 player2 = new Vector2(player.position.x, player.position.y);
        float distance = Vector2.Distance(self2, player2);

        if (distance > stopDistance)
        {
            Vector2 newPos = Vector2.MoveTowards(self2, player2, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(new Vector3(newPos.x, newPos.y, initialZ));
        }

        // flip
        if (spriteRenderer != null) spriteRenderer.flipX = player.position.x < transform.position.x;

        attackTimer -= Time.fixedDeltaTime;
        if (distance <= stopDistance && attackTimer <= 0f)
        {
            Attack();
            attackTimer = attackCooldown;
        }
    }

    protected virtual void Attack()
    {
        Debug.Log("Attack - override");
    }
}
