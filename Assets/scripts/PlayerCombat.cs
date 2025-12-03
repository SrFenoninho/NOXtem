using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class PlayerCombat : MonoBehaviour
{
    [Header("Light Attack")]
    public float lightDamage = 25f;
    public float lightAttackDuration = 0.3f;

    [Header("Heavy Attack")]
    public float heavyDamage = 60f;
    public float heavyWindupTime = 0.5f;
    public float heavyAttackDuration = 0.5f;

    [Header("Hitbox Settings")]
    public Hitbox attackHitbox;
    public string enemyTag = "Enemy";

    [Header("Cooldown")]
    public float attackCooldown = 0.2f;

    private bool isAttacking = false;
    private bool isHeavyCharging = false;
    private float nextAttackTime = 0f;

    void Update()
    {
        if (Time.time < nextAttackTime) return;
        if (isAttacking || isHeavyCharging) return;

        if (Input.GetMouseButtonDown(0))
        {
            LightAttack();
        }

        if (Input.GetMouseButtonDown(1))
        {
            StartHeavyAttack();
        }
    }

    void LightAttack()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        attackHitbox.EnableHitbox(lightDamage, enemyTag);

        Invoke(nameof(EndAttack), lightAttackDuration);
    }

    void StartHeavyAttack()
    {
        isHeavyCharging = true;
        Invoke(nameof(ExecuteHeavyAttack), heavyWindupTime);
    }

    void ExecuteHeavyAttack()
    {
        if (!isHeavyCharging) return;

        isHeavyCharging = false;
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        attackHitbox.EnableHitbox(heavyDamage, enemyTag);

        Invoke(nameof(EndAttack), heavyAttackDuration);
    }

    void EndAttack()
    {
        isAttacking = false;
        attackHitbox.DisableHitbox();
    }
}
