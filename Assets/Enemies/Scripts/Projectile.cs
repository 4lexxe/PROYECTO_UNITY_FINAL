using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 8f;
    public float lifetime = 4f;

    private Vector2 direction;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        // Si usa Rigidbody2D, moverlo con f�sica
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
        else
        {
            // fallback para proyectiles sin Rigidbody2D
            transform.Translate(direction * speed * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// Llamado por EnemyShooter para darle direcci�n al proyectil.
    /// </summary>
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        // Rotar sprite del proyectil hacia donde apunta
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
