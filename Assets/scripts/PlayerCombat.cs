using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("Hitbox Settings")]

    public Hitbox attackHitbox;
    public string enemyTag = "Enemy";
    public HitboxDefense defenseHitbox;
    public KeyCode defenseKey = KeyCode.Q;

    [Header("Light Attack Combo (1, 2, 3, 4)")]
    public float[] lightDamage = { 15f, 15f, 20f, 35f };
    public float[] lightHitboxDelay = { 0.2f, 0.2f, 0.2f, 0.3f };
    public float[] lightHitboxDuration = { 0.1f, 0.1f, 0.1f, 0.2f };
    public float[] lightImpulse = { 10f, 10f, 12f, 18f };
    public float[] lightKnockback = { 5f, 5f, 5f, 10f };
    public float[] lightStun = { 0.3f, 0.3f, 0.3f, 0.5f };

    [Header("Heavy Attack Combo")]
    public float heavyDamage = 50f;
    public float heavyHitboxDelay = 1.5f;
    public float heavyHitboxDuration = 0.2f;
    public float heavyImpulse = 0f;
    public float heavyKnockback = 40f;
    public float heavyStun = 2f;

    [Header("Air Attack (JumpAttack)")]
    public float airDamage = 25f;
    public float airHitboxDelay = 0.2f;
    public float airHitboxDuration = 0.2f;
    public float airKnockback = 10f;
    public float airStun = 0.5f;

    [Header("Launcher Attack (Hold Heavy)")]
    public float launcherDamage = 30f;
    public float launcherHitboxDelay = 0.3f;
    public float launcherHitboxDuration = 0.2f;
    public float launcherImpulse = 0f;
    public float launcherKnockback = 10f;
    public float launcherStun = 1.0f;

    [Header("Global Combat Settings")]
    public float lightAttackAnimSpeed = 2.5f;
    public float heavyAttackAnimSpeed = 1.2f;





    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private int comboStep = 0;
    private bool canAttack = true;
    private bool comboWindowOpen = false;

    private bool inputBuffered = false;
    private int bufferedAttackType = 0;

    private bool isDefending = false;
    public bool IsDefending => isDefending;
    public bool IsHeavyCharging => false;
    public bool IsAttacking => comboStep > 0;

    public bool IsMovementLocked => Time.time < movementLockEndTime;
    private float movementLockEndTime = 0f;

    private float heavyHoldStartTime = 0f;
    private bool isHoldingHeavy = false;
    private int airAttackCount = 0;
    private bool wasGrounded = true;

    private Animator anim;
    private TPMove tpMove;
    private PlayerComboSYS comboSystem;
    private Coroutine currentAttackRoutine;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        comboSystem = GetComponent<PlayerComboSYS>();
        tpMove = GetComponent<TPMove>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        HandleDefense();

        if (isDefending) return;

        if (tpMove != null)
        {
            if (tpMove.IsGrounded && !wasGrounded)
            {
                airAttackCount = 0;
                if (comboStep >= 30)
                {
                    CancelAttack();
                }
            }
            wasGrounded = tpMove.IsGrounded;
        }

        if (Input.GetMouseButtonDown(0)) RegisterInput(0);

        if (Input.GetMouseButtonDown(1))
        {
            heavyHoldStartTime = Time.time;
            isHoldingHeavy = true;
        }

        if (isHoldingHeavy)
        {
            if (Time.time - heavyHoldStartTime > 0.3f)
            {
                isHoldingHeavy = false;
                RegisterInput(2);
            }
            else if (Input.GetMouseButtonUp(1))
            {
                isHoldingHeavy = false;
                RegisterInput(1);
            }
        }
    }




    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    void HandleDefense()
    {
        bool wasDefending = isDefending;
        isDefending = Input.GetKey(defenseKey) && tpMove.IsGrounded;

        if (isDefending && !wasDefending)
        {
            CancelAttack();
            if (defenseHitbox != null) defenseHitbox.ActivateDefense();
            if (anim != null) anim.SetBool("isBlocking", true);
        }

        if (!isDefending && wasDefending)
        {
            if (defenseHitbox != null) defenseHitbox.DeactivateDefense();
            if (anim != null) anim.SetBool("isBlocking", false);
        }
    }

    void RegisterInput(int attackType)
    {
        if (tpMove != null && !tpMove.IsGrounded) return;

        if (canAttack)
        {
            ExecuteAttack(attackType);
        }
        else if (comboWindowOpen)
        {
            inputBuffered = true;
            bufferedAttackType = attackType;
        }
    }

    void ExecuteAttack(int attackType)
    {
        canAttack = false;
        comboWindowOpen = false;
        inputBuffered = false;

        if (attackType == 0)
        {
            if (comboStep >= 1 && comboStep < 4) comboStep++;
            else comboStep = 1;
        }
        else if (attackType == 1)
        {
            if (comboStep == 10) comboStep = 11;
            else comboStep = 10;
        }
        else if (attackType == 2)
        {
            comboStep = 40;
        }

        if (!tpMove.IsGrounded)
        {
            if (airAttackCount >= 4) return;

            comboStep = 30;
            airAttackCount++;
            tpMove.SuspendGravity(0.8f);
        }

        if (anim != null)
        {
            anim.SetInteger("comboStep", comboStep);

            if (comboStep >= 10 && comboStep <= 11 || comboStep == 40)
                anim.speed = heavyAttackAnimSpeed;
            else
                anim.speed = lightAttackAnimSpeed;

            anim.SetTrigger("doAttack");
        }

        if (currentAttackRoutine != null) StopCoroutine(currentAttackRoutine);
        currentAttackRoutine = StartCoroutine(AttackRoutine());
    }

    float GetSafe(float[] arr, int idx, float def)
    {
        if (arr != null && idx >= 0 && idx < arr.Length) return arr[idx];
        return def;
    }

    IEnumerator AttackRoutine()
    {
        float damage = 0f, delay = 0f, duration = 0f, impulse = 0f, knockback = 0f, stun = 0f;
        if (comboStep >= 1 && comboStep <= 4)
        {
            int lIdx = comboStep - 1;
            damage = GetSafe(lightDamage, lIdx, 15f);
            delay = GetSafe(lightHitboxDelay, lIdx, 0.2f);
            duration = GetSafe(lightHitboxDuration, lIdx, 0.1f);
            impulse = GetSafe(lightImpulse, lIdx, 10f);
            knockback = GetSafe(lightKnockback, lIdx, 5f);
            stun = GetSafe(lightStun, lIdx, 0.3f);
        }
        else if (comboStep >= 10 && comboStep <= 11)
        {
            damage = heavyDamage;
            delay = heavyHitboxDelay;
            duration = heavyHitboxDuration;
            impulse = heavyImpulse;
            knockback = heavyKnockback;
            stun = heavyStun;
        }
        else if (comboStep == 30)
        {
            damage = airDamage; delay = airHitboxDelay; duration = airHitboxDuration; impulse = 0f; knockback = airKnockback; stun = airStun;
        }
        else if (comboStep == 40)
        {
            damage = launcherDamage; delay = launcherHitboxDelay; duration = launcherHitboxDuration; impulse = launcherImpulse; knockback = launcherKnockback; stun = launcherStun;
        }

        if (comboStep >= 10 && comboStep <= 11)
        {
            movementLockEndTime = Time.time + 1.5f;
        }

        if (impulse > 0 && tpMove.IsGrounded) tpMove.AddImpulse(transform.forward, impulse);

        float currentAnimSpeed = (comboStep >= 10 && comboStep <= 11 || comboStep == 40) ? heavyAttackAnimSpeed : lightAttackAnimSpeed;

        StartCoroutine(HitboxRoutine(damage, knockback, stun, delay, duration, currentAnimSpeed));

        yield return new WaitForSeconds(0.1f);

        while (anim != null)
        {
            float animProgress = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;

            if (animProgress >= 0.6f)
            {
                comboWindowOpen = true;
                if (inputBuffered)
                {
                    ExecuteAttack(bufferedAttackType);
                    yield break;
                }
            }

            if (animProgress >= 0.95f)
            {
                break;
            }

            yield return null;
        }

        ResetComboState();
    }

    IEnumerator HitboxRoutine(float damage, float knockback, float stun, float delay, float duration, float animSpeed)
    {
        yield return new WaitForSeconds(delay / animSpeed);
        if (attackHitbox != null) attackHitbox.EnableHitbox(damage, enemyTag, this, knockback, stun);
        yield return new WaitForSeconds(duration / animSpeed);
        if (attackHitbox != null) attackHitbox.DisableHitbox();
    }





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void CancelAttack()
    {
        if (currentAttackRoutine != null) StopCoroutine(currentAttackRoutine);
        if (attackHitbox != null) attackHitbox.DisableHitbox();
        ResetComboState();
    }

    void ResetComboState()
    {
        comboStep = 0;
        canAttack = true;
        comboWindowOpen = false;
        inputBuffered = false;
        if (anim != null)
        {
            anim.SetInteger("comboStep", 0);
            anim.speed = 1f;
        }
    }

    public void OnHitLanded()
    {
        if (comboSystem != null)
            comboSystem.RegisterHit();
    }
}
