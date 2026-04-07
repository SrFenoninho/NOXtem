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

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private Rigidbody rb;
    private bool hasPlayedUnlockedSound = false;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = isLocked;
    }

    // ---------------------------------------------
    //  DESBLOQUEIO POR COLISaO
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

    void ClearMessage()
    {
        if (messageText != null) messageText.text = "";
    }
}
