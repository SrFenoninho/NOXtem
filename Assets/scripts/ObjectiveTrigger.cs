using UnityEngine;

public class ObjectiveTrigger : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [TextArea]
    public string objectiveText = "Objetivo: ";
    public bool needsInteraction = false;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private bool playerInside = false;
    private bool triggered = false;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Update()
    {
        if (playerInside && !triggered && needsInteraction && Input.GetKeyDown(KeyCode.E))
            Trigger();
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        playerInside = true;
        if (!needsInteraction)
            Trigger();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = false;
    }

    // ---------------------------------------------
    //  DISPARAR
    // ---------------------------------------------
    void Trigger()
    {
        triggered = true;
        ObjectiveManager.Instance?.ShowObjective(objectiveText);

        // Desativar collider apos uso
        GetComponent<Collider>().enabled = false;
        MeshRenderer mesh = GetComponent<MeshRenderer>();
        if (mesh != null) mesh.enabled = false;
    }
}