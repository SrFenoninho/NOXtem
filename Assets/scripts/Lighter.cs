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
    public GameObject rightArmRoot;
    private Renderer[] armRenderers;
    public Vector3 hiddenPosition = new Vector3(0.4f, -0.5f, 0.6f);
    public Vector3 visiblePosition = new Vector3(0.25f, -0.25f, 0.5f);
    public float drawSpeed = 8f;

    [Header("Audio")]
    public AudioClip igniteSound;
    public AudioClip extinguishSound;
    private AudioSource audioSource;

    [Header("Raio de Visibilidade")]
    public float radiusOff = 0.5f;
    public float radiusOn = 4f;
    public float softnessOff = 0.3f;
    public float softnessOn = 2.5f;
    public float lerpSpeed = 3f;

    [Header("Vinheta UI")]
    public Image vignetteImage;
    public float darkVignetteAlpha = 0.95f;
    public float litVignetteAlpha = 0.55f;
    public float poweredVignetteAlpha = 0f;
    public float vignetteSpeed = 3f;

    [Header("Filtro Amarelado UI")]
    public Image tintImage;
    public Color tintColor = new Color(1f, 0.8f, 0.3f, 0f);
    [Range(0f, 1f)]
    public float tintMaxAlpha = 0.12f;
    public float tintLerpSpeed = 4f;

    [Header("Luz Unity Nativa & Efeito de Fogo Vivo")]
    public Light lighterLight;
    public float lightIntensityOn = 2.0f;
    public float lightRangeOn = 4.0f;
    public Color flameColorPrimary = new Color(1.0f, 0.65f, 0.20f);
    public Color flameColorSecondary = new Color(1.0f, 0.45f, 0.10f);
    public bool useFireFlicker = true;
    public float flickerSpeed = 12f;
    public float flickerAmount = 0.5f;
    public float rangeFlickerAmount = 1.0f;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    [HideInInspector] public bool inputBlocked = false;
    private bool isLit = false;
    private float currentRadius;
    private float currentSoftness;
    private float targetRadius;
    private float targetSoftness;
    private float origRadiusOff;
    private float origRadiusOn;
    private float origSoftnessOff;
    private float origSoftnessOn;

    private FPMove fpMove;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        if (lighterLight == null)
            lighterLight = GetComponent<Light>() ?? GetComponentInChildren<Light>();

        origRadiusOff = radiusOff;
        origRadiusOn = radiusOn;
        origSoftnessOff = softnessOff;
        origSoftnessOn = softnessOn;

        if (lighterModel != null)
        {
            lighterModel.SetActive(true);
            lighterModel.transform.localPosition = hiddenPosition;
        }

        if (rightArmRoot != null)
        {
            armRenderers = rightArmRoot.GetComponentsInChildren<Renderer>();
            SetArmVisibility(true);
        }

        currentRadius = radiusOff;
        currentSoftness = softnessOff;
        targetRadius = radiusOff;
        targetSoftness = softnessOff;

        if (vignetteImage != null) SetAlpha(vignetteImage, darkVignetteAlpha);
        if (tintImage != null) SetAlpha(tintImage, 0f);

        fpMove = GetComponentInParent<FPMove>();

        UpdateLightComponent();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey)) Toggle();

        currentRadius = Mathf.Lerp(currentRadius, targetRadius, Time.deltaTime * lerpSpeed);
        currentSoftness = Mathf.Lerp(currentSoftness, targetSoftness, Time.deltaTime * lerpSpeed);

        UpdateVignette();
        UpdateTint();
        UpdateModel();
        UpdateLightComponent();
    }

    // ---------------------------------------------
    //  LIGAR / DESLIGAR
    // ---------------------------------------------
    void Toggle()
    {
        if (inputBlocked) return;
        if (GameStateManager.Instance != null && !GameStateManager.Instance.Is(GameState.Gameplay)) return;
        isLit = !isLit;

        targetRadius = isLit ? radiusOn : radiusOff;
        targetSoftness = isLit ? softnessOn : softnessOff;

        if (isLit && igniteSound != null) audioSource.PlayOneShot(igniteSound);
        if (!isLit && extinguishSound != null) audioSource.PlayOneShot(extinguishSound);

        SetArmVisibility(!isLit);

        // Debug.Log(isLit ? "Isqueiro aceso" : "Isqueiro apagado");
    }

    // ---------------------------------------------
    //  EFEITOS VISUAIS
    // ---------------------------------------------
    void UpdateVignette()
    {
        if (vignetteImage == null) return;

        bool powered = DarknessManager.Instance != null
            && !DarknessManager.Instance.IsDark()
            && !DarknessManager.Instance.IsInDarkZone();

        float target = powered ? poweredVignetteAlpha
                     : isLit ? litVignetteAlpha
                     : darkVignetteAlpha;

        Color c = vignetteImage.color;
        c.a = Mathf.Lerp(c.a, target, Time.deltaTime * vignetteSpeed);
        vignetteImage.color = c;
    }

    void UpdateTint()
    {
        if (tintImage == null) return;
        Color c = tintColor;
        c.a = Mathf.Lerp(tintImage.color.a, isLit ? tintMaxAlpha : 0f, Time.deltaTime * tintLerpSpeed);
        tintImage.color = c;
    }

    void UpdateModel()
    {
        if (lighterModel == null) return;
        Vector3 target = isLit ? visiblePosition : hiddenPosition;

        if (fpMove != null)
        {
            float cameraYDelta = fpMove.CurrentCameraY - fpMove.standingCameraY;
            target.y += cameraYDelta;
        }

        lighterModel.transform.localPosition = Vector3.Lerp(
            lighterModel.transform.localPosition, target, Time.deltaTime * drawSpeed);
    }

    void UpdateLightComponent()
    {
        if (lighterLight != null)
        {
            lighterLight.enabled = isLit;
            if (isLit)
            {
                if (useFireFlicker)
                {
                    float n1 = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
                    float n2 = Mathf.PerlinNoise(0f, Time.time * (flickerSpeed * 0.7f));

                    // 1. Transição dinâmica da cor da chama
                    lighterLight.color = Color.Lerp(flameColorSecondary, flameColorPrimary, n1);

                    // 2. Variação do raio de luz (usando Range Flicker Amount do Inspector)
                    float rangeDip = (n2 - 0.5f) * rangeFlickerAmount;
                    lighterLight.range = Mathf.Max(0.2f, lightRangeOn + rangeDip);

                    // 3. Variação da intensidade de luz (usando Flicker Amount do Inspector)
                    float intensityFlicker = (n1 - 0.5f) * flickerAmount;
                    lighterLight.intensity = Mathf.Max(0.05f, lightIntensityOn + intensityFlicker);
                }
                else
                {
                    lighterLight.color = flameColorPrimary;
                    lighterLight.range = lightRangeOn;
                    lighterLight.intensity = lightIntensityOn;
                }
            }
            else
            {
                lighterLight.intensity = 0f;
                lighterLight.range = 0.1f;
            }
        }
    }

    // ---------------------------------------------
    //  VALORES DE ZONA
    // ---------------------------------------------
    public void SetZoneValues(float darkRad, float darkSoft)
    {
        radiusOn = origRadiusOn;
        softnessOn = origSoftnessOn;

        radiusOff = darkRad;
        softnessOff = darkSoft;

        targetRadius = isLit ? radiusOn : radiusOff;
        targetSoftness = isLit ? softnessOn : softnessOff;

        currentRadius = targetRadius;
        currentSoftness = targetSoftness;
    }

    public void ClearZoneValues()
    {
        radiusOff = origRadiusOff;
        radiusOn = origRadiusOn;
        softnessOff = origSoftnessOff;
        softnessOn = origSoftnessOn;

        targetRadius = isLit ? radiusOn : radiusOff;
        targetSoftness = isLit ? softnessOn : softnessOff;

        currentRadius = targetRadius;
        currentSoftness = targetSoftness;
    }

    // ---------------------------------------------
    //  AUXILIARES
    // ---------------------------------------------
    void SetAlpha(Image img, float a) { Color c = img.color; c.a = a; img.color = c; }

    void SetArmVisibility(bool visible)
    {
        if (armRenderers == null) return;
        foreach (Renderer r in armRenderers)
        {
            if (r != null) r.enabled = visible;
        }
    }

    public void ForceLight(bool state)
    {
        if (isLit == state) return;
        isLit = state;
        targetRadius = isLit ? radiusOn : radiusOff;
        targetSoftness = isLit ? softnessOn : softnessOff;
        if (isLit && igniteSound != null) audioSource.PlayOneShot(igniteSound);
        if (!isLit && extinguishSound != null) audioSource.PlayOneShot(extinguishSound);

        SetArmVisibility(!isLit);
    }

    // ---------------------------------------------
    //  CONSULTAS
    // ---------------------------------------------
    public float GetCurrentRadius() => currentRadius;
    public float GetCurrentSoftness() => currentSoftness;
    public bool IsLit() => isLit;
}
