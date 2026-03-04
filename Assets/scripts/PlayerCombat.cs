using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Light Attack")]
    public float lightDamage = 25f;
    public float lightAttackDuration = 0.3f;

    [Header("Heavy Attack")]
    public float heavyDamage = 60f;
    public float heavyWindupTime = 0.5f;    // tempo de carregamento antes de executar
    public float heavyAttackDuration = 0.5f;

    [Header("Defense")]
    public KeyCode defenseKey = KeyCode.Q;
    public HitboxDefense defenseHitbox;
    private bool isDefending = false;
    public bool IsDefending => isDefending;
    public bool IsHeavyCharging => isHeavyCharging;

    [Header("Hitbox Settings")]
    public Hitbox attackHitbox;
    public string enemyTag = "Enemy";

    [Header("Cooldown")]
    public float attackCooldown = 0.2f;

    [Header("Knockback")]
    public float lightKnockback = 5f;
    public float heavyKnockback = 15f;

    [Header("Stun Duration")]
    public float lightStunDuration = 0.5f;
    public float heavyStunDuration = 1.5f;

    [Header("Attack Impulse")]
    public float lightImpulseForce = 3f;    // impulso para a frente no ataque leve
    public float heavyImpulseForce = 8f;    // impulso para a frente no ataque pesado

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private bool canAttack = true;
    private bool isHeavyCharging = false;
    private PlayerComboSYS comboSystem;
    private TPMove tpMove;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        comboSystem = GetComponent<PlayerComboSYS>();
        tpMove = GetComponent<TPMove>();
    }

    void Update()
    {
        HandleDefense();

        if (isDefending) return;
        if (!canAttack) return;

        if (Input.GetMouseButtonDown(0))
            LightAttack();

        if (Input.GetMouseButtonDown(1))
            StartHeavyAttack();
    }

    // ---------------------------------------------
    //  DEFESA
    // ---------------------------------------------
    void HandleDefense()
    {
        bool wasDefending = isDefending;
        // Só é possível defender se estiver no chão — evita ficar suspenso no ar
        isDefending = Input.GetKey(defenseKey) && tpMove.IsGrounded;

        if (isDefending && !wasDefending)
        {
            CancelAttack();
            if (defenseHitbox != null)
                defenseHitbox.ActivateDefense();
        }

        if (!isDefending && wasDefending)
        {
            if (defenseHitbox != null)
                defenseHitbox.DeactivateDefense();
        }
    }

    // ---------------------------------------------
    //  ATAQUES
    // ---------------------------------------------
    void LightAttack()
    {
        canAttack = false;
        tpMove?.AddImpulse(transform.forward, lightImpulseForce);
        attackHitbox.EnableHitbox(lightDamage, enemyTag, this, lightKnockback, lightStunDuration);
        Invoke(nameof(EndHitWindow), lightAttackDuration);
        Invoke(nameof(EndAttack), lightAttackDuration + attackCooldown);
    }

    void StartHeavyAttack()
    {
        canAttack = false;
        isHeavyCharging = true;
        Invoke(nameof(ExecuteHeavyAttack), heavyWindupTime);
    }

    void ExecuteHeavyAttack()
    {
        if (!isHeavyCharging) return;
        isHeavyCharging = false;
        tpMove?.AddImpulse(transform.forward, heavyImpulseForce);
        attackHitbox.EnableHitbox(heavyDamage, enemyTag, this, heavyKnockback, heavyStunDuration);
        Invoke(nameof(EndHitWindow), heavyAttackDuration);
        Invoke(nameof(EndAttack), heavyAttackDuration + attackCooldown);
    }

    void EndHitWindow()
    {
        attackHitbox.DisableHitbox();
    }

    void EndAttack()
    {
        canAttack = true;
    }

    // ---------------------------------------------
    //  CANCELAR ATAQUE
    // ---------------------------------------------
    // Chamado ao iniciar a defesa ou por forças externas
    public void CancelAttack()
    {
        if (isHeavyCharging)
        {
            CancelInvoke(nameof(ExecuteHeavyAttack));
            isHeavyCharging = false;
        }

        CancelInvoke(nameof(EndHitWindow));
        CancelInvoke(nameof(EndAttack));
        EndHitWindow();
        canAttack = true;
    }

    // ---------------------------------------------
    //  COMBO
    // ---------------------------------------------
    // Chamado pela Hitbox quando um acerto é registado
    public void OnHitLanded()
    {
        if (comboSystem != null)
            comboSystem.RegisterHit();
    }
}
