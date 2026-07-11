using UnityEngine;
using UnityEngine.AI;

public class BossMovement : MonoBehaviour
{
    public float walkSpeed = 3.5f;
    public float runSpeed = 7f;
    public float chargeSpeed = 16f;

    public NavMeshAgent agent { get; private set; }
    private BossController boss;

    public System.Collections.Generic.List<Vector3> pontosTaticos = new System.Collections.Generic.List<Vector3>();

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

    private void GerarPontosTaticos()
    {
        pontosTaticos.Clear();
        Vector3 centro = transform.position;
        int tentativas = 450;

        // Tenta encontrar a CombatZone incluindo objetos inativos na Scene (evita falhas de loading)
        CombatZone combatZoneObj = FindFirstObjectByType<CombatZone>();
        if (combatZoneObj == null)
        {
            CombatZone[] todosOsCombatZones = Resources.FindObjectsOfTypeAll<CombatZone>();
            if (todosOsCombatZones != null && todosOsCombatZones.Length > 0)
            {
                // Garante que o objeto pertence à scene ativa (e não a assets do projeto)
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
            zoneCollider = combatZoneObj.GetComponent<Collider>();
        }

        for (int i = 0; i < tentativas; i++)
        {
            // Sorteia um ponto num raio de 25 metros
            Vector3 pontoAleatorio = centro + Random.insideUnitSphere * 25f;
            pontoAleatorio.y = centro.y;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(pontoAleatorio, out hit, 10f, NavMesh.AllAreas))
            {
                // Se a CombatZone existir na scene, valida a distância geométrica das bordas de forma infalível
                if (zoneCollider != null)
                {
                    // 1. O próprio ponto tem de estar dentro
                    Vector3 closestSelf = zoneCollider.ClosestPoint(hit.position);
                    if (Vector3.Distance(hit.position, closestSelf) > 0.08f)
                    {
                        continue; // Fora da CombatZone!
                    }

                    // 2. Garante que os limites a 1.7 metros em cruz também estão dentro do trigger da arena
                    // (Isto impede o Boss de escolher pontos perto de paredes, mesmo que a arena esteja rodada no espaço!)
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
                            break; // Se um dos testes saiu do collider, o ponto original está muito perto da parede
                        }
                    }

                    if (muitoPertoDaParede)
                    {
                        continue; // Descarta ponto colado à quina/parede
                    }
                }

                // Garante que o ponto está longe das paredes/bordas do NavMesh
                NavMeshHit edgeHit;
                if (NavMesh.FindClosestEdge(hit.position, out edgeHit, NavMesh.AllAreas))
                {
                    if (edgeHit.distance >= 1.8f) // Pelo menos 1.8 metros longe de qualquer parede
                    {
                        // Evita acumulação de pontos muito colados para cobrir melhor a sala
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

        if (zoneCollider != null)
            Debug.Log("🎯 [BossMovement] Grelha tática gerada: " + pontosTaticos.Count + " pontos seguros estritamente DENTRO da CombatZone (margem de 1.7m das quinas).");
        else
            Debug.Log("🎯 [BossMovement] Grelha tática gerada: " + pontosTaticos.Count + " pontos seguros (CombatZone não encontrada na Scene, usando NavMesh geral).");
    }

    public void MoveTo(Vector3 targetPosition, float speed)
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
        agent.isStopped = false;
        agent.speed = speed;
        agent.SetDestination(targetPosition);
        UpdateAnimatorSpeed();
    }

    // Versão definitiva e infalível usando o Raycast nativo do próprio NavMeshAgent do Unity
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
            
            // O Raycast do próprio NavMeshAgent projeta o caminho na malha e deteta onde colide com a parede da arena
            NavMeshHit hit;
            if (agent.Raycast(targetPoint, out hit))
            {
                // Bateu no limite da arena (parede)!
                float distToWall = hit.distance;
                
                // Se a parede da arena estiver a mais de 3.5 metros, move-se, mas pára a 1.7 metros de distância dela
                if (distToWall > 3.5f)
                {
                    Vector3 safeTarget = hit.position - testDir * 1.7f;
                    
                    // Garante que o ponto seguro final é válido no NavMesh
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
                // Se o limite da parede estiver demasiado perto no ângulo testado, tenta o próximo ângulo de fuga
                continue; 
            }
            else
            {
                // Caminho no NavMesh está totalmente livre de limites/paredes
                agent.isStopped = false;
                agent.speed = speed;
                agent.SetDestination(targetPoint);
                UpdateAnimatorSpeed();
                return;
            }
        }

        // Se todos os ângulos falharem (arena super encurralada), tenta afastar-se o máximo possível
        StopMovement();
    }

    private Vector3 ultimaPosicaoVerificada;
    private float stuckTimer = 0f;
    private float sampleInterval = 0.2f;
    private float sampleTimer = 0f;

    // Deteta se o Boss está a patinar ou preso contra uma parede/coluna (mede deslocamento real no espaço)
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
            
            // Calcula a distância que o Boss realmente se moveu nos últimos 0.2 segundos
            float distanciaMovida = Vector3.Distance(transform.position, ultimaPosicaoVerificada);
            ultimaPosicaoVerificada = transform.position;

            // Se o NavMesh quer correr (> 0.8f) mas o Boss moveu-se menos de 0.15 metros (está colado a patinar na parede)
            if (agent.desiredVelocity.magnitude > 0.8f && distanciaMovida < 0.15f && agent.remainingDistance > 1.0f)
            {
                stuckTimer += sampleInterval;
                if (stuckTimer >= 0.4f) // Preso de verdade por 0.4 segundos
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
        if(agent != null) agent.updateRotation = state;
    }

    public void LookAt(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        dir.y = 0;
        if(dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);
    }

    // Chamado manualmente no Update do controller para manter o animator sincronizado
    public void UpdateAnimatorSpeed()
    {
        if (boss.anim == null) return;
        if (agent != null && agent.enabled && !agent.isStopped)
            boss.anim.SetFloat("Speed", agent.velocity.magnitude);
        else
            boss.anim.SetFloat("Speed", 0f);
    }
}
