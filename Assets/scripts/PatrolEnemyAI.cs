using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PatrolEnemyAI : MonoBehaviour
{




    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private enum State
    {
        Patrolling,
        Chasing,
        Searching,
        Attacking
    }




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("Patrol Settings")]

    public Transform[] waypoints;
    public float patrolSpeed = 3f;

    [Header("Detection & FOV")]
    public float detectionRadius = 12f;
    [Range(0, 360)] public float viewAngle = 90f;
    public LayerMask obstacleMask;
    public float chaseSpeed = 6f;

    [Header("Search Settings")]
    public int lookAroundCount = 4;
    public float lookRotateSpeed = 100f;
    public float lookHoldDuration = 1.5f;
    public float searchMoveDistance = 4f;

    [Header("Musica de Perseguicao")]
    public MusicManager musicManager;
    public AudioClip chaseMusic;
    public float chaseMusicVolume = 0.6f;
    public float chaseFadeDuration = 1f;

    [Header("Attack Settings (One Shot Kill)")]
    public float attackDamage = 9999f;
    public float attackRange = 1.8f;

    [Header("Animation")]
    public Animator animator;
    public string speedParam = "Speed";


    private NavMeshAgent agent;
    private AudioSource chaseMusicSource;
    private bool chaseMusicPlaying = false;
    private int currentWaypointIndex = -1;
    private float nextAttackTime = 0f;

    private Transform player;
    private PlayerHealth playerHealth;
    private State currentState;

    private Vector3 lastPlayerPosition;

    private enum SearchSubState
    {
        GoingToLastPosition,
        LookingAround,
        MovingForward,
    }
    private SearchSubState searchSubState;

    private float targetLookAngle;
    private float lookHoldTimer;
    private int looksRemaining;
    private int searchPhase;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;

        if (animator == null) animator = GetComponentInChildren<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }

        chaseMusicSource = gameObject.AddComponent<AudioSource>();
        chaseMusicSource.loop = true;
        chaseMusicSource.volume = 0f;
        if (chaseMusic != null) chaseMusicSource.clip = chaseMusic;

        currentState = State.Patrolling;
        PickRandomWaypoint();
    }

    void Update()
    {
        if (player == null) return;

        HandleStateTransitions();
        UpdateStateBehavior();

        if (animator != null)
        {
            animator.SetFloat(speedParam, agent.velocity.magnitude);
        }
    }




    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    void HandleStateTransitions()
    {
        bool canSee = CanSeePlayer();
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (canSee)
        {
            lastPlayerPosition = player.position;

            if (distToPlayer <= attackRange)
            {
                if (currentState != State.Attacking)
                {
                    currentState = State.Attacking;
                    StartChaseMusic();
                }
            }
            else
            {
                if (currentState != State.Chasing)
                {
                    currentState = State.Chasing;
                    StartChaseMusic();
                }
            }
        }
        else
        {
            if (currentState == State.Chasing || currentState == State.Attacking)
            {
                currentState = State.Searching;
                searchSubState = SearchSubState.GoingToLastPosition;
                agent.isStopped = false;
                agent.SetDestination(lastPlayerPosition);
                StopChaseMusic();
            }
        }
    }

    void UpdateStateBehavior()
    {
        switch (currentState)
        {
            case State.Patrolling: PatrolBehavior(); break;
            case State.Chasing: ChaseBehavior(); break;
            case State.Searching: SearchBehavior(); break;
            case State.Attacking: AttackBehavior(); break;
        }
    }

    void PatrolBehavior()
    {
        agent.speed = patrolSpeed;
        agent.isStopped = false;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            PickRandomWaypoint();
        }
    }

    void ChaseBehavior()
    {
        agent.speed = chaseSpeed;
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    void SearchBehavior()
    {
        switch (searchSubState)
        {

            case SearchSubState.GoingToLastPosition:
                agent.speed = patrolSpeed * 1.2f;
                agent.isStopped = false;

                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                {
                    EnterLookAround(phase: 0);
                }
                break;

            case SearchSubState.LookingAround:
                agent.isStopped = true;

                float currentY = transform.eulerAngles.y;
                float newY = Mathf.MoveTowardsAngle(currentY, targetLookAngle, lookRotateSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0f, newY, 0f);

                if (Mathf.Abs(Mathf.DeltaAngle(newY, targetLookAngle)) < 2f)
                {
                    lookHoldTimer -= Time.deltaTime;

                    if (lookHoldTimer <= 0f)
                    {
                        looksRemaining--;

                        if (looksRemaining > 0)
                        {
                            PickNextLookAngle();
                        }
                        else if (searchPhase == 0)
                        {
                            searchPhase = 1;
                            TryMoveForward();
                        }
                        else
                        {
                            agent.isStopped = false;
                            currentState = State.Patrolling;
                            PickRandomWaypoint();
                        }
                    }
                }
                break;

            case SearchSubState.MovingForward:
                agent.speed = patrolSpeed;
                agent.isStopped = false;

                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                {
                    EnterLookAround(phase: 1);
                }
                break;
        }
    }

    void EnterLookAround(int phase)
    {
        searchPhase = phase;
        looksRemaining = lookAroundCount;
        searchSubState = SearchSubState.LookingAround;
        PickNextLookAngle();
    }

    void PickNextLookAngle()
    {
        float newAngle;
        do { newAngle = Random.Range(0f, 360f); }
        while (Mathf.Abs(Mathf.DeltaAngle(newAngle, targetLookAngle)) < 30f);

        targetLookAngle = newAngle;
        lookHoldTimer = lookHoldDuration;
    }

    void TryMoveForward()
    {
        Vector3 forwardPos = transform.position + transform.forward * searchMoveDistance;
        NavMeshHit hit;

        if (NavMesh.SamplePosition(forwardPos, out hit, searchMoveDistance, NavMesh.AllAreas))
        {
            searchSubState = SearchSubState.MovingForward;
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }
        else
        {
            EnterLookAround(phase: 1);
        }
    }

    void AttackBehavior()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);

        if (Time.time >= nextAttackTime)
        {
            playerHealth?.TakeDamage(attackDamage, transform.position);
            nextAttackTime = Time.time + 1f;
        }
    }

    void PickRandomWaypoint()
    {
        if (waypoints.Length == 0) return;

        if (waypoints.Length > 1)
        {
            int nextIndex = currentWaypointIndex;
            while (nextIndex == currentWaypointIndex)
            {
                nextIndex = Random.Range(0, waypoints.Length);
            }
            currentWaypointIndex = nextIndex;
        }
        else
        {
            currentWaypointIndex = 0;
        }

        agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    bool CanSeePlayer()
    {
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        if (distToPlayer > detectionRadius) return false;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);

        if (angleToPlayer < viewAngle / 2f)
        {
            Vector3 startPos = transform.position + Vector3.up * 1.5f;
            Vector3 targetPos = player.position + Vector3.up * 1.5f;

            if (!Physics.Linecast(startPos, targetPos, obstacleMask))
            {
                return true;
            }
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.green;
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;
        Gizmos.DrawRay(transform.position + Vector3.up, leftBoundary * detectionRadius);
        Gizmos.DrawRay(transform.position + Vector3.up, rightBoundary * detectionRadius);
    }

    void StartChaseMusic()
    {
        if (chaseMusicPlaying || chaseMusic == null) return;
        chaseMusicPlaying = true;

        if (musicManager != null) musicManager.PauseMusic();

        chaseMusicSource.clip = chaseMusic;
        chaseMusicSource.volume = 0f;
        chaseMusicSource.Play();
        StartCoroutine(FadeChaseMusic(0f, chaseMusicVolume, chaseFadeDuration));
    }

    void StopChaseMusic()
    {
        if (!chaseMusicPlaying) return;
        chaseMusicPlaying = false;
        StartCoroutine(FadeOutAndResume());
    }

    System.Collections.IEnumerator FadeChaseMusic(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            chaseMusicSource.volume = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        chaseMusicSource.volume = to;
    }

    System.Collections.IEnumerator FadeOutAndResume()
    {
        yield return StartCoroutine(FadeChaseMusic(chaseMusicSource.volume, 0f, chaseFadeDuration));
        chaseMusicSource.Stop();
        if (musicManager != null) musicManager.ResumeMusic();
    }
}
