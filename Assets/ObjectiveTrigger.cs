using UnityEngine;
using UnityEngine.UI;
public class ObjectiveTrigger : MonoBehaviour
{
    public Text objectiveText;
    public string newObjectiveText = "Objective:\n";
    public bool needsInteraction = false;  

    private bool playerInside = false;

    void Update()
    {
        if (playerInside && needsInteraction && Input.GetKeyDown(KeyCode.E))
        {
            ChangeText();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;

            if (!needsInteraction)
            {
                ChangeText();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }

    void ChangeText()
    {
        if (objectiveText != null)
        {
            objectiveText.text = newObjectiveText;
            GetComponent<Collider>().enabled = false;
            MeshRenderer mesh = GetComponent<MeshRenderer>();
            if (mesh != null)
            {
                mesh.enabled = false;
            }
        }
    }
}