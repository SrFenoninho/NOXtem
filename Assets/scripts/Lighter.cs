using UnityEngine;
using UnityEngine.UI;

public class Lighter : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Input")]
    public KeyCode toggleKey = KeyCode.F;

    [Header("Viewmodel")]
    [Tooltip("Arraste o modelo 3D do isqueiro aqui. Ele deve ser filho da Camera do Player!")]
    public GameObject lighterModel;

    [Tooltip("Posicao para onde o isqueiro vai quando esta escondido (ex: -30m para baixo)")]
    public Vector3 hiddenPosition = new Vector3(0f, -30f, 0f);

    [Tooltip("Posicao exata na frente da camara quando esta visivel")]
    public Vector3 visiblePosition = new Vector3(0.25f, -0.25f, 0.5f);

    [Header("Audio")]
    public AudioClip igniteSound;
    public AudioClip extinguishSound;
    private AudioSource audioSource;

    [Header("Vinheta UI")]
    [Tooltip("Image no Canvas com textura de vinheta escura nas bordas")]
    public Image vignetteImage;
    public float darkVignetteAlpha = 0.95f;
    public float litVignetteAlpha = 0.55f;
    public float poweredVignetteAlpha = 0f;
    public float vignetteSpeed = 3f;

    [Header("Filtro Amarelado UI")]
    [Tooltip("Image no Canvas com cor amarela - cobre o ecra inteiro com alpha baixo")]
    public Image tintImage;
    [Tooltip("Cor do filtro - amarelo quente por defeito")]
    public Color tintColor = new Color(1f, 0.8f, 0.3f, 0f); // alpha comeca a 0
    [Tooltip("Alpha maximo do filtro quando o isqueiro esta aceso")]
    [Range(0f, 1f)]
    public float tintMaxAlpha = 0.15f;
    public float tintLerpSpeed = 4f;

    [Header("Fog do Isqueiro")]
    public float fogStartOff = 1f;
    public float fogEndOff = 1f;
    public float fogStartOn = 3f;
    public float fogEndOn = 4f;
    public float fogLerpSpeed = 3f;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private bool isLit = false;
    private float targetVignetteAlpha;
    private float targetTintAlpha;

    private float currentFogStart;
    private float currentFogEnd;
    private float targetFogStart;
    private float targetFogEnd;

    private static readonly int ID_DistStart = Shader.PropertyToID("_DistFogStart");
    private static readonly int ID_DistEnd = Shader.PropertyToID("_DistFogEnd");

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Garante que o isqueiro comeca escondido (TP para -30 metros)
        if (lighterModel != null)
        {
            lighterModel.SetActive(true); // Mantem ativo na hierarquia
            lighterModel.transform.localPosition = hiddenPosition;
        }

        // Fog comeca fechada
        currentFogStart = fogStartOff;
        currentFogEnd = fogEndOff;
        targetFogStart = fogStartOff;
        targetFogEnd = fogEndOff;
        Shader.SetGlobalFloat(ID_DistStart, currentFogStart);
        Shader.SetGlobalFloat(ID_DistEnd, currentFogEnd);

        // Vinheta comeca cheia
        targetVignetteAlpha = darkVignetteAlpha;
        SetVignetteAlpha(darkVignetteAlpha);

        // Filtro amarelo comeca invisivel
        targetTintAlpha = 0f;
        if (tintImage != null)
        {
            Color c = tintColor;
            c.a = 0f;
            tintImage.color = c;
        }
    }

    void Update()
    {
        HandleInput();
        // O HandleModelAnimation foi removido, agora o movimento e instantaneo
        HandleVignette();
        HandleTint();
        HandleFogTransition();
    }

    // ---------------------------------------------
    //  INPUT & TOGGLE
    // ---------------------------------------------
    void HandleInput()
    {
        if (Input.GetKeyDown(toggleKey))
            Toggle();
    }

    void Toggle()
    {
        isLit = !isLit;

        // Teleporta o modelo instantaneamente (para a frente da camara ou para baixo do chao)
        if (lighterModel != null)
        {
            lighterModel.transform.localPosition = isLit ? visiblePosition : hiddenPosition;
        }

        // Fog - so altera quando escuro
        if (DarknessManager.Instance == null || DarknessManager.Instance.IsDark())
        {
            targetFogStart = isLit ? fogStartOn : fogStartOff;
            targetFogEnd = isLit ? fogEndOn : fogEndOff;
        }

        // Filtro amarelo
        targetTintAlpha = isLit ? tintMaxAlpha : 0f;

        // Vinheta
        UpdateVignetteTarget();

        if (isLit && igniteSound != null)
            audioSource.PlayOneShot(igniteSound);
        else if (!isLit && extinguishSound != null)
            audioSource.PlayOneShot(extinguishSound);

        Debug.Log(isLit ? "Isqueiro aceso" : "Isqueiro apagado");
    }

    // ---------------------------------------------
    //  FILTRO AMARELADO (Canvas Image)
    // ---------------------------------------------
    void HandleTint()
    {
        if (tintImage == null) return;

        Color c = tintColor;
        c.a = Mathf.Lerp(tintImage.color.a, targetTintAlpha, Time.deltaTime * tintLerpSpeed);
        tintImage.color = c;
    }

    // ---------------------------------------------
    //  TRANSICAO DA FOG
    // ---------------------------------------------
    void HandleFogTransition()
    {
        if (DarknessManager.Instance != null && !DarknessManager.Instance.IsDark()) return;

        currentFogStart = Mathf.Lerp(currentFogStart, targetFogStart, Time.deltaTime * fogLerpSpeed);
        currentFogEnd = Mathf.Lerp(currentFogEnd, targetFogEnd, Time.deltaTime * fogLerpSpeed);

        Shader.SetGlobalFloat(ID_DistStart, currentFogStart);
        Shader.SetGlobalFloat(ID_DistEnd, currentFogEnd);
    }

    // ---------------------------------------------
    //  VINHETA
    // ---------------------------------------------
    void HandleVignette()
    {
        if (vignetteImage == null) return;

        if (DarknessManager.Instance != null && !DarknessManager.Instance.IsDark())
            targetVignetteAlpha = poweredVignetteAlpha;

        Color c = vignetteImage.color;
        c.a = Mathf.Lerp(c.a, targetVignetteAlpha, Time.deltaTime * vignetteSpeed);
        vignetteImage.color = c;
    }

    void UpdateVignetteTarget()
    {
        if (DarknessManager.Instance != null && !DarknessManager.Instance.IsDark()) return;
        targetVignetteAlpha = isLit ? litVignetteAlpha : darkVignetteAlpha;
    }

    void SetVignetteAlpha(float alpha)
    {
        if (vignetteImage == null) return;
        Color c = vignetteImage.color;
        c.a = alpha;
        vignetteImage.color = c;
    }

    // ---------------------------------------------
    //  ACESSO PUBLICO
    // ---------------------------------------------
    public bool IsLit() => isLit;
}