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
    public GameObject lighterModel;
    public Vector3 hiddenPosition = new Vector3(0.4f, -0.5f, 0.6f);
    public Vector3 visiblePosition = new Vector3(0.25f, -0.25f, 0.5f);
    public float drawSpeed = 8f;

    [Header("Audio")]
    public AudioClip igniteSound;
    public AudioClip extinguishSound;
    private AudioSource audioSource;

    [Header("Raio de Visibilidade")]
    [Tooltip("Raio em metros quando o isqueiro esta apagado")]
    public float radiusOff = 0.5f;
    [Tooltip("Raio em metros quando o isqueiro esta aceso")]
    public float radiusOn = 4f;
    [Tooltip("Softness quando apagado - fade mais duro")]
    public float softnessOff = 0.3f;
    [Tooltip("Softness quando aceso - fade mais suave")]
    public float softnessOn = 2.5f;
    public float lerpSpeed = 3f;

    [Header("Vinheta UI")]
    public Image vignetteImage;
    public float darkVignetteAlpha = 0.95f;
    public float litVignetteAlpha = 0.55f;
    public float poweredVignetteAlpha = 0f;
    public float vignetteSpeed = 3f;

    [Header("Filtro Amarelado UI")]
    [Tooltip("Image no Canvas cor solida amarela a cobrir o ecra")]
    public Image tintImage;
    public Color tintColor = new Color(1f, 0.8f, 0.3f, 0f);
    [Range(0f, 1f)]
    public float tintMaxAlpha = 0.12f;
    public float tintLerpSpeed = 4f;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private bool isLit = false;
    private float targetVignetteAlpha;
    private float targetTintAlpha;
    private Vector3 targetModelPos;

    private float currentRadius;
    private float currentSoftness;
    private float targetRadius;
    private float targetSoftness;

    private static readonly int ID_Radius = Shader.PropertyToID("_DarknessRadius");
    private static readonly int ID_Softness = Shader.PropertyToID("_DarknessSoftness");

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (lighterModel != null)
        {
            lighterModel.SetActive(true);
            lighterModel.transform.localPosition = hiddenPosition;
        }

        targetModelPos = hiddenPosition;

        // comecar com raio pequeno (isqueiro apagado)
        currentRadius = radiusOff;
        currentSoftness = softnessOff;
        targetRadius = radiusOff;
        targetSoftness = softnessOff;

        // vinheta cheia
        targetVignetteAlpha = darkVignetteAlpha;
        SetVignetteAlpha(darkVignetteAlpha);

        // tint invisivel
        targetTintAlpha = 0f;
        SetTintAlpha(0f);
    }

    void Update()
    {
        HandleInput();
        HandleModelAnimation();
        HandleVignette();
        HandleTint();
        HandleRadiusTransition();
    }

    // ---------------------------------------------
    //  INPUT
    // ---------------------------------------------
    void HandleInput()
    {
        if (Input.GetKeyDown(toggleKey))
            Toggle();
    }

    void Toggle()
    {
        isLit = !isLit;

        targetModelPos = isLit ? visiblePosition : hiddenPosition;

        // so altera o raio quando escuro
        if (DarknessManager.Instance == null || DarknessManager.Instance.IsDark())
        {
            targetRadius = isLit ? radiusOn : radiusOff;
            targetSoftness = isLit ? softnessOn : softnessOff;
        }

        targetTintAlpha = isLit ? tintMaxAlpha : 0f;

        UpdateVignetteTarget();

        if (isLit && igniteSound != null)
            audioSource.PlayOneShot(igniteSound);
        else if (!isLit && extinguishSound != null)
            audioSource.PlayOneShot(extinguishSound);

        Debug.Log(isLit ? "Isqueiro aceso" : "Isqueiro apagado");
    }

    // ---------------------------------------------
    //  TRANSICAO DO RAIO
    // ---------------------------------------------
    void HandleRadiusTransition()
    {
        if (DarknessManager.Instance != null && !DarknessManager.Instance.IsDark()) return;

        currentRadius = Mathf.Lerp(currentRadius, targetRadius, Time.deltaTime * lerpSpeed);
        currentSoftness = Mathf.Lerp(currentSoftness, targetSoftness, Time.deltaTime * lerpSpeed);

        Shader.SetGlobalFloat(ID_Radius, currentRadius);
        Shader.SetGlobalFloat(ID_Softness, currentSoftness);
    }

    // ---------------------------------------------
    //  ANIMACAO DO MODELO
    // ---------------------------------------------
    void HandleModelAnimation()
    {
        if (lighterModel == null) return;

        lighterModel.transform.localPosition = Vector3.Lerp(
            lighterModel.transform.localPosition,
            targetModelPos,
            Time.deltaTime * drawSpeed);
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

    // ---------------------------------------------
    //  FILTRO AMARELADO
    // ---------------------------------------------
    void HandleTint()
    {
        if (tintImage == null) return;
        Color c = tintColor;
        c.a = Mathf.Lerp(tintImage.color.a, targetTintAlpha, Time.deltaTime * tintLerpSpeed);
        tintImage.color = c;
    }

    void SetVignetteAlpha(float alpha)
    {
        if (vignetteImage == null) return;
        Color c = vignetteImage.color;
        c.a = alpha;
        vignetteImage.color = c;
    }

    void SetTintAlpha(float alpha)
    {
        if (tintImage == null) return;
        Color c = tintColor;
        c.a = alpha;
        tintImage.color = c;
    }

    // ---------------------------------------------
    //  ACESSO PUBLICO
    // ---------------------------------------------
    public bool IsLit() => isLit;
}