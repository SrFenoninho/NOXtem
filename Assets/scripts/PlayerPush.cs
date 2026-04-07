using UnityEngine;

public class PlayerPush : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    public float pushForce = 5f; // forca aplicada a objetos com Rigidbody ao colidir

    // ---------------------------------------------
    //  COLISaO
    // ---------------------------------------------
    // Chamado automaticamente pelo CharacterController ao colidir com um objeto
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;

        // Ignorar objetos sem Rigidbody ou cinematicos
        if (rb == null || rb.isKinematic) return;

        // Empurrar apenas horizontalmente, na direcao do movimento
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        rb.AddForce(pushDir * pushForce, ForceMode.Impulse);
    }
}
