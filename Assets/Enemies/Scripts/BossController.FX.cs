using UnityEngine;

public partial class BossController
{
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
        Object.Destroy(fx, teleportFXDuration + 0.2f);
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
        Object.Destroy(fx, explosionFxLifetime + 0.2f);
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
        Object.Destroy(fx, duration + 0.2f);
    }

    void SpawnDeathFX(Vector3 origin)
    {
        var fx = new GameObject("BossDeathFX");
        fx.transform.position = origin;
        var ps = fx.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = false;
        main.startLifetime = deathFXDuration;
        main.startSpeed = explosionFxSpeed;
        main.startSize = explosionFxSize * 1.2f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = deathFXColor;
        var emission = ps.emission; emission.enabled = true;
        var shape = ps.shape; shape.enabled = true; shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = explosionFxRadius * 1.5f;
        ps.Emit(Mathf.Max(1, explosionFxCount * 4));
        var sparksGo = new GameObject("Sparks");
        sparksGo.transform.SetParent(fx.transform);
        sparksGo.transform.localPosition = Vector3.zero;
        var sparks = sparksGo.AddComponent<ParticleSystem>();
        var sm = sparks.main; sm.loop = false; sm.startLifetime = deathFXDuration * 0.6f; sm.startSpeed = explosionFxSpeed * 1.6f; sm.startSize = explosionFxSize * 0.6f; sm.startColor = deathFXColor;
        var se = sparks.emission; se.enabled = true; se.rateOverTime = 120f;
        var ss = sparks.shape; ss.enabled = true; ss.shapeType = ParticleSystemShapeType.Cone; ss.angle = 45f; ss.radius = explosionFxRadius * 0.6f;
        var ringsGo = new GameObject("Rings");
        ringsGo.transform.SetParent(fx.transform);
        ringsGo.transform.localPosition = Vector3.zero;
        var rings = ringsGo.AddComponent<ParticleSystem>();
        var rm = rings.main; rm.loop = false; rm.startLifetime = deathFXDuration * 0.7f; rm.startSpeed = 0.2f; rm.startSize = Mathf.Max(0.04f, explosionFxSize * 0.8f); rm.startColor = deathFXColor;
        var re = rings.emission; re.enabled = true; rings.Emit(64);
        var rshape = rings.shape; rshape.enabled = true; rshape.shapeType = ParticleSystemShapeType.Circle; rshape.radius = explosionFxRadius * 1.2f;
        var lightGo = new GameObject("Light");
        lightGo.transform.SetParent(fx.transform);
        lightGo.transform.localPosition = Vector3.zero;
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = deathFXColor;
        light.intensity = deathLightIntensity;
        light.range = deathLightRange;
        light.shadows = LightShadows.None;
        if (deathSfx != null) AudioSource.PlayClipAtPoint(deathSfx, origin, Mathf.Clamp01(deathSfxVolume));
        if (deathCameraShake)
        {
            var shakeGo = new GameObject("CameraShakeOnBossDeath");
            var shaker = shakeGo.AddComponent<CameraShakeDuringDeath>();
            shaker.duration = Mathf.Max(0f, deathCameraShakeDuration);
            shaker.amplitude = deathCameraShakeAmplitude;
            shaker.frequency = deathCameraShakeFrequency;
        }
        var sp = fx.AddComponent<DeathOrbsSpawner>();
        sp.count = 24;
        sp.radius = explosionFxRadius * 1.3f;
        sp.orbSize = 0.5f;
        sp.lightIntensity = 6f;
        sp.lightRange = 10f;
        sp.color = deathFXColor;
        sp.lifetime = 3.5f;
        sp.delay = deathFXDuration;
        Object.Destroy(fx, deathFXDuration + 0.2f);
    }

    class CameraShakeDuringDeath : MonoBehaviour
    {
        public float duration = 1f;
        public float amplitude = 0.2f;
        public float frequency = 60f;
        void OnEnable()
        {
            StartCoroutine(Shake());
        }
        System.Collections.IEnumerator Shake()
        {
            Transform cam = null;
            if (Camera.main != null) cam = Camera.main.transform;
            else
            {
                var anyCam = Object.FindObjectOfType<Camera>();
                if (anyCam != null) cam = anyCam.transform;
            }
            if (cam == null)
            {
                Destroy(gameObject);
                yield break;
            }
            Vector3 lastOffset = Vector3.zero;
            float end = Time.realtimeSinceStartup + Mathf.Max(0f, duration);
            float minStep = 1f / Mathf.Max(1f, frequency);
            while (Time.realtimeSinceStartup < end)
            {
                Vector2 r = Random.insideUnitCircle * amplitude;
                Vector3 newOffset = new Vector3(r.x, r.y, 0f);
                cam.position += (newOffset - lastOffset);
                lastOffset = newOffset;
                yield return new WaitForEndOfFrame();
                yield return new WaitForSecondsRealtime(minStep);
            }
            if (lastOffset != Vector3.zero)
            {
                cam.position -= lastOffset;
            }
            Destroy(gameObject);
        }
    }

    class DeathOrbsSpawner : MonoBehaviour
    {
        public int count;
        public float radius;
        public float orbSize;
        public float lightIntensity;
        public float lightRange;
        public Color color;
        public float lifetime;
        public float delay;
        void OnEnable()
        {
            StartCoroutine(SpawnLater());
        }
        System.Collections.IEnumerator SpawnLater()
        {
            yield return new WaitForSeconds(Mathf.Max(0f, delay));
            Vector3 o = transform.position;
            int c = Mathf.Max(1, count);
            for (int i = 0; i < c; i++)
            {
                float ang = (Mathf.PI * 2f) * (i / (float)c);
                Vector3 pos = o + new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f) * Mathf.Max(0.1f, radius);
                var orb = new GameObject("DeathOrb");
                orb.transform.position = pos;
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.SetParent(orb.transform);
                sphere.transform.localPosition = Vector3.zero;
                sphere.transform.localScale = Vector3.one * Mathf.Max(0.1f, orbSize);
                var mr = sphere.GetComponent<MeshRenderer>();
                if (mr != null) mr.material.color = color;
                var sc = sphere.GetComponent<SphereCollider>();
                if (sc != null) Object.Destroy(sc);
                var lightGo = new GameObject("Light");
                lightGo.transform.SetParent(orb.transform);
                lightGo.transform.localPosition = Vector3.zero;
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = color;
                light.intensity = lightIntensity;
                light.range = lightRange;
                light.shadows = LightShadows.None;
                var ps = orb.AddComponent<ParticleSystem>();
                var main = ps.main; main.loop = false; main.startLifetime = lifetime; main.startSpeed = 0.2f; main.startSize = Mathf.Max(0.05f, orbSize * 0.5f); main.startColor = color; main.simulationSpace = ParticleSystemSimulationSpace.World;
                var emission = ps.emission; emission.enabled = true; emission.rateOverTime = 40f;
                var shape = ps.shape; shape.enabled = true; shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = orbSize * 0.25f;
                Object.Destroy(orb, Mathf.Max(0.2f, lifetime));
            }
        }
    }
}
