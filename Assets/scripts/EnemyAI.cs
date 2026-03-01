using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;

    [Header("Movement")]
    public float speed = 3f;
    public float chaseSpeed = 5f;
    public float detectionRadius = 15f;

    [Header("Attack")]
    public float attackDamage = 10f;
    public float attackInterval = 1f;
    public float attackRange = 1.5f;

    [Header("Knockback")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    [Header("Stun")]
    public float stunShakeIntensity = 0.05f;

    [Header("Stuck Detection")]
    public float stuckCountMax = 10f;

    [Header("Clone Spawn Range")]
    public float cloneSpawnMinRadius = 4f;
    public float cloneSpawnMaxRadius = 8f;
    public float cloneSpawnHeightOffset = 2f;

    // Assigned at runtime by EnemySpawn or EnemyCloneSpawner
    [HideInInspector] public GameObject enemyPrefab;
    [HideInInspector] public GameObject cloneSpawnerPrefab;
    [HideInInspector] public int generation = 0;
    [HideInInspector] public int maxGeneration = 3;
    [HideInInspector] public float externalKnockbackForce;

    private int normalLayer;
    private int knockbackLayer;

    private float currentHealth;
    private Transform player;
    private PlayerHealth playerHealth;
    private float nextAttack = 0f;

    public System.Action OnDeath;
    public bool isOriginal = false;

    private bool isKnockedBack = false;
    private float knockbackEndTime;
    private Vector3 knockbackDirection;

    private bool isStunned = false;
    private float stunEndTime;
    private Transform modelTransform;

    private CharacterController controller;
    private Vector3 moveDir;
    private float currentSpeed;

    private Vector3 previousPosition;
    private float stuckCount = 0f;

    private Vector3 modelOriginalLocalPosition;

    void Start()
    {
        currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }

        controller = GetComponent<CharacterController>();
        currentSpeed = Random.Range(speed * 0.8f, speed * 1.2f);
        previousPosition = transform.position;

        normalLayer = gameObject.layer;
        knockbackLayer = LayerMask.NameToLayer("EnemyKnockback");

        // Use first child as model for shake, fallback to self
        modelTransform = transform.childCount > 0 ? transform.GetChild(0) : transform;
        modelOriginalLocalPosition = modelTransform.localPosition;
    }

    void Update()
    {
        if (isOriginal) return;
        if (player == null) return;

        if (isStunned)
        {
            HandleStun();
            return;
        }

        HandleAttack();
    }

    void FixedUpdate()
    {
        if (isOriginal) return;
        if (player == null) return;

        if (isStunned) return;

        HandleMovement();
    }

    void HandleAttack()
    {
        if (playerHealth == null) return;
        if (Time.time < nextAttack) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= attackRange)
        {
            playerHealth.TakeDamage(attackDamage, transform.position);
            nextAttack = Time.time + attackInterval;
        }
    }

    void HandleStun()
    {
        modelTransform.localPosition = modelOriginalLocalPosition + new Vector3(
            Random.Range(-stunShakeIntensity, stunShakeIntensity),
            0,
            Random.Range(-stunShakeIntensity, stunShakeIntensity)
        );

        if (Time.time >= stunEndTime)
        {
            isStunned = false;
            modelTransform.localPosition = modelOriginalLocalPosition;
            gameObject.layer = normalLayer;
        }
    }

    void HandleMovement()
    {
        Vector3 playerPos = player.position;
        playerPos.y = transform.position.y;
        transform.LookAt(playerPos);

        float distanceToPlayer = Vector3.Distance(playerPos, transform.position);

        if (isKnockedBack)
        {
            moveDir.x = knockbackDirection.x * knockbackForce;
            moveDir.z = knockbackDirection.z * knockbackForce;
            if (Time.time >= knockbackEndTime)
            {
                isKnockedBack = false;
                if (stunEndTime > Time.time)
                    isStunned = true;
            }
        }
        else if (distanceToPlayer <= detectionRadius)
        {
            float speedToUse = distanceToPlayer < 7.5f ? chaseSpeed : currentSpeed;
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            moveDir.x = direction.x * speedToUse;
            moveDir.z = direction.z * speedToUse;
        }
        else
        {
            moveDir.x = 0;
            moveDir.z = 0;
        }

        if (controller.isGrounded)
            moveDir.y = -2f;
        else
            moveDir.y += Physics.gravity.y * 2f * Time.fixedDeltaTime;

        controller.Move(moveDir * Time.fixedDeltaTime);

        if (Vector3.Distance(previousPosition, transform.position) < 0.01f)
        {
            stuckCount++;
            if (stuckCount >= stuckCountMax)
            {
                stuckCount = 0f;
                Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
                controller.Move(randomDir * currentSpeed * Time.fixedDeltaTime * 5f);
            }
        }
        else
        {
            stuckCount = 0f;
        }

        previousPosition = transform.position;
    }

    public void TakeDamage(float damage, float knockback = 0f, float stunDuration = 0f)
    {
        currentHealth -= damage;

        if (player != null)
        {
            gameObject.layer = knockbackLayer;
            knockbackDirection = (transform.position - player.position).normalized;
            knockbackDirection.y = 0;
            isKnockedBack = true;
            knockbackEndTime = Time.time + knockbackDuration;
            knockbackForce = knockback > 0f ? knockback : knockbackForce;

            // Stun begins after knockback ends
            if (stunDuration > 0f)
            {
                stunEndTime = Time.time + knockbackDuration + stunDuration;
            }
        }

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        if (generation < maxGeneration && cloneSpawnerPrefab != null && enemyPrefab != null)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float distance = Random.Range(cloneSpawnMinRadius, cloneSpawnMaxRadius);
            Vector3 clonePos = transform.position + new Vector3(
                Mathf.Cos(angle) * distance,
                cloneSpawnHeightOffset,
                Mathf.Sin(angle) * distance
            );

            GameObject spawnerObj = Instantiate(cloneSpawnerPrefab, clonePos, Quaternion.identity);
            EnemyCloneSpawner spawner = spawnerObj.GetComponent<EnemyCloneSpawner>();
            if (spawner != null)
            {
                spawner.Initialize(enemyPrefab, cloneSpawnerPrefab, generation + 1, maxGeneration, () => OnDeath?.Invoke());
            }
        }

        OnDeath?.Invoke();
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}