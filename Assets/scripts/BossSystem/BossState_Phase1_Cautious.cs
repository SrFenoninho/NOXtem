using UnityEngine;

public class BossState_Phase1_Cautious : IBossState
{




    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private Vector3 currentPatrolTarget;
    private bool hasTarget = false;
    private float reevaluateTimer = 0f;





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void EnterState(BossController boss)
    {
        boss.health.isInvulnerable = false;
        hasTarget = false;
        reevaluateTimer = 0f;
    }

    public void UpdateState(BossController boss)
    {
        float dist = Vector3.Distance(boss.transform.position, boss.playerTarget.position);
        UnityEngine.AI.NavMeshAgent agent = boss.movement.agent;

        if (dist < 8.0f && boss.fleeCooldownTimer <= 0f)
        {
            boss.ChangeState(new BossState_Flee());
            return;
        }

        reevaluateTimer += Time.deltaTime;

        bool reachedDestination = hasTarget && agent != null && agent.enabled && agent.isOnNavMesh && 
                                  !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f;

        if (!hasTarget || reachedDestination || reevaluateTimer > 4.5f)
        {
            reevaluateTimer = 0f;
            Vector3 newTarget;
            if (EncontrarPontoPatrulhaSeguro(boss, out newTarget))
            {
                currentPatrolTarget = newTarget;
                hasTarget = true;
                boss.movement.MoveTo(currentPatrolTarget, boss.movement.walkSpeed);
            }
        }
    }

    public void ExitState(BossController boss)
    {
    }





    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    private bool EncontrarPontoPatrulhaSeguro(BossController boss, out Vector3 target)
    {
        target = boss.transform.position;
        Vector3 melhorPonto = boss.transform.position;
        float maiorDistancia = 0f;
        bool encontrouPontoValido = false;

        for (int i = 0; i < 30; i++)
        {
            if (boss.movement.pontosTaticos.Count == 0) break;
            Vector3 pontoCandidato = boss.movement.pontosTaticos[Random.Range(0, boss.movement.pontosTaticos.Count)];

            float testDist = Vector3.Distance(pontoCandidato, boss.playerTarget.position);

            if (testDist >= 10f)
            {
                target = pontoCandidato;
                return true;
            }

            if (testDist > maiorDistancia)
            {
                maiorDistancia = testDist;
                melhorPonto = pontoCandidato;
                encontrouPontoValido = true;
            }
        }

        if (encontrouPontoValido && maiorDistancia > 3.0f)
        {
            target = melhorPonto;
            return true;
        }
        return false;
    }
}
