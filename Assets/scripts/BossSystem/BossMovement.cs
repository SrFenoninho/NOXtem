using UnityEngine;
using UnityEngine.AI;

public class BossMovement : MonoBehaviour
{



    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    public float walkSpeed = 3.5f;
    public float runSpeed = 7f;
    public float chargeSpeed = 16f;


    public NavMeshAgent agent { get; private set; }




    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private BossController boss;


    public System.Collections.Generic.List<Vector3> pontosTaticos = new System.Collections.Generic.List<Vector3>();
    private bool controleManualDeRotacao = true;




    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void Initialize(BossController controller)
    {
        boss = controller;
        agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.speed = walkSpeed;
            agent.acceleration = 30f;
            agent.stoppingDistance = 0.5f;
            agent.autoBraking = true;
            agent.updateRotation = false;

            if (!agent.isOnNavMesh)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position, out hit, 15.0f, NavMesh.AllAreas))
                {
                    transform.position = hit.position;
                    agent.Warp(hit.position);
                }
            }
        }

        GerarPontosTaticos();
    }





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Update()
    {
        if (agent != null && agent.enabled && !agent.isStopped && controleManualDeRotacao)
        {
            if (agent.velocity.sqrMagnitude > 0.15f)
            {
                Vector3 moveDir = agent.velocity.normalized;
                moveDir.y = 0;
                if (moveDir != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(moveDir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 7.5f * Time.deltaTime);
                }
            }

            EvitarParedesPreventivamente();
        }
    }





    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    private void EvitarParedesPreventivamente()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh || !agent.hasPath) return;

        Vector3 moveDir = agent.velocity.normalized;
        if (moveDir == Vector3.zero) moveDir = transform.forward;

        int obstacleMask = ~LayerMask.GetMask("Player", "Ignore Raycast");

        RaycastHit physHit;
        Vector3 rayStart = transform.position + Vector3.up * 1.0f + moveDir * 0.8f; 
        float alcanceDoRaio = 2.0f;

        if (Physics.Raycast(rayStart, moveDir, out physHit, alcanceDoRaio, obstacleMask))
        {
            if (physHit.collider.isTrigger) 
            {
                Debug.DrawLine(rayStart, rayStart + moveDir * alcanceDoRaio, Color.green);
                return;
            }

            Debug.DrawLine(rayStart, physHit.point, Color.red);

            Vector3 wallNormal = physHit.normal;
            wallNormal.y = 0;

            Debug.DrawRay(physHit.point, wallNormal * 2.0f, Color.blue);

            Vector3 steeringAvoid = (moveDir + wallNormal * 1.6f).normalized;
            Vector3 targetAvoidPoint = transform.position + steeringAvoid * 3.5f;

            Debug.DrawLine(transform.position, targetAvoidPoint, Color.yellow);

            NavMeshHit navHit;
            if (NavMesh.SamplePosition(targetAvoidPoint, out navHit, 4f, NavMesh.AllAreas))
            {
                agent.SetDestination(navHit.position);
            }
        }
        else
        {
            Debug.DrawLine(rayStart, rayStart + moveDir * alcanceDoRaio, Color.green);
        }
    }

    private void GerarPontosTaticos()
    {
        pontosTaticos.Clear();
        Vector3 centro = transform.position;
        int tentativas = 450;

        CombatZone combatZoneObj = FindFirstObjectByType<CombatZone>();
        if (combatZoneObj == null)
        {
            CombatZone[] todosOsCombatZones = Resources.FindObjectsOfTypeAll<CombatZone>();
            if (todosOsCombatZones != null && todosOsCombatZones.Length > 0)
            {
                foreach (var cz in todosOsCombatZones)
                {
                    if (cz.gameObject.scene.isLoaded)
                    {
                        combatZoneObj = cz;
                        break;
                    }
                }
            }
        }

        Collider zoneCollider = null;
        if (combatZoneObj != null)
        {
            zoneCollider = combatZoneObj.GetComponentInChildren<Collider>();
        }

        for (int i = 0; i < tentativas; i++)
        {
            Vector3 pontoAleatorio = centro + Random.insideUnitSphere * 25f;
            pontoAleatorio.y = centro.y;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(pontoAleatorio, out hit, 10f, NavMesh.AllAreas))
            {
                bool pontoValidoNaZona = true;

                if (zoneCollider != null)
                {
                    try
                    {
                        Vector3 closestSelf = zoneCollider.ClosestPoint(hit.position);
                        if (Vector3.Distance(hit.position, closestSelf) > 0.08f)
                        {
                            pontoValidoNaZona = false;
                        }
                        else
                        {
                            bool muitoPertoDaParede = false;
                            Vector3[] testesDeBorda = {
                                hit.position + Vector3.forward * 1.7f,
                                hit.position + Vector3.back * 1.7f,
                                hit.position + Vector3.left * 1.7f,
                                hit.position + Vector3.right * 1.7f
                            };

                            foreach (Vector3 pTeste in testesDeBorda)
                            {
                                Vector3 closestT = zoneCollider.ClosestPoint(pTeste);
                                if (Vector3.Distance(pTeste, closestT) > 0.08f)
                                {
                                    muitoPertoDaParede = true;
                                    break;
                                }
                            }

                            if (muitoPertoDaParede)
                            {
                                pontoValidoNaZona = false;
                            }
                        }
                    }
                    catch (System.Exception)
                    {
                        zoneCollider = null;
                    }
                }

                if (!pontoValidoNaZona) continue;

                NavMeshHit edgeHit;
                if (NavMesh.FindClosestEdge(hit.position, out edgeHit, NavMesh.AllAreas))
                {
                    if (edgeHit.distance >= 1.8f) 
                    {
                        bool muitoProximo = false;
                        foreach (Vector3 p in pontosTaticos)
                        {
                            if (Vector3.Distance(p, hit.position) < 2.0f)
                            {
                                muitoProximo = true;
                                break;
                            }
                        }

                        if (!muitoProximo)
                        {
                            pontosTaticos.Add(hit.position);
                        }
                    }
                }
            }

            if (pontosTaticos.Count >= 70) break;
        }

        if (pontosTaticos.Count < 15)
        {
            pontosTaticos.Clear();
            for (int i = 0; i < 200; i++)
            {
                Vector3 pontoAleatorio = centro + Random.insideUnitSphere * 25f;
                pontoAleatorio.y = centro.y;

                NavMeshHit hit;
                if (NavMesh.SamplePosition(pontoAleatorio, out hit, 12f, NavMesh.AllAreas))
                {
                    NavMeshHit edgeHit;
                    if (NavMesh.FindClosestEdge(hit.position, out edgeHit, NavMesh.AllAreas))
                    {
                        if (edgeHit.distance >= 1.7f)
                        {
                            bool muitoProximo = false;
                            foreach (Vector3 p in pontosTaticos)
                            {
                                if (Vector3.Distance(p, hit.position) < 2.0f)
                                {
                                    muitoProximo = true;
                                    break;
                                }
                            }
                            if (!muitoProximo) pontosTaticos.Add(hit.position);
                        }
                    }
                }
                if (pontosTaticos.Count >= 50) break;
            }
        }

    }

    public void MoveTo(Vector3 targetPosition, float speed)
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
        agent.isStopped = false;
        agent.speed = speed;
        agent.SetDestination(targetPosition);
        UpdateAnimatorSpeed();
    }

    public void FleeFrom(Vector3 threatPosition, float speed, float distance)
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;

        Vector3 dirAway = (transform.position - threatPosition).normalized;
        dirAway.y = 0;

        float[] angles = { 0f, 30f, -30f, 60f, -60f, 90f, -90f, 180f };
        foreach (float angle in angles)
        {
            Vector3 testDir = Quaternion.Euler(0, angle, 0) * dirAway;
            Vector3 targetPoint = transform.position + testDir * distance;

            NavMeshHit hit;
            if (agent.Raycast(targetPoint, out hit))
            {
                float distToWall = hit.distance;

                if (distToWall > 3.5f)
                {
                    Vector3 safeTarget = hit.position - testDir * 1.7f;

                    NavMeshHit sampleHit;
                    if (NavMesh.SamplePosition(safeTarget, out sampleHit, 3f, NavMesh.AllAreas))
                    {
                        agent.isStopped = false;
                        agent.speed = speed;
                        agent.SetDestination(sampleHit.position);
                        UpdateAnimatorSpeed();
                        return;
                    }
                }
                continue; 
            }
            else
            {
                agent.isStopped = false;
                agent.speed = speed;
                agent.SetDestination(targetPoint);
                UpdateAnimatorSpeed();
                return;
            }
        }

        StopMovement();
    }

    private Vector3 ultimaPosicaoVerificada;
    private float stuckTimer = 0f;
    private float sampleInterval = 0.2f;
    private float sampleTimer = 0f;

    public bool VerificarSeEstaPreso()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh || agent.isStopped || !agent.hasPath)
        {
            stuckTimer = 0f;
            sampleTimer = 0f;
            return false;
        }

        sampleTimer += Time.deltaTime;
        if (sampleTimer >= sampleInterval)
        {
            sampleTimer = 0f;

            float distanciaMovida = Vector3.Distance(transform.position, ultimaPosicaoVerificada);
            ultimaPosicaoVerificada = transform.position;

            if (agent.desiredVelocity.magnitude > 0.8f && distanciaMovida < 0.15f && agent.remainingDistance > 1.0f)
            {
                stuckTimer += sampleInterval;
                if (stuckTimer >= 0.4f)
                {
                    stuckTimer = 0f;
                    return true;
                }
            }
            else
            {
                stuckTimer = 0f;
            }
        }
        return false;
    }

    public void StopMovement()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh) 
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            if (agent.hasPath) agent.ResetPath(); 
        }
        if (boss.anim != null) boss.anim.SetFloat("Speed", 0f);
    }

    public void SetRotationUpdate(bool state)
    {
        if (agent != null) agent.updateRotation = state;
        controleManualDeRotacao = state;
    }

    public void LookAt(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        dir.y = 0;
        if(dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);
    }

    public void UpdateAnimatorSpeed()
    {
        if (boss.anim == null) return;
        if (agent != null && agent.enabled && !agent.isStopped)
            boss.anim.SetFloat("Speed", agent.velocity.magnitude);
        else
            boss.anim.SetFloat("Speed", 0f);
    }
}
