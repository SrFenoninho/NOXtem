using UnityEngine;
using UnityEngine.UI;

public class LockedDoor : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    public Text messageText;
    public string requiredKeyID = "Door"; // ID da chave necessária para destravar
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
        rb.isKinematic = true; // imóvel enquanto trancada
    }

    // ---------------------------------------------
    //  DESBLOQUEIO POR COLISÃO
    // ---------------------------------------------
    // Porta cai fisicamente ao ser desbloqueada pelo jogador com a chave certa
    public void OnTriggerEnter(Collider other)
    {
        if (!isLocked) return;

        if (other.CompareTag("Player"))
        {
            PlayerKeys playerKeys = other.GetComponent<PlayerKeys>();
            if (playerKeys != null && playerKeys.HasKey(requiredKeyID))
            {
                isLocked = false;
                rb.isKinematic = false; // ativar física — a porta cai
                messageText.text = "A door is unlocked";
            }
            else
            {
                messageText.text = $"You need a {requiredKeyID} key";
            }
        }

        CancelInvoke(nameof(ClearMessage));
        Invoke(nameof(ClearMessage), 2f);
    }

    void ClearMessage()
    {
        messageText.text = "";
    }
}
