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
    public float projectileSpeed = 6f;
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
    public bool useLaserForFlying = true;
    public int laserDamage = 1;
    public float laserWidth = 0.06f;
    public float laserDuration = 0.2f;
    public Color laserColor = new Color(1f, 0f, 0f, 1f);
    public float flyHighOffset = 3.0f;
    public float flyLowOffset = 0.8f;
    public int projectileDamage = 1;
    public int maxHealth = 3;
    public int damageFromPlayer = 1;
    public bool showHealthBar = true;
    public Vector2 healthBarSize = new Vector2(1.2f, 0.12f);
    public Vector2 healthBarOffset = new Vector2(0f, 1.1f);
    public Color healthColorFull = new Color(0.1f, 1f, 0.1f, 1f);
    public Color healthColorEmpty = new Color(1f, 0.1f, 0.1f, 1f);
    public Color healthBackgroundColor = new Color(0f, 0f, 0f, 0.6f);
    public int maxHealth = 3;
    public int damageFromPlayer = 1;
    private Transform player;
    private Animator anim;
    private SpriteRenderer sr;
    private float lastAttack;
    private float lastShot;
    private Rigidbody2D rb;
    private int health;
    private Transform hpRoot;
    private LineRenderer hpBack;
    private LineRenderer hpLine;

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
        if (showHealthBar) InitHealthBar();
    }

    void Update()
    {
        if (player == null) return;
        Vector3 self = transform.position;
        Vector3 target = isFlyingType
            ? new Vector3(player.position.x, Mathf.Lerp(player.position.y + flyLowOffset, player.position.y + flyHighOffset, Mathf.PingPong(Time.time * hoverFrequency, 1f)), self.z)
            : new Vector3(player.position.x, self.y, self.z);
        float dist = Vector2.Distance(self, player.position);
        float dir = Mathf.Sign(target.x - self.x);
        UpdateFacing(dir);
        if (dist > aggroRange)
        {
            if (anim != null && HasParam(speedParamName, AnimatorControllerParameterType.Float)) anim.SetFloat(speedParamName, 0f);
            return;
        }
        if (dist > attackRange)
        {
            transform.position = Vector3.MoveTowards(self, new Vector3(target.x, isFlyingType ? target.y : self.y, self.z), speed * Time.deltaTime);
            if (anim != null && HasParam(speedParamName, AnimatorControllerParameterType.Float)) anim.SetFloat(speedParamName, Mathf.Abs(dir) * speed);
            if (dist <= shootRange && Time.time - lastShot >= fireRate)
            {
                if (isFlyingType && useLaserForFlying)
                {
                    bool useLaser = laserAtLongRange ? dist >= laserSwitchDistance : dist <= laserSwitchDistance;
                    if (useLaser) ShootLaserAtPlayer(); else ShootAtPlayer();
                }
                else
                {
                    ShootAtPlayer();
                }
                lastShot = Time.time;
            }
            return;
        }
        UpdateFacing(dir);
        if (Time.time - lastAttack >= attackCooldown)
        {
            if (anim != null && HasParam(attackTriggerName, AnimatorControllerParameterType.Trigger)) anim.SetTrigger(attackTriggerName);
            lastAttack = Time.time;
            if (anim != null && HasParam(speedParamName, AnimatorControllerParameterType.Float)) anim.SetFloat(speedParamName, 0f);
            if (dist <= shootRange && Time.time - lastShot >= fireRate)
            {
                if (isFlyingType && useLaserForFlying)
                {
                    bool useLaser = laserAtLongRange ? dist >= laserSwitchDistance : dist <= laserSwitchDistance;
                    if (useLaser) ShootLaserAtPlayer(); else ShootAtPlayer();
                }
                else
                {
                    ShootAtPlayer();
                }
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
        if (player == null) return;
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
        if (anim != null && HasParam("Shoot", AnimatorControllerParameterType.Trigger)) anim.SetTrigger("Shoot");
    }

    void ShootLaserAtPlayer()
    {
        if (player == null) return;
        Vector3 origin = transform.position;
        if (firePoint != null) origin = firePoint.position;
        Vector2 dir = (player.position - origin);
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
        dir.Normalize();
        var hit = Physics2D.Raycast(origin, dir, 100f);
        Vector3 end = hit.collider != null ? (Vector3)hit.point : origin + (Vector3)(dir * 100f);
        var go = new GameObject("Laser");
        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.startWidth = laserWidth;
        lr.endWidth = laserWidth;
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = laserColor;
        lr.material = mat;
        lr.startColor = laserColor;
        lr.endColor = laserColor;
        lr.SetPosition(0, origin);
        lr.SetPosition(1, end);
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            var pc = hit.collider.GetComponent<PlayerController>();
            if (pc != null) pc.RecibirDanio(laserDamage);
        }
        Destroy(go, laserDuration);
    }

    void InitHealthBar()
    {
        if (hpRoot != null) return;
        hpRoot = new GameObject("HPBar").transform;
        hpRoot.SetParent(transform);
        hpRoot.localPosition = new Vector3(-healthBarSize.x * 0.5f + healthBarOffset.x, healthBarOffset.y, 0f);
        hpBack = hpRoot.gameObject.AddComponent<LineRenderer>();
        hpBack.useWorldSpace = false;
        hpBack.positionCount = 2;
        hpBack.startWidth = healthBarSize.y;
        hpBack.endWidth = healthBarSize.y;
        var backMat = new Material(Shader.Find("Sprites/Default"));
        backMat.color = healthBackgroundColor;
        hpBack.material = backMat;
        hpBack.startColor = healthBackgroundColor;
        hpBack.endColor = healthBackgroundColor;
        hpBack.sortingOrder = 1000;
        hpBack.SetPosition(0, Vector3.zero);
        hpBack.SetPosition(1, new Vector3(healthBarSize.x, 0f, 0f));
        hpLine = hpRoot.gameObject.AddComponent<LineRenderer>();
        hpLine.useWorldSpace = false;
        hpLine.positionCount = 2;
        hpLine.startWidth = healthBarSize.y * 0.9f;
        hpLine.endWidth = healthBarSize.y * 0.9f;
        var lineMat = new Material(Shader.Find("Sprites/Default"));
        lineMat.color = healthColorFull;
        hpLine.material = lineMat;
        hpLine.sortingOrder = 1001;
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (!showHealthBar || hpLine == null) return;
        float pct = Mathf.Clamp01((float)health / Mathf.Max(1, maxHealth));
        hpLine.SetPosition(0, Vector3.zero);
        hpLine.SetPosition(1, new Vector3(healthBarSize.x * pct, 0f, 0f));
        var col = Color.Lerp(healthColorEmpty, healthColorFull, pct);
        hpLine.startColor = col;
        hpLine.endColor = col;
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
            rbp.linearVelocity = dir * projectileSpeed;
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
        var go = new GameObject("SimpleProjectile3D");
        go.transform.position = origin;
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.SetParent(go.transform);
        sphere.transform.localPosition = Vector3.zero;
        sphere.transform.localScale = Vector3.one * simpleProjectileSize;
        var mr = sphere.GetComponent<MeshRenderer>();
        if (mr != null) mr.material.color = simpleProjectileColor;
        var sc = sphere.GetComponent<SphereCollider>();
        if (sc != null) Object.Destroy(sc);
        var col2d = go.AddComponent<CircleCollider2D>();
        col2d.isTrigger = true;
        col2d.radius = Mathf.Max(0.05f, simpleProjectileSize * 0.5f);
        var rbp = go.AddComponent<Rigidbody2D>();
        rbp.bodyType = RigidbodyType2D.Dynamic;
        rbp.gravityScale = 0f;
        rbp.linearVelocity = dir.normalized * projectileSpeed;
        go.transform.localScale = Vector3.one * simpleProjectileSize;
        if (rotateProjectileToDirection)
        {
            float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            go.transform.rotation = Quaternion.AngleAxis(ang, Vector3.forward);
        }
        var dmg = go.AddComponent<ProjectileDamage2D>();
        dmg.damage = projectileDamage;
        dmg.targetTag = "Player";
        Destroy(go, simpleProjectileLifetime);
        return go;
    }

    public class ProjectileDamage2D : MonoBehaviour
    {
        public int damage = 1;
        public string targetTag = "Player";
        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(targetTag)) return;
            var pc = other.GetComponent<PlayerController>();
            if (pc != null) pc.RecibirDanio(damage);
            Destroy(gameObject);
        }
    }

    bool HasParam(string param, AnimatorControllerParameterType type)
    {
        if (anim == null) return false;
        var ps = anim.parameters;
        for (int i = 0; i < ps.Length; i++)
        {
            if (ps[i].name == param && ps[i].type == type) return true;
        }
        return false;
    }

    public void TakeDamage(int amount)
    {
        health = Mathf.Max(0, health - amount);
        UpdateHealthBar();
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
        if (hpRoot != null) Destroy(hpRoot.gameObject);
        Destroy(gameObject);
    }
}
