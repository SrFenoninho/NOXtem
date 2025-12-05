using UnityEngine;
public class HitboxDefense : MonoBehaviour
{
    private Collider myCollider;

    void Awake()
    {
        myCollider = GetComponent<Collider>();
        myCollider.enabled = false;
        myCollider.isTrigger = true;
    }

    public void EnableDefense()
    {
        myCollider.enabled = true;
        Debug.Log("Defense hitbox enabled");
    }

    public void DisableDefense()
    {
        myCollider.enabled = false;
        Debug.Log("Defense hitbox disabled");
    }
}
