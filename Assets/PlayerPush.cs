using UnityEngine;

public class PlayerPush : MonoBehaviour
{
    public float pushForce = 5f;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;

        if (rb == null || rb.isKinematic)
            return;

        // direção horizontal (não empurramos para cima)
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        // aplica impulso
        rb.AddForce(pushDir * pushForce, ForceMode.Impulse);
    }
}
