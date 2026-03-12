using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [Header("Respawn")]
    public Transform respawnPoint;

    void OnTriggerEnter(Collider other)
    {
        // player
        if (other.CompareTag("Player"))
        {
            Teleport(other.gameObject);
            return;
        }

        // inimigo
        if (other.GetComponent<EnemyAI>() != null)
        {
            Teleport(other.gameObject);
        }
    }

    void Teleport(GameObject obj)
    {
        CharacterController cc = obj.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            obj.transform.position = respawnPoint.position;
            cc.enabled = true;
        }
        else
        {
            obj.transform.position = respawnPoint.position;
        }
    }
}