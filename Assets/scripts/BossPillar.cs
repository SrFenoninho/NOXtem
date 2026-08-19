using UnityEngine;

public class BossPillar : MonoBehaviour, IInteractable
{



    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("Modelos")]

    public GameObject pilarIntacto;
    public GameObject pilarDestruido;

    [HideInInspector] public bool isLastPillar = false;
    [HideInInspector] public FinalBossAI bossAssociado;
    [HideInInspector] public BossController novoBossAssociado;
    [HideInInspector] public bool jaDestruido = false; 





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    private void Start()
    {
        if(pilarIntacto != null) pilarIntacto.SetActive(true);
        if(pilarDestruido != null) pilarDestruido.SetActive(false);

        if (bossAssociado == null) bossAssociado = FindFirstObjectByType<FinalBossAI>();
        if (novoBossAssociado == null) novoBossAssociado = FindFirstObjectByType<BossController>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (isLastPillar && other.CompareTag("Player"))
        {
            AtivarCutsceneFinal(other.gameObject);
            return;
        }

        VerificarColisaoComBoss(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isLastPillar && collision.gameObject.CompareTag("Player"))
        {
            AtivarCutsceneFinal(collision.gameObject);
            return;
        }

        VerificarColisaoComBoss(collision.gameObject);
    }




    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    private void AtivarCutsceneFinal(GameObject jogador)
    {
        if (novoBossAssociado != null && novoBossAssociado.currentPhase == BossController.BossPhase.ReadyToDie)
        {
            novoBossAssociado.ExecuteFinalCutscene();
        }
        else if (bossAssociado != null && bossAssociado.IsReadyForExecution())
        {
            bossAssociado.ExecuteFinalCutscene(jogador.transform);
        }
    }

    private void VerificarColisaoComBoss(GameObject obj)
    {
        if (jaDestruido || isLastPillar) return;

        BossController boss = obj.GetComponentInParent<BossController>();
        if (boss == null) boss = obj.GetComponent<BossController>();

        if (boss != null)
        {
            if (boss.currentState is BossState_PillarCharge chargeState)
            {
                ReceberImpactoDoBoss();
                chargeState.OnPillarHit(boss);

                if (boss.health != null)
                {
                    bool wasInvul = boss.health.isInvulnerable;
                    boss.health.isInvulnerable = false; 
                    boss.health.TakeDamage(10f);
                    boss.health.isInvulnerable = wasInvul;
                }
            }
        }
    }




    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void ReceberImpactoDoBoss()
    {
        if(jaDestruido || isLastPillar) return;

        jaDestruido = true;

        if(pilarDestruido != null) 
        {
            GameObject destrocos = Instantiate(pilarDestruido);
            destrocos.transform.position = pilarIntacto != null ? pilarIntacto.transform.position : transform.position;
            destrocos.transform.rotation = pilarIntacto != null ? pilarIntacto.transform.rotation : transform.rotation;
            destrocos.SetActive(true);
        }

        if(pilarIntacto != null) pilarIntacto.SetActive(false);
    }

    public void Interact(GameObject interactor)
    {
        if(isLastPillar) AtivarCutsceneFinal(interactor);
    }

    public string GetInteractMessage() 
    {
        if(isLastPillar)
        {
            if (novoBossAssociado != null && novoBossAssociado.currentPhase == BossController.BossPhase.ReadyToDie) return "Finalizar!";
            if (bossAssociado != null && bossAssociado.IsReadyForExecution()) return "Finalizar!";
        }
        return "";
    }
}
