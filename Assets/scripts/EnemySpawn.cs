using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public GameObject cloneSpawnerPrefab;   // prefab do CloneSpawner para passar aos inimigos
    public int enemyCount = 20;             // numero de inimigos a criar no inicio
    public float spawnRadius = 10f;         // raio a volta do spawner
    public float spawnHeightOffset = 2f;    // ligeiramente acima do chao para o CharacterController aterrar

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private int currentEnemies = 0;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        SpawnAllEnemies();
    }

    // ---------------------------------------------
    //  SPAWN
    // ---------------------------------------------
    void SpawnAllEnemies()
    {
        for (int i = 0; i < enemyCount; i++)
            SpawnEnemy();
    }

    void SpawnEnemy()
    {
        // Posicao aleatoria dentro do raio de spawn
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

    // ---------------------------------------------
    //  MORTE DE INIMIGO
    // ---------------------------------------------
    void HandleEnemyDeath()
    {
        currentEnemies--;
    }

    // ---------------------------------------------
    //  DESENHOS DE DEPURAcaO
    // ---------------------------------------------
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius * 0.3f); // raio minimo
    }
}
