using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FinalBossAI : MonoBehaviour
{
    public enum BossPhase { Phase1, PillarEvent1, Phase2, PillarEvent2, Phase3, ReadyToDie, Cutscene }
    public enum BossState { Idle, Fighting, Evasion, Charging, ParabolicJump, Stunned, Execution }
    
    [Header("Estado Atual (Apenas leitura)")]
    public BossPhase currentPhase = BossPhase.Phase1;
    public BossState currentState = BossState.Idle;

    [Header("Referencias ABSOLUTAS")]
    public Transform playerTarget;
    public PlayerHealth playerHealthRef;

    public float currentHealth;
    public float maxHealth = 300f;
    public string nextSceneName = "Scene_Epilogo";

    [Header("Movimento")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 7f;
    public float chargeSpeed = 16f;

    [Header("Ataque Melee")]
    public float attackRange = 2.5f;
    public float attackDamage = 15f;
    public float attackCooldown = 2.5f;
    private float nextAttackTime = 0f;

    [Header("Ataque Salto Parabólico")]
    public float jumpAttackRadius = 6f;
    public float jumpAttackDamage = 30f;
    public float jumpAttackCooldown = 6f;
    public float jumpHeight = 6f; 
    private float nextJumpAttackTime = 0f;

    [Header("Referências Internas")]
    public Animator anim;
    private NavMeshAgent agent;
    private Rigidbody rb;
    private BossPillar ultimoPilarNoMapa;

    private GameObject bossHealthCanvas;
    private UnityEngine.UI.Text bossHealthText;

    void Start()
    {
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        
        agent.stoppingDistance = attackRange - 1f;
        if (agent.stoppingDistance < 0) agent.stoppingDistance = 0f;

        if (agent.enabled && !agent.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 15.0f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                agent.Warp(hit.position);
            }
        }

        CreateHealthText();
        
        if (playerTarget == null) 
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if(p != null) playerTarget = p.transform;
        }
        if (playerHealthRef == null && playerTarget != null)
        {
            playerHealthRef = playerTarget.GetComponent<PlayerHealth>();
        }
    }

    void Update()
    {
        if (currentPhase == BossPhase.Cutscene || playerTarget == null) 
        {
            StopMovement();
            return;
        }

        if (currentState == BossState.Charging || 
            currentState == BossState.ParabolicJump || 
            currentState == BossState.Stunned ||
            currentState == BossState.Execution)
        {
            return;
        }

        switch(currentPhase)
        {
            case BossPhase.Phase1: UpdatePhaseCautious(false); break;
            case BossPhase.PillarEvent1: UpdatePillarEvent(); break;
            case BossPhase.Phase2: UpdatePhaseAggressive(); break;
            case BossPhase.PillarEvent2: UpdatePillarEvent(); break;
            case BossPhase.Phase3: UpdatePhaseCautious(true); break; 
            case BossPhase.ReadyToDie: UpdatePhaseExecution(); break; 
        }

        if(anim != null && agent.enabled && currentState != BossState.ParabolicJump)
        {
            anim.SetFloat("Speed", agent.velocity.magnitude);
        }
    }

    private void StopMovement()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh) 
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            if (agent.hasPath) agent.ResetPath(); 
        }
    }

    private void LookAtPlayer()
    {
        if (playerTarget == null) return;
        Vector3 dir = (playerTarget.position - transform.position).normalized;
        dir.y = 0;
        if(dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);
    }

    // =========================================================
    // LÓGICA DAS FASES
    // =========================================================

    private void UpdatePhaseCautious(bool isTired)
    {
        currentState = BossState.Fighting;
        float dist = Vector3.Distance(transform.position, playerTarget.position);

        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = isTired ? walkSpeed : runSpeed * 0.8f; 
            agent.SetDestination(playerTarget.position);
        }

        if(dist <= attackRange + 1.5f && Time.time >= nextAttackTime)
        {
            StartCoroutine(PerformMeleeAttackRoutine());
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void UpdatePhaseAggressive()
    {
        currentState = BossState.Fighting;
        float dist = Vector3.Distance(transform.position, playerTarget.position);

        if(dist > 5.5f && Time.time >= nextJumpAttackTime)
        {
            StartCoroutine(PerformParabolicJump());
            nextJumpAttackTime = Time.time + jumpAttackCooldown;
            return;
        }

        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = runSpeed;
            agent.SetDestination(playerTarget.position);
        }

        if(dist <= attackRange + 1.5f && Time.time >= nextAttackTime)
        {
            StartCoroutine(PerformMeleeAttackRoutine());
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void UpdatePillarEvent()
    {
        currentState = BossState.Evasion;
        float dist = Vector3.Distance(transform.position, playerTarget.position);

        // Assim que se conseguir afastar para 12m ou mais, arranca o Charge mortal!
        if (dist > 12f)
        {
            StartCoroutine(PerformPillarCharge());
            return;
        }

        // Foge do jogador
        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = runSpeed * 1.2f; 
            
            Vector3 dirAway = (transform.position - playerTarget.position).normalized;
            Vector3 fleePos = transform.position + dirAway * 8f;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(fleePos, out hit, 4f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            else
            {
                // Se ficar encurralado, força logo a Carga
                StartCoroutine(PerformPillarCharge());
            }
        }
    }

    private void UpdatePhaseExecution()
    {
        currentState = BossState.Execution;
        if(ultimoPilarNoMapa != null)
        {
            if(Vector3.Distance(transform.position, ultimoPilarNoMapa.transform.position) > 3.5f)
            {
                if (agent.enabled && agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                    agent.speed = walkSpeed * 0.4f; 
                    agent.SetDestination(ultimoPilarNoMapa.transform.position);
                }
            }
            else
            {
                StopMovement();
            }
        }
        else
        {
            StopMovement();
        }
    }

    // =========================================================
    // ATAQUES ESPECIAIS (COROUTINES)
    // =========================================================

    private IEnumerator PerformMeleeAttackRoutine()
    {
        currentState = BossState.Stunned; 
        StopMovement();
        LookAtPlayer();
        
        string animName = Random.value > 0.5f ? "Attack1" : "Attack2";
        TryTriggerAnim(animName);

        yield return new WaitForSeconds(0.8f); 
        
        if (playerTarget != null && playerHealthRef != null && Vector3.Distance(transform.position, playerTarget.position) <= attackRange + 2f)
        {
            playerHealthRef.TakeDamage(attackDamage, transform.position);
        }

        yield return new WaitForSeconds(0.6f);
        currentState = BossState.Idle;
    }

    private IEnumerator PerformPillarCharge()
    {
        currentState = BossState.Charging;
        StopMovement();
        LookAtPlayer();
        
        TryTriggerAnim("Roar"); 
        yield return new WaitForSeconds(1.0f);

        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = chargeSpeed;
            agent.acceleration = 120f; 
        }
        
        float elapsedCharge = 0f;
        while (currentState == BossState.Charging)
        {
            if (agent.enabled && agent.isOnNavMesh) agent.SetDestination(playerTarget.position);
            
            if (Vector3.Distance(transform.position, playerTarget.position) <= 2.2f)
            {
                if (playerHealthRef != null) playerHealthRef.TakeDamage(30f, transform.position);
                break; 
            }
            
            elapsedCharge += Time.deltaTime;
            if (elapsedCharge >= 6f) break; 
            
            yield return null;
        }

        currentState = BossState.Idle;
        StopMovement();
    }

    private IEnumerator PerformParabolicJump()
    {
        currentState = BossState.ParabolicJump;
        StopMovement();
        LookAtPlayer();
        TryTriggerAnim("JumpAttack");

        yield return new WaitForSeconds(0.4f); 

        bool originalKinematic = false;
        if (rb != null)
        {
            originalKinematic = rb.isKinematic;
            rb.isKinematic = true;
        }

        agent.enabled = false;
        Vector3 startPos = transform.position;
        Vector3 targetPos = playerTarget.position;
        float duration = 0.8f; 
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float normalizedTime = elapsed / duration;
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, normalizedTime);
            currentPos.y += Mathf.Sin(normalizedTime * Mathf.PI) * jumpHeight;
            transform.position = currentPos;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        
        if (rb != null) rb.isKinematic = originalKinematic;

        agent.enabled = true;
        if (!agent.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
                agent.Warp(hit.position);
        }
        StopMovement();

        if (playerTarget != null && Vector3.Distance(transform.position, playerTarget.position) <= jumpAttackRadius + 2f)
        {
            if (playerHealthRef != null) playerHealthRef.TakeDamage(jumpAttackDamage, transform.position); 
        }

        yield return new WaitForSeconds(1.5f);
        currentState = BossState.Idle;
    }

    // =========================================================
    // DANO E TRANSIÇÕES DE VIDA DO BOSS (O SEGREDO)
    // =========================================================

    public void TakeDamage(float amount)
    {
        if(currentPhase == BossPhase.ReadyToDie || currentPhase == BossPhase.Cutscene) return; 

        // SÓ FICA IMUNE DURANTE OS EVENTOS DO PILAR! Fora isso, podes bater-lhe livremente.
        if (currentPhase == BossPhase.PillarEvent1 || currentPhase == BossPhase.PillarEvent2)
        {
            return;
        }

        currentHealth -= amount;
        
        if (currentState == BossState.Idle || currentState == BossState.Fighting)
        {
            StartCoroutine(HitStunRoutine());
        }

        UpdateHealthUI();
        CheckPhases();
    }

    private void CheckPhases()
    {
        if (currentHealth <= 25f && currentPhase != BossPhase.ReadyToDie)
        {
            currentPhase = BossPhase.ReadyToDie;
            EncontrarUltimoPilar();
            return;
        }

        if (currentHealth <= 125f && currentPhase == BossPhase.Phase2)
        {
            currentPhase = BossPhase.PillarEvent2;
            currentState = BossState.Idle;
            return;
        }

        if (currentHealth <= 225f && currentPhase == BossPhase.Phase1)
        {
            currentPhase = BossPhase.PillarEvent1;
            currentState = BossState.Idle;
            return;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if(currentState == BossState.Charging && other.CompareTag("Pilar"))
        {
            BossPillar pilar = other.GetComponentInParent<BossPillar>();
            if(pilar != null && !pilar.jaDestruido)
            {
                pilar.ReceberImpactoDoBoss(); 
                
                currentState = BossState.Idle;
                StopMovement();
                
                // Explodir o pilar força o Boss a avançar de fase no guião!
                if (currentPhase == BossPhase.PillarEvent1) currentPhase = BossPhase.Phase2;
                else if (currentPhase == BossPhase.PillarEvent2) currentPhase = BossPhase.Phase3;
            }
        }
    }

    // =========================================================
    // UTILS
    // =========================================================

    private IEnumerator HitStunRoutine()
    {
        currentState = BossState.Stunned;
        StopMovement();
        TryTriggerAnim("Hit");
        yield return new WaitForSeconds(0.6f);
        currentState = BossState.Idle; 
    }

    private void EncontrarUltimoPilar()
    {
        BossPillar[] todosOsPilares = FindObjectsByType<BossPillar>(FindObjectsSortMode.None);
        foreach(BossPillar p in todosOsPilares)
        {
            if(!p.jaDestruido)
            {
                ultimoPilarNoMapa = p;
                p.isLastPillar = true; 
                p.bossAssociado = this; 
                break; 
            }
        }
    }

    public bool IsReadyForExecution()
    {
        return currentPhase == BossPhase.ReadyToDie && agent != null && agent.isStopped;
    }

    public void ExecuteFinalCutscene(Transform playerTransform)
    {
        currentPhase = BossPhase.Cutscene;
        currentState = BossState.Execution;
        if (bossHealthCanvas != null) Destroy(bossHealthCanvas);
        if(anim != null) anim.Play("FinalHit");
        Invoke("LoadNextScene", 4f);
    }

    private void TryTriggerAnim(string triggerName)
    {
        if(anim == null) return;
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == triggerName && param.type == AnimatorControllerParameterType.Trigger)
            {
                anim.SetTrigger(triggerName);
                return;
            }
        }
    }

    private void CreateHealthText()
    {
        bossHealthCanvas = new GameObject("BossHealthCanvas");
        Canvas c = bossHealthCanvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 100;
        bossHealthCanvas.AddComponent<UnityEngine.UI.CanvasScaler>();

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(bossHealthCanvas.transform);
        bossHealthText = textObj.AddComponent<UnityEngine.UI.Text>();

        Font arialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "LegacyRuntime.ttf");
        bossHealthText.font = arialFont;
        bossHealthText.fontSize = 24;
        bossHealthText.color = Color.red;
        bossHealthText.alignment = TextAnchor.UpperCenter;
        
        UpdateHealthUI();

        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0, -20);
        rect.sizeDelta = new Vector2(300, 50);
    }

    private void UpdateHealthUI()
    {
        if (bossHealthText != null)
        {
            bossHealthText.text = "BOSS HP: " + currentHealth;
        }
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
