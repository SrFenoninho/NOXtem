using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameMenuManager : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR - REFERENCIAS
    // ---------------------------------------------
    [Header("Referências")]
    public FPMove fPMove;
    public string mainMenuSceneName = "MainMenu"; // Nome da cena do menu principal para carregar ao sair

    [Header("Animação do Inventário")]
    [Header("Player Frames - Por Nível de Vida")]
    public Texture2D[] playerFrames_HP100;  // Vida > 75
    public Texture2D[] playerFrames_HP75;   // Vida <= 75 e > 50
    public Texture2D[] playerFrames_HP50;   // Vida <= 50 e > 25
    public Texture2D[] playerFrames_HP25;   // Vida <= 25 e > 10
    public Texture2D[] playerFrames_HP10;   // Vida <= 10 e > 5
    public Texture2D[] playerFrames_HP5;    // Vida <= 5
    public float playerFPS = 8f;
    
    public Texture2D[] williamFrames;
    public float williamFPS = 6f;

    [Header("Ícones dos Itens")]
    public KeyIconEntry[] keyImages; // Mapeia cada keyName (ID) a uma imagem

    // Campo para a musica do inventario - arrasta o ficheiro de audio aqui no Unity
    [Header("Áudio do Inventário")]
    public AudioClip inventoryMusic;
    public float inventoryMusicVolume = 0.5f;

    [Header("Defaults das Definições")]
    [Range(10f, 300f)] public float defaultSensitivity = 100f;
    [Range(0f, 1f)] public float defaultVolume = 1f;

    // ---------------------------------------------
    //  INSPETOR - CORES
    // ---------------------------------------------
    [Header("Cores - Fundo")]
    public Color corFundoEscuro = new Color(0f, 0f, 0f, 0.88f);
    public Color corJanela = new Color(0.08f, 0.08f, 0.08f, 1f);
    public Color corBarraTabs = new Color(0.05f, 0.05f, 0.05f, 1f);
    public Color corTabAtiva = new Color(0.2f, 0.2f, 0.2f, 1f);
    public Color corTabInativa = new Color(0.1f, 0.1f, 0.1f, 1f);
    public Color corZonaGrelha = new Color(0.06f, 0.06f, 0.06f, 1f);
    public Color corSlot = new Color(0.15f, 0.15f, 0.15f, 1f);
    public Color corImagemVazia = new Color(0.12f, 0.12f, 0.12f, 1f);

    [Header("Cores - Botões")]
    public Color corBotaoSair = new Color(0.6f, 0.1f, 0.1f, 1f);
    public Color corBotaoVoltar = new Color(0.15f, 0.15f, 0.15f, 1f);
    public Color corBotaoQualidade = new Color(0.2f, 0.2f, 0.2f, 1f);

    [Header("Cores - Slider")]
    public Color corSliderBg = new Color(0.3f, 0.3f, 0.3f, 1f);
    public Color corSliderFill = new Color(0.2f, 0.6f, 1f, 1f);

    [Header("Cores - Texto")]
    public Color corTextoNormal = Color.white;
    public Color corTextoLabels = new Color(0.3f, 1f, 0.3f);
    public Color corTextoVazio = new Color(0.5f, 0.5f, 0.5f);

    [Header("Cores - Tipos de Item (Fallback)")]
    public Color corItemChave = new Color(1f, 0.85f, 0.3f);
    public Color corItemNota = new Color(0.9f, 0.9f, 0.9f);
    public Color corItemFerramenta = new Color(0.5f, 0.8f, 1f);
    public Color corItemDesconhecido = new Color(0.5f, 0.5f, 0.5f);



    // ---------------------------------------------
    //  INSPETOR - TAMANHOS
    // ---------------------------------------------
    [Header("Tamanhos - Janela (0 a 1)")]
    public Vector2 janelaMargem = Vector2.zero;

    [Header("Tamanhos - Slots de Inventário")]
    public float slotTamanho = 90f;
    public float slotEspaco = 8f;
    public int slotsPorLinha = 5;

    [Header("Tamanhos - Texto")]
    public int fonteTabs = 14;
    public int fonteLabels = 14;
    public int fonteDef = 16;
    public int fonteBotoes = 14;
    public int fonteSlotsNome = 10;
    public int fonteVazio = 18;
    public int fonteSairTitulo = 20;

    [Header("Personalização Visual")]
    public TMP_FontAsset customFont;    // Fonte personalizada para o inventário/menu de pausa (opcional)

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private GameObject menuRoot;
    private GameObject tabInventario, tabDefinicoes, tabSair;
    private Button btnInventario, btnDefinicoes, btnSairTab;

    private List<GameObject> slots = new List<GameObject>();
    private Transform slotsRoot;

    private float currentSensitivity;
    private float currentVolume;
    private int currentTextureQuality;

    private bool isOpen = false;

    private RawImage playerRawImage;
    private RawImage williamRawImage;
    private float playerAnimTimer = 0f;
    private float williamAnimTimer = 0f;
    private int playerFrameIndex = 0;
    private int williamFrameIndex = 0;
    
    // Controlo de mudancas de array de frames
    private Texture2D[] currentPlayerFrameArray;
    private Texture2D[] previousPlayerFrameArray;

    // AudioSource dedicado a musica do inventario
    private AudioSource inventoryAudioSource;
    // Lista dos sons que pausamos ao abrir (para os retomar ao fechar)
    private List<AudioSource> pausedSources = new List<AudioSource>();

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        currentSensitivity = fPMove != null ? fPMove.mouseSensitivity : defaultSensitivity;
        currentVolume = defaultVolume;
        currentTextureQuality = QualitySettings.globalTextureMipmapLimit;

        inventoryAudioSource = gameObject.AddComponent<AudioSource>();
        inventoryAudioSource.loop = true;
        inventoryAudioSource.volume = inventoryMusicVolume;

        InventoryManager.Instance?.AddItem(
            new InventoryItem("lighter", "Isqueiro", "tool", "Um isqueiro desgastado. A única fonte de luz."));

        BuildMenu();
        menuRoot.SetActive(false);

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += RefreshInventory;
    }

    void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshInventory;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ToggleMenu();

        // unscaledDeltaTime para as animacoes funcionarem mesmo com o jogo pausado
        if (playerRawImage != null)
        {
            currentPlayerFrameArray = GetCurrentPlayerFrameArray();
            
            // Se mudou de array, resetar o index
            if (currentPlayerFrameArray != previousPlayerFrameArray)
            {
                playerFrameIndex = 0;
                previousPlayerFrameArray = currentPlayerFrameArray;
                playerAnimTimer = 0f;
            }
            
            if (currentPlayerFrameArray != null && currentPlayerFrameArray.Length > 0)
            {
                playerAnimTimer += Time.unscaledDeltaTime;
                if (playerAnimTimer >= 1f / playerFPS)
                {
                    playerAnimTimer = 0f;
                    playerFrameIndex = (playerFrameIndex + 1) % currentPlayerFrameArray.Length;
                    if (currentPlayerFrameArray[playerFrameIndex] != null)
                        playerRawImage.texture = currentPlayerFrameArray[playerFrameIndex];
                }
            }
        }

        if (williamRawImage != null && williamFrames != null && williamFrames.Length > 1)
        {
            williamAnimTimer += Time.unscaledDeltaTime;
            if (williamAnimTimer >= 1f / williamFPS)
            {
                williamAnimTimer = 0f;
                williamFrameIndex = (williamFrameIndex + 1) % williamFrames.Length;
                if (williamFrames[williamFrameIndex] != null)
                    williamRawImage.texture = williamFrames[williamFrameIndex];
            }
        }
    }

    // ---------------------------------------------
    //  ABRIR / FECHAR
    // ---------------------------------------------
    void ToggleMenu()
    {
        if (!isOpen)
        {
            // So abre se o jogo estiver em estado normal
            if (GameStateManager.Instance != null && !GameStateManager.Instance.CanOpenInventory())
                return;

            OpenMenu();
        }
        else
        {
            CloseMenu();
        }
    }

    void OpenMenu()
    {
        isOpen = true;
        menuRoot.SetActive(true);

        Time.timeScale = 0f;

        PauseAllAudio();

        if (inventoryMusic != null)
        {
            inventoryAudioSource.clip = inventoryMusic;
            inventoryAudioSource.Play();
        }

        if (fPMove != null) { fPMove.inputBlocked = true; fPMove.cameraBlocked = true; }
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        GameStateManager.Instance?.PushState(GameState.Inventory);
        ShowTab("inventario");
    }

    void CloseMenu()
    {
        isOpen = false;
        menuRoot.SetActive(false);

        inventoryAudioSource.Stop();

        ResumeAllAudio();

        Time.timeScale = 1f;

        if (fPMove != null) { fPMove.inputBlocked = false; fPMove.cameraBlocked = false; }
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        GameStateManager.Instance?.PopState();
    }

    // ---------------------------------------------
    //  GESTAO DE AUDIO
    // ---------------------------------------------
    void PauseAllAudio()
    {
        pausedSources.Clear();
        foreach (AudioSource src in FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
        {
            if (src == inventoryAudioSource) continue;
            if (src.isPlaying)
            {
                src.Pause();
                pausedSources.Add(src);
            }
        }
    }

    void ResumeAllAudio()
    {
        foreach (AudioSource src in pausedSources)
        {
            if (src != null)
                src.UnPause();
        }
        pausedSources.Clear();
    }

    // ---------------------------------------------
    //  SELECAO DE FRAMES DO PLAYER POR VIDA
    // ---------------------------------------------
    Texture2D[] GetCurrentPlayerFrameArray()
    {
        // Obter a vida do player
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth == null)
            return playerFrames_HP100; // Default se nao encontrar

        float currentHealth = playerHealth.currentHealth;
        float maxHealth = playerHealth.maxHealth;
        float healthPercent = (currentHealth / maxHealth) * 100f;

        // Determinar qual array usar baseado na percentagem de vida
        if (healthPercent > 75f)
            return playerFrames_HP100;
        else if (healthPercent > 50f)
            return playerFrames_HP75;
        else if (healthPercent > 25f)
            return playerFrames_HP50;
        else if (healthPercent > 10f)
            return playerFrames_HP25;
        else if (healthPercent > 5f)
            return playerFrames_HP10;
        else
            return playerFrames_HP5;
    }
    // ---------------------------------------------
    //  CONSTRUCAO DO MENU
    // ---------------------------------------------
    void BuildMenu()
    {
        // Garantir que existe um Canvas na hierarquia.
        // Se o GameMenuManager ja estiver dentro de um Canvas, usa esse.
        // Se nao, cria um Canvas raiz no proprio GameObject.
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        Transform menuParent;

        if (parentCanvas != null)
        {
            // Ja estamos dentro de um Canvas - usar a raiz desse Canvas
            menuParent = parentCanvas.transform;
        }
        else
        {
            // Nao ha Canvas - criar um no proprio GameObject
            if (GetComponent<Canvas>() == null)
            {
                gameObject.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                gameObject.AddComponent<CanvasScaler>();
                gameObject.AddComponent<GraphicRaycaster>();
            }

            Canvas canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            menuParent = transform;
        }

        // MenuRoot ocupa todo o ecra (100% do Canvas)
        menuRoot = CreatePanel(menuParent, "MenuRoot",
            Vector2.zero, Vector2.one, corFundoEscuro);

        // A janela interior respeita a margem configurada
        Vector2 minJ = janelaMargem;
        Vector2 maxJ = Vector2.one - janelaMargem;
        GameObject window = CreatePanel(menuRoot.transform, "Window",
            minJ, maxJ, corJanela);

        BuildTopBar(window.transform);
        BuildTabInventario(window.transform);
        BuildTabDefinicoes(window.transform);
        BuildTabSair(window.transform);
    }

    void BuildTopBar(Transform parent)
    {
        GameObject bar = CreatePanel(parent, "TopBar",
            new Vector2(0f, 0.9f), Vector2.one, corBarraTabs);

        btnInventario = CreateTabButton(bar.transform, "Inventário",
            new Vector2(0f, 0f), new Vector2(0.25f, 1f),
            () => ShowTab("inventario"));

        btnDefinicoes = CreateTabButton(bar.transform, "Definições",
            new Vector2(0.25f, 0f), new Vector2(0.5f, 1f),
            () => ShowTab("definicoes"));

        btnSairTab = CreateTabButton(bar.transform, "Guardar e Sair",
            new Vector2(0.5f, 0f), new Vector2(0.75f, 1f),
            () => ShowTab("sair"));
    }

    void BuildTabInventario(Transform parent)
    {
        tabInventario = CreatePanel(parent, "TabInventario",
            Vector2.zero, new Vector2(1f, 0.9f), Color.clear);

        GameObject gridZone = CreatePanel(tabInventario.transform, "GridZone",
            Vector2.zero, new Vector2(0.58f, 1f), corZonaGrelha);

        GameObject slotsRootObj = new GameObject("SlotsRoot", typeof(RectTransform));
        slotsRootObj.transform.SetParent(gridZone.transform, false);
        RectTransform srt = slotsRootObj.GetComponent<RectTransform>();
        srt.anchorMin = new Vector2(0.03f, 0.03f);
        srt.anchorMax = new Vector2(0.97f, 0.97f);
        srt.offsetMin = srt.offsetMax = Vector2.zero;

        slotsRoot = slotsRootObj.transform;

        GameObject rightTop = CreatePanel(tabInventario.transform, "",
            new Vector2(0.6f, 0.5f), Vector2.one, Color.clear);

        CreateLabel(rightTop.transform, "",
            new Vector2(0f, 0.88f), new Vector2(1f, 1f), fonteLabels, corTextoLabels);
        playerRawImage = CreateRawImageBox(rightTop.transform,
            playerFrames_HP100 != null && playerFrames_HP100.Length > 0 ? playerFrames_HP100[0] : null,
            new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.85f));

        GameObject rightBot = CreatePanel(tabInventario.transform, "",
            new Vector2(0.6f, 0f), new Vector2(1f, 0.5f), Color.clear);

        CreateLabel(rightBot.transform, "",
            new Vector2(0f, 0.88f), new Vector2(1f, 1f), fonteLabels, corTextoLabels);
        williamRawImage = CreateRawImageBox(rightBot.transform,
            williamFrames != null && williamFrames.Length > 0 ? williamFrames[0] : null,
            new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.85f));

        RefreshInventory();
    }

    void BuildTabDefinicoes(Transform parent)
    {
        tabDefinicoes = CreatePanel(parent, "TabDefinicoes",
            Vector2.zero, new Vector2(1f, 0.9f), Color.clear);

        float rowH = 0.1f;

        CreateLabel(tabDefinicoes.transform, "Sensibilidade do Rato",
            new Vector2(0.05f, 0.75f), new Vector2(0.45f, 0.75f + rowH), fonteDef);
        Slider sliderSens = CreateSlider(tabDefinicoes.transform,
            new Vector2(0.45f, 0.75f), new Vector2(0.9f, 0.75f + rowH),
            10f, 300f, currentSensitivity);
        sliderSens.onValueChanged.AddListener(v =>
        {
            currentSensitivity = v;
            if (fPMove != null) fPMove.mouseSensitivity = v;
        });

        CreateLabel(tabDefinicoes.transform, "Volume",
            new Vector2(0.05f, 0.6f), new Vector2(0.45f, 0.6f + rowH), fonteDef);
        Slider sliderVol = CreateSlider(tabDefinicoes.transform,
            new Vector2(0.45f, 0.6f), new Vector2(0.9f, 0.6f + rowH),
            0f, 1f, currentVolume);
        sliderVol.onValueChanged.AddListener(v =>
        {
            currentVolume = v;
            AudioListener.volume = v;
        });

        CreateLabel(tabDefinicoes.transform, "Qualidade das Texturas",
            new Vector2(0.05f, 0.45f), new Vector2(0.45f, 0.45f + rowH), fonteDef);
        string[] qLabels = { "Alta", "Média", "Baixa" };
        int[] qLevels = { 0, 2, 4 };
        for (int i = 0; i < 3; i++)
        {
            int q = qLevels[i];
            float bx = 0.45f + i * 0.15f;
            Button btn = CreateButton(tabDefinicoes.transform, qLabels[i],
                new Vector2(bx, 0.46f), new Vector2(bx + 0.13f, 0.54f),
                corBotaoQualidade);
            btn.onClick.AddListener(() =>
            {
                currentTextureQuality = q;
                QualitySettings.globalTextureMipmapLimit = q;
            });
        }
    }

    void BuildTabSair(Transform parent)
    {
        tabSair = CreatePanel(parent, "TabSair",
            Vector2.zero, new Vector2(1f, 0.9f), Color.clear);

        CreateLabel(tabSair.transform, "Tens a certeza que queres sair?",
            new Vector2(0.2f, 0.55f), new Vector2(0.8f, 0.7f), fonteSairTitulo);

        Button btnSairConfirm = CreateButton(tabSair.transform, "Guardar e Sair do Jogo",
            new Vector2(0.3f, 0.38f), new Vector2(0.7f, 0.52f), corBotaoSair);
        btnSairConfirm.onClick.AddListener(() =>
        {
            // Guarda o progresso físico e inventário do jogador
            SaveSystem.GuardarProgresso();

            // Guarda as definições do utilizador
            PlayerPrefs.SetFloat("Sensitivity", currentSensitivity);
            PlayerPrefs.SetFloat("Volume", currentVolume);
            PlayerPrefs.SetInt("TextureQuality", currentTextureQuality);
            PlayerPrefs.Save();

            // Repõe o timeScale (obrigatório para o menu principal não iniciar congelado)
            Time.timeScale = 1f;

            // Carrega a cena do menu principal
            SceneManager.LoadScene(mainMenuSceneName);
        });

        Button btnVoltar = CreateButton(tabSair.transform, "Voltar ao Jogo",
            new Vector2(0.3f, 0.22f), new Vector2(0.7f, 0.36f), corBotaoVoltar);
        btnVoltar.onClick.AddListener(CloseMenu);
    }

    // ---------------------------------------------
    //  MOSTRAR TAB
    // ---------------------------------------------
    void ShowTab(string tab)
    {
        tabInventario.SetActive(tab == "inventario");
        if (tab == "inventario") RefreshInventory();
        tabDefinicoes.SetActive(tab == "definicoes");
        tabSair.SetActive(tab == "sair");

        btnInventario.GetComponent<Image>().color = tab == "inventario" ? corTabAtiva : corTabInativa;
        btnDefinicoes.GetComponent<Image>().color = tab == "definicoes" ? corTabAtiva : corTabInativa;
        btnSairTab.GetComponent<Image>().color = tab == "sair" ? corTabAtiva : corTabInativa;
    }

    // ---------------------------------------------
    //  REFRESH DO INVENTARIO
    // ---------------------------------------------
    void RefreshInventory()
    {
        if (slotsRoot == null) return;

        foreach (GameObject s in slots) Destroy(s);
        slots.Clear();

        List<InventoryItem> items = InventoryManager.Instance?.GetAllItems();
        if (items == null || items.Count == 0)
        {
            GameObject msg = new GameObject("EmptyMsg", typeof(RectTransform), typeof(TextMeshProUGUI));
            msg.transform.SetParent(slotsRoot, false);
            RectTransform rt = msg.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            TextMeshProUGUI t = msg.GetComponent<TextMeshProUGUI>();
            t.text = "Inventário vazio"; t.alignment = TMPro.TextAlignmentOptions.Center;
            t.fontSize = fonteVazio; t.enableAutoSizing = false;
            if (customFont != null) t.font = customFont;
            slots.Add(msg);
            return;
        }

        for (int i = 0; i < items.Count; i++)
        {
            InventoryItem item = items[i];
            int col = i % slotsPorLinha;
            int row = i / slotsPorLinha;

            GameObject slot = new GameObject("Slot_" + i, typeof(RectTransform), typeof(Image));
            slot.transform.SetParent(slotsRoot, false);
            RectTransform slotRT = slot.GetComponent<RectTransform>();
            slotRT.anchorMin = slotRT.anchorMax = new Vector2(0f, 1f);
            slotRT.pivot = new Vector2(0f, 1f);
            slotRT.sizeDelta = new Vector2(slotTamanho, slotTamanho);
            slotRT.anchoredPosition = new Vector2(
                col * (slotTamanho + slotEspaco),
               -row * (slotTamanho + slotEspaco));
            slot.GetComponent<Image>().color = corSlot;

            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(RawImage));
            iconObj.transform.SetParent(slot.transform, false);
            RectTransform iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.05f, 0.30f);
            iconRT.anchorMax = new Vector2(0.95f, 1f);
            iconRT.offsetMin = iconRT.offsetMax = Vector2.zero;
            RawImage iconImg = iconObj.GetComponent<RawImage>();
            Texture2D foundIcon = GetKeyImage(item.itemID);

            if (foundIcon != null)
            {
                iconImg.texture = foundIcon;
                iconImg.color = Color.white;
            }
            else
            {
                iconImg.color = GetTypeColor(item.itemType);
            }

            GameObject nameObj = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameObj.transform.SetParent(slot.transform, false);
            RectTransform nameRT = nameObj.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0f, 0f);
            nameRT.anchorMax = new Vector2(1f, 0.28f);
            nameRT.offsetMin = nameRT.offsetMax = Vector2.zero;
            TextMeshProUGUI nameText = nameObj.GetComponent<TextMeshProUGUI>();
            nameText.text = item.itemName; nameText.alignment = TMPro.TextAlignmentOptions.Center;
            nameText.fontSize = fonteSlotsNome; nameText.enableAutoSizing = false;
            if (customFont != null) nameText.font = customFont;

            slots.Add(slot);
        }
    }

    // ---------------------------------------------
    //  AUXILIARES DE UI
    // ---------------------------------------------
    GameObject CreatePanel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = color;
        return go;
    }

    Button CreateTabButton(Transform parent, string label,
        Vector2 anchorMin, Vector2 anchorMax,
        UnityEngine.Events.UnityAction action)
    {
        GameObject go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = corTabInativa;

        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(go.transform, false);
        RectTransform tRT = textObj.GetComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
        tRT.offsetMin = tRT.offsetMax = Vector2.zero;
        TextMeshProUGUI t = textObj.GetComponent<TextMeshProUGUI>();
        t.text = label; t.alignment = TMPro.TextAlignmentOptions.Center;
        t.fontSize = fonteTabs; t.enableAutoSizing = false;
        if (customFont != null) t.font = customFont;

        Button btn = go.GetComponent<Button>();
        btn.onClick.AddListener(action);
        return btn;
    }

    Button CreateButton(Transform parent, string label,
        Vector2 anchorMin, Vector2 anchorMax, Color color)
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
        t.text = label; t.alignment = TMPro.TextAlignmentOptions.Center;
        t.fontSize = fonteBotoes; t.enableAutoSizing = false;
        if (customFont != null) t.font = customFont;

        return go.GetComponent<Button>();
    }

    TextMeshProUGUI CreateLabel(Transform parent, string text,
        Vector2 anchorMin, Vector2 anchorMax,
        int fontSize = 14, Color? color = null)
    {
        GameObject go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        TextMeshProUGUI t = go.GetComponent<TextMeshProUGUI>();
        t.text = text; t.alignment = TMPro.TextAlignmentOptions.Left;
        t.fontSize = fontSize; t.enableAutoSizing = false;
        if (customFont != null) t.font = customFont;
        return t;
    }

    Slider CreateSlider(Transform parent,
        Vector2 anchorMin, Vector2 anchorMax,
        float min, float max, float value)
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
        slider.fillRect = fillRT; slider.handleRect = handleRT;
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.minValue = min; slider.maxValue = max; slider.value = value;
        slider.direction = Slider.Direction.LeftToRight;
        return slider;
    }

    RawImage CreateRawImageBox(Transform parent, Texture2D texture,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject go = new GameObject("ImageBox", typeof(RectTransform), typeof(RawImage));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        AspectRatioFitter fitter = go.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        fitter.aspectRatio = 1f;
        RawImage img = go.GetComponent<RawImage>();
        img.color = texture != null ? Color.white : corImagemVazia;
        if (texture != null) img.texture = texture;
        return img;
    }

    // Procurar imagem de chave por ID (keyName)
    Texture2D GetKeyImage(string keyID)
    {
        if (keyImages == null) return null;
        
        foreach (var entry in keyImages)
            if (entry.keyID == keyID)
                return entry.keyImage;
        
        return null;
    }

    Color GetTypeColor(string type) => type switch
    {
        "key" => corItemChave,
        "note" => corItemNota,
        "tool" => corItemFerramenta,
        _ => corItemDesconhecido
    };
}

public class InventoryTabRef : MonoBehaviour
{
    public Transform slotsRoot;
}

[System.Serializable]
public class KeyIconEntry
{
    public string keyID;      // ID da chave (ex: "Door", "lighter", etc)
    public Texture2D keyImage; // Imagem para essa chave
}
