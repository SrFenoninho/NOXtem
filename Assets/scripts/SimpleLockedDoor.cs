using UnityEngine;

public class SimpleLockedDoor : MonoBehaviour
{
    public bool isLocked = true;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = isLocked;
        }
    }

    public void Unlock()
    {
        if (!isLocked) return;

        isLocked = false;

        if (rb != null)
        {
            rb.isKinematic = false;
        }

        Debug.Log(gameObject.name + " has been unlocked!");
    }
}