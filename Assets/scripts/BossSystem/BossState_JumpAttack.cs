using UnityEngine;
using System.Collections;

public class BossState_JumpAttack : IBossState
{
    private Coroutine routine;

    public void EnterState(BossController boss)
    {
        routine = boss.StartCoroutine(JumpRoutine(boss));
    }

    public void UpdateState(BossController boss) {}

    public void ExitState(BossController boss)
    {
        if (routine != null) boss.StopCoroutine(routine);
    }

    private IEnumerator JumpRoutine(BossController boss)
    {
        boss.movement.StopMovement();
        boss.movement.LookAt(boss.playerTarget.position);
        boss.combat.TriggerAnim("JumpAttack");

        yield return new WaitForSeconds(0.4f);

        boss.movement.agent.enabled = false;
        Vector3 startPos = boss.transform.position;
        Vector3 targetPos = boss.playerTarget.position;
        float elapsed = 0f;

        while (elapsed < boss.combat.jumpAirTime)
        {
            float normalizedTime = elapsed / boss.combat.jumpAirTime;
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, normalizedTime);
            currentPos.y += Mathf.Sin(normalizedTime * Mathf.PI) * boss.combat.jumpHeight;
            boss.transform.position = currentPos;
            elapsed += Time.deltaTime;
            yield return null;
        }

        boss.transform.position = targetPos;
        boss.movement.agent.enabled = true;
        boss.movement.StopMovement();

        boss.vfx.TriggerCameraShake(0.8f, 0.4f);
        boss.vfx.PlayAoeParticles();
        boss.combat.DealAreaDamage(boss.combat.jumpAttackDamage, 35f);

        boss.combat.nextJumpAttackTime = Time.time + boss.combat.jumpAttackCooldown;

        yield return new WaitForSeconds(1.5f);
        boss.TriggerPhase(boss.currentPhase);
    }
}
