using UnityEngine;

public class PlayerPush : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    public float pushForce = 5f; // força aplicada a objetos com Rigidbody ao colidir

    // ---------------------------------------------
    //  COLISÃO
    // ---------------------------------------------
    // Chamado automaticamente pelo CharacterController ao colidir com um objeto
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;

        // Ignorar objetos sem Rigidbody ou cinemáticos
        if (rb == null || rb.isKinematic) return;

        // Empurrar apenas horizontalmente, na direção do movimento
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        rb.AddForce(pushDir * pushForce, ForceMode.Impulse);
    }
}
