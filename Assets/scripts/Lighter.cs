using UnityEngine;
using UnityEngine.UI;

public class Lighter : MonoBehaviour
{
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

    private bool isLit = false;
    private float currentRadius;
    private float currentSoftness;
    private float targetRadius;
    private float targetSoftness;
    private float origRadiusOff;
    private float origRadiusOn;
    private float origSoftnessOff;
    private float origSoftnessOn;

    void Start()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        origRadiusOff = radiusOff;
        origRadiusOn = radiusOn;
        origSoftnessOff = softnessOff;
        origSoftnessOn = softnessOn;

        if (lighterModel != null)
        {
            lighterModel.SetActive(true);
            lighterModel.transform.localPosition = hiddenPosition;
        }

        currentRadius = radiusOff;
        currentSoftness = softnessOff;
        targetRadius = radiusOff;
        targetSoftness = softnessOff;

        if (vignetteImage != null) SetAlpha(vignetteImage, darkVignetteAlpha);
        if (tintImage != null) SetAlpha(tintImage, 0f);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey)) Toggle();

        currentRadius = Mathf.Lerp(currentRadius, targetRadius, Time.deltaTime * lerpSpeed);
        currentSoftness = Mathf.Lerp(currentSoftness, targetSoftness, Time.deltaTime * lerpSpeed);

        UpdateVignette();
        UpdateTint();
        UpdateModel();
    }

    void Toggle()
    {
        isLit = !isLit;

        targetRadius = isLit ? radiusOn : radiusOff;
        targetSoftness = isLit ? softnessOn : softnessOff;

        if (isLit && igniteSound != null) audioSource.PlayOneShot(igniteSound);
        if (!isLit && extinguishSound != null) audioSource.PlayOneShot(extinguishSound);

        Debug.Log(isLit ? "Isqueiro aceso" : "Isqueiro apagado");
    }

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
        lighterModel.transform.localPosition = Vector3.Lerp(
            lighterModel.transform.localPosition, target, Time.deltaTime * drawSpeed);
    }

    // chamado pelo DarkZone ao entrar
    public void SetZoneValues(float darkRad, float darkSoft)
    {
        // CORREÇÃO: O isqueiro quando ACESO usa sempre a sua força original (ex: 4f)
        radiusOn = origRadiusOn;
        softnessOn = origSoftnessOn;

        // O isqueiro quando APAGADO passa a usar a escuridão da zona (ex: 0.5f)
        radiusOff = darkRad;
        softnessOff = darkSoft;

        targetRadius = isLit ? radiusOn : radiusOff;
        targetSoftness = isLit ? softnessOn : softnessOff;

        // Forçamos a variável current para que a escuridão caia imediatamente se ele estiver apagado
        currentRadius = targetRadius;
        currentSoftness = targetSoftness;
    }

    // chamado pelo DarkZone ao sair
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

    void SetAlpha(Image img, float a) { Color c = img.color; c.a = a; img.color = c; }

    // chamado pelo IntroManager para acender/apagar o isqueiro programaticamente
    public void ForceLight(bool state)
    {
        if (isLit == state) return;
        isLit = state;
        targetRadius = isLit ? radiusOn : radiusOff;
        targetSoftness = isLit ? softnessOn : softnessOff;
        if (isLit && igniteSound != null) audioSource.PlayOneShot(igniteSound);
        if (!isLit && extinguishSound != null) audioSource.PlayOneShot(extinguishSound);
    }

    public float GetCurrentRadius() => currentRadius;
    public float GetCurrentSoftness() => currentSoftness;
    public bool IsLit() => isLit;
}