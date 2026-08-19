using UnityEngine;

public class CombatZone : MonoBehaviour
{





    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    public static bool InCombatZone { get; private set; } = false;

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
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        InCombatZone = false;
        Apply(false);
    }




    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    void Apply(bool active)
    {
        if (fpCombat != null) fpCombat.enabled = active;
        if (lighter != null) lighter.inputBlocked = active;
        if (!active && lighter != null) lighter.ForceLight(false);
    }
}
