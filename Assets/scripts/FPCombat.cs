using UnityEngine;

public class FPCombat : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Light Attack")]
    public float lightDamage = 25f;
    public float lightAttackDuration = 0.3f;
    public float attackCooldown = 0.2f;

    [Header("Knockback e Stun")]
    public float lightKnockback = 5f;
    public float lightStunDuration = 0.5f;

    [Header("Impulso")]
    public float lightImpulseForce = 3f;

    [Header("Hitbox")]
    public Hitbox attackHitbox;
    public string enemyTag = "Enemy";

    [Header("Defesa")]
    public HitboxDefense defenseHitbox;
    public Camera fpCamera;             // para orientar a parede corretamente

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private bool canAttack = true;
    private bool isDefending = false;
    private PlayerComboSYS comboSystem;
    private FPMove fpMove;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        comboSystem = GetComponent<PlayerComboSYS>();
        fpMove = GetComponent<FPMove>();
    }

    void Update()
    {
        HandleDefense();

        if (isDefending) return;
        if (!canAttack) return;

        if (Input.GetMouseButtonDown(0))
            LightAttack();
    }

    // ---------------------------------------------
    //  DEFESA
    // ---------------------------------------------
    void HandleDefense()
    {
        bool wasDefending = isDefending;
        isDefending = Input.GetMouseButton(1);

        if (isDefending && !wasDefending)
        {
            CancelAttack();
            if (fpMove != null) fpMove.inputBlocked = true;
            if (defenseHitbox != null)
            {
                // Alinhar spawnPoint com a direcao da camara FP
                if (fpCamera != null && defenseHitbox.spawnPoint != null)
                {
                    defenseHitbox.spawnPoint.rotation = Quaternion.Euler(
                        0f, fpCamera.transform.eulerAngles.y, 0f);
                }
                defenseHitbox.ActivateDefense();
            }
        }

        if (!isDefending && wasDefending)
        {
            if (fpMove != null) fpMove.inputBlocked = false;
            if (defenseHitbox != null)
                defenseHitbox.DeactivateDefense();
        }
    }

    // ---------------------------------------------
    //  ATAQUE
    // ---------------------------------------------
    void LightAttack()
    {
        canAttack = false;

        // Impulso para a frente
        if (fpMove != null)
        {
            // FPMove nao tem AddImpulse � aplicar via moveDir nao e possivel diretamente
            // por isso o impulso e ignorado em FP por agora
        }

        attackHitbox.EnableHitbox(lightDamage, enemyTag, null, lightKnockback, lightStunDuration);
        Invoke(nameof(EndHitWindow), lightAttackDuration);
        Invoke(nameof(EndAttack), lightAttackDuration + attackCooldown);
    }

    void EndHitWindow() => attackHitbox.DisableHitbox();
    void EndAttack() => canAttack = true;

    // ---------------------------------------------
    //  CANCELAR
    // ---------------------------------------------
    public void CancelAttack()
    {
        CancelInvoke(nameof(EndHitWindow));
        CancelInvoke(nameof(EndAttack));
        EndHitWindow();
        canAttack = true;
    }

    // ---------------------------------------------
    //  COMBO
    // ---------------------------------------------
    public void OnHitLanded()
    {
        if (comboSystem != null)
            comboSystem.RegisterHit();
    }
}