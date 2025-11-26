using UnityEngine;

public class SpawnZone : MonoBehaviour
{
    public GameObject enemyPrefab;
    public bool hasSpawned = false;
    public float spawnRadius = 6f;
    public float horizontalJitter = 1f;
    public LayerMask groundLayer;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasSpawned)
        {
            Transform player = other.transform;
            int side = Random.value < 0.5f ? -1 : 1;
            Vector3 basePos = player.position + Vector3.right * side * spawnRadius;
            basePos.x += Random.Range(-horizontalJitter, horizontalJitter);
            Vector3 spawnPos = basePos;
            var hit = Physics2D.Raycast(basePos + Vector3.up * 2f, Vector2.down, 10f, groundLayer);
            if (hit.collider != null) spawnPos.y = hit.point.y;
            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            hasSpawned = true;
        }
    }
}
