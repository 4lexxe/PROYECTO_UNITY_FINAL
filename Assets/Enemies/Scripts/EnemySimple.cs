using UnityEngine;

public class EnemySimple : MonoBehaviour
{
    public float speed = 2f;
    public float aggroRange = 12f;
    public float attackRange = 1.6f;
    public float attackCooldown = 1.0f;
    public string attackTriggerName = "Attack";
    public string speedParamName = "Speed";
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float shootRange = 8f;
    public float fireRate = 1.5f;
    public float projectileSpeed = 8f;
    public Vector2 fireOffset = new Vector2(0.5f, 0.3f);
    public int maxHealth = 3;
    public int damageFromPlayer = 1;
    private Transform player;
    private Animator anim;
    private SpriteRenderer sr;
    private float lastAttack;
    private float lastShot;
    private Rigidbody2D rb;
    private int health;

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        sr = GetComponent<SpriteRenderer>();
        lastAttack = -attackCooldown;
        lastShot = -fireRate;
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        health = maxHealth;
    }

    void Update()
    {
        if (player == null) return;
        Vector3 self = transform.position;
        Vector3 target = new Vector3(player.position.x, self.y, self.z);
        float dist = Vector2.Distance(self, player.position);
        if (dist > aggroRange)
        {
            if (anim != null) anim.SetFloat(speedParamName, 0f);
            return;
        }
        if (dist > attackRange)
        {
            float dir = Mathf.Sign(target.x - self.x);
            if (sr != null) sr.flipX = dir < 0f;
            transform.position = Vector3.MoveTowards(self, new Vector3(target.x, self.y, self.z), speed * Time.deltaTime);
            if (anim != null) anim.SetFloat(speedParamName, Mathf.Abs(dir) * speed);
            if (dist <= shootRange && Time.time - lastShot >= fireRate)
            {
                ShootAtPlayer();
                lastShot = Time.time;
            }
            return;
        }
        if (Time.time - lastAttack >= attackCooldown)
        {
            if (anim != null) anim.SetTrigger(attackTriggerName);
            lastAttack = Time.time;
            if (anim != null) anim.SetFloat(speedParamName, 0f);
            if (dist <= shootRange && Time.time - lastShot >= fireRate)
            {
                ShootAtPlayer();
                lastShot = Time.time;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (player == null) return;
        if (other.CompareTag("Player"))
        {
            if (Time.time - lastAttack >= attackCooldown)
            {
                if (anim != null) anim.SetTrigger(attackTriggerName);
                lastAttack = Time.time;
            }
            var pa = player.GetComponentInChildren<Animator>();
            if (pa != null)
            {
                var st = pa.GetCurrentAnimatorStateInfo(0);
                bool playerIsAttacking = st.IsName("attack_0") || st.IsName("attack_1");
                if (playerIsAttacking)
                {
                    TakeDamage(damageFromPlayer);
                }
            }
        }
    }

    void ShootAtPlayer()
    {
        if (projectilePrefab == null || player == null) return;
        Vector3 origin = transform.position;
        if (firePoint != null) origin = firePoint.position;
        else
        {
            float dirX = sr != null && sr.flipX ? -1f : 1f;
            origin += new Vector3(fireOffset.x * dirX, fireOffset.y, 0f);
        }
        Vector2 dir = (player.position - origin);
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
        dir.Normalize();
        var proj = Instantiate(projectilePrefab, origin, Quaternion.identity);
        var rbp = proj.GetComponent<Rigidbody2D>();
        if (rbp == null) rbp = proj.AddComponent<Rigidbody2D>();
        rbp.gravityScale = 0f;
        rbp.linearVelocity = dir * projectileSpeed;
        if (anim != null)
        {
            var id = Animator.StringToHash("Shoot");
            for (int i = 0; i < anim.parameterCount; i++)
            {
                if (anim.parameters[i].type == AnimatorControllerParameterType.Trigger && anim.parameters[i].nameHash == id)
                {
                    anim.SetTrigger("Shoot");
                    break;
                }
            }
        }
    }

    void TakeDamage(int amount)
    {
        health -= amount;
        if (health <= 0) Die();
    }

    void Die()
    {
        if (anim != null)
        {
            int id = Animator.StringToHash("Die");
            for (int i = 0; i < anim.parameterCount; i++)
            {
                if (anim.parameters[i].type == AnimatorControllerParameterType.Trigger && anim.parameters[i].nameHash == id)
                {
                    anim.SetTrigger("Die");
                    break;
                }
            }
        }
        Destroy(gameObject);
    }
}
