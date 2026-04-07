using UnityEngine;

public class DarkZone : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    public string zoneID = "DarkZone_1";

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private Transform player;
    private Collider zoneCollider;
    private Lighter lighter;
    private bool playerInZone = false;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        zoneCollider = GetComponent<Collider>();
        zoneCollider.isTrigger = true;

        lighter = Object.FindFirstObjectByType<Lighter>();
    }

    void Update()
    {
        if (player == null || zoneCollider == null) return;

        // Verifica a cada frame se o jogador esta dentro da hitbox da zona
        bool containsPlayer = zoneCollider.bounds.Contains(player.position);

        if (containsPlayer && !playerInZone)
        {
            playerInZone = true;
            Debug.Log($"Entered Dark Zone: {zoneID}");

            // Avisa o manager global que estamos no escuro
            if (DarknessManager.Instance != null)
            {
                DarknessManager.Instance.SetInDarkZone(true);

                // Passa os valores base do DarknessManager para o isqueiro
                if (lighter != null)
                {
                    lighter.SetZoneValues(DarknessManager.Instance.darkRadius, DarknessManager.Instance.darkSoftness);
                }
            }
        }
        else if (!containsPlayer && playerInZone)
        {
            // O jogador acabou de sair da zona
            playerInZone = false;
            Debug.Log($"Exited Dark Zone: {zoneID}");

            if (lighter != null)
            {
                lighter.ClearZoneValues();
            }

            if (DarknessManager.Instance != null)
            {
                DarknessManager.Instance.SetInDarkZone(false);
            }
        }
    }
}
