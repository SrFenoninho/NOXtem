using UnityEngine;

public class HitboxDefense : MonoBehaviour
{
    [Header("Defense Settings")]
    public GameObject defenseWallPrefab; // I need to vent, this was such a difficult solution to reach, I really spent a lot of time on this, and I know there are still bugs but I'm getting closer to making a good defense, BTW IDK what I doing!
    public Transform spawnPoint; 

    private GameObject currentDefenseWall;

    public void ActivateDefense()
    {
        if (currentDefenseWall != null) return;
        if (defenseWallPrefab != null && spawnPoint != null)
        {
            currentDefenseWall = Instantiate(defenseWallPrefab, spawnPoint.position, spawnPoint.rotation);
            Debug.Log("Defense wall created!");
        }
    }

    public void DeactivateDefense()
    {
        if (currentDefenseWall != null)
        {
            Destroy(currentDefenseWall);
            currentDefenseWall = null;
            Debug.Log("Defense wall destroyed!");
        }
    }
}