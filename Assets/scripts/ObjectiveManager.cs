using UnityEngine;
using TMPro;
using System.Collections;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Objetivo Inicial")]
    [TextArea] public string initialObjective = "Objetivo: Liga os geradores.";
    public float initialDelay = 10f;

    [Header("Painel de Notificacao (canto inferior)")]
    public GameObject notificationPanel;
    public TMP_Text notificationText;

    [Header("Painel Completo (centro do ecra - TAB)")]
    public GameObject fullPanel;
    public TMP_Text fullText;

    [Header("Duracao")]
    public float notificationDuration = 5f;
    public float fadeDuration = 0.5f;

    [Header("Audio")]
    public AudioClip objectiveSound;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private string currentObjective = "";
    private bool fullPanelOpen = false;
    private Coroutine notificationCoroutine;
    private AudioSource audioSource;
    private CanvasGroup notificationCanvasGroup;

    // Referencias para bloquear movimento
    private TPMove tpMove;
    private FPMove fpMove;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        // Audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Garantir CanvasGroup no Painel de Notificacao para fade
        if (notificationPanel != null)
        {
            notificationCanvasGroup = notificationPanel.GetComponent<CanvasGroup>();
            if (notificationCanvasGroup == null)
                notificationCanvasGroup = notificationPanel.AddComponent<CanvasGroup>();
        }

        // Encontrar scripts de movimento
        tpMove = FindAnyObjectByType<TPMove>();
        fpMove = FindAnyObjectByType<FPMove>();

        // Esconder paineis ao iniciar
        if (notificationPanel != null) notificationPanel.SetActive(false);
        if (fullPanel != null) fullPanel.SetActive(false);

        // Objetivo inicial com delay
        StartCoroutine(ShowInitialObjective());
    }

    void Update()
    {
        // TAB para abrir/fechar painel completo
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (fullPanelOpen)
                CloseFullPanel();
            else
                OpenFullPanel();
        }
    }

    // ---------------------------------------------
    //  OBJETIVO INICIAL
    // ---------------------------------------------
    IEnumerator ShowInitialObjective()
    {
        yield return new WaitForSeconds(initialDelay);
        ShowObjective(initialObjective);
    }

    // ---------------------------------------------
    //  MOSTRAR OBJETIVO (chamado pelo ObjectiveTrigger)
    // ---------------------------------------------
    public void ShowObjective(string text)
    {
        currentObjective = text;

        // Atualizar texto do painel completo
        if (fullText != null)
            fullText.text = currentObjective;

        // Som ao mudar de objetivo
        if (objectiveSound != null)
            audioSource.PlayOneShot(objectiveSound);

        // Mostrar notificacao no canto inferior
        ShowNotification();
    }

    // ---------------------------------------------
    //  notificacao (canto inferior - 5 segundos)
    // ---------------------------------------------
    void ShowNotification()
    {
        if (notificationPanel == null || notificationText == null) return;

        if (notificationCoroutine != null)
            StopCoroutine(notificationCoroutine);

        notificationCoroutine = StartCoroutine(NotificationRoutine());
    }

    IEnumerator NotificationRoutine()
    {
        notificationText.text = currentObjective;
        notificationPanel.SetActive(true);

        // Fade in
        yield return StartCoroutine(FadeCanvasGroup(notificationCanvasGroup, 0f, 1f, fadeDuration));

        // Manter visivel
        yield return new WaitForSeconds(notificationDuration);

        // Fade out
        yield return StartCoroutine(FadeCanvasGroup(notificationCanvasGroup, 1f, 0f, fadeDuration));

        notificationPanel.SetActive(false);
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        if (cg == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        cg.alpha = to;
    }

    // ---------------------------------------------
    //  PAINEL COMPLETO (TAB - permanente)
    // ---------------------------------------------
    void OpenFullPanel()
    {
        if (fullPanel == null) return;
        if (string.IsNullOrEmpty(currentObjective)) return;

        fullPanelOpen = true;
        fullText.text = currentObjective;
        fullPanel.SetActive(true);

        // Esconder notificacao se estiver visivel
        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
            notificationCoroutine = null;
        }
        if (notificationPanel != null)
            notificationPanel.SetActive(false);

        // Bloquear movimento
        SetMovementBlocked(true);

        // Mostrar cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void CloseFullPanel()
    {
        fullPanelOpen = false;
        if (fullPanel != null)
            fullPanel.SetActive(false);

        // Desbloquear movimento
        SetMovementBlocked(false);

        // Esconder cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ---------------------------------------------
    //  BLOQUEIO DE MOVIMENTO
    // ---------------------------------------------
    void SetMovementBlocked(bool blocked)
    {
        if (tpMove != null) tpMove.inputBlocked = blocked;
        if (fpMove != null) fpMove.inputBlocked = blocked;
    }
}
