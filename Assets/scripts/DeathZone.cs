using UnityEngine;

public class DeathZone : MonoBehaviour
{




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("Respawn")]

    public Transform respawnPoint;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Hitbox>() != null) return;

        if (other.CompareTag("Player"))
        {
            Teleport(other.gameObject);
            return;
        }

        if (other.GetComponent<EnemyAI>() != null)
        {
            Teleport(other.gameObject);
        }
    }




    // ---------------------------------------------
    //  PRIVATE METHODS
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
