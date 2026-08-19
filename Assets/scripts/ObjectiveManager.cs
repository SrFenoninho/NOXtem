using UnityEngine;
using TMPro;
using System.Collections;

public class ObjectiveManager : MonoBehaviour
{




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    public static ObjectiveManager Instance { get; private set; }

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
    //  PRIVATE STATE
    // ---------------------------------------------
    private string currentObjective = "";
    private bool fullPanelOpen = false;
    private Coroutine notificationCoroutine;
    private AudioSource audioSource;
    private CanvasGroup notificationCanvasGroup;

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
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (notificationPanel != null)
        {
            notificationCanvasGroup = notificationPanel.GetComponent<CanvasGroup>();
            if (notificationCanvasGroup == null)
                notificationCanvasGroup = notificationPanel.AddComponent<CanvasGroup>();
        }

        tpMove = FindAnyObjectByType<TPMove>();
        fpMove = FindAnyObjectByType<FPMove>();

        if (notificationPanel != null) notificationPanel.SetActive(false);
        if (fullPanel != null) fullPanel.SetActive(false);

        StartCoroutine(ShowInitialObjective());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!fullPanelOpen && GameStateManager.Instance != null && !GameStateManager.Instance.Is(GameState.Gameplay))
                return;

            if (fullPanelOpen)
                CloseFullPanel();
            else
                OpenFullPanel();
        }
    }




    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    IEnumerator ShowInitialObjective()
    {
        yield return new WaitForSeconds(initialDelay);
        ShowObjective(initialObjective);
    }





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void ShowObjective(string text)
    {
        currentObjective = text;

        if (fullText != null)
            fullText.text = currentObjective;

        if (objectiveSound != null)
            audioSource.PlayOneShot(objectiveSound);

        ShowNotification();
    }

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

        yield return StartCoroutine(FadeCanvasGroup(notificationCanvasGroup, 0f, 1f, fadeDuration));

        yield return new WaitForSeconds(notificationDuration);

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

    void OpenFullPanel()
    {
        if (fullPanel == null) return;
        if (string.IsNullOrEmpty(currentObjective)) return;

        fullPanelOpen = true;
        fullText.text = currentObjective;
        fullPanel.SetActive(true);

        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
            notificationCoroutine = null;
        }
        if (notificationPanel != null)
            notificationPanel.SetActive(false);

        SetMovementBlocked(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void CloseFullPanel()
    {
        fullPanelOpen = false;
        if (fullPanel != null)
            fullPanel.SetActive(false);

        SetMovementBlocked(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void SetMovementBlocked(bool blocked)
    {
        if (tpMove != null) tpMove.inputBlocked = blocked;
        if (fpMove != null) fpMove.inputBlocked = blocked;
    }
}
