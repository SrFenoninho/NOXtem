using UnityEngine;

public class DarkZone : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    public string zoneID = "DarkZone_1";

    [Header("Agachamento")]
    public bool forceCrouch = false;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private Transform player;
    private Collider zoneCollider;
    private Lighter lighter;
    private bool playerInZone = false;
    private FPMove fpMove;

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
        fpMove = Object.FindFirstObjectByType<FPMove>();
    }

    void Update()
    {
        if (player == null || zoneCollider == null) return;

        // Verifica a cada frame se o jogador esta dentro da hitbox da zona
        bool containsPlayer = zoneCollider.bounds.Contains(player.position);

        if (forceCrouch && playerInZone && fpMove != null && !fpMove.isCrouching)
            fpMove.isCrouching = true;

        if (containsPlayer && !playerInZone)
        {
            playerInZone = true;
            Debug.Log($"Entered Dark Zone: {zoneID}");

            // Avisa o manager global que estamos no escuro
            if (DarknessManager.Instance != null)
            {
                DarknessManager.Instance.SetInDarkZone(true);
                if (lighter != null)
                    lighter.SetZoneValues(DarknessManager.Instance.darkRadius, DarknessManager.Instance.darkSoftness);
            }

            if (forceCrouch && fpMove != null && !fpMove.isCrouching)
                fpMove.isCrouching = true;
        }
        else if (!containsPlayer && playerInZone)
        {
            // O jogador acabou de sair da zona
            playerInZone = false;
            Debug.Log($"Exited Dark Zone: {zoneID}");

            if (lighter != null)
                lighter.ClearZoneValues();

            if (DarknessManager.Instance != null)
                DarknessManager.Instance.SetInDarkZone(false);

            if (forceCrouch && fpMove != null)
                fpMove.isCrouching = false;
        }
    }
}