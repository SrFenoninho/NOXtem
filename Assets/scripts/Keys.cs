using UnityEngine;
using UnityEngine.UI;

public class Keys : MonoBehaviour, IInteractable
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Key Settings")]
    public string keyName = "Door"; // ID único desta chave

    [Header("UI")]
    public Text messageText;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private bool alreadyPickedUp = false;

    // ---------------------------------------------
    //  INTERFACE IInteractable
    // ---------------------------------------------
    public string GetInteractMessage()
    {
        return $"Press E to pick up {keyName} key.";
    }

    public void Interact(GameObject player)
    {
        if (alreadyPickedUp) return;

        PlayerKeys playerKeys = player.GetComponent<PlayerKeys>();
        if (playerKeys != null)
        {
            playerKeys.AddKey(keyName);
            alreadyPickedUp = true;

            if (messageText != null)
            {
                messageText.text = $"You picked up the key: {keyName}.";
                Invoke(nameof(ClearMessage), 2f);
            }

            // Desativar colisões e esconder o objeto imediatamente
            GetComponent<Collider>().enabled = false;

            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
                meshRenderer.enabled = false;

            // Destruir o GameObject após a mensagem desaparecer
            Destroy(gameObject, 2.1f);
        }
    }

    // ---------------------------------------------
    //  UI
    // ---------------------------------------------
    void ClearMessage()
    {
        if (messageText != null)
            messageText.text = "";
    }
}