using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DarknessPromptTrigger : MonoBehaviour
{
    [Header("Prompt Settings")]
    [TextArea] public string promptMessage = "Press [F] to turn on Lighter";
    public float promptDuration = 4f;
    public bool triggerOnlyOnce = false;

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnlyOnce) return;

        if (other.CompareTag("Player") || other.GetComponent<FPMove>() != null)
        {
            Lighter lighter = other.GetComponentInChildren<Lighter>();
            if (lighter != null && !lighter.IsLit())
            {
                ObjectiveManager.Instance?.ShowObjective(promptMessage);
                if (triggerOnlyOnce) hasTriggered = true;
            }
        }
    }
}
