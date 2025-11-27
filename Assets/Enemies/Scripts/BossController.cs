using UnityEngine;

public partial class BossController : MonoBehaviour
{
    public float speed = 2.2f;
    public float aggroRange = 18f;
    public float attackRange = 2.2f;
    public float attackCooldown = 1.5f;
    public float skill1Cooldown = 6f;
    public string attackTriggerName = "Attack";
    public string skill1TriggerName = "Skill1";
    public string speedParamName = "Speed";
    public LayerMask groundLayer;
    public bool alwaysAggro = true;
    public Transform[] teleportPoints;
    public float teleportRadius = 7f;
    public float teleportYOffset = 0.5f;
    public float teleportFXDuration = 0.4f;
    public Color teleportFXColor = new Color(0.6f, 0.2f, 1f, 1f);
    public AudioClip teleportSfx;
    public float teleportSfxVolume = 1f;
    public enum AttackType { Simple, RadialWave, Bomb, MeleeOnly, Combined }
    public AttackType attackType = AttackType.Simple;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float shootRange = 12f;
    public float fireRate = 2.0f;
    public float projectileSpeed = 4f;
    public Vector2 fireOffset = new Vector2(0.6f, 0.4f);
    public float projectileScale = 1f;
    public bool rotateProjectileToDirection = true;
    public bool projectileHoming = true;
    public float homingDuration = 3.5f;
    public bool useSimpleProjectile = true;
    public float simpleProjectileLifetime = 5f;
    public float simpleProjectileSize = 0.7f;
    public Color simpleProjectileColor = new Color(0.9f, 0.2f, 0.2f, 1f);
    public bool simpleProjectileLight = true;
    public bool simpleProjectileLightSyncColor = true;
    public Color simpleProjectileLightColor = new Color(1f, 0.6f, 0.2f, 1f);
    public float simpleProjectileLightIntensity = 2f;
    public float simpleProjectileLightRange = 5f;
    public AudioClip projectileBreakSfx;
    public float projectileBreakSfxVolume = 1f;
    public AudioClip shootSfx;
    public float shootSfxVolume = 1f;
    public int projectileDamage = 2;
    public int radialCount = 16;
    public float bombFuseSeconds = 1.4f;
    public int bombMiniCount = 10;
    public float bombSpeedMultiplier = 0.6f;
    public float radialRingRadius = 1.0f;
    public float radialFxDuration = 0.7f;
    public int explosionFxCount = 36;
    public float explosionFxSpeed = 4.5f;
    public float explosionFxSize = 0.14f;
    public float explosionFxLifetime = 0.55f;
    public float explosionFxRadius = 1.4f;
    public Color explosionFxColor = new Color(1f, 0.6f, 0.2f, 1f);
    public Color radialFxColor = new Color(1f, 1f, 1f, 0.8f);
    public bool isFlyingType = true;
    public float flyHighOffset = 4.0f;
    public float flyLowOffset = 1.4f;
    public float hoverFrequency = 2.0f;
    public int maxHealth = 100;
    public bool showHealthBar = true;
    public Vector2 healthBarSize = new Vector2(2.6f, 0.16f);
    public Vector2 healthBarOffset = new Vector2(0f, 1.6f);
    public bool alignHealthBarToCollider = true;
    public float healthBarMargin = 0.2f;
    public Color healthColorFull = new Color(0.1f, 1f, 0.1f, 1f);
    public Color healthColorEmpty = new Color(1f, 0.1f, 0.1f, 1f);
    public Color healthBackgroundColor = new Color(0f, 0f, 0f, 0.6f);
    public int teleportDamage = 3;
    public float teleportDamageRadius = 1.8f;
    public float teleportKnockback = 4f;
    public int damageFromPlayer = 1;
    public float teleportOnDamageCooldown = 0.5f;
    public float grabCooldown = 8f;
    public float grabLiftHeight = 6f;
    public float grabLiftDuration = 0.8f;
    public float grabHoldSeconds = 0.2f;
    public float grabDropSpeed = 12f;
    public int grabDropDamage = 1;
    public AudioClip grabSfx;
    public float grabSfxVolume = 1f;
    public AudioClip levitateSfx;
    public float levitateSfxVolume = 1f;
    public AudioClip groundImpactSfx;
    public float groundImpactSfxVolume = 1f;
    public AudioClip ambientLoopSfx;
    public float ambientLoopVolume = 1f;
    public AudioClip deathSfx;
    public float deathSfxVolume = 1f;
    public float deathFXDuration = 7f;
    public Color deathFXColor = new Color(1f, 0.8f, 0.2f, 1f);
    public float deathLightIntensity = 3f;
    public float deathLightRange = 8f;
    public bool deathCameraShake = true;
    public float deathCameraShakeAmplitude = 0.25f;
    public float deathCameraShakeFrequency = 60f;
    public float deathCameraShakeDuration = 7f;
    public bool impactCameraShake = true;
    public float impactCameraShakeAmplitude = 0.22f;
    public float impactCameraShakeFrequency = 60f;
    public float impactCameraShakeDuration = 0.4f;

    private Transform player;
    private Animator anim;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private float lastAttack;
    private float lastSkill;
    private float lastShot;
    private float lastTeleportOnDamage;
    private float lastGrab;
    private int health;
    private Transform hpRoot;
    private LineRenderer hpBack;
    private LineRenderer hpLine;
    private AudioSource ambientSrc;
    private GameObject ambientGo;

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        lastAttack = -attackCooldown;
        lastSkill = -skill1Cooldown;
        lastShot = -fireRate;
        lastTeleportOnDamage = -teleportOnDamageCooldown;
        lastGrab = -grabCooldown;
        health = maxHealth;
        if (showHealthBar) InitHealthBar();
        if (ambientLoopSfx != null)
        {
            ambientGo = new GameObject("BossAmbientSfx");
            ambientGo.transform.SetParent(transform);
            ambientGo.transform.localPosition = Vector3.zero;
            ambientSrc = ambientGo.AddComponent<AudioSource>();
            ambientSrc.playOnAwake = false;
            ambientSrc.loop = true;
            ambientSrc.spatialBlend = 0f;
            ambientSrc.volume = Mathf.Clamp01(ambientLoopVolume);
            ambientSrc.clip = ambientLoopSfx;
            ambientSrc.priority = 128;
            ambientSrc.Play();
        }
    }

    void Update()
    {
        if (player == null) return;
        Vector3 self = transform.position;
        float dist = Vector2.Distance(self, player.position);
        if (showHealthBar && alignHealthBarToCollider) UpdateHealthBarPosition();
        float dir = Mathf.Sign(player.position.x - self.x);
        if (sr != null) sr.flipX = dir < 0f;
        if (!alwaysAggro && dist > aggroRange)
        {
            if (anim != null && HasParam(speedParamName, AnimatorControllerParameterType.Float)) anim.SetFloat(speedParamName, 0f);
            return;
        }
        if (dist > attackRange)
        {
            Vector3 target;
            if (isFlyingType)
            {
                float yOsc = Mathf.Lerp(player.position.y + flyLowOffset, player.position.y + flyHighOffset, Mathf.PingPong(Time.time * hoverFrequency, 1f));
                target = new Vector3(player.position.x, yOsc, self.z);
            }
            else
            {
                target = new Vector3(player.position.x, self.y, self.z);
            }
            transform.position = Vector3.MoveTowards(self, target, speed * Time.deltaTime);
            if (anim != null && HasParam(speedParamName, AnimatorControllerParameterType.Float)) anim.SetFloat(speedParamName, Mathf.Abs(dir) * speed);
        }
        else
        {
            if (anim != null && HasParam(speedParamName, AnimatorControllerParameterType.Float)) anim.SetFloat(speedParamName, 0f);
            if (Time.time - lastAttack >= attackCooldown)
            {
                if (anim != null && HasParam(attackTriggerName, AnimatorControllerParameterType.Trigger)) anim.SetTrigger(attackTriggerName);
                lastAttack = Time.time;
            }
        }
        if (Time.time - lastSkill >= skill1Cooldown && dist <= aggroRange)
        {
            if (anim != null && HasParam(skill1TriggerName, AnimatorControllerParameterType.Trigger)) anim.SetTrigger(skill1TriggerName);
            StartCoroutine(SkillTeleport());
            lastSkill = Time.time;
        }
        if (Time.time - lastGrab >= grabCooldown && dist <= attackRange)
        {
            StartCoroutine(SkillGrabLiftDrop());
            lastGrab = Time.time;
        }
        if (dist <= shootRange && Time.time - lastShot >= fireRate)
        {
            ShootAtPlayer();
            lastShot = Time.time;
        }
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var pa = other.GetComponentInChildren<Animator>();
            if (pa != null)
            {
                var st = pa.GetCurrentAnimatorStateInfo(0);
                bool playerIsAttacking = st.IsName("attack_0") || st.IsName("attack_1");
                if (playerIsAttacking)
                {
                    TakeDamage(damageFromPlayer);
                    StartCoroutine(SkillTeleportOpposite());
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
        if (attackType == AttackType.MeleeOnly) return;
        if (attackType == AttackType.RadialWave)
        {
            ShootRadialWave(origin);
            if (shootSfx != null) AudioSource.PlayClipAtPoint(shootSfx, origin, Mathf.Clamp01(shootSfxVolume));
            if (anim != null && HasParam("Shoot", AnimatorControllerParameterType.Trigger)) anim.SetTrigger("Shoot");
            return;
        }
        if (attackType == AttackType.Bomb)
        {
            ShootBomb(origin, dir);
            if (shootSfx != null) AudioSource.PlayClipAtPoint(shootSfx, origin, Mathf.Clamp01(shootSfxVolume));
            if (anim != null && HasParam("Shoot", AnimatorControllerParameterType.Trigger)) anim.SetTrigger("Shoot");
            return;
        }
        if (attackType == AttackType.Combined)
        {
            int pick = Random.Range(0, 3);
            if (pick == 0)
            {
                GameObject sp = CreateSimpleProjectile(origin, dir);
                if (projectileHoming && sp != null) StartCoroutine(Homing(sp));
            }
            else if (pick == 1)
            {
                ShootRadialWave(origin);
            }
            else
            {
                ShootBomb(origin, dir);
            }
            if (shootSfx != null) AudioSource.PlayClipAtPoint(shootSfx, origin, Mathf.Clamp01(shootSfxVolume));
            if (anim != null && HasParam("Shoot", AnimatorControllerParameterType.Trigger)) anim.SetTrigger("Shoot");
            return;
        }
        GameObject proj;
        if (useSimpleProjectile || projectilePrefab == null)
        {
            proj = CreateSimpleProjectile(origin, dir);
        }
        else
        {
            proj = Object.Instantiate(projectilePrefab, origin, Quaternion.identity);
            if (projectileScale != 1f) proj.transform.localScale = proj.transform.localScale * projectileScale;
            var rbp = proj.GetComponent<Rigidbody2D>();
            if (rbp == null) rbp = proj.AddComponent<Rigidbody2D>();
            rbp.bodyType = RigidbodyType2D.Dynamic;
            rbp.gravityScale = 0f;
            rbp.linearVelocity = dir * projectileSpeed;
            rbp.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var col2d = proj.GetComponent<Collider2D>();
            if (col2d == null)
            {
                col2d = proj.AddComponent<CircleCollider2D>();
                (col2d as CircleCollider2D).isTrigger = true;
                (col2d as CircleCollider2D).radius = Mathf.Max(0.06f, simpleProjectileSize * 0.35f);
            }
            else
            {
                col2d.isTrigger = true;
                var cc = col2d as CircleCollider2D;
                if (cc != null) cc.radius = Mathf.Max(0.06f, simpleProjectileSize * 0.35f);
            }
            var dmg = proj.GetComponent<ProjectileDamage2D>();
            if (dmg == null)
            {
                dmg = proj.AddComponent<ProjectileDamage2D>();
                dmg.damage = projectileDamage;
                dmg.targetTag = "Player";
                dmg.breakClip = projectileBreakSfx;
                dmg.breakVolume = projectileBreakSfxVolume;
                dmg.proximityDamage = true;
                dmg.proximityRadius = 0.6f;
            }
            if (rotateProjectileToDirection)
            {
                float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                proj.transform.rotation = Quaternion.AngleAxis(ang, Vector3.forward);
            }
        }
        if (projectileHoming && proj != null) StartCoroutine(Homing(proj));
        if (shootSfx != null) AudioSource.PlayClipAtPoint(shootSfx, origin, Mathf.Clamp01(shootSfxVolume));
        if (anim != null && HasParam("Shoot", AnimatorControllerParameterType.Trigger)) anim.SetTrigger("Shoot");
    }











    public void TakeDamage(int amount)
    {
        health = Mathf.Max(0, health - amount);
        UpdateHealthBar();
        if (health <= 0) Die();
        else
        {
            if (Time.time - lastTeleportOnDamage >= teleportOnDamageCooldown)
            {
                StartCoroutine(SkillTeleportOpposite());
                lastTeleportOnDamage = Time.time;
            }
        }
    }

    void Die()
    {
        if (hpRoot != null) Object.Destroy(hpRoot.gameObject);
        if (ambientSrc != null) ambientSrc.Stop();
        if (ambientGo != null) Object.Destroy(ambientGo);
        SpawnDeathFX(transform.position);
        Object.Destroy(gameObject);
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
}
