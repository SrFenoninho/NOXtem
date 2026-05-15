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
    public float searchDuration = 10f; // Quanto tempo ele procura antes de desistir
    public float searchPointRadius = 10f; // Raio da área de busca aleatória

    [Header("Attack Settings (One Shot Kill)")]
    public float attackDamage = 9999f;
    public float attackRange = 1.8f;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private NavMeshAgent agent;
    private int currentWaypointIndex = -1;
    private float searchTimer = 0f;
    private float nextSearchMoveTimer = 0f;
    private float nextAttackTime = 0f;
    
    private Transform player;
    private PlayerHealth playerHealth;
    private State currentState;

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

        // Inicia sempre em movimento
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
    //  MaQUINA DE ESTADOS
    // ---------------------------------------------
    void HandleStateTransitions()
    {
        bool canSee = CanSeePlayer();
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (canSee)
        {
            if (distToPlayer <= attackRange)
                currentState = State.Attacking;
            else
                currentState = State.Chasing;
        }
        else
        {
            // Se perdeu o jogador enquanto perseguia ou atacava -> Começa a procurar
            if (currentState == State.Chasing || currentState == State.Attacking)
            {
                currentState = State.Searching;
                searchTimer = searchDuration;
                nextSearchMoveTimer = 0f; // Força movimento imediato na busca
            }
        }
    }

    void UpdateStateBehavior()
    {
        switch (currentState)
        {
            case State.Patrolling: PatrolBehavior(); break;
            case State.Chasing:    ChaseBehavior();  break;
            case State.Searching:  SearchBehavior(); break;
            case State.Attacking:  AttackBehavior(); break;
        }
    }

    // ---------------------------------------------
    //  COMPORTAMENTOS
    // ---------------------------------------------
    void PatrolBehavior()
    {
        agent.speed = patrolSpeed;
        agent.isStopped = false;

        // Quando chega a um ponto, escolhe logo o próximo sem parar
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
        agent.speed = patrolSpeed * 1.2f; // Procura um pouco mais rápido que a patrulha
        agent.isStopped = false;

        searchTimer -= Time.deltaTime;

        // Desiste da busca após 10 segundos
        if (searchTimer <= 0f)
        {
            currentState = State.Patrolling;
            PickRandomWaypoint();
            return;
        }

        // Move-se aleatoriamente pela sala enquanto procura
        nextSearchMoveTimer -= Time.deltaTime;
        if (nextSearchMoveTimer <= 0f || (agent.remainingDistance < 0.5f && !agent.pathPending))
        {
            Vector3 randomSearchPoint = transform.position + Random.insideUnitSphere * searchPointRadius;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomSearchPoint, out hit, searchPointRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            nextSearchMoveTimer = 3f; // Tenta um novo ponto a cada 3 segundos ou ao chegar
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
    //  LOGICA DE VISaO (FOV)
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
    //  DESENHOS DE DEPURAcaO
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
}
