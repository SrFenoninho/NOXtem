using UnityEngine;
using UnityEngine.UI;

public class ObjectiveTrigger : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    public Text objectiveText;
    public string newObjectiveText = "Objective:\n"; // texto a mostrar ao ativar
    public bool needsInteraction = false;            // se verdadeiro, espera que o jogador prime E

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private bool playerInside = false;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Update()
    {
        // Aguardar interação manual se needsInteraction estiver ativo
        if (playerInside && needsInteraction && Input.GetKeyDown(KeyCode.E))
            ChangeText();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;

            // Ativar automaticamente se não precisar de interação
            if (!needsInteraction)
                ChangeText();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = false;
    }

    // ---------------------------------------------
    //  ATUALIZAÇÃO DO OBJETIVO
    // ---------------------------------------------
    void ChangeText()
    {
        if (objectiveText != null)
        {
            objectiveText.text = newObjectiveText;

            // Desativar o trigger e o mesh após uso — só deve disparar uma vez
            GetComponent<Collider>().enabled = false;
            MeshRenderer mesh = GetComponent<MeshRenderer>();
            if (mesh != null)
                mesh.enabled = false;
        }
    }
}
