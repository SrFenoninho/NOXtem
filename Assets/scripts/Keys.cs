using UnityEngine;
using UnityEngine.UI;

public class Keys : MonoBehaviour
{
    public Text messageText;
    public string keyName = "Door"; // "Door" is just an example key name

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerKeys playerKeys = other.GetComponent<PlayerKeys>();
            if (playerKeys != null)
            {
                playerKeys.AddKey(keyName);
                messageText.text = $"You picked up the key: {keyName}.";

                GetComponent<Collider>().enabled = false; // Disable further collisions
                GetComponent<MeshRenderer>().enabled = false; // Hide the key object

            }
        }
        CancelInvoke(nameof(ClearMessage));
        Invoke(nameof(ClearMessage), 2f);
    }
    void ClearMessage()
    {
        messageText.text = "";
        Destroy(gameObject);
    }
}
