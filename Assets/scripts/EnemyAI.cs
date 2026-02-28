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
    public float attackRange = 1.5f;        // Distance at which enemy deals damage directly

    [Header("Knockback")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    [Header("Stuck Detection")]
    public float stuckCountMax = 10f;

    private float currentHealth;
    private Transform player;
    private PlayerHealth playerHealth;
    private float nextAttack = 0f;

    public System.Action OnDeath;
    public bool isOriginal = false;

    private bool isKnockedBack = false;
    private float knockbackEndTime;
    private Vector3 knockbackDirection;

    private CharacterController controller;
    private Vector3 moveDir;
    private float currentSpeed;

    private Vector3 previousPosition;
    private float stuckCount = 0f;

    void Start()
    {
        currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>(); // Get it directly, no need for triggers
        }

        controller = GetComponent<CharacterController>();
        currentSpeed = Random.Range(speed * 0.8f, speed * 1.2f);
        previousPosition = transform.position;
    }

    void Update()
    {
        if (isOriginal) return;
        if (player == null) return;

        // Attack check in Update for reliability
        HandleAttack();
    }

    void FixedUpdate()
    {
        if (isOriginal) return;
        if (player == null) return;

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
                isKnockedBack = false;
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

        // Smoother gravity - no more trembling
        if (controller.isGrounded)
            moveDir.y = -2f;                // Small constant to keep grounded, not -10
        else
            moveDir.y += Physics.gravity.y * 2f * Time.fixedDeltaTime;

        controller.Move(moveDir * Time.fixedDeltaTime);

        // Stuck detection
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

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (player != null)
        {
            knockbackDirection = (transform.position - player.position).normalized;
            knockbackDirection.y = 0;
            isKnockedBack = true;
            knockbackEndTime = Time.time + knockbackDuration;
        }

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}