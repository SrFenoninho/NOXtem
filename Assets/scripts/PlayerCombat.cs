using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Light Attack")]
    public float lightDamage = 25f;
    public float lightAttackDuration = 0.3f;

    [Header("Heavy Attack")]
    public float heavyDamage = 60f;
    public float heavyWindupTime = 0.5f;
    public float heavyAttackDuration = 0.5f;

    [Header("Defense")]
    public KeyCode defenseKey = KeyCode.Q;
    public HitboxDefense defenseHitbox;
    private bool isDefending = false;
    public bool IsDefending => isDefending;

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
        isDefending = Input.GetKey(defenseKey);

        if (defenseHitbox != null)
        {
            if (isDefending)
                defenseHitbox.EnableDefense();
            else
                defenseHitbox.DisableDefense();
        }

        if (isDefending) return;
        if (Time.time < nextAttackTime) return;
        if (isAttacking || isHeavyCharging) return;

        if (Input.GetMouseButtonDown(0))
            LightAttack();
        if (Input.GetMouseButtonDown(1))
            StartHeavyAttack();
    }

    void LightAttack()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;
        attackHitbox.EnableHitbox(lightDamage, enemyTag, this);
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
        attackHitbox.EnableHitbox(heavyDamage, enemyTag, this);
        Invoke(nameof(EndAttack), heavyAttackDuration);
    }

    void EndAttack()
    {
        isAttacking = false;
        attackHitbox.DisableHitbox();
    }
}