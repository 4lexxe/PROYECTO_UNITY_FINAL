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
    public float projectileScale = 1f;
    public bool rotateProjectileToDirection = true;
    public bool isFlyingType = false;
    public float hoverAmplitude = 1.0f;
    public float hoverFrequency = 2.0f;
    public float hoverYOffset = 1.0f;
    public bool projectileHoming = true;
    public float homingDuration = 3.0f;
    public bool useSimpleProjectile = true;
    public float simpleProjectileLifetime = 4f;
    public float simpleProjectileSize = 0.6f;
    public Color simpleProjectileColor = new Color(1f, 0f, 0f, 1f);
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
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        lastAttack = -attackCooldown;
        lastShot = -fireRate;
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        health = maxHealth;
        if (CompareTag("Flying_eye")) isFlyingType = true;
    }

    void Update()
    {
        if (player == null) return;
        Vector3 self = transform.position;
        Vector3 target = isFlyingType
            ? new Vector3(player.position.x, player.position.y + hoverYOffset + Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude, self.z)
            : new Vector3(player.position.x, self.y, self.z);
        float dist = Vector2.Distance(self, player.position);
        float dir = Mathf.Sign(target.x - self.x);
        UpdateFacing(dir);
        if (dist > aggroRange)
        {
            if (anim != null) anim.SetFloat(speedParamName, 0f);
            return;
        }
        if (dist > attackRange)
        {
            transform.position = Vector3.MoveTowards(self, new Vector3(target.x, isFlyingType ? target.y : self.y, self.z), speed * Time.deltaTime);
            if (anim != null) anim.SetFloat(speedParamName, Mathf.Abs(dir) * speed);
            if (dist <= shootRange && Time.time - lastShot >= fireRate)
            {
                ShootAtPlayer();
                lastShot = Time.time;
            }
            return;
        }
        UpdateFacing(dir);
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
        GameObject proj;
        if (useSimpleProjectile || projectilePrefab == null)
        {
            proj = CreateSimpleProjectile(origin, dir);
        }
        else
        {
            proj = Instantiate(projectilePrefab, origin, Quaternion.identity);
            if (projectileScale != 1f) proj.transform.localScale = proj.transform.localScale * projectileScale;
            var rbp = proj.GetComponent<Rigidbody2D>();
            if (rbp == null) rbp = proj.AddComponent<Rigidbody2D>();
            rbp.bodyType = RigidbodyType2D.Dynamic;
            rbp.gravityScale = 0f;
            rbp.linearVelocity = dir * projectileSpeed;
            if (rotateProjectileToDirection)
            {
                float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                proj.transform.rotation = Quaternion.AngleAxis(ang, Vector3.forward);
            }
        }
        if (projectileHoming && proj != null) StartCoroutine(Homing(proj));
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

    void UpdateFacing(float dir)
    {
        if (sr != null) sr.flipX = dir < 0f;
    }

    System.Collections.IEnumerator Homing(GameObject proj)
    {
        float until = Time.time + homingDuration;
        var rbp = proj != null ? proj.GetComponent<Rigidbody2D>() : null;
        while (proj != null && rbp != null && player != null && Time.time < until)
        {
            Vector3 origin = proj.transform.position;
            Vector2 dir = (player.position - origin);
            if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
            dir.Normalize();
            rbp.velocity = dir * projectileSpeed;
            if (rotateProjectileToDirection)
            {
                float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                proj.transform.rotation = Quaternion.AngleAxis(ang, Vector3.forward);
            }
            yield return new WaitForFixedUpdate();
        }
    }

    GameObject CreateSimpleProjectile(Vector3 origin, Vector2 dir)
    {
        var go = new GameObject("SimpleProjectile");
        go.transform.position = origin;
        var srp = go.AddComponent<SpriteRenderer>();
        var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, simpleProjectileColor);
        tex.Apply();
        var sp = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
        srp.sprite = sp;
        var col2d = go.AddComponent<CircleCollider2D>();
        col2d.isTrigger = true;
        col2d.radius = Mathf.Max(0.05f, simpleProjectileSize * 0.5f);
        var rbp = go.AddComponent<Rigidbody2D>();
        rbp.bodyType = RigidbodyType2D.Dynamic;
        rbp.gravityScale = 0f;
        rbp.velocity = dir.normalized * projectileSpeed;
        go.transform.localScale = Vector3.one * simpleProjectileSize;
        if (rotateProjectileToDirection)
        {
            float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            go.transform.rotation = Quaternion.AngleAxis(ang, Vector3.forward);
        }
        Destroy(go, simpleProjectileLifetime);
        return go;
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
