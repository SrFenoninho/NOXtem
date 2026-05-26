using UnityEngine;
using UnityEngine.UI;

public class LockedDoor : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    public Text messageText;
    public string requiredKeyID = "Door";
    public bool isLocked = true;

    [Header("audio")]
    public AudioClip lockedSound;        // Toca se o jogador nao tiver a chave
    public AudioClip unlockedSound;      // Toca ao destrancar a porta com sucesso

    [Header("Glow Settings")]
    public bool enableGlowWhenKeyAvailable = true;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private Rigidbody rb;
    private bool hasPlayedUnlockedSound = false;
    private GlowEmitter doorGlow;
    private bool hasGlowedThisRun = false;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = isLocked;

        InitializeGlow();
    }

    // ---------------------------------------------
    //  INICIALIZACAO DO GLOW
    // ---------------------------------------------
    void InitializeGlow()
    {
        if (!enableGlowWhenKeyAvailable) return;

        doorGlow = GetComponent<GlowEmitter>();
        if (doorGlow == null)
        {
            doorGlow = gameObject.AddComponent<GlowEmitter>();
            doorGlow.glowColor = Color.white;
            doorGlow.enableGlow = false;
        }

        doorGlow.DisableGlow();
    }

    // ---------------------------------------------
    //  CALLBACK DE CHAVE COLETADA
    // ---------------------------------------------
    public void OnKeyPickedUp(string keyName)
    {
        if (keyName == requiredKeyID && doorGlow != null && enableGlowWhenKeyAvailable && !hasGlowedThisRun)
        {
            doorGlow.EnableGlow();
            hasGlowedThisRun = true;
        }
    }

    // ---------------------------------------------
    //  DESBLOQUEIO POR COLISAO
    // ---------------------------------------------
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (isLocked)
            {
                PlayerKeys playerKeys = other.GetComponent<PlayerKeys>();
                if (playerKeys != null && playerKeys.HasKey(requiredKeyID))
                {
                    isLocked = false;
                    if (rb != null) rb.isKinematic = false;

                    if (doorGlow != null && enableGlowWhenKeyAvailable)
                    {
                        doorGlow.DisableGlow();
                    }

                    if (messageText != null) messageText.text = "A door is unlocked";

                    if (!hasPlayedUnlockedSound && unlockedSound != null)
                    {
                        AudioSource.PlayClipAtPoint(unlockedSound, transform.position);
                        hasPlayedUnlockedSound = true;
                    }
                }
                else
                {
                    if (messageText != null) messageText.text = $"You need a {requiredKeyID} key";

                    if (lockedSound != null)
                    {
                        AudioSource.PlayClipAtPoint(lockedSound, transform.position);
                    }
                }
            }
            else
            {
                if (!hasPlayedUnlockedSound && unlockedSound != null)
                {
                    AudioSource.PlayClipAtPoint(unlockedSound, transform.position);
                    hasPlayedUnlockedSound = true;
                }
            }

            CancelInvoke(nameof(ClearMessage));
            Invoke(nameof(ClearMessage), 2f);
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