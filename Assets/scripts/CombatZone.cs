using UnityEngine;

public class CombatZone : MonoBehaviour
{
    // ---------------------------------------------
    //  ESTADO GLOBAL ESTATICO
    // ---------------------------------------------
    public static bool InCombatZone { get; private set; } = false;

    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Referencias")]
    public FPCombat fpCombat;
    public Lighter lighter;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        Apply(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        InCombatZone = true;
        Apply(true);
        Debug.Log("Entrou na zona de combate");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        InCombatZone = false;
        Apply(false);
        Debug.Log("Saiu da zona de combate");
    }

    void Apply(bool active)
    {
        if (fpCombat != null) fpCombat.enabled = active;
        if (lighter != null) lighter.inputBlocked = active;
        if (!active && lighter != null) lighter.ForceLight(false);
    }
}