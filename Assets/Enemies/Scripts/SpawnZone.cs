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
    public bool isBossZone = false;
    public GameObject bossPrefab;
    public float bossSpawnRadius = 8f;
    public float bossHorizontalJitter = 0.6f;
    public bool bossStartLeft = true;
    public bool bossEnsureAggro = true;
    public bool bossTeleportFromChildren = true;
    public Transform[] bossTeleportPoints;

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
            if (isBossZone && bossPrefab != null)
            {
                SpawnBoss(other.transform);
            }
            else
            {
                StartCoroutine(SpawnWave(other.transform));
            }
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

    void SpawnBoss(Transform player)
    {
        int side = bossStartLeft ? -1 : 1;
        Vector3 basePos = player.position + Vector3.right * side * bossSpawnRadius;
        basePos.x += Random.Range(-bossHorizontalJitter, bossHorizontalJitter);
        Vector3 spawnPos = basePos;
        var hit = Physics2D.Raycast(basePos + Vector3.up * 2f, Vector2.down, 20f, groundLayer);
        if (hit.collider != null) spawnPos.y = hit.point.y;
        var boss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        if (bossEnsureAggro)
        {
            var bc = boss.GetComponent<BossController>();
            if (bc == null) bc = boss.GetComponentInChildren<BossController>();
            if (bc != null) bc.alwaysAggro = true;
        }
        if (bossTeleportFromChildren)
        {
            var bc = boss.GetComponent<BossController>();
            if (bc == null) bc = boss.GetComponentInChildren<BossController>();
            if (bc != null)
            {
                System.Collections.Generic.List<Transform> pts = new System.Collections.Generic.List<Transform>();
                for (int i = 0; i < transform.childCount; i++)
                {
                    var t = transform.GetChild(i);
                    pts.Add(t);
                }
                if (pts.Count > 0) bc.teleportPoints = pts.ToArray();
            }
        }
        else if (bossTeleportPoints != null && bossTeleportPoints.Length > 0)
        {
            var bc = boss.GetComponent<BossController>();
            if (bc == null) bc = boss.GetComponentInChildren<BossController>();
            if (bc != null) bc.teleportPoints = bossTeleportPoints;
        }
    }
}
