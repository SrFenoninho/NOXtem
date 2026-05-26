using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InteractiveDoor : MonoBehaviour, IInteractable
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Door Settings")]
    public string requiredKeyName = "MainDoor";
    public bool isLocked = true;

    [Header("Teleport")]
    public Transform destination;

    [Header("Camera Angle After Teleport")]
    [Range(0f, 360f)]
    public float cameraHorizontalAngle = 0f;

    [Header("UI")]
    public Text messageText;

    [Header("audio")]
    public AudioClip doorLockedSound;
    public AudioClip doorOpenSound;
    private AudioSource audioSource;

    [Header("Agachamento & Delay")]
    public bool forcePlayerToCrouch = false;
    public float crouchDelayBeforeTeleport = 0.5f;

    [Header("Atualizar Objetivo (Opcional)")]
    public bool updateObjectiveOnInteract = false;
    [TextArea] public string nextObjectiveText = "";

    [Header("Glow Settings")]
    public bool enableGlowWhenKeyAvailable = true;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private GlowEmitter doorGlow;
    private bool hasGlowedThisRun = false;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (doorOpenSound != null || doorLockedSound != null))
            audioSource = gameObject.AddComponent<AudioSource>();

        InitializeGlow();
    }

    // ---------------------------------------------
    //  GLOW
    // ---------------------------------------------
    void InitializeGlow()
    {
        if (!enableGlowWhenKeyAvailable) return;

        doorGlow = GetComponent<GlowEmitter>();
        if (doorGlow == null)
        {
            doorGlow = gameObject.AddComponent<GlowEmitter>();
            doorGlow.glowColor = Color.white;
        }

        doorGlow.DisableGlow();
    }

    public void OnKeyPickedUp(string keyName)
    {
        if (keyName == requiredKeyName && !hasGlowedThisRun && enableGlowWhenKeyAvailable && doorGlow != null)
        {
            doorGlow.EnableGlow();
            hasGlowedThisRun = true;
            Debug.Log("Door glow enabled for key: " + keyName);
        }
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

                if (doorGlow != null && hasGlowedThisRun)
                    doorGlow.DisableGlow();

                OpenDoor(player);
            }
            else
            {
                if (messageText != null)
                {
                    messageText.text = $"You need a {requiredKeyName} key.";
                    Invoke(nameof(ClearMessage), 2f);
                }

                if (doorLockedSound != null && audioSource != null)
                    audioSource.PlayOneShot(doorLockedSound);
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
    public void OpenDoor(GameObject player)
    {
        if (destination == null) return;

        if (doorOpenSound != null && audioSource != null)
            audioSource.PlayOneShot(doorOpenSound);

        CharacterController controller = player.GetComponent<CharacterController>();
        FPMove fpMove = player.GetComponent<FPMove>();

        if (forcePlayerToCrouch && fpMove != null)
        {
            fpMove.isCrouching = true;
            StartCoroutine(TeleportAfterDelay(player, controller, fpMove));
        }
        else
        {
            PerformTeleport(player, controller, fpMove);
        }
    }

    IEnumerator TeleportAfterDelay(GameObject player, CharacterController controller, FPMove fpMove)
    {
        yield return new WaitForSeconds(crouchDelayBeforeTeleport);
        PerformTeleport(player, controller, fpMove);
    }

    void PerformTeleport(GameObject player, CharacterController controller, FPMove fpMove)
    {
        float currentYRotation = player.transform.eulerAngles.y;

        if (controller != null)
        {
            controller.enabled = false;
            player.transform.position = destination.position;
            player.transform.rotation = Quaternion.Euler(0f, currentYRotation + cameraHorizontalAngle, 0f);
            controller.enabled = true;
        }
        else
        {
            player.transform.position = destination.position;
            player.transform.rotation = Quaternion.Euler(0f, currentYRotation + cameraHorizontalAngle, 0f);
        }

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

        if (updateObjectiveOnInteract && !string.IsNullOrEmpty(nextObjectiveText))
        {
            ObjectiveManager.Instance?.ShowObjective(nextObjectiveText);
        }
    }

    // ---------------------------------------------
    //  UI
    // ---------------------------------------------
    void ClearMessage()
    {
        if (messageText != null) messageText.text = "";
    }
}