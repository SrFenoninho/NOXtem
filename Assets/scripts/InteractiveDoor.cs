using UnityEngine;
using UnityEngine.UI;

public class InteractiveDoor : MonoBehaviour, IInteractable
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Door Settings")]
    public string requiredKeyName = "MainDoor"; // ID da chave necessária para abrir
    public bool isLocked = true;

    [Header("Teleport")]
    public Transform destination;               // destino após atravessar a porta

    [Header("Camera Angle After Teleport")]
    [Range(0f, 360f)]
    public float cameraHorizontalAngle = 0f;    // ângulo da câmera após teletransporte

    [Header("UI")]
    public Text messageText;

    [Header("Audio")]
    public AudioClip doorOpenSound;
    private AudioSource audioSource;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        // Adicionar AudioSource automaticamente se não existir
        if (audioSource == null && doorOpenSound != null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    // ---------------------------------------------
    //  INTERFACE IInteractable
    // ---------------------------------------------
    public string GetInteractMessage()
    {
        return isLocked
            ? $"Press E to open door (Requires {requiredKeyName} key)"
            : "Press E to open door";
    }

    public void Interact(GameObject player)
    {
        if (isLocked)
        {
            PlayerKeys playerKeys = player.GetComponent<PlayerKeys>();
            if (playerKeys != null && playerKeys.HasKey(requiredKeyName))
            {
                isLocked = false;
                OpenDoor(player);
            }
            else
            {
                if (messageText != null)
                {
                    messageText.text = $"You need a {requiredKeyName} key.";
                    Invoke(nameof(ClearMessage), 2f);
                }
                Debug.Log("Porta trancada! É necessária a chave: " + requiredKeyName);
            }
        }
        else
        {
            OpenDoor(player);
        }
    }

    // ---------------------------------------------
    //  ABRIR PORTA / TELETRANSPORTE
    // ---------------------------------------------
    void OpenDoor(GameObject player)
    {
        if (destination == null)
        {
            Debug.LogError("Nenhum destino definido para a porta!");
            return;
        }

        if (doorOpenSound != null && audioSource != null)
            audioSource.PlayOneShot(doorOpenSound);

        CharacterController controller = player.GetComponent<CharacterController>();

        // Desativar CharacterController antes de teletransportar para evitar conflitos de física
        if (controller != null)
        {
            controller.enabled = false;
            player.transform.position = destination.position;
            player.transform.rotation = Quaternion.Euler(0f, cameraHorizontalAngle, 0f);
            controller.enabled = true;
        }
        else
        {
            player.transform.position = destination.position;
            player.transform.rotation = Quaternion.Euler(0f, cameraHorizontalAngle, 0f);
        }

        // Repor a rotação vertical da câmera via reflexão (campo privado do FPMove)
        FPMove fpMove = player.GetComponent<FPMove>();
        if (fpMove != null)
        {
            var field = typeof(FPMove).GetField("xRotation",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
                field.SetValue(fpMove, 0f);
        }

        if (messageText != null)
        {
            messageText.text = "Door opened";
            Invoke(nameof(ClearMessage), 2f);
        }

        Debug.Log("Jogador teletransportado para: " + destination.name);
    }

    void ClearMessage()
    {
        if (messageText != null) messageText.text = "";
    }
}
