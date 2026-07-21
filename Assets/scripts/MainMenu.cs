using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    // GameObjects gerados dinamicamente em tempo de execução
    private GameObject canvasObj;
    private GameObject mainPanel;
    private GameObject optionsPanel;

    private Button continueButton;
    private Slider sensitivitySlider;
    private Slider volumeSlider;
    private AudioSource menuAudioSource; // Referência direta para controlar o volume da música

    [Header("Personalização Visual (Opcional)")]
    public Texture2D backgroundTexture;  // Arrastar a imagem de fundo para o menu principal
    public TMP_FontAsset customFont;     // Arrastar a fonte personalizada do TextMeshPro

    // Cores de design (Consistente com a estética do inventário do GameMenuManager)
    private Color corFundo = new Color(0.04f, 0.04f, 0.04f, 1f); // Quase preto cinematográfico
    private Color corJanela = new Color(0.08f, 0.08f, 0.08f, 0.95f); // Janela cinza escura
    private Color corBotaoNormal = new Color(0.15f, 0.15f, 0.15f, 1f);
    private Color corBotaoSair = new Color(0.5f, 0.1f, 0.1f, 1f);
    private Color corTextoNormal = Color.white;
    private Color corTextoDesativado = new Color(0.4f, 0.4f, 0.4f, 1f);
    private Color corSliderBg = new Color(0.25f, 0.25f, 0.25f, 1f);
    private Color corSliderFill = new Color(0f, 0.5f, 1f, 1f);

    private void Start()
    {
        // 1. Garante que o cursor está visível e desbloqueado no Menu Principal
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 2. Garante a existência do EventSystem na cena (obrigatório para cliques da UI)
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
        }

        // 3. Constrói toda a UI dinamicamente por código
        BuildUI();

        // 4. Carrega as definições guardadas de sensibilidade e volume
        float savedSensitivity = PlayerPrefs.GetFloat("Sensitivity", 100f);
        float savedVolume = PlayerPrefs.GetFloat("Volume", 1f);

        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = savedSensitivity;
            sensitivitySlider.onValueChanged.AddListener(delegate { ApplySensitivity(); });
        }

        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(delegate { ApplyVolume(); });
            AudioListener.volume = savedVolume;
        }

        // Deteta e controla o AudioSource existente no GameObject para a música de fundo
        menuAudioSource = GetComponent<AudioSource>();
        if (menuAudioSource != null)
        {
            menuAudioSource.volume = savedVolume;
        }

        // 5. Verifica se existe um save de progresso para habilitar/desabilitar o botão Continuar
        if (continueButton != null)
        {
            bool temSave = PlayerPrefs.HasKey("SavedScene");
            continueButton.interactable = temSave;

            // Se não tiver save, muda a cor do texto do botão para cinzento indicando estar trancado
            if (!temSave)
            {
                TextMeshProUGUI textComp = continueButton.GetComponentInChildren<TextMeshProUGUI>();
                if (textComp != null) textComp.color = corTextoDesativado;
            }
        }
    }

    // ---------------------------------------------
    //  CONSTRUÇÃO DINÂMICA DA UI (100% RESPONSIVA)
    // ---------------------------------------------
    private void BuildUI()
    {
        // A. Cria o Canvas Principal
        canvasObj = new GameObject("MainMenuCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // B. Imagem de Fundo (Tela Inteira)
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

        // C. Título do Jogo
        CreateLabel(bg.transform, "NØXtem", new Vector2(0.1f, 0.72f), new Vector2(0.9f, 0.9f), 96, true);

        // D. Painel Principal (Contém os botões Jogar, Continuar, Opções, Sair)
        mainPanel = CreatePanel(bg.transform, "MainPanel", new Vector2(0.35f, 0.12f), new Vector2(0.65f, 0.65f), Color.clear);
        
        CreateButton(mainPanel.transform, "New Game", new Vector2(0f, 0.76f), new Vector2(1f, 0.96f), corBotaoNormal, PlayGame);
        continueButton = CreateButton(mainPanel.transform, "Continue", new Vector2(0f, 0.51f), new Vector2(1f, 0.71f), corBotaoNormal, ContinueGame);
        CreateButton(mainPanel.transform, "Options", new Vector2(0f, 0.26f), new Vector2(1f, 0.46f), corBotaoNormal, OpenOptions);
        CreateButton(mainPanel.transform, "Quit", new Vector2(0f, 0f), new Vector2(1f, 0.2f), corBotaoSair, QuitGame);

        // E. Painel de Opções (Contém Sliders e Botão de Voltar)
        optionsPanel = CreatePanel(bg.transform, "OptionsPanel", new Vector2(0.3f, 0.12f), new Vector2(0.7f, 0.68f), corJanela);

        // Sub-título do painel de opções
        CreateLabel(optionsPanel.transform, "Options", new Vector2(0.1f, 0.82f), new Vector2(0.9f, 0.94f), 32, true);

        // Slider de Sensibilidade
        CreateLabel(optionsPanel.transform, "Mouse Sensitivity", new Vector2(0.1f, 0.66f), new Vector2(0.9f, 0.76f), 20, false);
        sensitivitySlider = CreateSlider(optionsPanel.transform, new Vector2(0.1f, 0.54f), new Vector2(0.9f, 0.64f), 10f, 300f, 100f);

        // Slider de Volume
        CreateLabel(optionsPanel.transform, "Master Volume", new Vector2(0.1f, 0.38f), new Vector2(0.9f, 0.48f), 20, false);
        volumeSlider = CreateSlider(optionsPanel.transform, new Vector2(0.1f, 0.26f), new Vector2(0.9f, 0.36f), 0f, 1f, 1f);

        // Botão de Voltar das opções
        CreateButton(optionsPanel.transform, "Save and Back", new Vector2(0.2f, 0.04f), new Vector2(0.8f, 0.18f), corBotaoNormal, CloseOptions);

        // Começa com o painel de opções ocultado
        optionsPanel.SetActive(false);
    }

    // ---------------------------------------------
    //  AÇÕES DOS BOTÕES
    // ---------------------------------------------
    public void PlayGame()
    {
        // Começa novo jogo: Limpa o save de progresso antigo para não herdar posições
        SaveSystem.LimparSaveProgresso();
        LoadingManager.Carregar("Floor1");
    }

    public void ContinueGame()
    {
        if (PlayerPrefs.HasKey("SavedScene"))
        {
            string savedScene = PlayerPrefs.GetString("SavedScene");
            
            // Ativa a flag global para o SaveSystem aplicar o warp no Start da nova cena
            SaveSystem.carregarSaveAoIniciar = true;
            LoadingManager.Carregar(savedScene);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OpenOptions()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        
        // Grava as definições fisicamente ao fechar o menu de opções
        PlayerPrefs.Save();
    }

    private void ApplySensitivity()
    {
        if (sensitivitySlider != null)
        {
            PlayerPrefs.SetFloat("Sensitivity", sensitivitySlider.value);
        }
    }

    private void ApplyVolume()
    {
        if (volumeSlider != null)
        {
            PlayerPrefs.SetFloat("Volume", volumeSlider.value);
            AudioListener.volume = volumeSlider.value; // Aplica o volume global do Unity em tempo real

            // Sincroniza o volume da música de fundo local
            if (menuAudioSource != null)
            {
                menuAudioSource.volume = volumeSlider.value;
            }
        }
    }

    // ---------------------------------------------
    //  MÉTODOS AUXILIARES DE GERAR UI DINÂMICA
    // ---------------------------------------------
    private GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = color;
        return go;
    }

    private Button CreateButton(Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax, Color color, UnityEngine.Events.UnityAction action)
    {
        GameObject go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = color;

        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(go.transform, false);
        RectTransform tRT = textObj.GetComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
        tRT.offsetMin = tRT.offsetMax = Vector2.zero;
        
        TextMeshProUGUI t = textObj.GetComponent<TextMeshProUGUI>();
        t.text = label; 
        t.alignment = TextAlignmentOptions.Center;
        t.fontSize = 32; // Aumentado de 24 para 32 para melhor legibilidade
        t.enableAutoSizing = false;
        t.color = corTextoNormal;
        
        // Aplica a fonte personalizada se o utilizador a arrastar no inspector
        if (customFont != null) t.font = customFont;

        Button btn = go.GetComponent<Button>();
        btn.onClick.AddListener(action);
        return btn;
    }

    private TextMeshProUGUI CreateLabel(Transform parent, string text, Vector2 anchorMin, Vector2 anchorMax, int fontSize, bool centerAlign)
    {
        GameObject go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        
        TextMeshProUGUI t = go.GetComponent<TextMeshProUGUI>();
        t.text = text; 
        t.alignment = centerAlign ? TextAlignmentOptions.Center : TextAlignmentOptions.Left;
        t.fontSize = fontSize; 
        t.enableAutoSizing = false;
        t.color = corTextoNormal;
        
        // Aplica a fonte personalizada se o utilizador a arrastar no inspector
        if (customFont != null) t.font = customFont;

        return t;
    }

    private Slider CreateSlider(Transform parent, Vector2 anchorMin, Vector2 anchorMax, float min, float max, float value)
    {
        GameObject go = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(go.transform, false);
        RectTransform bgRT = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0f, 0.25f); bgRT.anchorMax = new Vector2(1f, 0.75f);
        bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
        bg.GetComponent<Image>().color = corSliderBg;

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(go.transform, false);
        RectTransform faRT = fillArea.GetComponent<RectTransform>();
        faRT.anchorMin = new Vector2(0f, 0.25f); faRT.anchorMax = new Vector2(1f, 0.75f);
        faRT.offsetMin = new Vector2(5f, 0f); faRT.offsetMax = new Vector2(-15f, 0f);

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = new Vector2(0f, 1f);
        fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
        fill.GetComponent<Image>().color = corSliderFill;

        GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(go.transform, false);
        RectTransform haRT = handleArea.GetComponent<RectTransform>();
        haRT.anchorMin = Vector2.zero; haRT.anchorMax = Vector2.one;
        haRT.offsetMin = new Vector2(10f, 0f); haRT.offsetMax = new Vector2(-10f, 0f);

        GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRT = handle.GetComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(20f, 0f);
        handle.GetComponent<Image>().color = corTextoNormal;

        Slider slider = go.GetComponent<Slider>();
        slider.fillRect = fillRT; 
        slider.handleRect = handleRT;
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.minValue = min; 
        slider.maxValue = max; 
        slider.value = value;
        slider.direction = Slider.Direction.LeftToRight;
        return slider;
    }
}
