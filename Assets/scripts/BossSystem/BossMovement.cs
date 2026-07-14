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
    private bool controleManualDeRotacao = true;

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
            agent.updateRotation = false; // Desativa a rotação brusca automática do Unity

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

    void Update()
    {
        // Rotação suave manual do Boss em curvas
        if (agent != null && agent.enabled && !agent.isStopped && controleManualDeRotacao)
        {
            if (agent.velocity.sqrMagnitude > 0.15f)
            {
                Vector3 moveDir = agent.velocity.normalized;
                moveDir.y = 0;
                if (moveDir != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(moveDir);
                    // Rotação suave com interpolação de 7.5f para curvas orgânicas
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 7.5f * Time.deltaTime);
                }
            }

            // Wall avoidance preventivo: desvia a rota antes de colidir fisicamente com a parede
            EvitarParedesPreventivamente();
        }
    }

    private void EvitarParedesPreventivamente()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh || !agent.hasPath) return;

        Vector3 moveDir = agent.velocity.normalized;
        if (moveDir == Vector3.zero) moveDir = transform.forward;

        // Máscara que colide com todos os obstáculos físicos sólidos do mapa (exclui o Player e IgnoreRaycast)
        int obstacleMask = ~LayerMask.GetMask("Player", "Ignore Raycast");

        RaycastHit physHit;
        // O raio nasce 0.8 metros à frente do peito do Boss para nunca colidir com o próprio corpo do Boss
        Vector3 rayStart = transform.position + Vector3.up * 1.0f + moveDir * 0.8f; 
        float alcanceDoRaio = 2.0f;

        // Lança o sensor preventivo a 2.0 metros à frente da sua trajetória de movimento
        if (Physics.Raycast(rayStart, moveDir, out physHit, alcanceDoRaio, obstacleMask))
        {
            // Ignora colisores do tipo Trigger (como a CombatZone ou partículas)
            if (physHit.collider.isTrigger) 
            {
                // Desenha linha verde fina de trigger ignorado
                Debug.DrawLine(rayStart, rayStart + moveDir * alcanceDoRaio, Color.green);
                return;
            }

            // Desenha linha VERMELHA até ao ponto exato do impacto (Obstáculo detetado!)
            Debug.DrawLine(rayStart, physHit.point, Color.red);

            // Obtém a normal da colisão (vetor que aponta para fora do obstáculo/pilar/parede)
            Vector3 wallNormal = physHit.normal;
            wallNormal.y = 0;

            // Desenha linha AZUL a sair da parede (mostra a força de empurrão perpendicular)
            Debug.DrawRay(physHit.point, wallNormal * 2.0f, Color.blue);

            // Cria um desvio lateral suave somando a normal perpendicular ao vetor de movimento
            Vector3 steeringAvoid = (moveDir + wallNormal * 1.6f).normalized;
            Vector3 targetAvoidPoint = transform.position + steeringAvoid * 3.5f;

            // Desenha linha AMARELA até ao destino de desvio que o Boss está a tentar tomar
            Debug.DrawLine(transform.position, targetAvoidPoint, Color.yellow);

            NavMeshHit navHit;
            // Valida o ponto de desvio no NavMesh e redireciona o agente preventivamente
            if (NavMesh.SamplePosition(targetAvoidPoint, out navHit, 4f, NavMesh.AllAreas))
            {
                agent.SetDestination(navHit.position);
            }
        }
        else
        {
            // Se o caminho estiver livre, desenha a linha VERDE (Sensor ativo e limpo de obstáculos)
            Debug.DrawLine(rayStart, rayStart + moveDir * alcanceDoRaio, Color.green);
        }
    }

    private void GerarPontosTaticos()
    {
        pontosTaticos.Clear();
        Vector3 centro = transform.position;
        int tentativas = 450;

        // Procura a CombatZone (incluindo objetos inativos)
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

        // Tenta buscar o Collider no objeto da CombatZone ou nos seus filhos
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
                        // 1. O próprio ponto tem de estar dentro
                        Vector3 closestSelf = zoneCollider.ClosestPoint(hit.position);
                        if (Vector3.Distance(hit.position, closestSelf) > 0.08f)
                        {
                            pontoValidoNaZona = false;
                        }
                        else
                        {
                            // 2. Garante que os limites a 1.7 metros em cruz também estão dentro do trigger da arena
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
                        // Se o collider for um MeshCollider não-convexo, o ClosestPoint falha no Unity.
                        // Nesse caso, desligamos o zoneCollider e confiamos no fallback geral de NavMesh
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

        // FALLBACK SUPREMO: Se a CombatZone não tinha collider válido ou foi restritiva demais
        if (pontosTaticos.Count < 15)
        {
            Debug.LogWarning("⚠️ [BossMovement] Poucos pontos gerados (" + pontosTaticos.Count + "). A ativar fallback no NavMesh geral...");
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

        if (zoneCollider != null)
            Debug.Log("🎯 [BossMovement] Grelha tática gerada: " + pontosTaticos.Count + " pontos estritamente DENTRO da CombatZone.");
        else
            Debug.Log("🎯 [BossMovement] Grelha tática gerada: " + pontosTaticos.Count + " pontos (Usando NavMesh geral com margem de segurança).");
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
        if (agent != null) agent.updateRotation = state;
        controleManualDeRotacao = state; // Se for false, desliga a rotação suave do Update e deixa o script rodar manualmente
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
