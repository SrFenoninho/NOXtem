using UnityEngine;

public class SimpleLockedDoor : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    public bool isLocked = true;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private Rigidbody rb;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = isLocked; // cinematico enquanto trancada
    }

    // ---------------------------------------------
    //  DESBLOQUEIO
    // ---------------------------------------------
    // Chamado externamente (ex: CardReaderInteraction) para destrancar a porta
    public void Unlock()
    {
        if (!isLocked) return;

        isLocked = false;

        if (rb != null)
            rb.isKinematic = false; // ativar fisica - a porta cai

        // Debug.Log(gameObject.name + " foi destrancada!");
    }
}
