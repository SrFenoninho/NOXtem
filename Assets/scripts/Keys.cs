using UnityEngine;
using UnityEngine.UI;

public class Keys : MonoBehaviour, IInteractable
{
    [Header("Key Settings")]
    public string keyName = "Door"; // "Door" is just an example key name

    [Header("UI")]
    public Text messageText;

    private bool alreadyPickedUp = false;

    public string GetInteractMessage()
    {
        return $"Press E to pick up {keyName}";
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

            GetComponent<Collider>().enabled = false; // Disable further collisions

            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
                meshRenderer.enabled = false; // Hide the key object

            Destroy(gameObject, 2.1f);
        }
    }
    void ClearMessage()
    {
        if (messageText != null)
            messageText.text = "";
    }
}