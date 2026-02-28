using UnityEngine;
using System.Collections;

public class EnemySpawn : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public float spawnRadius = 10f;
    public float spawnInterval = 2f;
    public int maxEnemies = 10;

    [Header("Difficulty")]
    public bool increaseDifficulty = true;      // Gradually reduce spawn interval over time
    public float minSpawnInterval = 0.5f;       // Minimum interval at max difficulty
    public float difficultyRate = 0.05f;        // How fast interval decreases per second

    [Header("Spawn Height")]
    public float spawnHeightOffset = 2f;        // Spawn enemies slightly above ground so CharacterController lands naturally

    private int currentEnemies = 0;
    private float currentSpawnInterval;

    void Start()
    {
        currentSpawnInterval = spawnInterval;
        StartCoroutine(SpawnRoutine());
    }

    void Update()
    {
        // Gradually increase difficulty by reducing spawn interval
        if (increaseDifficulty && currentSpawnInterval > minSpawnInterval)
        {
            currentSpawnInterval -= difficultyRate * Time.deltaTime;
            currentSpawnInterval = Mathf.Max(currentSpawnInterval, minSpawnInterval);
        }
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (currentEnemies < maxEnemies)
                SpawnEnemy();

            yield return new WaitForSeconds(currentSpawnInterval);
        }
    }

    void SpawnEnemy()
    {
        float angle = Random.Range(0f, Mathf.PI * 2);
        float distance = Random.Range(spawnRadius * 0.5f, spawnRadius);

        Vector3 spawnPos = transform.position + new Vector3(
            Mathf.Cos(angle) * distance,
            spawnHeightOffset,              // Slightly above ground so enemy falls into place
            Mathf.Sin(angle) * distance
        );

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        currentEnemies++;

        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.isOriginal = false;
            ai.OnDeath += () => currentEnemies--;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius * 0.5f);
    }
}