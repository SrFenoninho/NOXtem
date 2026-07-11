using UnityEngine;

public class BossState_Phase2_Aggro : IBossState
{
    public void EnterState(BossController boss)
    {
        boss.health.isInvulnerable = false;
    }

    public void UpdateState(BossController boss)
    {
        float dist = Vector3.Distance(boss.transform.position, boss.playerTarget.position);

        if(dist > 5.5f && Time.time >= boss.combat.nextJumpAttackTime)
        {
            boss.ChangeState(new BossState_JumpAttack());
            return;
        }

        boss.movement.MoveTo(boss.playerTarget.position, boss.movement.runSpeed);

        // Ataca apenas se estiver mesmo encostado, para garantir que não soco o ar a 2.5 metros
        if(dist <= 1.8f && Time.time >= boss.combat.nextAttackTime)
        {
            boss.ChangeState(new BossState_MeleeAttack());
        }
    }

    public void ExitState(BossController boss) {}
}
