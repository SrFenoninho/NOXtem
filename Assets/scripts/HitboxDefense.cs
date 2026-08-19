using UnityEngine;

public class HitboxDefense : MonoBehaviour
{




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("Defense Settings")]

    public GameObject defenseWallPrefab;
    public Transform spawnPoint;





    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private GameObject currentDefenseWall;





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void ActivateDefense()
    {
        if (currentDefenseWall != null) return;

        if (defenseWallPrefab != null && spawnPoint != null)
        {
            currentDefenseWall = Instantiate(defenseWallPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);

            PlayerHealth ph = GetPlayerHealth();
            if(ph != null) ph.isDefending = true;

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

        }
    }





    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
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





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    private void OnDrawGizmos()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = new Color(0f, 0.6f, 1f, 0.4f);
            Gizmos.matrix = spawnPoint.localToWorldMatrix;

            Gizmos.DrawCube(Vector3.zero, new Vector3(2.6f, 2.0f, 0.4f));

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(2.6f, 2.0f, 0.4f));
        }
    }
}
