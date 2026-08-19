using UnityEngine;

public class SimpleLockedDoor : MonoBehaviour
{




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    public bool isLocked = true;





    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private Rigidbody rb;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = isLocked;
    }





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void Unlock()
    {
        if (!isLocked) return;

        isLocked = false;

        if (rb != null)
            rb.isKinematic = false;

    }
}
