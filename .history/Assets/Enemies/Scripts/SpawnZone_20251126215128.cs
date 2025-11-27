using UnityEngine;

public class SpawnZone : MonoBehaviour
{
    public GameObject enemyPrefab;
    public bool hasSpawned = false;
    public float spawnRadius = 6f;
    public float horizontalJitter = 1f;
    public LayerMask groundLayer;
    public bool autoIsTrigger = true;
    public int totalToSpawn = 6;
    public float spawnInterval = 0.8f;
    public bool alternateSides = true;
    public bool startLeft = true;
    public bool ensureAggro = true;

    void Awake()
    {
        var c = GetComponent<Collider2D>();
        if (autoIsTrigger && c != null) c.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasSpawned)
        {
            if (enemyPrefab == null)
            {
                Debug.LogWarning("SpawnZone: enemyPrefab no asignado");
                return;
            }
            hasSpawned = true;
            StartCoroutine(SpawnWave(other.transform));
        }
    }

    System.Collections.IEnumerator SpawnWave(Transform player)
    {
        int count = Mathf.Max(1, totalToSpawn);
        int side = startLeft ? -1 : 1;
        for (int i = 0; i < count; i++)
        {
            int useSide = side;
            if (alternateSides) side = -side;
            Vector3 basePos = player.position + Vector3.right * useSide * spawnRadius;
            basePos.x += Random.Range(-horizontalJitter, horizontalJitter);
            Vector3 spawnPos = basePos;
            var hit = Physics2D.Raycast(basePos + Vector3.up * 2f, Vector2.down, 10f, groundLayer);
            if (hit.collider != null) spawnPos.y = hit.point.y;
            var go = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            if (ensureAggro)
            {
                var es = go.GetComponent<EnemySimple>();
                if (es == null) es = go.GetComponentInChildren<EnemySimple>();
                if (es != null) es.alwaysAggro = true;
            }
            if (i < count - 1) yield return new WaitForSeconds(Mathf.Max(0f, spawnInterval));
        }
    }
}
