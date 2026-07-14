using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BossHealth))]
[RequireComponent(typeof(BossMovement))]
[RequireComponent(typeof(BossCombat))]
[RequireComponent(typeof(BossVFX))]
public class BossController : MonoBehaviour
{
    public enum BossPhase { Phase1, PillarEvent1, Phase2, PillarEvent2, Phase3, ReadyToDie, Cutscene }
    
    [Header("Estado Central")]
    public BossPhase currentPhase = BossPhase.Phase1;
    public IBossState currentState;

    [Header("Referencias")]
    public Transform playerTarget;
    public PlayerHealth playerHealthRef;
    public Animator anim;

    // Componentes Modulares
    public BossHealth health { get; private set; }
    public BossMovement movement { get; private set; }
    public BossCombat combat { get; private set; }
    public BossVFX vfx { get; private set; }

    public string nextSceneName = "Scene_Epilogo";
    [HideInInspector] public BossPillar ultimoPilarNoMapa;
    [HideInInspector] public float fleeCooldownTimer = 0f; // Cooldown de segurança para o boss não fugir sem parar

    void Awake()
    {
        health = GetComponent<BossHealth>();
        movement = GetComponent<BossMovement>();
        combat = GetComponent<BossCombat>();
        vfx = GetComponent<BossVFX>();

        if (playerTarget == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTarget = p.transform;
        }

        if (playerHealthRef == null && playerTarget != null)
        {
            playerHealthRef = playerTarget.GetComponent<PlayerHealth>();
        }
    }

    void Start()
    {
        health.Initialize(this);
        movement.Initialize(this);
        combat.Initialize(this);
        vfx.Initialize(this);

        TriggerPhase(BossPhase.Phase1);
    }

    void Update()
    {
        if (playerTarget == null) return;
        
        // Decrementa o temporizador de cooldown de fuga
        if (fleeCooldownTimer > 0f)
        {
            fleeCooldownTimer -= Time.deltaTime;
        }

        if (currentState != null)
        {
            currentState.UpdateState(this);
        }

        if (movement != null) 
        {
            movement.UpdateAnimatorSpeed();

            // Se o corpo físico do Boss colidir com uma parede e ele não conseguir avançar, força desvio instantâneo!
            if (movement.VerificarSeEstaPreso())
            {
                Debug.LogWarning("⚠️ [AI] Boss detetou obstrução (parede física). A forçar desvio de rota!");
                movement.StopMovement();

                if (currentState is BossState_Flee)
                {
                    // Se estava a fugir, força uma nova fuga para outro ponto tático livre
                    ChangeState(new BossState_Flee());
                }
                else if (currentState is BossState_Phase1_Cautious || currentState is BossState_Phase3_Exhausted)
                {
                    // Se estava a patrulhar, força a recriação do estado para limpar o hasTarget e escolher novo ponto
                    TriggerPhase(currentPhase);
                }
                else if (currentState is BossState_PillarCharge)
                {
                    // Se estava a carregar/fugir para o pilar e bateu na parede, reinicia a fase para ele recuar em segurança
                    Debug.LogWarning("⚠️ [AI] Boss ficou obstruído na parede durante a fase do pilar. A resetar investida!");
                    TriggerPhase(currentPhase);
                }
            }
        }
    }

    public void ChangeState(IBossState newState)
    {
        string oldName = (currentState != null) ? currentState.GetType().Name : "Null";
        string newName = (newState != null) ? newState.GetType().Name : "Null";
        Debug.Log("🔄 [FSM] Estado alterado de " + oldName + " para " + newName);

        if (currentState != null)
        {
            currentState.ExitState(this);
        }
        
        currentState = newState;
        
        if (currentState != null)
        {
            currentState.EnterState(this);
        }
    }

    public void TriggerPhase(BossPhase newPhase)
    {
        currentPhase = newPhase;
        
        switch(currentPhase)
        {
            case BossPhase.Phase1:
                ChangeState(new BossState_Phase1_Cautious());
                break;
            case BossPhase.PillarEvent1:
            case BossPhase.PillarEvent2:
                ChangeState(new BossState_PillarCharge());
                break;
            case BossPhase.Phase2:
                ChangeState(new BossState_Phase2_Aggro());
                break;
            case BossPhase.Phase3:
                ChangeState(new BossState_Phase3_Exhausted());
                break;
            case BossPhase.ReadyToDie:
                EncontrarUltimoPilar();
                ChangeState(new BossState_ReadyForCutscene());
                break;
        }
    }

    public void OnTookDamage(int consecutiveHits)
    {
        // Se já está a atacar, a saltar, na carga ao pilar ou na cutscene final, ele tem Hyper Armor (ignora hits)
        if (currentState is BossState_MeleeAttack || currentState is BossState_JumpAttack || currentState is BossState_PillarCharge || currentState is BossState_ReadyForCutscene)
            return;

        if (consecutiveHits >= 3)
        {
            health.ResetConsecutiveHits();
            ChangeState(new BossState_MeleeAttack());
            return;
        }

        // Se levar hit 1 ou 2, entra/renova o atordoamento (Stun)
        ChangeState(new BossState_Stunned());
    }

    private void EncontrarUltimoPilar()
    {
        BossPillar[] todosOsPilares = FindObjectsByType<BossPillar>(FindObjectsSortMode.None);
        System.Collections.Generic.List<BossPillar> pilaresValidos = new System.Collections.Generic.List<BossPillar>();

        foreach(BossPillar p in todosOsPilares)
        {
            if(!p.jaDestruido)
            {
                pilaresValidos.Add(p);
            }
        }

        if (pilaresValidos.Count > 0)
        {
            int randomIndex = Random.Range(0, pilaresValidos.Count);
            ultimoPilarNoMapa = pilaresValidos[randomIndex];
            ultimoPilarNoMapa.isLastPillar = true; 
            ultimoPilarNoMapa.bossAssociado = GetComponent<FinalBossAI>();
        }
    }

    public void ExecuteFinalCutscene()
    {
        currentPhase = BossPhase.Cutscene;
        ChangeState(null); // Desliga states
        if(anim != null) anim.Play("FinalHit");
        Invoke("LoadNextScene", 4f);
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
