using UnityEngine;
using System.Collections;

public class BossState_MeleeAttack : IBossState
{
    private Coroutine routine;

    public void EnterState(BossController boss)
    {
        routine = boss.StartCoroutine(AttackRoutine(boss));
    }

    public void UpdateState(BossController boss) {}

    public void ExitState(BossController boss)
    {
        if (routine != null) boss.StopCoroutine(routine);
    }

    private IEnumerator AttackRoutine(BossController boss)
    {
        // 1. Para o movimento a 100% imediatamente
        boss.movement.StopMovement();
        boss.movement.LookAt(boss.playerTarget.position);

        // 2. Dispara a animação de soco (o boss ataca parado)
        boss.combat.TriggerMeleeAnim();

        // 3. Espera pelo delay de impacto da animação
        yield return new WaitForSeconds(boss.combat.attackHitDelay);

        // 4. Aplica o golpe no player
        boss.combat.DealMeleeDamage();
        boss.combat.nextAttackTime = Time.time + boss.combat.attackCooldown;

        // 5. Espera o recovery curto da animação de ataque acabar
        yield return new WaitForSeconds(boss.combat.attackRecoveryTime);

        // 6. Fuga Pós-Ataque IMEDIATA (Usa o BossState_Flee inteligente e baseado na grelha tática)
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
