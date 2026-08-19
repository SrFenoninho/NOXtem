using UnityEngine;
using System.Collections;

public class BossState_PillarCharge : IBossState
{




    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private Coroutine routine;
    private bool bateuNumPilar = false;





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
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





    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    private IEnumerator ChargeRoutine(BossController boss)
    {
        while (!bateuNumPilar)
        {
            boss.movement.SetRotationUpdate(true);

            if (Vector3.Distance(boss.transform.position, boss.playerTarget.position) < 12f)
            {
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

                boss.movement.MoveTo(targetPoint, boss.movement.runSpeed * 1.3f);

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

            boss.movement.StopMovement();
            boss.movement.LookAt(boss.playerTarget.position);
            boss.combat.TriggerAnim("Roar");
            yield return new WaitForSeconds(1.0f);

            Vector3 chargeDirection = (boss.playerTarget.position - boss.transform.position).normalized;
            chargeDirection.y = 0;

            boss.movement.SetRotationUpdate(false);
            if(boss.movement.agent != null) boss.movement.agent.acceleration = 200f;
            boss.movement.MoveTo(boss.transform.position + chargeDirection * 30f, boss.movement.chargeSpeed * 1.2f);

            float elapsedCharge = 0f;

            while (elapsedCharge < 5f && !bateuNumPilar)
            {
                if (Vector3.Distance(boss.transform.position, boss.playerTarget.position) <= 2.2f)
                {
                    boss.combat.DealAreaDamage(30f, 35f);
                }
                elapsedCharge += Time.deltaTime;
                yield return null;
            }

            if (!bateuNumPilar)
            {
                boss.movement.StopMovement();
                yield return new WaitForSeconds(1.5f);
            }
        }
    }
}
