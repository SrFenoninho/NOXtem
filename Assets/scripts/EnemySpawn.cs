using UnityEngine;

public class EnemySpawn : MonoBehaviour
{




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("Spawn Settings")]

    public GameObject enemyPrefab;
    public GameObject cloneSpawnerPrefab;
    public int enemyCount = 20;
    public float spawnRadius = 10f;
    public float spawnHeightOffset = 2f;





    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private int currentEnemies = 0;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        EnemyCombatManager.Clear();
        SpawnAllEnemies();
    }




    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    void SpawnAllEnemies()
    {
        for (int i = 0; i < enemyCount; i++)
            SpawnEnemy();
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

        if (cloneSpawnerPrefab != null && enemyPrefab != null)
        {
            GameObject spawnerObj = Instantiate(cloneSpawnerPrefab, spawnPos, Quaternion.identity);
            currentEnemies++;

            EnemyCloneSpawner spawner = spawnerObj.GetComponent<EnemyCloneSpawner>();
            if (spawner != null)
            {
                EnemyAI aiPrefab = enemyPrefab.GetComponent<EnemyAI>();
                int maxGen = aiPrefab != null ? aiPrefab.maxGeneration : 3;
                spawner.Initialize(enemyPrefab, cloneSpawnerPrefab, 0, maxGen, HandleEnemyDeath);
            }
        }
        else if (enemyPrefab != null)
        {
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
