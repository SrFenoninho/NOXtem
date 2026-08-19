using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BossHealth))]
[RequireComponent(typeof(BossMovement))]
[RequireComponent(typeof(BossCombat))]
[RequireComponent(typeof(BossVFX))]
public class BossController : MonoBehaviour
{




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    public enum BossPhase { Phase1, PillarEvent1, Phase2, PillarEvent2, Phase3, ReadyToDie, Cutscene }

    [Header("Estado Central")]
    public BossPhase currentPhase = BossPhase.Phase1;
    public IBossState currentState;

    [Header("Referencias")]
    public Transform playerTarget;
    public PlayerHealth playerHealthRef;
    public Animator anim;

    public BossHealth health { get; private set; }
    public BossMovement movement { get; private set; }
    public BossCombat combat { get; private set; }
    public BossVFX vfx { get; private set; }

    public string nextSceneName = "Scene_Epilogo";
    [HideInInspector] public BossPillar ultimoPilarNoMapa;
    [HideInInspector] public float fleeCooldownTimer = 0f;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
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

            if (movement.VerificarSeEstaPreso())
            {
                movement.StopMovement();

                if (currentState is BossState_Flee)
                {
                    ChangeState(new BossState_Flee());
                }
                else if (currentState is BossState_Phase1_Cautious || currentState is BossState_Phase3_Exhausted)
                {
                    TriggerPhase(currentPhase);
                }
                else if (currentState is BossState_PillarCharge)
                {
                    TriggerPhase(currentPhase);
                }
            }
        }
    }





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void ChangeState(IBossState newState)
    {
        string oldName = (currentState != null) ? currentState.GetType().Name : "Null";
        string newName = (newState != null) ? newState.GetType().Name : "Null";

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
        if (currentState is BossState_MeleeAttack || currentState is BossState_JumpAttack || currentState is BossState_PillarCharge || currentState is BossState_ReadyForCutscene)
            return;

        if (consecutiveHits >= 3)
        {
            health.ResetConsecutiveHits();
            ChangeState(new BossState_MeleeAttack());
            return;
        }

        ChangeState(new BossState_Stunned());
    }





    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
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
        ChangeState(null);

        if (anim != null) 
        {
            int estadoID = Animator.StringToHash("FinalHit");
            if (anim.HasState(0, estadoID))
            {
                anim.Play("FinalHit");
            }
        }

        Invoke("LoadNextScene", 4f);
    }

    private void LoadNextScene()
    {
        LoadingManager.Carregar(nextSceneName);
    }
}
