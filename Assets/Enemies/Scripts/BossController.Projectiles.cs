using UnityEngine;

public partial class BossController
{
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
        if (simpleProjectileLight)
        {
            var lightGo = new GameObject("Light");
            lightGo.transform.SetParent(bomb.transform);
            lightGo.transform.localPosition = Vector3.zero;
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            Color lc = simpleProjectileLightSyncColor ? (mr != null && mr.material != null ? mr.material.color : simpleProjectileColor) : simpleProjectileLightColor;
            light.color = lc;
            light.intensity = simpleProjectileLightIntensity * 0.8f;
            light.range = simpleProjectileLightRange * 0.8f;
            light.shadows = LightShadows.None;
        }
        var trail = bomb.AddComponent<ParticleSystem>();
        var tmain = trail.main; tmain.loop = true; tmain.startLifetime = 0.45f; tmain.startSpeed = 0.15f; tmain.startSize = Mathf.Max(0.06f, simpleProjectileSize * 0.3f); tmain.simulationSpace = ParticleSystemSimulationSpace.World; tmain.startColor = simpleProjectileColor;
        var temission = trail.emission; temission.enabled = true; temission.rateOverTime = 28f;
        var tshape = trail.shape; tshape.enabled = true; tshape.shapeType = ParticleSystemShapeType.Sphere; tshape.radius = simpleProjectileSize * 0.2f;
        var bb = bomb.AddComponent<BombBehaviour>();
        bb.owner = this;
        bb.fuseSeconds = bombFuseSeconds;
        bb.count = bombMiniCount;
        bb.breakClip = projectileBreakSfx;
        bb.breakVolume = projectileBreakSfxVolume;
        var dmg = bomb.AddComponent<ProjectileDamage2D>();
        dmg.damage = projectileDamage;
        dmg.targetTag = "Player";
        dmg.breakClip = projectileBreakSfx;
        dmg.breakVolume = projectileBreakSfxVolume;
        dmg.proximityDamage = true;
        dmg.proximityRadius = 0.6f;
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
        if (simpleProjectileLight)
        {
            var lightGo = new GameObject("Light");
            lightGo.transform.SetParent(go.transform);
            lightGo.transform.localPosition = Vector3.zero;
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            Color lc = simpleProjectileLightSyncColor ? (mr != null && mr.material != null ? mr.material.color : simpleProjectileColor) : simpleProjectileLightColor;
            light.color = lc;
            light.intensity = simpleProjectileLightIntensity;
            light.range = simpleProjectileLightRange;
            light.shadows = LightShadows.None;
        }
        var trail = go.AddComponent<ParticleSystem>();
        var tmain = trail.main; tmain.loop = true; tmain.startLifetime = 0.35f; tmain.startSpeed = 0.1f; tmain.startSize = Mathf.Max(0.05f, simpleProjectileSize * 0.25f); tmain.simulationSpace = ParticleSystemSimulationSpace.World; tmain.startColor = simpleProjectileColor;
        var temission = trail.emission; temission.enabled = true; temission.rateOverTime = 20f;
        var tshape = trail.shape; tshape.enabled = true; tshape.shapeType = ParticleSystemShapeType.Sphere; tshape.radius = simpleProjectileSize * 0.15f;
        var sc = sphere.GetComponent<SphereCollider>();
        if (sc != null) Object.Destroy(sc);
        var col2d = go.AddComponent<CircleCollider2D>();
        col2d.isTrigger = true;
        col2d.radius = Mathf.Max(0.06f, simpleProjectileSize * 0.35f);
        var rbp = go.AddComponent<Rigidbody2D>();
        rbp.bodyType = RigidbodyType2D.Dynamic;
        rbp.gravityScale = 0f;
        rbp.linearVelocity = dir.normalized * projectileSpeed;
        rbp.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
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
        dmg.proximityDamage = true;
        dmg.proximityRadius = 0.6f;
        Object.Destroy(go, simpleProjectileLifetime);
        return go;
    }

    public class ProjectileDamage2D : MonoBehaviour
    {
        public int damage = 1;
        public string targetTag = "Player";
        public AudioClip breakClip;
        public float breakVolume = 1f;
        public bool proximityDamage = true;
        public float proximityRadius = 0.6f;
        Transform player;
        void OnEnable()
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        void Update()
        {
            if (!proximityDamage || player == null) return;
            Vector3 pp = player.position;
            Vector3 tp = transform.position;
            float r = proximityRadius;
            if ((pp - tp).sqrMagnitude <= r * r)
            {
                var pm = player.GetComponent<PlayerMovement>();
                if (pm == null) pm = player.GetComponentInParent<PlayerMovement>();
                if (pm != null && pm.IsAttackingForProjectile())
                {
                    Break();
                    return;
                }
                var pc = player.GetComponent<PlayerController>();
                if (pc == null) pc = player.GetComponentInParent<PlayerController>();
                if (pc != null) pc.RecibirDanio(damage);
                Destroy(gameObject);
            }
        }
        void OnTriggerEnter2D(Collider2D other)
        {
            var pc = other.GetComponent<PlayerController>();
            var pm = other.GetComponent<PlayerMovement>();
            if (pc == null) pc = other.GetComponentInParent<PlayerController>();
            if (pm == null) pm = other.GetComponentInParent<PlayerMovement>();
            bool isPlayer = other.CompareTag(targetTag) || pc != null || pm != null;
            if (isPlayer)
            {
                if (pm != null && pm.IsAttackingForProjectile())
                {
                    Break();
                    return;
                }
                if (pc != null)
                {
                    pc.RecibirDanio(damage);
                }
                else if (pm != null)
                {
                    pm.gameObject.SendMessage("RecibirDanio", damage, SendMessageOptions.DontRequireReceiver);
                }
                Destroy(gameObject);
                return;
            }
        }
        void OnTriggerStay2D(Collider2D other)
        {
            var pm = other.GetComponent<PlayerMovement>();
            if (pm == null) pm = other.GetComponentInParent<PlayerMovement>();
            if (pm != null && pm.IsAttackingForProjectile())
            {
                Break();
            }
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
            var lightGo = new GameObject("Light");
            lightGo.transform.SetParent(fx.transform);
            lightGo.transform.localPosition = Vector3.zero;
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = col;
            light.intensity = 6f;
            light.range = 8f;
            light.shadows = LightShadows.None;
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
}
