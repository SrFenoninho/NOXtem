using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR (Configuração de Combos)
    // ---------------------------------------------
    [Header("Hitbox Settings")]
    public Hitbox attackHitbox;
    public string enemyTag = "Enemy";
    public HitboxDefense defenseHitbox;
    public KeyCode defenseKey = KeyCode.Q;

    // Em vez de eventos de animação confusos, usamos tempos configuráveis!
    // Podes afinar exatamente aos milissegundos no Inspector quando a espada bate.
    [Header("Light Attack Combo (1, 2, 3, 4)")]
    public float[] lightDamage = { 15f, 15f, 20f, 35f };
    public float[] lightHitboxDelay = { 0.2f, 0.2f, 0.2f, 0.3f };       // Quando o dano ativa
    public float[] lightHitboxDuration = { 0.1f, 0.1f, 0.1f, 0.2f };    // Quanto tempo dura o dano
    public float[] lightImpulse = { 10f, 10f, 12f, 18f };               // Força de lunge muito maior
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
    public float launcherImpulse = 0f; // Launcher normalmente levanta em vez de atirar para a frente
    public float launcherKnockback = 10f;
    public float launcherStun = 1.0f;

    [Header("Global Combat Settings")]
    public float lightAttackAnimSpeed = 2.5f; // Velocidade de ataques normais/aéreos
    public float heavyAttackAnimSpeed = 1.2f; // Velocidade de ataques pesados/launcher

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private int comboStep = 0; // 0 = nenhum, 1-4 = Light, 10-11 = Heavy, 30 = Air
    private bool canAttack = true;
    private bool comboWindowOpen = false;
    
    // Input Buffering
    private bool inputBuffered = false;
    private int bufferedAttackType = 0; // 0 = Light, 1 = Heavy

    private bool isDefending = false;
    public bool IsDefending => isDefending;
    public bool IsHeavyCharging => false; // Mantido para compatibilidade com TPMove antigo se necessário
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

        // Resetar contador aéreo e cancelar ataques aéreos mal toca no chão
        if (tpMove != null)
        {
            if (tpMove.IsGrounded && !wasGrounded)
            {
                airAttackCount = 0;
                if (comboStep >= 30) // Se estava num ataque aéreo/launcher, reseta o sistema
                {
                    CancelAttack();
                }
            }
            wasGrounded = tpMove.IsGrounded;
        }

        // Lógica de cliques
        if (Input.GetMouseButtonDown(0)) RegisterInput(0);
        
        // Lógica do Heavy (Hold)
        if (Input.GetMouseButtonDown(1))
        {
            heavyHoldStartTime = Time.time;
            isHoldingHeavy = true;
        }
        
        if (isHoldingHeavy)
        {
            // Se mantiver premido por 0.3s, dispara o Launcher instantaneamente!
            if (Time.time - heavyHoldStartTime > 0.3f)
            {
                isHoldingHeavy = false;
                RegisterInput(2); // Launcher
            }
            // Se largar antes de 0.3s, dispara o Heavy normal
            else if (Input.GetMouseButtonUp(1))
            {
                isHoldingHeavy = false;
                RegisterInput(1); // Normal Heavy
            }
        }
    }

    // ---------------------------------------------
    //  DEFESA
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

    // ---------------------------------------------
    //  SISTEMA DE COMBO E BUFFERING
    // ---------------------------------------------
    void RegisterInput(int attackType)
    {
        // Impede completamente que o jogador ataque enquanto estiver no ar
        if (tpMove != null && !tpMove.IsGrounded) return;

        if (canAttack)
        {
            ExecuteAttack(attackType);
        }
        else if (comboWindowOpen)
        {
            // O jogador clicou rápido demais, mas guardamos a ação na memória (Buffer)!
            inputBuffered = true;
            bufferedAttackType = attackType;
        }
    }

    void ExecuteAttack(int attackType)
    {
        canAttack = false;
        comboWindowOpen = false;
        inputBuffered = false;

        // Decidir qual é o próximo passo do combo
        if (attackType == 0) // Light
        {
            if (comboStep >= 1 && comboStep < 4) comboStep++;
            else comboStep = 1;
        }
        else if (attackType == 1) // Heavy normal
        {
            if (comboStep == 10) comboStep = 11;
            else comboStep = 10;
        }
        else if (attackType == 2) // Launcher
        {
            comboStep = 40;
        }

        // Sobrescrita de ataque aéreo
        if (!tpMove.IsGrounded)
        {
            if (airAttackCount >= 4) return; // Limitar combos no ar a 4 golpes
            
            comboStep = 30; // ID do JumpAttack
            airAttackCount++;
            tpMove.SuspendGravity(0.8f); // Efeito Devil May Cry (flutuar ao bater)
        }

        // Dizer ao Animator para tocar a animação
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
        // Obter estatísticas do ataque atual
        float damage = 0f, delay = 0f, duration = 0f, impulse = 0f, knockback = 0f, stun = 0f;
        if (comboStep >= 1 && comboStep <= 4) // Light
        {
            int lIdx = comboStep - 1;
            damage = GetSafe(lightDamage, lIdx, 15f);
            delay = GetSafe(lightHitboxDelay, lIdx, 0.2f);
            duration = GetSafe(lightHitboxDuration, lIdx, 0.1f);
            impulse = GetSafe(lightImpulse, lIdx, 10f);
            knockback = GetSafe(lightKnockback, lIdx, 5f);
            stun = GetSafe(lightStun, lIdx, 0.3f);
        }
        else if (comboStep >= 10 && comboStep <= 11) // Heavy
        {
            damage = heavyDamage;
            delay = heavyHitboxDelay;
            duration = heavyHitboxDuration;
            impulse = heavyImpulse;
            knockback = heavyKnockback;
            stun = heavyStun;
        }
        else if (comboStep == 30) // Aéreo
        {
            damage = airDamage; delay = airHitboxDelay; duration = airHitboxDuration; impulse = 0f; knockback = airKnockback; stun = airStun;
        }
        else if (comboStep == 40) // Launcher
        {
            damage = launcherDamage; delay = launcherHitboxDelay; duration = launcherHitboxDuration; impulse = launcherImpulse; knockback = launcherKnockback; stun = launcherStun;
        }

        // Bloquear movimento no Heavy (combos pesados prendem-te ao chao durante 1.5s)
        if (comboStep >= 10 && comboStep <= 11)
        {
            movementLockEndTime = Time.time + 1.5f;
        }

        // Impulso físico para a frente (só no chão)
        if (impulse > 0 && tpMove.IsGrounded) tpMove.AddImpulse(transform.forward, impulse);

        float currentAnimSpeed = (comboStep >= 10 && comboStep <= 11 || comboStep == 40) ? heavyAttackAnimSpeed : lightAttackAnimSpeed;

        // Ativar Hitbox usando Coroutine independente para não bloquear o fluxo principal
        StartCoroutine(HitboxRoutine(damage, knockback, stun, delay, duration, currentAnimSpeed));

        // Esperar um pouco para garantir que a animação começou a transição
        yield return new WaitForSeconds(0.1f);

        // ABRIR JANELA PARA O PRÓXIMO GOLPE
        // O jogador pode carregar no botão para encadear o combo a partir dos 60% da animação
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

            // Se a animação chegou aos 95% e não houve clique, reseta
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
        // Ajusta os tempos das hitboxes à nova velocidade da animação para manteres a sincronia perfeita!
        yield return new WaitForSeconds(delay / animSpeed);
        if (attackHitbox != null) attackHitbox.EnableHitbox(damage, enemyTag, this, knockback, stun);
        yield return new WaitForSeconds(duration / animSpeed);
        if (attackHitbox != null) attackHitbox.DisableHitbox();
    }

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
            anim.speed = 1f; // Volta a velocidade normal de movimento
        }
    }

    // ---------------------------------------------
    //  EVENTOS
    // ---------------------------------------------
    public void OnHitLanded()
    {
        if (comboSystem != null)
            comboSystem.RegisterHit();
    }
}
