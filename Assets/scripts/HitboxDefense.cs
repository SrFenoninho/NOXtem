using UnityEngine;

public class HitboxDefense : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Defense Settings")]
    // Prefab do "muro" de defesa instanciado a frente do jogador ao defender
    public GameObject defenseWallPrefab;
    public Transform spawnPoint;            // ponto de origem da parede de defesa

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private GameObject currentDefenseWall;

    // ---------------------------------------------
    //  ATIVAR / DESATIVAR
    // ---------------------------------------------
    public void ActivateDefense()
    {
        if (currentDefenseWall != null) return; // ja existe uma parede ativa

        if (defenseWallPrefab != null && spawnPoint != null)
        {
            currentDefenseWall = Instantiate(defenseWallPrefab, spawnPoint.position, spawnPoint.rotation);
            Debug.Log("Parede de defesa criada!");
        }
    }

    public void DeactivateDefense()
    {
        if (currentDefenseWall != null)
        {
            Destroy(currentDefenseWall);
            currentDefenseWall = null;
            Debug.Log("Parede de defesa destruida!");
        }
    }
}
