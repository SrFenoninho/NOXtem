using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ElevatorDoors : MonoBehaviour, IInteractable
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Elevator Doors")]
    public Transform door1;
    public Transform door2;

    [Header("Movement Settings")]
    public Vector3 moveDirection = Vector3.right;
    public float door1Distance = 2f; // Distância que a Porta 1 vai andar
    public float door2Distance = 1f; // Distância que a Porta 2 vai andar
    public float openDuration = 1.5f; // Segundos que demora a abrir

    [Header("Security")]
    public bool isLocked = false;
    public string requiredKeyName = "ElevatorKey";

    [Header("UI & Objectives")]
    public Text messageText;
    public bool updateObjectiveOnInteract = false;
    [TextArea] public string nextObjectiveText = "";

    [Header("Audio")]
    public AudioClip openSound;
    public AudioClip lockedSound;
    public AudioClip grantedSound;
    private AudioSource audioSource;

    [Header("Glow")]
    public bool enableGlow = true;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private GlowEmitter glow;
    private bool isOpened = false;
    private Vector3 door1StartPos;
    private Vector3 door2StartPos;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (openSound != null || lockedSound != null || grantedSound != null))
            audioSource = gameObject.AddComponent<AudioSource>();

        if (door1 != null) door1StartPos = door1.localPosition;
        if (door2 != null) door2StartPos = door2.localPosition;

        InitializeGlow();
    }

    void InitializeGlow()
    {
        if (!enableGlow) return;

        glow = GetComponent<GlowEmitter>();
        if (glow == null)
        {
            glow = gameObject.AddComponent<GlowEmitter>();
            glow.glowColor = Color.white;
            glow.enableGlow = false;
        }
    }

    public void OnKeyPickedUp(string keyName)
    {
        if (keyName == this.requiredKeyName && enableGlow && glow != null)
            glow.EnableGlow();
    }

    // ---------------------------------------------
    //  INTERFACE IInteractable
    // ---------------------------------------------
    public string GetInteractMessage()
    {
        if (isOpened) return ""; // Ou algo como "Portas abertas"
        if (isLocked) return $"Press E to use card reader (Requires {requiredKeyName})";
        return "Press E to open elevator doors";
    }

    public void Interact(GameObject player)
    {
        if (isOpened) return;

        if (isLocked)
        {
            PlayerKeys playerKeys = player.GetComponent<PlayerKeys>();
            if (playerKeys != null && playerKeys.HasKey(requiredKeyName))
            {
                isLocked = false;
                if (glow != null && enableGlow) glow.DisableGlow();

                if (grantedSound != null && audioSource != null)
                    audioSource.PlayOneShot(grantedSound);

                if (messageText != null)
                {
                    messageText.text = "Access Granted!";
                    Invoke(nameof(ClearMessage), 2f);
                }

                OpenDoors();
            }
            else
            {
                if (lockedSound != null && audioSource != null)
                    audioSource.PlayOneShot(lockedSound);

                if (messageText != null)
                {
                    messageText.text = $"You need a {requiredKeyName}";
                    Invoke(nameof(ClearMessage), 2f);
                }
            }
        }
        else
        {
            OpenDoors();
        }
    }

    // ---------------------------------------------
    //  ABRIR AS PORTAS
    // ---------------------------------------------
    void OpenDoors()
    {
        if (isOpened) return;
        isOpened = true;

        if (openSound != null && audioSource != null)
            audioSource.PlayOneShot(openSound);

        if (updateObjectiveOnInteract && !string.IsNullOrEmpty(nextObjectiveText))
        {
            ObjectiveManager.Instance?.ShowObjective(nextObjectiveText);
        }

        StartCoroutine(AnimateDoorsRoutine());
    }

    IEnumerator AnimateDoorsRoutine()
    {
        float elapsed = 0f;

        Vector3 moveDirNorm = moveDirection.normalized;
        Vector3 door1TargetPos = door1StartPos + (moveDirNorm * door1Distance);
        Vector3 door2TargetPos = door2StartPos + (moveDirNorm * door2Distance);

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / openDuration);

            // Transição suave (acelera no início e trava no fim)
            float smoothStep = Mathf.SmoothStep(0f, 1f, t);

            if (door1 != null)
                door1.localPosition = Vector3.Lerp(door1StartPos, door1TargetPos, smoothStep);

            if (door2 != null)
                door2.localPosition = Vector3.Lerp(door2StartPos, door2TargetPos, smoothStep);

            yield return null;
        }

        // Garante que param exatamente na posição final
        if (door1 != null) door1.localPosition = door1TargetPos;
        if (door2 != null) door2.localPosition = door2TargetPos;
    }

    void ClearMessage()
    {
        if (messageText != null) messageText.text = "";
    }
}
