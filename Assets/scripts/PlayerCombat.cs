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

    // canAttack controls whether the player can start a new attack
    // the hitbox itself controls whether damage is being dealt
    private bool canAttack = true;

    private bool isHeavyCharging = false;

    // Reference to combo system - registered hits automatically
    private PlayerComboSYS comboSystem;

    void Start()
    {
        comboSystem = GetComponent<PlayerComboSYS>();
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

    void HandleDefense()
    {
        bool wasDefending = isDefending;
        isDefending = Input.GetKey(defenseKey);

        if (isDefending && !wasDefending)
        {
            CancelAttack(); // Cancel any attack when defense starts
            if (defenseHitbox != null)
                defenseHitbox.ActivateDefense();
        }

        if (!isDefending && wasDefending)
        {
            if (defenseHitbox != null)
                defenseHitbox.DeactivateDefense();
        }
    }

    void LightAttack()
    {
        canAttack = false;
        attackHitbox.EnableHitbox(lightDamage, enemyTag, this);
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
        if (!isHeavyCharging) return;           // Was cancelled (e.g. took damage)
        isHeavyCharging = false;
        attackHitbox.EnableHitbox(heavyDamage, enemyTag, this);
        Invoke(nameof(EndHitWindow), heavyAttackDuration);
        Invoke(nameof(EndAttack), heavyAttackDuration + attackCooldown);
    }

    void EndHitWindow()
    {
        attackHitbox.DisableHitbox();
    }

    void EndAttack()
    {
        canAttack = true;                       // Ready to attack again
    }

    // Call this when player takes damage to cancel heavy windup
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

    // Called by Hitbox when it hits an enemy - registers combo hit
    public void OnHitLanded()
    {
        if (comboSystem != null)
            comboSystem.RegisterHit();
    }
}