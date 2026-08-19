using UnityEngine;

public class ObjectiveTrigger : MonoBehaviour
{




    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    [TextArea]



    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    public string objectiveText = "Objetivo: ";
    public bool needsInteraction = false;

    [Header("Glow Settings")]
    public bool enableGlow = true;


    private bool playerInside = false;
    private bool triggered = false;
    private GlowEmitter triggerGlow;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        if (enableGlow)
        {
            triggerGlow = GetComponent<GlowEmitter>();
            if (triggerGlow == null)
            {
                triggerGlow = gameObject.AddComponent<GlowEmitter>();
                triggerGlow.glowColor = Color.white;
            }
        }
    }

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
    //  PRIVATE METHODS
    // ---------------------------------------------
    void Trigger()
    {
        triggered = true;
        ObjectiveManager.Instance?.ShowObjective(objectiveText);

        if (triggerGlow != null)
            triggerGlow.DisableGlow();

        GetComponent<Collider>().enabled = false;
        MeshRenderer mesh = GetComponent<MeshRenderer>();
        if (mesh != null) mesh.enabled = false;
    }
}
