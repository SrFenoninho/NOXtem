using UnityEngine;

public class DarkZoneTrigger : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Tooltip("Tem de ser igual ao Zone ID definido no DarknessManager")]
    public string zoneID = "Zone_A";

    [Tooltip("Se verdadeiro, escurece ao entrar e ilumina ao sair")]
    public bool toggleOnExit = true;

    // ---------------------------------------------
    //  TRIGGER
    // ---------------------------------------------
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (DarknessManager.Instance != null)
            DarknessManager.Instance.SetDarkZone(zoneID, true);

        Debug.Log($"Zona escura ativada: {zoneID}");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!toggleOnExit) return;

        if (DarknessManager.Instance != null)
            DarknessManager.Instance.SetDarkZone(zoneID, false);

        Debug.Log($"Zona escura desativada: {zoneID}");
    }
}
