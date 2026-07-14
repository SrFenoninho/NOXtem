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
            currentDefenseWall = Instantiate(defenseWallPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);
            
            PlayerHealth ph = GetPlayerHealth();
            if(ph != null) ph.isDefending = true;

            Debug.Log("Parede de defesa criada!");
        }
    }

    public void DeactivateDefense()
    {
        if (currentDefenseWall != null)
        {
            Destroy(currentDefenseWall);
            currentDefenseWall = null;
            
            PlayerHealth ph = GetPlayerHealth();
            if(ph != null) ph.isDefending = false;

            Debug.Log("Parede de defesa destruida!");
        }
    }

    private PlayerHealth GetPlayerHealth()
    {
        PlayerHealth ph = GetComponentInParent<PlayerHealth>();
        if (ph == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) ph = p.GetComponent<PlayerHealth>();
        }
        return ph;
    }

    // Desenha um gizmo visual na Scene View do Unity para veres a forma e dimensão da barreira de defesa
    private void OnDrawGizmos()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = new Color(0f, 0.6f, 1f, 0.4f); // Azul holográfico semi-transparente
            Gizmos.matrix = spawnPoint.localToWorldMatrix;
            
            // Desenha um cubo a simular o muro físico de defesa (2.6m largura, 2.0m altura, 0.4m espessura)
            Gizmos.DrawCube(Vector3.zero, new Vector3(2.6f, 2.0f, 0.4f));
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(2.6f, 2.0f, 0.4f));
        }
    }
}
