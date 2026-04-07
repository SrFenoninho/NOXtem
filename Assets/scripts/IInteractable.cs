using UnityEngine;

// ---------------------------------------------
//  INTERFACE DE INTERAcaO
// ---------------------------------------------
// Qualquer objeto interagivel (portas, chaves, terminais...)
// deve implementar esta interface para funcionar com PlayerInteraction.
public interface IInteractable
{
    void Interact(GameObject player);       // logica executada ao pressionar E
    string GetInteractMessage();            // texto mostrado na UI quando o jogador aponta para o objeto
}
