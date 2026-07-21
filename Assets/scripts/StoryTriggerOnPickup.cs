using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoryTriggerOnPickup : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Monitoring object")]
    public GameObject watchObject;

    [Header("Look At")]
    public Transform lookTarget;
    public float lookSpeed = 3f;

    [Header("Audio")]
    public AudioClip voiceLine;

    [Header("Subtitles")]
    public TMP_Text subtitleText;
    public GameObject subtitlePanel;
    public SubtitleLine[] subtitles;

    [Header("References")]
    public FPMove playerMovement;
    public float slowMultiplier = 0f;
    public float lookDuration = 4f;
    public bool triggerInDarkZone = false;
    public bool frozen = false;

    [Header("Glow Settings")]
    public bool enableGlow = true;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private float origSpeed;
    private float origSprintSpeed;
    private bool triggered = false;
    private GlowEmitter triggerGlow;
    private TMP_Text activeSubtitleText;
    private GameObject createdCanvasObj;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        if (enableGlow)
        {
            triggerGlow = GetComponent<GlowEmitter>();
            if (triggerGlow == null)
            {
                triggerGlow = gameObject.AddComponent<GlowEmitter>();
                triggerGlow.glowColor = Color.white;
            }
        }
    }

    void Update()
    {
        if (triggered) return;

        if (watchObject == null)
        {
            triggered = true;

            if (!triggerInDarkZone && DarknessManager.Instance != null && DarknessManager.Instance.IsDark()) return;

            if (triggerGlow != null)
                triggerGlow.DisableGlow();

            StartCoroutine(RunSequence());
        }
    }

    // ---------------------------------------------
    //  SEQUENCIA
    // ---------------------------------------------
    IEnumerator RunSequence()
    {
        GameStateManager.Instance?.PushState(GameState.Cutscene);

        if (playerMovement != null)
        {
            origSpeed = playerMovement.speed;
            origSprintSpeed = playerMovement.sprintSpeed;

            if (frozen)
            {
                playerMovement.inputBlocked = true;
            }
            else if (slowMultiplier > 0f)
            {
                playerMovement.speed = origSpeed * slowMultiplier;
                playerMovement.sprintSpeed = origSprintSpeed * slowMultiplier;
            }

            if (lookTarget != null) playerMovement.cameraBlocked = true;
        }

        AudioSource audio = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        if (voiceLine != null)
            audio.PlayOneShot(voiceLine);

        Coroutine subCoroutine = StartCoroutine(PlaySubtitles());

        float elapsed = 0f;

        Camera cam = playerMovement != null ? playerMovement.GetComponentInChildren<Camera>() : null;
        Transform camT = cam != null ? cam.transform : null;

        while (elapsed < lookDuration)
        {
            elapsed += Time.deltaTime;

            if (lookTarget != null && camT != null)
            {
                Vector3 dirCam = lookTarget.position - camT.position;
                float pitch = -Mathf.Asin(Mathf.Clamp(dirCam.normalized.y, -1f, 1f)) * Mathf.Rad2Deg;

                camT.localRotation = Quaternion.Slerp(
                    camT.localRotation,
                    Quaternion.Euler(pitch, 0f, 0f),
                    Time.deltaTime * lookSpeed);
            }

            yield return null;
        }

        if (playerMovement != null)
        {
            if (frozen)
            {
                playerMovement.inputBlocked = false;
            }
            else if (slowMultiplier > 0f)
            {
                playerMovement.speed = origSpeed;
                playerMovement.sprintSpeed = origSprintSpeed;
            }

            if (lookTarget != null)
            {
                playerMovement.cameraBlocked = false;
                playerMovement.SyncCameraRotation();
            }
        }

        if (subCoroutine != null)
            yield return subCoroutine;

        if (voiceLine != null)
            yield return new WaitForSeconds(Mathf.Max(0f, voiceLine.length - lookDuration));

        CleanupSubtitles();

        GameStateManager.Instance?.PopState();
    }

    // ---------------------------------------------
    //  SISTEMA DE LEGENDAS
    // ---------------------------------------------
    private Image activeSubtitleBgImage;

    IEnumerator PlaySubtitles()
    {
        if (subtitles == null || subtitles.Length == 0) yield break;

        TMP_Text textComp = GetOrCreateSubtitleText();
        if (textComp == null) yield break;

        Image bgImage = activeSubtitleBgImage != null ? activeSubtitleBgImage : textComp.GetComponentInParent<Image>();

        foreach (var sub in subtitles)
        {
            if (sub == null || string.IsNullOrEmpty(sub.text)) continue;

            textComp.text = "";
            if (bgImage != null) bgImage.enabled = false;
            if (subtitlePanel != null) subtitlePanel.SetActive(false);

            if (sub.delayBefore > 0f)
                yield return new WaitForSeconds(sub.delayBefore);

            textComp.text = sub.text;

            if (bgImage != null)
            {
                bgImage.enabled = true;
            }

            if (subtitlePanel != null)
                subtitlePanel.SetActive(true);

            yield return new WaitForSeconds(sub.duration);

            textComp.text = "";
            if (bgImage != null) bgImage.enabled = false;
            if (subtitlePanel != null) subtitlePanel.SetActive(false);
        }

        CleanupSubtitles();
    }

    TMP_Text GetOrCreateSubtitleText()
    {
        if (subtitleText != null)
        {
            activeSubtitleText = subtitleText;
            activeSubtitleText.textWrappingMode = TextWrappingModes.Normal;

            Image parentBg = activeSubtitleText.GetComponentInParent<Image>();
            if (parentBg == null && activeSubtitleText.transform.parent != null)
            {
                GameObject customBgObj = new GameObject("SubtitleBackground_Auto", typeof(RectTransform), typeof(Image));
                customBgObj.transform.SetParent(activeSubtitleText.transform.parent, false);

                Image bgImg = customBgObj.GetComponent<Image>();
                bgImg.color = new Color(0f, 0f, 0f, 0.75f);
                bgImg.raycastTarget = false;
                activeSubtitleBgImage = bgImg;

                RectTransform customBgRt = customBgObj.GetComponent<RectTransform>();
                customBgRt.anchorMin = new Vector2(0.5f, 0.08f);
                customBgRt.anchorMax = new Vector2(0.5f, 0.08f);
                customBgRt.pivot = new Vector2(0.5f, 0.5f);

                ContentSizeFitter fitter = customBgObj.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                HorizontalLayoutGroup layout = customBgObj.AddComponent<HorizontalLayoutGroup>();
                layout.padding = new RectOffset(30, 30, 12, 12);
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;

                activeSubtitleText.transform.SetParent(customBgObj.transform, false);

                LayoutElement layoutElement = activeSubtitleText.gameObject.GetComponent<LayoutElement>() ?? activeSubtitleText.gameObject.AddComponent<LayoutElement>();
                layoutElement.preferredWidth = 1400f;

                createdCanvasObj = customBgObj;
            }
            else
            {
                activeSubtitleBgImage = parentBg;
            }
            return activeSubtitleText;
        }

        TMP_Text[] allTexts = FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
        foreach (TMP_Text t in allTexts)
        {
            if (t.gameObject.name.ToLower().Contains("subtitle") || t.gameObject.name.ToLower().Contains("legenda"))
            {
                activeSubtitleText = t;
                activeSubtitleBgImage = t.GetComponentInParent<Image>();
                return activeSubtitleText;
            }
        }

        createdCanvasObj = new GameObject("SubtitleCanvas_Auto");
        Canvas canvas = createdCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 998;

        CanvasScaler scaler = createdCanvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Barra Preta de Fundo (Centrada e auto-ajustável)
        GameObject bgObj = new GameObject("SubtitleBackground");
        bgObj.transform.SetParent(createdCanvasObj.transform, false);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.75f);
        bgImage.enabled = false;
        activeSubtitleBgImage = bgImage;

        RectTransform bgRt = bgImage.rectTransform;
        bgRt.anchorMin = new Vector2(0.5f, 0.08f);
        bgRt.anchorMax = new Vector2(0.5f, 0.08f);
        bgRt.pivot = new Vector2(0.5f, 0.5f);

        ContentSizeFitter autoFitter = bgObj.AddComponent<ContentSizeFitter>();
        autoFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        autoFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        HorizontalLayoutGroup autoLayout = bgObj.AddComponent<HorizontalLayoutGroup>();
        autoLayout.padding = new RectOffset(30, 30, 12, 12);
        autoLayout.childAlignment = TextAnchor.MiddleCenter;
        autoLayout.childControlWidth = true;
        autoLayout.childControlHeight = true;
        autoLayout.childForceExpandWidth = false;
        autoLayout.childForceExpandHeight = false;

        // Objeto de Texto
        GameObject textObj = new GameObject("SubtitleText");
        textObj.transform.SetParent(bgObj.transform, false);

        TMP_Text tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 28;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.color = Color.white;

        LayoutElement autoLayoutElement = textObj.AddComponent<LayoutElement>();
        autoLayoutElement.preferredWidth = 1400f;

        activeSubtitleText = tmp;
        return activeSubtitleText;
    }

    void CleanupSubtitles()
    {
        if (activeSubtitleText != null)
            activeSubtitleText.text = "";

        if (activeSubtitleBgImage != null)
            activeSubtitleBgImage.enabled = false;

        if (subtitlePanel != null)
            subtitlePanel.SetActive(false);
    }
}