using UnityEngine;

public class BossPillar : MonoBehaviour, IInteractable
{
    [Header("Modelos")]
    public GameObject pilarIntacto;
    public GameObject pilarDestruido;
    
    [HideInInspector] public bool isLastPillar = false;
    [HideInInspector] public FinalBossAI bossAssociado;
    [HideInInspector] public BossController novoBossAssociado;
    [HideInInspector] public bool jaDestruido = false; 

    private void Start()
    {
        if(pilarIntacto != null) pilarIntacto.SetActive(true);
        if(pilarDestruido != null) pilarDestruido.SetActive(false);

        // Auto-associa os sistemas de AI do Boss na cena
        if (bossAssociado == null) bossAssociado = FindFirstObjectByType<FinalBossAI>();
        if (novoBossAssociado == null) novoBossAssociado = FindFirstObjectByType<BossController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // NOVIDADE: Se for o último pilar, mal o jogador lhe toca (entra no Trigger) ele despoleta o Final!
        if (isLastPillar && other.CompareTag("Player"))
        {
            AtivarCutsceneFinal(other.gameObject);
            return;
        }

        VerificarColisaoComBoss(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // NOVIDADE: Mesmo que o collider seja sólido, se o jogador esbarrar nele no final, ativa o fim!
        if (isLastPillar && collision.gameObject.CompareTag("Player"))
        {
            AtivarCutsceneFinal(collision.gameObject);
            return;
        }

        VerificarColisaoComBoss(collision.gameObject);
    }

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

        // Procura se o objeto que chocou é o Boss
        BossController boss = obj.GetComponentInParent<BossController>();
        if (boss == null) boss = obj.GetComponent<BossController>();

        if (boss != null)
        {
            // Se o Boss estiver no estado de investida (PillarCharge) aos 225 de vida ou 125 de vida
            if (boss.currentState is BossState_PillarCharge chargeState)
            {
                // Debug.Log("💥 [BossPillar] Boss colidiu fisicamente com o pilar intacto! A ativar destruição e causar dano!");
                ReceberImpactoDoBoss();
                chargeState.OnPillarHit(boss);

                // NOVIDADE: Aplica 10 de DANO imediato ao colidir (remove a invulnerabilidade de charge temporariamente para garantir)
                if (boss.health != null)
                {
                    bool wasInvul = boss.health.isInvulnerable;
                    boss.health.isInvulnerable = false; 
                    boss.health.TakeDamage(10f); // Dá os 10 de dano que pediste
                    boss.health.isInvulnerable = wasInvul; // Devolve o estado
                }
            }
        }
    }

    public void ReceberImpactoDoBoss()
    {
        if(jaDestruido || isLastPillar) return;
        
        jaDestruido = true;
        
        // Copia (Instantiate) o modelo destruído e teletransporta-o para o ponto exato
        if(pilarDestruido != null) 
        {
            GameObject destrocos = Instantiate(pilarDestruido);
            destrocos.transform.position = pilarIntacto != null ? pilarIntacto.transform.position : transform.position;
            destrocos.transform.rotation = pilarIntacto != null ? pilarIntacto.transform.rotation : transform.rotation;
            destrocos.SetActive(true);
        }

        // Esconde o intacto
        if(pilarIntacto != null) pilarIntacto.SetActive(false);
    }

    // ---------------------------------------------
    //  IINTERACTABLE (Mantido como redundância se ele clicar no 'E')
    // ---------------------------------------------
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
