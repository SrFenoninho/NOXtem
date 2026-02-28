using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public GameObject cloneSpawnerPrefab;       // Assign the CloneSpawner prefab here
    public int enemyCount = 20;                 // How many enemies to spawn at start
    public float spawnRadius = 10f;             // Radius around spawner
    public float spawnHeightOffset = 2f;        // Slightly above ground so CharacterController lands naturally

    private int currentEnemies = 0;

    void Start()
    {
        SpawnAllEnemies();
    }

    void SpawnAllEnemies()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float distance = Random.Range(spawnRadius * 0.3f, spawnRadius);

        Vector3 spawnPos = transform.position + new Vector3(
            Mathf.Cos(angle) * distance,
            spawnHeightOffset,
            Mathf.Sin(angle) * distance
        );

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        currentEnemies++;

        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.isOriginal = false;
            ai.generation = 0;
            ai.enemyPrefab = enemyPrefab;
            ai.cloneSpawnerPrefab = cloneSpawnerPrefab;
            ai.OnDeath += HandleEnemyDeath;
        }
    }

    void HandleEnemyDeath()
    {
        currentEnemies--;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius * 0.3f);
    }
}