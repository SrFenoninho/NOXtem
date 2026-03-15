using UnityEngine;

public class DeathZone : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Respawn")]
    public Transform respawnPoint;

    // ---------------------------------------------
    //  TRIGGER
    // ---------------------------------------------
    void OnTriggerEnter(Collider other)
    {
        // jogador
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

    // ---------------------------------------------
    //  TELETRANSPORTE
    // ---------------------------------------------
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