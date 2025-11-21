using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    public string requiredKeyID = "Door";
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
                Debug.Log("Porta destrancada!");
            }
            else
            {
                Debug.Log("Precisa da chave correta");
            }
        }
    }
}
