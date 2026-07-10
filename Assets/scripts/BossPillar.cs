using UnityEngine;

public class BossPillar : MonoBehaviour, IInteractable
{
    [Header("Modelos")]
    public GameObject pilarIntacto;
    public GameObject pilarDestruido;
    
    [HideInInspector] public bool isLastPillar = false;
    [HideInInspector] public FinalBossAI bossAssociado;
    [HideInInspector] public bool jaDestruido = false; 

    private void Start()
    {
        if(pilarIntacto != null) pilarIntacto.SetActive(true);
        if(pilarDestruido != null) pilarDestruido.SetActive(false);
    }

    public void ReceberImpactoDoBoss()
    {
        if(jaDestruido || isLastPillar) return;
        
        jaDestruido = true;
        
        // Copia (Instantiate) o modelo destruído e teletransporta-o para o ponto exato onde estava o pilar intacto!
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
    //  IINTERACTABLE
    // ---------------------------------------------
    public void Interact(GameObject interactor)
    {
        if(isLastPillar && bossAssociado != null && bossAssociado.IsReadyForExecution())
        {
            bossAssociado.ExecuteFinalCutscene(interactor.transform);
        }
    }

    public string GetInteractMessage() 
    {
        if(isLastPillar && bossAssociado != null && bossAssociado.IsReadyForExecution())
            return "Agarrar Pilar (Finalizar)";
            
        return "";
    }
}
