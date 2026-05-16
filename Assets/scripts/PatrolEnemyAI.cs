using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PatrolEnemyAI : MonoBehaviour
{
    // ---------------------------------------------
    //  FASES DA IA
    // ---------------------------------------------
    private enum State
    {
        Patrolling,
        Chasing,
        Searching,
        Attacking
    }

    // ---------------------------------------------
    //  INSPETOR
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
    public float searchDuration = 10f;
    public float searchPointRadius = 10f;

    [Header("Musica de Perseguicao")]
    public MusicManager musicManager;
    public AudioClip chaseMusic;
    public float chaseMusicVolume = 0.6f;
    public float chaseFadeDuration = 1f;

    [Header("Attack Settings (One Shot Kill)")]
    public float attackDamage = 9999f;
    public float attackRange = 1.8f;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private NavMeshAgent agent;
    private AudioSource chaseMusicSource;
    private bool chaseMusicPlaying = false;
    private int currentWaypointIndex = -1;
    private float searchTimer = 0f;
    private float nextSearchMoveTimer = 0f;
    private float nextAttackTime = 0f;

    private Transform player;
    private PlayerHealth playerHealth;
    private State currentState;
    
    // Último local onde viu o jogador
    private Vector3 lastPlayerPosition;
    private bool hasReachedLastPlayerPosition = false;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;

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
    }

    // ---------------------------------------------
    //  MAQUINA DE ESTADOS
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
                if (currentState != State.Attacking)  // ← SÓ MUDA SE NÃO JÁ ESTÁ
                {
                    currentState = State.Attacking;
                    StartChaseMusic();
                }
            }
            else
            {
                if (currentState != State.Chasing)  // ← SÓ MUDA SE NÃO JÁ ESTÁ
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
                searchTimer = searchDuration;
                nextSearchMoveTimer = 0f;
                hasReachedLastPlayerPosition = false;
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

    // ---------------------------------------------
    //  COMPORTAMENTOS
    // ---------------------------------------------
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
        agent.speed = patrolSpeed * 1.2f;
        agent.isStopped = false;

        searchTimer -= Time.deltaTime;

        // Se ainda não chegou ao último local do jogador
        if (!hasReachedLastPlayerPosition)
        {
            // Vai para o último local onde viu o jogador
            agent.SetDestination(lastPlayerPosition);

            // Verifica se chegou ao local
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                hasReachedLastPlayerPosition = true;
                nextSearchMoveTimer = 0f;
            }
        }
        else
        {
            // Se já chegou ao último local, começa a procurar aleatoriamente
            nextSearchMoveTimer -= Time.deltaTime;
            if (nextSearchMoveTimer <= 0f || (agent.remainingDistance < 0.5f && !agent.pathPending))
            {
                Vector3 randomSearchPoint = transform.position + Random.insideUnitSphere * searchPointRadius;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomSearchPoint, out hit, searchPointRadius, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
                nextSearchMoveTimer = 3f;
            }
        }

        // Se o tempo de procura acabou, volta a patrulhar
        if (searchTimer <= 0f)
        {
            currentState = State.Patrolling;
            PickRandomWaypoint();
            return;
        }
    }

    void AttackBehavior()
    {
        // No One Shot Kill, ele investe até ao fim
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

    // ---------------------------------------------
    //  LOGICA DE VISAO (FOV)
    // ---------------------------------------------
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

    // ---------------------------------------------
    //  DESENHOS DE DEBUGACAO
    // ---------------------------------------------
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Cone de Visão
        Gizmos.color = Color.green;
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;
        Gizmos.DrawRay(transform.position + Vector3.up, leftBoundary * detectionRadius);
        Gizmos.DrawRay(transform.position + Vector3.up, rightBoundary * detectionRadius);
    }

    // ---------------------------------------------
    //  MUSICA DE PERSEGUICAO
    // ---------------------------------------------
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