using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class FinalBossAI : MonoBehaviour
{
    public enum BossPhase { Phase1, PillarCharge1, Phase2, PillarCharge2, Phase3, ReadyToDie, Cutscene }
    
    [Header("Estado Atual (Apenas leitura)")]
    public BossPhase currentPhase = BossPhase.Phase1;
    public bool debugMode = true;

    [Header("Referencias ABSOLUTAS (ARRASTAR NO UNITY)")]
    public Transform playerTarget;
    public PlayerHealth playerHealthRef;

    public float currentHealth;
    
    [Header("Configurações Base")]
    public float maxHealth = 300f;
    public string nextSceneName = "Scene_Epilogo";

    [Header("Movimento")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 7f;
    public float retreatSpeed = 4f;
    public float chargeSpeed = 15f;

    [Header("Ataques Básicos")]
    public float attackRange = 2.5f;
    public float attackDamage = 15f;
    public float attackCooldown = 2.5f;
    private float nextAttackTime = 0f;

    [Header("Jump Attack (Área Imbloqueável)")]
    public float jumpAttackRadius = 6f;
    public float jumpAttackDamage = 30f;
    public float jumpAttackCooldown = 5f;
    private float nextJumpAttackTime = 0f;

    [Header("Referências Internas")]
    public Animator anim;
    private NavMeshAgent agent;

    private bool isCharging = false;
    private bool isInvulnerable = false;
    
    private BossPillar ultimoPilarNoMapa;

    private GameObject bossHealthCanvas;
    private UnityEngine.UI.Text bossHealthText;

    private BossPillar alvoFugaAtual = null;
    private float tempoInicioFuga = 0f;

    private int consecutiveHitsReceived = 0;

    void Start()
    {
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();
        
        agent.stoppingDistance = attackRange - 1f;
        if (agent.stoppingDistance < 0) agent.stoppingDistance = 0f;

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
            agent.isStopped = true;
            return;
        }

        CheckPhaseTransitions();

        switch(currentPhase)
        {
            case BossPhase.Phase1: UpdatePhase1(); break;
            case BossPhase.PillarCharge1:
            case BossPhase.PillarCharge2: UpdatePillarCharge(); break;
            case BossPhase.Phase2: UpdatePhase2(); break;
            case BossPhase.Phase3: UpdatePhase3(); break;
            case BossPhase.ReadyToDie: UpdateReadyToDie(); break; 
        }

        if(anim != null && agent.enabled && currentPhase != BossPhase.Cutscene)
        {
            anim.SetFloat("Speed", agent.velocity.magnitude);
        }
    }

    // ---------------------------------------------
    //  MUDANÇA DE FASES
    // ---------------------------------------------
    void CheckPhaseTransitions()
    {
        if(currentHealth <= 25f && currentPhase != BossPhase.ReadyToDie)
        {
            currentPhase = BossPhase.ReadyToDie;
            isInvulnerable = true;
            
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
            return;
        }

        if(currentHealth <= 125f && currentPhase == BossPhase.Phase2)
        {
            currentPhase = BossPhase.PillarCharge2;
            isInvulnerable = true;
            isCharging = false;
        }
        else if(currentHealth <= 225f && currentPhase == BossPhase.Phase1)
        {
            currentPhase = BossPhase.PillarCharge1;
            isInvulnerable = true;
            isCharging = false;
        }
    }

    // ---------------------------------------------
    //  COMPORTAMENTOS DE CADA FASE
    // ---------------------------------------------
    void UpdatePhase1()
    {
        if (playerTarget == null) return;
        float dist = Vector3.Distance(transform.position, playerTarget.position);
        
        if(dist <= attackRange + 2.5f)
        {
            agent.isStopped = true;
            LookAtPlayer(); 

            if(Time.time >= nextAttackTime)
            {
                TryTriggerAnim("Attack1"); 
                
                if (playerHealthRef != null) playerHealthRef.TakeDamage(attackDamage, transform.position);
                else Debug.LogError("ERRO: O BOSS TENTOU ATACAR MAS O playerHealthRef está VAZIO NO INSPECTOR!");

                nextAttackTime = Time.time + attackCooldown;
                consecutiveHitsReceived = 0; 
            }
        }
        else if (dist <= attackRange + 5f && Time.time < nextAttackTime)
        {
            agent.isStopped = false;
            agent.speed = retreatSpeed;
            Vector3 pointBehind = transform.position + (transform.position - playerTarget.position).normalized * 5f;
            agent.SetDestination(pointBehind);
        }
        else
        {
            agent.isStopped = false;
            agent.speed = walkSpeed;
            agent.SetDestination(playerTarget.position);
        }
    }

    void UpdatePhase2()
    {
        if (playerTarget == null) return;
        float dist = Vector3.Distance(transform.position, playerTarget.position);
        agent.isStopped = false;
        agent.speed = runSpeed;
        agent.SetDestination(playerTarget.position);

        if(dist <= jumpAttackRadius + 1.5f && Time.time >= nextJumpAttackTime)
        {
            agent.isStopped = true; 
            LookAtPlayer();
            TryTriggerAnim("JumpAttack");
            nextJumpAttackTime = Time.time + jumpAttackCooldown;
            Invoke("TriggerAoEDamage", 0.7f); 
            consecutiveHitsReceived = 0;
            return;
        }

        if(dist <= attackRange + 2.5f && Time.time >= nextAttackTime)
        {
            LookAtPlayer();
            TryTriggerAnim("Attack2");
            if (playerHealthRef != null) playerHealthRef.TakeDamage(attackDamage + 10f, transform.position); 
            nextAttackTime = Time.time + (attackCooldown - 1f); 
            consecutiveHitsReceived = 0;
        }
    }

    void UpdatePhase3()
    {
        if (playerTarget == null) return;
        float dist = Vector3.Distance(transform.position, playerTarget.position);
        agent.isStopped = false;
        agent.speed = walkSpeed * 0.6f; 
        agent.SetDestination(playerTarget.position);

        if(dist <= attackRange + 2.5f && Time.time >= nextAttackTime)
        {
            LookAtPlayer();
            TryTriggerAnim("Attack1");
            if (playerHealthRef != null) playerHealthRef.TakeDamage(attackDamage - 5f, transform.position);
            nextAttackTime = Time.time + (attackCooldown + 1.5f); 
            consecutiveHitsReceived = 0;
        }
    }

    void UpdatePillarCharge()
    {
        if (playerTarget == null) return;
        
        agent.isStopped = false; 

        if(!isCharging)
        {
            if (alvoFugaAtual == null)
            {
                alvoFugaAtual = GetFarthestPillar();
                tempoInicioFuga = Time.time;
            }

            agent.speed = runSpeed;
            if (alvoFugaAtual != null) agent.SetDestination(alvoFugaAtual.transform.position);
            
            float distToAlvo = alvoFugaAtual != null ? Vector3.Distance(transform.position, alvoFugaAtual.transform.position) : 0f;
            if(distToAlvo < 4f || Time.time - tempoInicioFuga > 3.5f) 
            {
                isCharging = true;
                alvoFugaAtual = null;
            }
        }
        else
        {
            agent.speed = chargeSpeed;
            agent.SetDestination(playerTarget.position);
            LookAtPlayer();
            
            float dist = Vector3.Distance(transform.position, playerTarget.position);
            if(dist <= attackRange + 2.5f)
            {
                if (playerHealthRef != null) playerHealthRef.TakeDamage(30f, transform.position);
                isCharging = false; 
                consecutiveHitsReceived = 0;
            }
        }
    }

    private BossPillar GetFarthestPillar()
    {
        BossPillar[] pilares = FindObjectsByType<BossPillar>(FindObjectsSortMode.None);
        BossPillar bestPillar = null;
        float maxDist = -1f;

        foreach(BossPillar p in pilares)
        {
            if (p == null || !p.gameObject.activeInHierarchy || p.jaDestruido) continue;

            float distToPlayer = Vector3.Distance(p.transform.position, playerTarget.position);
            if(distToPlayer > maxDist)
            {
                maxDist = distToPlayer;
                bestPillar = p;
            }
        }
        
        return bestPillar;
    }

    void UpdateReadyToDie()
    {
        if(ultimoPilarNoMapa != null)
        {
            float distParaPilar = Vector3.Distance(transform.position, ultimoPilarNoMapa.transform.position);
            
            if(distParaPilar > 3f)
            {
                agent.isStopped = false;
                agent.speed = walkSpeed * 0.5f; 
                agent.SetDestination(ultimoPilarNoMapa.transform.position);
            }
            else
            {
                agent.isStopped = true;
            }
        }
        else
        {
            agent.isStopped = true;
        }
    }

    // ---------------------------------------------
    //  MECÂNICAS EXTRAS
    // ---------------------------------------------
    private void LookAtPlayer()
    {
        if (playerTarget == null) return;
        Vector3 dir = (playerTarget.position - transform.position).normalized;
        dir.y = 0;
        if(dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);
    }

    private void TriggerAoEDamage()
    {
        if (playerTarget == null) return;
        if(Vector3.Distance(transform.position, playerTarget.position) <= jumpAttackRadius + 2f)
        {
            if (playerHealthRef != null) playerHealthRef.TakeDamage(jumpAttackDamage, transform.position); 
        }
    }

    public void TakeDamage(float amount)
    {
        if(isInvulnerable) return; 

        currentHealth -= amount;
        TryTriggerAnim("Hit");
        CheckPhaseTransitions();

        if(bossHealthText != null)
        {
            bossHealthText.text = "BOSS HP: " + currentHealth;
        }

        consecutiveHitsReceived++;
        if (consecutiveHitsReceived >= 4)
        {
            consecutiveHitsReceived = 0;
            ForceRetaliationAttack();
        }
    }

    private void ForceRetaliationAttack()
    {
        if (playerTarget == null) return;
        
        if (Vector3.Distance(transform.position, playerTarget.position) <= attackRange + 3.5f)
        {
            LookAtPlayer();
            TryTriggerAnim("Attack2");
            if (playerHealthRef != null) playerHealthRef.TakeDamage(attackDamage + 5f, transform.position);
            nextAttackTime = Time.time + attackCooldown; 
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(isCharging && other.CompareTag("Pilar"))
        {
            BossPillar pilar = other.GetComponentInParent<BossPillar>();
            if(pilar != null && !pilar.jaDestruido)
            {
                pilar.ReceberImpactoDoBoss(); 
                isCharging = false;
                isInvulnerable = false;
                
                if (currentPhase == BossPhase.PillarCharge1) currentPhase = BossPhase.Phase2;
                else if (currentPhase == BossPhase.PillarCharge2) currentPhase = BossPhase.Phase3;

                TakeDamage(25f); 
            }
        }
    }

    // ---------------------------------------------
    //  CUTSCENE FINAL DO EPISÓDIO
    // ---------------------------------------------
    public bool IsReadyForExecution()
    {
        if(currentPhase == BossPhase.ReadyToDie && agent.isStopped) 
            return true;
            
        return false;
    }

    public void ExecuteFinalCutscene(Transform playerTransform)
    {
        currentPhase = BossPhase.Cutscene;
        
        if (bossHealthCanvas != null) Destroy(bossHealthCanvas);
        
        if(playerHealthRef != null)
        {
            playerHealthRef.TakeDamage(9999f, transform.position); 
        }
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

        GameObject textObj = new GameObject("HealthText");
        textObj.transform.SetParent(bossHealthCanvas.transform, false);
        bossHealthText = textObj.AddComponent<UnityEngine.UI.Text>();
        bossHealthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        bossHealthText.color = new Color(0.9f, 0.1f, 0.1f, 1f);
        bossHealthText.fontSize = 50;
        bossHealthText.alignment = TextAnchor.UpperCenter;
        bossHealthText.text = "BOSS HP: " + currentHealth;
        
        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.85f);
        rt.anchorMax = new Vector2(1, 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
