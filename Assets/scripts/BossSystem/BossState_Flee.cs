using UnityEngine;

public class BossState_Flee : IBossState
{




    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private Vector3 targetFleePoint;
    private float fleeTimeout = 3.5f;
    private float timer = 0f;





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void EnterState(BossController boss)
    {
        timer = 0f;
        boss.health.isInvulnerable = false;
        boss.fleeCooldownTimer = 4.0f;

        Vector3 melhorPonto = boss.transform.position;
        float maiorDistancia = 0f;
        bool encontrouPonto = false;

        foreach (Vector3 ponto in boss.movement.pontosTaticos)
        {
            float distPlayerToPonto = Vector3.Distance(ponto, boss.playerTarget.position);

            if (distPlayerToPonto > maiorDistancia)
            {
                maiorDistancia = distPlayerToPonto;
                melhorPonto = ponto;
                encontrouPonto = true;
            }
        }

        if (encontrouPonto)
        {
            targetFleePoint = melhorPonto;
        }
        else
        {
            Vector3 dirAway = (boss.transform.position - boss.playerTarget.position).normalized;
            targetFleePoint = boss.transform.position + dirAway * 6f;
        }

        boss.movement.MoveTo(targetFleePoint, boss.movement.runSpeed * 1.35f);
        if (boss.movement.agent != null)
        {
            boss.movement.agent.acceleration = 120f;
            boss.movement.agent.autoBraking = true;
        }
    }

    public void UpdateState(BossController boss)
    {
        timer += Time.deltaTime;

        UnityEngine.AI.NavMeshAgent agent = boss.movement.agent;
        bool chegouAoDestino = agent != null && agent.enabled && agent.isOnNavMesh && 
                               !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.8f;

        if (chegouAoDestino || timer >= fleeTimeout)
        {
            boss.TriggerPhase(boss.currentPhase);
        }
    }

    public void ExitState(BossController boss)
    {
        if (boss.movement.agent != null)
        {
            boss.movement.agent.acceleration = 30f;
        }
    }
}
