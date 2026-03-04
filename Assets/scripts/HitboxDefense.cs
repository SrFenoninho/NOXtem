using UnityEngine;

public class HitboxDefense : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Defense Settings")]
    // Prefab do "muro" de defesa instanciado à frente do jogador ao defender
    // Nota: esta solução demorou muito a chegar, ainda tem bugs — trabalho em progresso!
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
        if (currentDefenseWall != null) return; // já existe uma parede ativa

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
            Debug.Log("Parede de defesa destruída!");
        }
    }
}
