using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{




    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private GameObject canvasObj;
    private GameObject mainPanel;
    private GameObject optionsPanel;
    private GameObject controlsPanel;

    private Button continueButton;
    private Slider sensitivitySlider;
    private Slider volumeSlider;
    private TextMeshProUGUI sensitivityValueText;
    private TextMeshProUGUI volumeValueText;
    private AudioSource menuAudioSource;




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("Personalização Visual (Opcional)")]
    public Texture2D backgroundTexture;
    public TMP_FontAsset customFont;

    private Color corFundo            = new Color(0.04f, 0.04f, 0.04f, 1f);
    private Color corJanela           = new Color(0.08f, 0.08f, 0.08f, 0.95f);
    private Color corBotaoNormal      = new Color(0.15f, 0.15f, 0.15f, 1f);
    private Color corBotaoSair        = new Color(0.5f,  0.1f,  0.1f,  1f);
    private Color corBotaoQualidade   = new Color(0.2f,  0.2f,  0.25f, 1f);
    private Color corTextoNormal      = Color.white;
    private Color corTextoDesativado  = new Color(0.4f, 0.4f, 0.4f, 1f);
    private Color corSliderBg         = new Color(0.25f, 0.25f, 0.25f, 1f);
    private Color corSliderFill       = new Color(0f,   0.5f,  1f,   1f);

    [Header("Tunnel Background")]
    public float tunnelRotateSpeed = 10f;
    public float tunnelZoomSpeed   = 2.5f;
    public int   tunnelLayerCount  = 40;
    public float tunnelMinSize     = 12f;
    public float tunnelMaxSize     = 3600f;

    public Color tunnelColorA  = new Color(1.00f, 1.00f, 1.00f, 1f);
    public Color tunnelColorB  = new Color(0.86f, 0.86f, 0.86f, 1f);
    public Color tunnelBgColor = new Color(0.96f, 0.96f, 0.96f, 1f);

    private readonly List<RectTransform> tunnelLayers = new List<RectTransform>();
    private readonly List<Image>          tunnelImages = new List<Image>();
    private float tunnelProgress;
    private float tunnelAngle;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    private void Start()
    {
        Cursor.visible   = true;
        Cursor.lockState = CursorLockMode.None;

        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));

        BuildUI();

        float savedSensitivity = PlayerPrefs.GetFloat("Sensitivity", 100f);
        float savedVolume      = PlayerPrefs.GetFloat("Volume", 1f);
        int   savedQuality     = PlayerPrefs.GetInt("TextureQuality", 0);
        QualitySettings.globalTextureMipmapLimit = savedQuality;

        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = savedSensitivity;
            sensitivitySlider.onValueChanged.AddListener(delegate { ApplySensitivity(); });
            ApplySensitivity();
        }

        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(delegate { ApplyVolume(); });
            ApplyVolume();
        }

        menuAudioSource = GetComponent<AudioSource>();
        if (menuAudioSource != null)
            menuAudioSource.volume = savedVolume;

        if (continueButton != null)
        {
            bool temSave = PlayerPrefs.HasKey("SavedScene");
            continueButton.interactable = temSave;
            if (!temSave)
            {
                TextMeshProUGUI textComp = continueButton.GetComponentInChildren<TextMeshProUGUI>();
                if (textComp != null) textComp.color = corTextoDesativado;
            }
        }
    }

    private void Update()
    {
        if (tunnelLayers.Count == 0) return;

        tunnelProgress += Time.deltaTime * tunnelZoomSpeed;
        tunnelAngle    += Time.deltaTime * tunnelRotateSpeed;

        int count = tunnelLayers.Count;
        float ratio = Mathf.Pow(tunnelMaxSize / tunnelMinSize, 1.0f / count);

        int cycleIndex = Mathf.FloorToInt(tunnelProgress);
        float fracProgress = tunnelProgress - cycleIndex;

        for (int i = 0; i < count; i++)
        {
            int layerIndex = count - 1 - i;

            float exponent = layerIndex + fracProgress;
            float size = tunnelMinSize * Mathf.Pow(ratio, exponent);

            RectTransform lrt = tunnelLayers[i];
            lrt.sizeDelta     = new Vector2(size, size);
            lrt.localRotation = Quaternion.Euler(0f, 0f, 45f + tunnelAngle);

            int colorIndex = (layerIndex - cycleIndex) % 2;
            if (colorIndex < 0) colorIndex += 2;

            Image img = tunnelImages[i];
            Color baseColor = (colorIndex == 0) ? tunnelColorA : tunnelColorB;

            // Fade suave no ponto central do túnel para eliminar qualquer piscar ao nascer o anel
            float alpha = Mathf.Clamp01((size - 10f) / 80f);
            baseColor.a = alpha;

            img.color = baseColor;
        }
    }




    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    private void BuildUI()
    {
        canvasObj = new GameObject("MainMenuCanvas",
            typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler),  typeof(GraphicRaycaster));

        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GameObject bg;
        if (backgroundTexture != null)
        {
            bg = new GameObject("Background", typeof(RectTransform), typeof(RawImage));
            bg.transform.SetParent(canvasObj.transform, false);
            RectTransform rt = bg.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            bg.GetComponent<RawImage>().texture = backgroundTexture;
        }
        else
        {
            bg = CreatePanel(canvasObj.transform, "Background", Vector2.zero, Vector2.one, corFundo);
        }

        BuildTunnel(bg.transform);

        TextMeshProUGUI titleLabel = CreateLabel(bg.transform, "NØXtem",
            new Vector2(0.1f, 0.74f), new Vector2(0.9f, 0.96f), 120, true);
        if (titleLabel != null)
        {
            titleLabel.color = Color.black;
            titleLabel.fontStyle = FontStyles.Bold;
        }

        mainPanel = CreatePanel(bg.transform, "MainPanel",
            new Vector2(0.35f, 0.12f), new Vector2(0.65f, 0.65f), Color.clear);

        CreateButton(mainPanel.transform, "New Game",
            new Vector2(0f, 0.76f), new Vector2(1f, 0.96f), corBotaoNormal, PlayGame);
        continueButton = CreateButton(mainPanel.transform, "Continue",
            new Vector2(0f, 0.51f), new Vector2(1f, 0.71f), corBotaoNormal, ContinueGame);
        CreateButton(mainPanel.transform, "Options",
            new Vector2(0f, 0.26f), new Vector2(1f, 0.46f), corBotaoNormal, OpenOptions);
        CreateButton(mainPanel.transform, "Quit",
            new Vector2(0f, 0f),    new Vector2(1f, 0.2f),  corBotaoSair,   QuitGame);

        optionsPanel = CreatePanel(bg.transform, "OptionsPanel",
            new Vector2(0.25f, 0.08f), new Vector2(0.75f, 0.74f), corJanela);

        CreateLabel(optionsPanel.transform, "Options",
            new Vector2(0.1f, 0.86f), new Vector2(0.9f, 0.96f), 32, true);

        sensitivityValueText = CreateLabel(optionsPanel.transform, "Mouse Sensitivity: 100%",
            new Vector2(0.1f, 0.72f), new Vector2(0.9f, 0.82f), 20, false);
        sensitivitySlider = CreateSlider(optionsPanel.transform,
            new Vector2(0.1f, 0.62f), new Vector2(0.9f, 0.70f), 10f, 500f, 100f);

        volumeValueText = CreateLabel(optionsPanel.transform, "Master Volume: 100%",
            new Vector2(0.1f, 0.48f), new Vector2(0.9f, 0.58f), 20, false);
        volumeSlider = CreateSlider(optionsPanel.transform,
            new Vector2(0.1f, 0.38f), new Vector2(0.9f, 0.46f), 0f, 1f, 1f);

        CreateLabel(optionsPanel.transform, "Graphics Quality",
            new Vector2(0.1f, 0.26f), new Vector2(0.4f, 0.34f), 20, false);

        string[] qLabels = { "High", "Medium", "Low" };
        int[]    qLevels = { 0, 2, 4 };
        for (int i = 0; i < 3; i++)
        {
            int q = qLevels[i];
            float bx = 0.45f + i * 0.16f;
            CreateButton(optionsPanel.transform, qLabels[i],
                new Vector2(bx, 0.25f), new Vector2(bx + 0.14f, 0.34f),
                corBotaoQualidade, () =>
                {
                    QualitySettings.globalTextureMipmapLimit = q;
                    PlayerPrefs.SetInt("TextureQuality", q);
                    PlayerPrefs.Save();
                });
        }

        CreateButton(optionsPanel.transform, "Controls",
            new Vector2(0.1f, 0.05f), new Vector2(0.45f, 0.17f), corBotaoNormal, OpenControls);
        CreateButton(optionsPanel.transform, "Save and Back",
            new Vector2(0.55f, 0.05f), new Vector2(0.9f, 0.17f), corBotaoNormal, CloseOptions);

        controlsPanel = CreatePanel(bg.transform, "ControlsPanel",
            new Vector2(0.15f, 0.08f), new Vector2(0.85f, 0.74f), corJanela);
        CreateLabel(controlsPanel.transform, "Controls Overview",
            new Vector2(0.1f, 0.88f), new Vector2(0.9f, 0.98f), 32, true);

        GameObject col1 = CreatePanel(controlsPanel.transform, "Col1",
            new Vector2(0.04f, 0.18f), new Vector2(0.48f, 0.86f), Color.clear);
        CreateLabel(col1.transform, "1st Person (Exploration & Horror)",
            new Vector2(0f, 0.88f), new Vector2(1f, 0.98f), 22, true);
        CreateLabel(col1.transform,
            "W, A, S, D  -  Move\nLeft Shift  -  Sprint\nLeft Ctrl  -  Crouch\nE  -  Interact\nF  -  Lighter\nTAB  -  Inventory\nESC  -  Pause Menu",
            new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.85f), 18, false);

        GameObject col2 = CreatePanel(controlsPanel.transform, "Col2",
            new Vector2(0.52f, 0.18f), new Vector2(0.96f, 0.86f), Color.clear);
        CreateLabel(col2.transform, "3rd Person (Combat & Boss)",
            new Vector2(0f, 0.88f), new Vector2(1f, 0.98f), 22, true);
        CreateLabel(col2.transform,
            "W, A, S, D  -  Move\nSpace  -  Jump\nLeft Shift  -  Sprint\nLeft Click  -  Light Attack Combo\nRight Click  -  Heavy Attack\nQ  -  Block / Defense\nTAB  -  Inventory\nESC  -  Pause Menu",
            new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.85f), 18, false);

        CreateButton(controlsPanel.transform, "Back to Options",
            new Vector2(0.35f, 0.03f), new Vector2(0.65f, 0.15f), corBotaoNormal, CloseControls);

        optionsPanel.SetActive(false);
        controlsPanel.SetActive(false);
    }

    private void BuildTunnel(Transform parent)
    {
        var bgObj = new GameObject("TunnelBg", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(parent, false);
        var bgRT            = bgObj.GetComponent<RectTransform>();
        bgRT.anchorMin      = Vector2.zero; bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin      = bgRT.offsetMax = Vector2.zero;
        var bgImg           = bgObj.GetComponent<Image>();
        bgImg.color         = tunnelBgColor;
        bgImg.raycastTarget = false;

        tunnelLayers.Clear();
        tunnelImages.Clear();

        for (int i = tunnelLayerCount - 1; i >= 0; i--)
        {
            var go = new GameObject("Tunnel_" + i, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(bgObj.transform, false);

            var lrt              = go.GetComponent<RectTransform>();
            lrt.anchorMin        = new Vector2(0.5f, 0.5f);
            lrt.anchorMax        = new Vector2(0.5f, 0.5f);
            lrt.pivot            = new Vector2(0.5f, 0.5f);
            lrt.anchoredPosition = Vector2.zero;

            var img           = go.GetComponent<Image>();
            img.raycastTarget = false;

            tunnelLayers.Add(lrt);
            tunnelImages.Add(img);
        }
    }




    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void PlayGame()
    {
        SaveSystem.LimparSaveProgresso();
        LoadingManager.Carregar("Floor1");
    }

    public void ContinueGame()
    {
        if (PlayerPrefs.HasKey("SavedScene"))
        {
            SaveSystem.carregarSaveAoIniciar = true;
            LoadingManager.Carregar(PlayerPrefs.GetString("SavedScene"));
        }
    }

    public void QuitGame()    => Application.Quit();
    public void OpenOptions()  { mainPanel?.SetActive(false); controlsPanel?.SetActive(false); optionsPanel?.SetActive(true); }
    public void CloseOptions() { mainPanel?.SetActive(true);  optionsPanel?.SetActive(false);  PlayerPrefs.Save(); }
    public void OpenControls() { optionsPanel?.SetActive(false); controlsPanel?.SetActive(true); }
    public void CloseControls(){ controlsPanel?.SetActive(false); optionsPanel?.SetActive(true); }

    private void ApplySensitivity()
    {
        if (sensitivitySlider == null) return;
        float val = sensitivitySlider.value;
        PlayerPrefs.SetFloat("Sensitivity", val);
        if (sensitivityValueText != null)
            sensitivityValueText.text = $"Mouse Sensitivity: {Mathf.RoundToInt(val)}%";
    }

    private void ApplyVolume()
    {
        if (volumeSlider == null) return;
        float val = volumeSlider.value;
        PlayerPrefs.SetFloat("Volume", val);
        AudioListener.volume = val;
        if (volumeValueText != null)
            volumeValueText.text = $"Master Volume: {Mathf.RoundToInt(val * 100f)}%";
        if (menuAudioSource != null)
            menuAudioSource.volume = val;
    }

    private GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = color;
        return go;
    }

    private Button CreateButton(Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax, Color color, UnityEngine.Events.UnityAction action)
    {
        var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = color;

        var textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(go.transform, false);
        var tRT = textObj.GetComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
        tRT.offsetMin = tRT.offsetMax = Vector2.zero;

        var t = textObj.GetComponent<TextMeshProUGUI>();
        t.text            = label;
        t.alignment       = TextAlignmentOptions.Center;
        t.fontSize        = 32;
        t.enableAutoSizing = false;
        t.color           = corTextoNormal;
        if (customFont != null) t.font = customFont;

        var btn = go.GetComponent<Button>();
        btn.onClick.AddListener(action);
        return btn;
    }

    private TextMeshProUGUI CreateLabel(Transform parent, string text, Vector2 anchorMin, Vector2 anchorMax, int fontSize, bool centerAlign)
    {
        var go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        var t = go.GetComponent<TextMeshProUGUI>();
        t.text            = text;
        t.alignment       = centerAlign ? TextAlignmentOptions.Center : TextAlignmentOptions.Left;
        t.fontSize        = fontSize;
        t.enableAutoSizing = false;
        t.color           = corTextoNormal;
        if (customFont != null) t.font = customFont;

        return t;
    }

    private Slider CreateSlider(Transform parent, Vector2 anchorMin, Vector2 anchorMax, float min, float max, float value)
    {
        var go = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(go.transform, false);
        var bgRT = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0f, 0.25f); bgRT.anchorMax = new Vector2(1f, 0.75f);
        bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
        bg.GetComponent<Image>().color = corSliderBg;

        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(go.transform, false);
        var faRT = fillArea.GetComponent<RectTransform>();
        faRT.anchorMin = new Vector2(0f, 0.25f); faRT.anchorMax = new Vector2(1f, 0.75f);
        faRT.offsetMin = new Vector2(5f, 0f);    faRT.offsetMax = new Vector2(-15f, 0f);

        var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        var fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = new Vector2(0f, 1f);
        fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
        fill.GetComponent<Image>().color = corSliderFill;

        var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(go.transform, false);
        var haRT = handleArea.GetComponent<RectTransform>();
        haRT.anchorMin = Vector2.zero; haRT.anchorMax = Vector2.one;
        haRT.offsetMin = new Vector2(10f, 0f); haRT.offsetMax = new Vector2(-10f, 0f);

        var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handle.transform.SetParent(handleArea.transform, false);
        var handleRT = handle.GetComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(20f, 0f);
        handle.GetComponent<Image>().color = corTextoNormal;

        var slider = go.GetComponent<Slider>();
        slider.fillRect      = fillRT;
        slider.handleRect    = handleRT;
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.minValue      = min;
        slider.maxValue      = max;
        slider.value         = value;
        slider.direction     = Slider.Direction.LeftToRight;
        return slider;
    }
}
