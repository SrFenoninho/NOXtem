using UnityEngine;
using UnityEngine.UI;

public class LockedDoor : MonoBehaviour
{
    public Text messageText;
    public string requiredKeyID = "Door"; // "Door" is just an example key ID
    public bool isLocked = true;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>(); 
        rb.isKinematic = true;
    }
    public void OnTriggerEnter(Collider other)
    {
        if (!isLocked) return;

        if (other.CompareTag("Player"))
        {
            PlayerKeys playerKeys = other.GetComponent<PlayerKeys>();
            if (playerKeys != null && playerKeys.HasKey(requiredKeyID))
            {
                isLocked = false;
                rb.isKinematic = false;
                messageText.text = "A door is unlocked";
            }
            else
            {
                messageText.text = "You need a security key";
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
