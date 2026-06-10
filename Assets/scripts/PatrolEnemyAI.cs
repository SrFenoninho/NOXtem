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
    public int lookAroundCount = 4;           // Nº de direções a olhar em cada fase
    public float lookRotateSpeed = 100f;      // Velocidade de rotação (graus/segundo)
    public float lookHoldDuration = 1.5f;     // Tempo parado a olhar para cada direção
    public float searchMoveDistance = 4f;     // Distância a avançar entre as fases de busca

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

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private NavMeshAgent agent;
    private AudioSource chaseMusicSource;
    private bool chaseMusicPlaying = false;
    private int currentWaypointIndex = -1;
    private float nextAttackTime = 0f;

    private Transform player;
    private PlayerHealth playerHealth;
    private State currentState;

    // Último local onde viu o jogador
    private Vector3 lastPlayerPosition;

    // ---------------------------------------------
    //  SUB-ESTADOS DE BUSCA
    // ---------------------------------------------
    private enum SearchSubState
    {
        GoingToLastPosition,  // A caminho do último local onde viu o jogador
        LookingAround,        // Parado a olhar para os lados
        MovingForward,        // A avançar um pouco antes de olhar outra vez
    }
    private SearchSubState searchSubState;

    // Controlo da rotação de busca
    private float targetLookAngle;     // Ângulo Y alvo para a rotação atual
    private float lookHoldTimer;       // Tempo restante a olhar nessa direção
    private int looksRemaining;        // Olhares que faltam nesta fase
    private int searchPhase;           // 0 = 1ª fase de olhar | 1 = avançou, 2ª fase

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
        switch (searchSubState)
        {
            // --------------------------------------------------
            // FASE 0: Vai até ao último local onde viu o jogador
            // --------------------------------------------------
            case SearchSubState.GoingToLastPosition:
                agent.speed = patrolSpeed * 1.2f;
                agent.isStopped = false;

                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                {
                    // Chegou — para e começa a olhar para os lados (fase 1)
                    EnterLookAround(phase: 0);
                }
                break;

            // --------------------------------------------------
            // FASE 1 / 3: Olha para os lados aleatoriamente
            // --------------------------------------------------
            case SearchSubState.LookingAround:
                agent.isStopped = true;

                // Roda suavemente em direção ao ângulo alvo
                float currentY = transform.eulerAngles.y;
                float newY = Mathf.MoveTowardsAngle(currentY, targetLookAngle, lookRotateSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0f, newY, 0f);

                // Chegou à direção alvo?
                if (Mathf.Abs(Mathf.DeltaAngle(newY, targetLookAngle)) < 2f)
                {
                    lookHoldTimer -= Time.deltaTime;

                    if (lookHoldTimer <= 0f)
                    {
                        looksRemaining--;

                        if (looksRemaining > 0)
                        {
                            // Escolhe mais uma direção aleatória
                            PickNextLookAngle();
                        }
                        else if (searchPhase == 0)
                        {
                            // Terminou 1ª fase de olhar → avança um pouco
                            searchPhase = 1;
                            TryMoveForward();
                        }
                        else
                        {
                            // Terminou 2ª fase de olhar sem ver nada → volta à patrulha
                            agent.isStopped = false;
                            currentState = State.Patrolling;
                            PickRandomWaypoint();
                        }
                    }
                }
                break;

            // --------------------------------------------------
            // FASE 2: Avança um pouco antes de olhar outra vez
            // --------------------------------------------------
            case SearchSubState.MovingForward:
                agent.speed = patrolSpeed;
                agent.isStopped = false;

                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                {
                    // Chegou ao ponto intermédio → olha outra vez (fase 2)
                    EnterLookAround(phase: 1);
                }
                break;
        }
    }

    // Inicia uma fase de "olhar para os lados"
    void EnterLookAround(int phase)
    {
        searchPhase = phase;
        looksRemaining = lookAroundCount;
        searchSubState = SearchSubState.LookingAround;
        PickNextLookAngle();
    }

    // Escolhe um ângulo Y aleatório (evita repetir o atual)
    void PickNextLookAngle()
    {
        float newAngle;
        do { newAngle = Random.Range(0f, 360f); }
        while (Mathf.Abs(Mathf.DeltaAngle(newAngle, targetLookAngle)) < 30f);

        targetLookAngle = newAngle;
        lookHoldTimer = lookHoldDuration;
    }

    // Tenta avançar um pouco na direção em que está a olhar
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
            // Sem NavMesh à frente — salta direto para a 2ª fase de olhar
            EnterLookAround(phase: 1);
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