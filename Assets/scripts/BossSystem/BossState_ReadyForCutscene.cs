using UnityEngine;

public class BossState_ReadyForCutscene : IBossState
{
    public void EnterState(BossController boss)
    {
        boss.health.isInvulnerable = true;
    }

    public void UpdateState(BossController boss)
    {
        if(boss.ultimoPilarNoMapa != null)
        {
            if(Vector3.Distance(boss.transform.position, boss.ultimoPilarNoMapa.transform.position) > 3.5f)
            {
                boss.movement.MoveTo(boss.ultimoPilarNoMapa.transform.position, boss.movement.walkSpeed * 0.4f);
            }
            else
            {
                boss.movement.StopMovement();
            }
        }
        else
        {
            boss.movement.StopMovement();
        }
    }

    public void ExitState(BossController boss) {}
}
