using UnityEngine;

public interface IInteractable
{



    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    void Interact(GameObject player);
    string GetInteractMessage();
}
