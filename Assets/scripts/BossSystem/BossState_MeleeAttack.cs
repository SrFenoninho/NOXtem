using UnityEngine;
using System.Collections;

public class BossState_MeleeAttack : IBossState
{




    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private Coroutine routine;





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void EnterState(BossController boss)
    {
        routine = boss.StartCoroutine(AttackRoutine(boss));
    }

    public void UpdateState(BossController boss) {}

    public void ExitState(BossController boss)
    {
        if (routine != null) boss.StopCoroutine(routine);
    }





    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    private IEnumerator AttackRoutine(BossController boss)
    {
        boss.movement.StopMovement();
        boss.movement.LookAt(boss.playerTarget.position);

        boss.combat.TriggerMeleeAnim();

        yield return new WaitForSeconds(boss.combat.attackHitDelay);

        boss.combat.DealMeleeDamage();
        boss.combat.nextAttackTime = Time.time + boss.combat.attackCooldown;

        yield return new WaitForSeconds(boss.combat.attackRecoveryTime);

        if (boss.currentPhase == BossController.BossPhase.Phase1 || 
            boss.currentPhase == BossController.BossPhase.Phase3)
        {
            boss.ChangeState(new BossState_Flee());
        }
        else
        {
            boss.TriggerPhase(boss.currentPhase);
        }
    }
}
