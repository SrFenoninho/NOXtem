using UnityEngine;
using System.Collections;

public class BossState_PillarCharge : IBossState
{
    private Coroutine routine;
    private bool bateuNumPilar = false;

    public void EnterState(BossController boss)
    {
        boss.health.isInvulnerable = true;
        routine = boss.StartCoroutine(ChargeRoutine(boss));
    }

    public void UpdateState(BossController boss) {}

    public void ExitState(BossController boss)
    {
        boss.health.isInvulnerable = false;
        boss.movement.SetRotationUpdate(true);
        if(routine != null) boss.StopCoroutine(routine);
    }

    public void OnPillarHit(BossController boss)
    {
        bateuNumPilar = true;
        boss.movement.StopMovement();
        if (boss.currentPhase == BossController.BossPhase.PillarEvent1) boss.TriggerPhase(BossController.BossPhase.Phase2);
        else if (boss.currentPhase == BossController.BossPhase.PillarEvent2) boss.TriggerPhase(BossController.BossPhase.Phase3);
    }

    private IEnumerator ChargeRoutine(BossController boss)
    {
        while (!bateuNumPilar)
        {
            boss.movement.SetRotationUpdate(true);
            
            // 1. Fuga preparatória: afasta-se para preparar a investida
            if (Vector3.Distance(boss.transform.position, boss.playerTarget.position) < 12f)
            {
                // Escolhe o ponto tático mais distante do jogador para recuar com segurança
                Vector3 targetPoint = boss.transform.position;
                float maiorDist = 0f;
                bool encontrouPonto = false;

                foreach (Vector3 ponto in boss.movement.pontosTaticos)
                {
                    float d = Vector3.Distance(ponto, boss.playerTarget.position);
                    if (d > maiorDist)
                    {
                        maiorDist = d;
                        targetPoint = ponto;
                        encontrouPonto = true;
                    }
                }

                if (!encontrouPonto)
                {
                    Vector3 dirAway = (boss.transform.position - boss.playerTarget.position).normalized;
                    targetPoint = boss.transform.position + dirAway * 8f;
                }

                // Corre de forma focada para o ponto central tático
                boss.movement.MoveTo(targetPoint, boss.movement.runSpeed * 1.3f);

                // Espera até ele atingir a distância segura de 12 metros ou chegar perto do ponto
                float safetyTimer = 0f;
                UnityEngine.AI.NavMeshAgent agent = boss.movement.agent;
                while (Vector3.Distance(boss.transform.position, boss.playerTarget.position) < 12f && safetyTimer < 2.5f)
                {
                    safetyTimer += Time.deltaTime;
                    bool chegou = agent != null && agent.enabled && agent.isOnNavMesh && 
                                  !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.8f;
                    if (chegou) break;
                    yield return null;
                }
            }

            // 2. Prepara a Investida
            boss.movement.StopMovement();
            boss.movement.LookAt(boss.playerTarget.position);
            boss.combat.TriggerAnim("Roar");
            yield return new WaitForSeconds(1.0f);

            // 3. Define direção rectilínea (Não curva) e ARRANCAR!
            Vector3 chargeDirection = (boss.playerTarget.position - boss.transform.position).normalized;
            chargeDirection.y = 0;

            boss.movement.SetRotationUpdate(false);
            if(boss.movement.agent != null) boss.movement.agent.acceleration = 200f; // Arranque bruto
            boss.movement.MoveTo(boss.transform.position + chargeDirection * 30f, boss.movement.chargeSpeed * 1.2f); // Corre mais rápido e mais longe
            
            float elapsedCharge = 0f;

            // 4. Enquanto corre, deteta danos em área no player ou timeout
            while (elapsedCharge < 5f && !bateuNumPilar)
            {
                if (Vector3.Distance(boss.transform.position, boss.playerTarget.position) <= 2.2f)
                {
                    // Atropelou o jogador a meio da viagem
                    boss.combat.DealAreaDamage(30f, 35f); // muito knockback
                }
                elapsedCharge += Time.deltaTime;
                yield return null;
            }

            // 5. Se falhou o pilar (Timeout da corrida), ele respira e repete a investida de novo!
            if (!bateuNumPilar)
            {
                boss.movement.StopMovement();
                yield return new WaitForSeconds(1.5f); // Descansa 1.5 seg antes de tentar fugir e voltar a investir
            }
        }
    }
}
