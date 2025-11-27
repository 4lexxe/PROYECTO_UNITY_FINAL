using UnityEngine;

public partial class BossController
{
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
        DamageArea(to);
        yield return null;
    }

    System.Collections.IEnumerator SkillTeleportOpposite()
    {
        Vector3 startPos = transform.position;
        SpawnTeleportFX(startPos);
        if (teleportSfx != null) AudioSource.PlayClipAtPoint(teleportSfx, startPos, Mathf.Clamp01(teleportSfxVolume));
        yield return new WaitForSeconds(teleportFXDuration * 0.5f);
        Vector3 to = GetTeleportDestinationOpposite();
        transform.position = to;
        SpawnTeleportFX(to);
        if (teleportSfx != null) AudioSource.PlayClipAtPoint(teleportSfx, to, Mathf.Clamp01(teleportSfxVolume));
        DamageArea(to);
        yield return null;
    }

    System.Collections.IEnumerator SkillGrabLiftDrop()
    {
        if (player == null) yield break;
        Vector3 p = player.position;
        SpawnTeleportFX(p);
        if (grabSfx != null) AudioSource.PlayClipAtPoint(grabSfx, p, Mathf.Clamp01(grabSfxVolume));
        GameObject levGo = null;
        AudioSource levSrc = null;
        if (levitateSfx != null)
        {
            levGo = new GameObject("LevitateSfx");
            levGo.transform.position = p;
            levSrc = levGo.AddComponent<AudioSource>();
            levSrc.playOnAwake = false;
            levSrc.loop = true;
            levSrc.spatialBlend = 0f;
            levSrc.volume = Mathf.Clamp01(levitateSfxVolume);
            levSrc.clip = levitateSfx;
            levSrc.Play();
        }
        var pm = player.GetComponent<PlayerMovement>();
        var pc = player.GetComponent<PlayerController>();
        var rbp = player.GetComponent<Rigidbody2D>();
        if (pm != null) pm.enabled = false;
        RigidbodyConstraints2D orig = rbp != null ? rbp.constraints : RigidbodyConstraints2D.FreezeRotation;
        float origGrav = rbp != null ? rbp.gravityScale : 0f;
        if (rbp != null)
        {
            rbp.gravityScale = 0f;
            rbp.linearVelocity = Vector2.zero;
            rbp.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        Vector3 target = p + Vector3.up * Mathf.Max(0.5f, grabLiftHeight);
        float t0 = Time.time;
        while (Time.time - t0 < grabLiftDuration)
        {
            if (player == null) break;
            player.position = Vector3.Lerp(player.position, target, 0.2f);
            if (levGo != null) levGo.transform.position = player.position;
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForSeconds(Mathf.Max(0f, grabHoldSeconds));
        if (levSrc != null) levSrc.Stop();
        if (levGo != null) Object.Destroy(levGo);
        SpawnTeleportFX(player.position);
        if (rbp != null)
        {
            rbp.gravityScale = origGrav > 0f ? origGrav : 1f;
            rbp.linearVelocity = new Vector2(rbp.linearVelocity.x, -Mathf.Abs(grabDropSpeed));
        }
        bool landed = false;
        float timeout = 2.0f;
        float end = Time.time + timeout;
        while (!landed && Time.time < end && player != null)
        {
            var hit = Physics2D.Raycast(player.position, Vector2.down, 0.25f, groundLayer);
            if (hit.collider != null && rbp != null && rbp.linearVelocity.y <= 0f)
            {
                landed = true;
                break;
            }
            yield return new WaitForFixedUpdate();
        }
        if (groundImpactSfx != null) AudioSource.PlayClipAtPoint(groundImpactSfx, player.position, Mathf.Clamp01(groundImpactSfxVolume));
        if (pc != null) pc.RecibirDanio(grabDropDamage);
        SpawnExplosionFX(player.position);
        if (impactCameraShake)
        {
            var shakeGo = new GameObject("CameraShakeOnImpact");
            var shaker = shakeGo.AddComponent<CameraShakeDuringDeath>();
            shaker.duration = Mathf.Max(0f, impactCameraShakeDuration);
            shaker.amplitude = impactCameraShakeAmplitude;
            shaker.frequency = impactCameraShakeFrequency;
        }
        if (pm != null) pm.enabled = true;
        if (rbp != null) rbp.constraints = orig;
        if (pm != null)
        {
            int dir = player.position.x > transform.position.x ? 1 : -1;
            pm.StartSlide(dir);
        }
        yield return null;
    }

    Vector3 GetTeleportDestinationOpposite()
    {
        if (player == null) return transform.position;
        int side = transform.position.x < player.position.x ? 1 : -1; // opposite side
        Vector3 basePos = player.position + Vector3.right * side * teleportRadius;
        basePos.y += teleportYOffset;
        Vector3 spawnPos = basePos;
        var hit = Physics2D.Raycast(basePos + Vector3.up * 2f, Vector2.down, 20f, groundLayer);
        if (hit.collider != null) spawnPos.y = hit.point.y + teleportYOffset;
        return spawnPos;
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

    void DamageArea(Vector3 origin)
    {
        var hits = Physics2D.OverlapCircleAll(origin, teleportDamageRadius);
        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (h.CompareTag("Player"))
            {
                var pc = h.GetComponent<PlayerController>();
                if (pc != null) pc.RecibirDanio(teleportDamage);
                var rbp = h.attachedRigidbody;
                if (rbp != null)
                {
                    Vector2 dir = (h.transform.position - origin).normalized;
                    rbp.AddForce(dir * teleportKnockback, ForceMode2D.Impulse);
                }
            }
        }
    }
}
