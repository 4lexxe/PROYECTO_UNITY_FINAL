using UnityEngine;

public class BossController : MonoBehaviour
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
    public float fireRate = 1.2f;
    public float projectileSpeed = 7f;
    public Vector2 fireOffset = new Vector2(0.6f, 0.4f);
    public float projectileScale = 1f;
    public bool rotateProjectileToDirection = true;
    public bool projectileHoming = true;
    public float homingDuration = 3.5f;
    public bool useSimpleProjectile = true;
    public float simpleProjectileLifetime = 5f;
    public float simpleProjectileSize = 0.7f;
    public Color simpleProjectileColor = new Color(0.9f, 0.2f, 0.2f, 1f);
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

    private Transform player;
    private Animator anim;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private float lastAttack;
    private float lastSkill;
    private float lastShot;

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
    }

    void Update()
    {
        if (player == null) return;
        Vector3 self = transform.position;
        float dist = Vector2.Distance(self, player.position);
        float dir = Mathf.Sign(player.position.x - self.x);
        if (sr != null) sr.flipX = dir < 0f;
        if (!alwaysAggro && dist > aggroRange)
        {
            if (anim != null && HasParam(speedParamName, AnimatorControllerParameterType.Float)) anim.SetFloat(speedParamName, 0f);
            return;
        }
        if (dist > attackRange)
        {
            Vector3 target = new Vector3(player.position.x, self.y, self.z);
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
        if (dist <= shootRange && Time.time - lastShot >= fireRate)
        {
            ShootAtPlayer();
            lastShot = Time.time;
        }
    }

    System.Collections.IEnumerator SkillTeleport()
    {
        Vector3 startPos = transform.position;
        SpawnTeleportFX(startPos);
        if (teleportSfx != null) AudioSource.PlayClipAtPoint(teleportSfx, startPos, Mathf.Clamp01(teleportSfxVolume));
        yield return new WaitForSeconds(teleportFXDuration * 0.5f);
        Vector3 to = GetTeleportDestination();
        transform.position = to;
        SpawnTeleportFX(to);
        if (teleportSfx != null) AudioSource.PlayClipAtPoint(teleportSfx, to, Mathf.Clamp01(teleportSfxVolume));
        yield return null;
    }

    Vector3 GetTeleportDestination()
    {
        if (teleportPoints != null && teleportPoints.Length > 0)
        {
            int idx = Random.Range(0, teleportPoints.Length);
            return teleportPoints[idx].position;
        }
        if (player == null) return transform.position;
        int side = Random.value < 0.5f ? -1 : 1;
        Vector3 basePos = player.position + Vector3.right * side * teleportRadius;
        basePos.y += teleportYOffset;
        Vector3 spawnPos = basePos;
        var hit = Physics2D.Raycast(basePos + Vector3.up * 2f, Vector2.down, 20f, groundLayer);
        if (hit.collider != null) spawnPos.y = hit.point.y + teleportYOffset;
        return spawnPos;
    }

    void SpawnTeleportFX(Vector3 origin)
    {
        var fx = new GameObject("TeleportFX");
        fx.transform.position = origin;
        var ps = fx.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = false;
        main.startLifetime = teleportFXDuration;
        main.startSpeed = 0f;
        main.startSize = 0.12f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = teleportFXColor;
        var emission = ps.emission; emission.enabled = true;
        var shape = ps.shape; shape.enabled = true; shape.shapeType = ParticleSystemShapeType.Circle; shape.radius = 0.8f;
        ps.Emit(24);
        Destroy(fx, teleportFXDuration + 0.2f);
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

    void ShootRadialWave(Vector3 origin)
    {
        int count = Mathf.Max(1, radialCount);
        SpawnRingFX(origin, radialRingRadius, radialFxColor, radialFxDuration, count);
        for (int i = 0; i < count; i++)
        {
            float ang = (Mathf.PI * 2f) * (i / (float)count);
            Vector2 d = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
            Vector3 pos = origin + new Vector3(d.x, d.y, 0f) * radialRingRadius;
            CreateSimpleProjectile(pos, d);
        }
    }

    void ShootBomb(Vector3 origin, Vector2 dir)
    {
        var bomb = new GameObject("BombProjectile");
        bomb.transform.position = origin;
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.SetParent(bomb.transform);
        sphere.transform.localPosition = Vector3.zero;
        sphere.transform.localScale = Vector3.one * simpleProjectileSize;
        var mr = sphere.GetComponent<MeshRenderer>();
        if (mr != null) mr.material.color = simpleProjectileColor;
        var sc = sphere.GetComponent<SphereCollider>();
        if (sc != null) Object.Destroy(sc);
        var col2d = bomb.AddComponent<CircleCollider2D>();
        col2d.isTrigger = true;
        col2d.radius = Mathf.Max(0.05f, simpleProjectileSize * 0.5f);
        var rbp = bomb.AddComponent<Rigidbody2D>();
        rbp.bodyType = RigidbodyType2D.Dynamic;
        rbp.gravityScale = 0f;
        rbp.linearVelocity = dir.normalized * projectileSpeed * bombSpeedMultiplier;
        if (rotateProjectileToDirection)
        {
            float a = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            bomb.transform.rotation = Quaternion.AngleAxis(a, Vector3.forward);
        }
        var bb = bomb.AddComponent<BombBehaviour>();
        bb.owner = this;
        bb.fuseSeconds = bombFuseSeconds;
        bb.count = bombMiniCount;
        bb.breakClip = projectileBreakSfx;
        bb.breakVolume = projectileBreakSfxVolume;
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
        var go = new GameObject("BossSimpleProjectile3D");
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
        dmg.breakClip = projectileBreakSfx;
        dmg.breakVolume = projectileBreakSfxVolume;
        Object.Destroy(go, simpleProjectileLifetime);
        return go;
    }

    public class ProjectileDamage2D : MonoBehaviour
    {
        public int damage = 1;
        public string targetTag = "Player";
        public AudioClip breakClip;
        public float breakVolume = 1f;
        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(targetTag)) return;
            var pc = other.GetComponent<PlayerController>();
            if (pc != null) pc.RecibirDanio(damage);
            Destroy(gameObject);
        }
        public void Break()
        {
            SpawnBreakFX();
            Destroy(gameObject);
        }
        void SpawnBreakFX()
        {
            var fx = new GameObject("ProjectileBreakFX");
            fx.transform.position = transform.position;
            var ps = fx.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = false;
            main.startLifetime = 0.25f;
            main.startSpeed = 2.2f;
            main.startSize = 0.08f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            Color col = Color.red;
            var mr = GetComponentInChildren<MeshRenderer>();
            if (mr != null && mr.material != null) col = mr.material.color;
            main.startColor = col;
            var emission = ps.emission; emission.enabled = true;
            ps.Emit(16);
            if (breakClip != null)
            {
                var src = fx.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 0f;
                src.volume = Mathf.Clamp01(breakVolume);
                src.PlayOneShot(breakClip, Mathf.Clamp01(breakVolume));
            }
            Destroy(fx, 0.6f);
        }
    }

    public class BombBehaviour : MonoBehaviour
    {
        public BossController owner;
        public float fuseSeconds = 1.2f;
        public int count = 8;
        public AudioClip breakClip;
        public float breakVolume = 1f;
        void OnEnable()
        {
            StartCoroutine(Explode());
        }
        System.Collections.IEnumerator Explode()
        {
            yield return new WaitForSeconds(fuseSeconds);
            Vector3 o = transform.position;
            int c = Mathf.Max(1, count);
            for (int i = 0; i < c; i++)
            {
                float ang = (Mathf.PI * 2f) * (i / (float)c);
                Vector2 d = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
                if (owner != null) owner.CreateSimpleProjectile(o, d);
            }
            if (breakClip != null) AudioSource.PlayClipAtPoint(breakClip, o, Mathf.Clamp01(breakVolume));
            if (owner != null) owner.SpawnExplosionFX(o);
            Destroy(gameObject);
        }
    }

    void SpawnExplosionFX(Vector3 origin)
    {
        var fx = new GameObject("ExplosionFX");
        fx.transform.position = origin;
        var ps = fx.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = false;
        main.startLifetime = explosionFxLifetime;
        main.startSpeed = explosionFxSpeed;
        main.startSize = explosionFxSize;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = explosionFxColor;
        var emission = ps.emission; emission.enabled = true;
        var shape = ps.shape; shape.enabled = true; shape.shapeType = ParticleSystemShapeType.Circle; shape.radius = explosionFxRadius;
        ps.Emit(Mathf.Max(1, explosionFxCount));
        Destroy(fx, explosionFxLifetime + 0.2f);
    }

    void SpawnRingFX(Vector3 origin, float radius, Color color, float duration, int count)
    {
        var fx = new GameObject("RadialRingFX");
        fx.transform.position = origin;
        var ps = fx.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = false;
        main.startLifetime = duration;
        main.startSpeed = 0f;
        main.startSize = Mathf.Max(0.04f, explosionFxSize * 0.5f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = color;
        var emission = ps.emission; emission.enabled = true;
        var shape = ps.shape; shape.enabled = true; shape.shapeType = ParticleSystemShapeType.Circle; shape.radius = radius;
        ps.Emit(Mathf.Max(1, count));
        Destroy(fx, duration + 0.2f);
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
