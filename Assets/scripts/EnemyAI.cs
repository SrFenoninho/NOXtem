using UnityEngine;

public class EnemyAI : MonoBehaviour
{




    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private enum Phase
    {
        Spawning,
        OrbitFar,
        ApproachClose,
        ReadyToAttack,
        Attacking,
        Reposition,
    }




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("Animation")]

    public Animator animator;
    public string spawnTrigger = "Spawn";
    public string moveXFloat = "MoveX";
    public string moveZFloat = "MoveZ";
    public string attackTrigger = "Attack";
    public float spawnDuration = 1.5f;

    [Header("Health")]
    public float maxHealth = 100f;

    [Header("Movement")]
    public float speed = 3f;
    public float chaseSpeed = 5f;
    public float detectionRadius = 15f;

    [Header("Orbit")]
    public float farOrbitRadius = 8f;
    public float closeOrbitRadius = 4.5f;
    public float orbitSpeed = 60f;

    private float orbitAngle;

    [Header("Approach")]
    public float approachChancePerSecond = 0.25f;
    public int maxCloseEnemies = 3;
    public float readyToAttackTimeout = 3f;

    [Header("Attack")]
    public float attackDamage = 10f;
    public float attackInterval = 1f;
    public float attackRange = 1.5f;
    public float attackLungeDuration = 0.35f;
    public float attackFinishPause = 0.4f;
    public float attackGiveUpTimeout = 5f;

    [Header("Reposition")]
    public float repositionDistance = 5f;
    public float repositionDuration = 2f;

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

    [HideInInspector] public GameObject enemyPrefab;
    [HideInInspector] public GameObject cloneSpawnerPrefab;
    [HideInInspector] public int generation = 0;
    [HideInInspector] public int maxGeneration = 3;
    [HideInInspector] public float externalKnockbackForce;

    public System.Action OnDeath;
    public bool isOriginal = false;

    private Phase currentPhase = Phase.OrbitFar;

    private float currentHealth;
    private Transform player;
    private PlayerHealth playerHealth;
    private float nextAttack = 0f;

    private bool isKnockedBack = false;
    private float knockbackEndTime;
    private Vector3 knockbackDirection;
    private float storedKnockbackForce;

    private bool isStunned = false;
    private float stunEndTime;

    private CharacterController controller;
    private Vector3 moveDir;
    private float currentSpeed;

    private Vector3 previousPosition;
    private float stuckCount = 0f;

    private Transform modelTransform;
    private Vector3 modelOriginalLocalPosition;

    private int normalLayer;
    private int knockbackLayer;

    private float phaseTimer = 0f;
    private float approachCheckTimer = 0f;
    private Vector3 repositionDir;
    private bool hitLanded = false;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Awake()
    {
        EnemyCombatManager.Register(this);
    }

    void Start()
    {
        currentHealth = maxHealth;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null)
            animator.applyRootMotion = false;

        PlayerHealth healthComponent = Object.FindAnyObjectByType<PlayerHealth>();
        if (healthComponent != null)
        {
            player = healthComponent.transform;
            playerHealth = healthComponent;
        }

        controller = GetComponent<CharacterController>();
        currentSpeed = Random.Range(speed * 0.8f, speed * 1.2f);
        previousPosition = transform.position;

        normalLayer = gameObject.layer;
        knockbackLayer = LayerMask.NameToLayer("EnemyKnockback");

        modelTransform = transform.childCount > 0 ? transform.GetChild(0) : transform;
        modelOriginalLocalPosition = modelTransform.localPosition;

        orbitAngle = Random.Range(0f, 360f);

        SetPhase(Phase.Spawning);
        if (animator != null)
            animator.SetTrigger(spawnTrigger);
    }

    void OnDestroy()
    {
        EnemyCombatManager.Unregister(this);
    }

    void Update()
    {
        if (isOriginal || player == null) return;

        if (isStunned)
        {
            HandleStun();
            return;
        }

        UpdatePhase();
    }

    void FixedUpdate()
    {
        if (isOriginal || player == null) return;
        if (isStunned) return;

        HandleMovement();
    }




    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    void UpdatePhase()
    {
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentPhase)
        {
            case Phase.Spawning:
                phaseTimer += Time.deltaTime;

                bool animationFinished = false;
                if (animator != null)
                {
                    AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
                    if (state.IsName("Spawn") && state.normalizedTime >= 0.95f)
                        animationFinished = true;
                    else if (!state.IsName("Spawn") && phaseTimer > 0.5f)
                        animationFinished = true;
                }
                else if (phaseTimer >= spawnDuration)
                {
                    animationFinished = true;
                }

                if (animationFinished)
                {
                    SetPhase(Phase.OrbitFar);
                }
                break;

            case Phase.OrbitFar:

                orbitAngle += orbitSpeed * 0.5f * Time.deltaTime;

                approachCheckTimer += Time.deltaTime;
                if (approachCheckTimer >= 1f)
                {
                    approachCheckTimer = 0f;
                    if (CountCommittedEnemies() < maxCloseEnemies
                        && Random.value < approachChancePerSecond)
                    {
                        SetPhase(Phase.ApproachClose);
                    }
                }
                break;

            case Phase.ApproachClose:

                orbitAngle += orbitSpeed * Time.deltaTime;
                phaseTimer += Time.deltaTime;

                if (distToPlayer <= closeOrbitRadius + 0.3f)
                {
                    SetPhase(Phase.ReadyToAttack);
                    break;
                }

                if (phaseTimer >= readyToAttackTimeout)
                {
                    orbitAngle += Random.Range(60f, 120f);
                    SetPhase(Phase.OrbitFar);
                }
                break;

            case Phase.ReadyToAttack:

                phaseTimer += Time.deltaTime;

                if (Time.time >= nextAttack && EnemyCombatManager.RequestAttackToken(this))
                {
                    SetPhase(Phase.Attacking);
                    break;
                }

                if (phaseTimer >= readyToAttackTimeout)
                {
                    orbitAngle += Random.Range(60f, 120f);
                    SetPhase(Phase.OrbitFar);
                }
                break;

            case Phase.Attacking:

                phaseTimer += Time.deltaTime;

                if (phaseTimer >= attackGiveUpTimeout)
                {
                    EnemyCombatManager.ReleaseToken(this);
                    nextAttack = Time.time + attackInterval;
                    orbitAngle += Random.Range(120f, 240f);
                    SetPhase(Phase.OrbitFar);
                    break;
                }

                if (distToPlayer <= attackRange && !hitLanded)
                {
                    if (animator != null) animator.SetTrigger(attackTrigger);
                    playerHealth?.TakeDamage(attackDamage, transform.position);
                    nextAttack = Time.time + attackInterval;
                    hitLanded = true;
                }

                if (phaseTimer >= attackLungeDuration + attackFinishPause && hitLanded)
                {
                    EnemyCombatManager.ReleaseToken(this);
                    SetPhase(Phase.Reposition);
                }
                break;

            case Phase.Reposition:

                phaseTimer += Time.deltaTime;

                if (phaseTimer >= repositionDuration)
                {
                    orbitAngle += Random.Range(90f, 180f);
                    SetPhase(Phase.OrbitFar);
                }
                break;
        }
    }

    void SetPhase(Phase next)
    {
        phaseTimer = 0f;

        if (next == Phase.Attacking)
            hitLanded = false;

        if (next == Phase.Reposition)
        {
            repositionDir = (transform.position - player.position).normalized;
            repositionDir.y = 0;
        }

        currentPhase = next;
    }

    void HandleMovement()
    {
        if (isKnockedBack)
        {
            ApplyKnockbackMove();
            return;
        }

        if (currentPhase == Phase.Spawning)
        {
            moveDir.x = 0;
            moveDir.z = 0;
            ApplyGravity();
            controller.Move(moveDir * Time.fixedDeltaTime);
            return;
        }

        Vector3 targetPos = GetTargetPosition();
        Vector3 toTarget = targetPos - transform.position;
        toTarget.y = 0;

        float speedMult = currentPhase == Phase.Attacking ? 2.2f
                        : currentPhase == Phase.ApproachClose ? 1.2f
                        : currentPhase == Phase.Reposition ? 1.0f
                        : 0.7f;

        if (toTarget.magnitude > 0.15f)
        {
            moveDir.x = toTarget.normalized.x * currentSpeed * speedMult;
            moveDir.z = toTarget.normalized.z * currentSpeed * speedMult;
        }
        else
        {
            moveDir.x = 0;
            moveDir.z = 0;
        }

        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(lookDir);

        if (animator != null)
        {
            Vector3 flatMoveDir = new Vector3(moveDir.x, 0, moveDir.z);
            Vector3 localMove = transform.InverseTransformDirection(flatMoveDir);

            animator.SetFloat(moveXFloat, localMove.x / speed);
            animator.SetFloat(moveZFloat, localMove.z / speed);
        }

        ApplyGravity();
        controller.Move(moveDir * Time.fixedDeltaTime);
    }

    Vector3 GetTargetPosition()
    {
        float rad;
        Vector3 toMe = transform.position - player.position;
        float currentAngle = Mathf.Atan2(toMe.z, toMe.x) * Mathf.Rad2Deg;
        float angleDiff = Mathf.DeltaAngle(currentAngle, orbitAngle);
        float clampedAngle = currentAngle + Mathf.Clamp(angleDiff, -15f, 15f);

        switch (currentPhase)
        {
            case Phase.OrbitFar:
            case Phase.ReadyToAttack:
                rad = clampedAngle * Mathf.Deg2Rad;
                return player.position + new Vector3(
                    Mathf.Cos(rad) * farOrbitRadius, 0,
                    Mathf.Sin(rad) * farOrbitRadius);

            case Phase.ApproachClose:
                float t = Mathf.Clamp01(phaseTimer / readyToAttackTimeout);
                float radius = Mathf.Lerp(farOrbitRadius, closeOrbitRadius, t);
                rad = clampedAngle * Mathf.Deg2Rad;
                return player.position + new Vector3(
                    Mathf.Cos(rad) * radius, 0,
                    Mathf.Sin(rad) * radius);

            case Phase.Attacking:
                return player.position;

            case Phase.Reposition:
                return transform.position + repositionDir * repositionDistance;

            default:
                return transform.position;
        }
    }

    void ApplyKnockbackMove()
    {
        moveDir.x = knockbackDirection.x * storedKnockbackForce;
        moveDir.z = knockbackDirection.z * storedKnockbackForce;

        if (Time.time >= knockbackEndTime)
        {
            isKnockedBack = false;
            if (stunEndTime > Time.time)
                isStunned = true;
        }

        ApplyGravity();
        controller.Move(moveDir * Time.fixedDeltaTime);
    }

    void ApplyGravity()
    {
        if (controller.isGrounded)
            moveDir.y = -2f;
        else
            moveDir.y += Physics.gravity.y * 2f * Time.fixedDeltaTime;
    }

    void CheckStuck()
    {
        if (Vector3.Distance(previousPosition, transform.position) < 0.01f)
        {
            stuckCount++;
            if (stuckCount >= stuckCountMax)
            {
                stuckCount = 0f;
                Vector3 randomDir = new Vector3(
                    Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
                controller.Move(randomDir * currentSpeed * Time.fixedDeltaTime * 5f);
            }
        }
        else
        {
            stuckCount = 0f;
        }
        previousPosition = transform.position;
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





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void TakeDamage(float damage, float knockback = 0f, float stunDuration = 0f)
    {
        currentHealth -= damage;

        if (player != null)
        {
            gameObject.layer = knockbackLayer;
            knockbackDirection = (transform.position - player.position).normalized;
            knockbackDirection.y = 0;
            storedKnockbackForce = knockback > 0f ? knockback : knockbackForce;
            isKnockedBack = true;
            knockbackEndTime = Time.time + knockbackDuration;

            if (stunDuration > 0f)
                stunEndTime = Time.time + knockbackDuration + stunDuration;

            if (currentPhase == Phase.Attacking)
            {
                EnemyCombatManager.ReleaseToken(this);
                SetPhase(Phase.Reposition);
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
                spawner.Initialize(enemyPrefab, cloneSpawnerPrefab, generation + 1,
                                   maxGeneration, () => OnDeath?.Invoke());
        }

        OnDeath?.Invoke();
        Destroy(gameObject);
    }

    int CountCommittedEnemies()
    {
        int count = 0;
        Collider[] hits = Physics.OverlapSphere(player.position, farOrbitRadius + 1f);
        foreach (var h in hits)
        {
            if (h.gameObject == gameObject) continue;
            EnemyAI other = h.GetComponent<EnemyAI>();
            if (other != null &&
                (other.currentPhase == Phase.ApproachClose ||
                 other.currentPhase == Phase.ReadyToAttack ||
                 other.currentPhase == Phase.Attacking))
                count++;
        }
        return count;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (player != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(player.position, farOrbitRadius);
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(player.position, closeOrbitRadius);
        }
    }
}
