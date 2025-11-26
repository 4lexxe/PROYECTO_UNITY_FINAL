using UnityEngine;

public class EnemyShooter : EnemyBase
{
    [Header("Proyectil")]
    public GameObject projectilePrefab;
    public Transform shootPoint;
    public float projectileSpeed = 10f;

    protected override void Attack()
    {
        Shoot();
    }

    private void Shoot()
    {
        if (projectilePrefab == null || shootPoint == null || player == null) return;

        GameObject proj = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);

        // calculamos direcci�n en X/Y hacia player (manteniendo Z del proyectil tal cual)
        Vector2 dir = (new Vector2(player.position.x, player.position.y) - new Vector2(shootPoint.position.x, shootPoint.position.y)).normalized;

        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = dir * projectileSpeed;
        }
        else
        {
            // fallback: mover por script del proyectil
            Projectile projScript = proj.GetComponent<Projectile>();
            if (projScript != null) projScript.SetDirection(dir);
        }

        // opcional: rotar sprite del proyectil para que apunte
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        proj.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
