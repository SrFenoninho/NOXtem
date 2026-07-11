using UnityEngine;

public class BossCombat : MonoBehaviour
{
    [Header("Ataque Melee")]
    public float attackRange = 2.5f;
    public float attackDamage = 15f;
    public float attackCooldown = 2.5f;
    public float attackHitDelay = 0.8f; 
    public float attackRecoveryTime = 0.6f;
    [HideInInspector] public float nextAttackTime = 0f;

    [Header("Ataque Salto Parabólico")]
    public float jumpAttackRadius = 6f;
    public float jumpAttackDamage = 30f;
    public float jumpAttackCooldown = 6f;
    public float jumpHeight = 6f; 
    public float jumpAirTime = 1.2f;
    [HideInInspector] public float nextJumpAttackTime = 0f;

    private BossController boss;

    public void Initialize(BossController controller)
    {
        boss = controller;
    }

    public void TriggerMeleeAnim()
    {
        if(boss.anim != null)
        {
            boss.anim.ResetTrigger("Attack1");
            boss.anim.ResetTrigger("Attack2");
        }
        string animName = Random.value > 0.5f ? "Attack1" : "Attack2";
        TriggerAnim(animName);
    }

    public void TriggerAnim(string triggerName)
    {
        if(boss.anim == null) return;
        foreach (AnimatorControllerParameter param in boss.anim.parameters)
        {
            if (param.name == triggerName && param.type == AnimatorControllerParameterType.Trigger)
            {
                boss.anim.SetTrigger(triggerName);
                return;
            }
        }
    }

    public void DealMeleeDamage()
    {
        if (boss.playerTarget != null && boss.playerHealthRef != null && Vector3.Distance(transform.position, boss.playerTarget.position) <= attackRange + 2f)
        {
            boss.playerHealthRef.TakeDamage(attackDamage, transform.position);
        }
    }

    public void DealAreaDamage(float damageOverride, float knockbackForce)
    {
        if (boss.playerTarget != null && Vector3.Distance(transform.position, boss.playerTarget.position) <= jumpAttackRadius + 3f)
        {
            if (boss.playerHealthRef != null) 
            {
                boss.playerHealthRef.TakeAreaDamageWithKnockback(damageOverride, transform.position, knockbackForce, 0.6f); 
            }
        }
    }
}
