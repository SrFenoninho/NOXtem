using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Raycast Settings")]
    public Camera playerCamera;
    public float interactionDistance = 3f;
    public LayerMask interactableLayer;

    [Header("UI")]
    public CrosshairUI crosshairUI;
    public Text messageText;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private IInteractable currentInteractable;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Update()
    {
        CheckForInteractable();

        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
            currentInteractable.Interact(gameObject);
    }

    // ---------------------------------------------
    //  DETEcaO DE INTERAGiVEIS
    // ---------------------------------------------
    // Raycast do centro do ecra para detetar objetos interagiveis
    void CheckForInteractable()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                if (currentInteractable != interactable)
                {
                    // Atualizar UI
                    currentInteractable = interactable;
                    crosshairUI.SetInteract();

                    if (messageText != null)
                        messageText.text = interactable.GetInteractMessage();
                }
                return;
            }
        }

        // Sem interagivel a frente - repor UI normal
        if (currentInteractable != null)
        {
            currentInteractable = null;
            crosshairUI.SetNormal();

            if (messageText != null)
                messageText.text = "";
        }
    }
}
