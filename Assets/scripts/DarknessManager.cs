using UnityEngine;

public class DarknessManager : MonoBehaviour
{
    // ---------------------------------------------
    //  SINGLETON
    // ---------------------------------------------
    public static DarknessManager Instance { get; private set; }

    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Controlo")]
    public bool startWithDarkness = true;

    [Header("Estado Escuro (sem energia)")]
    public float darkGlobalDarkness = 0.85f;  // escuridao base - chao fica escuro
    public float darkHeightFogDensity = 0.9f;
    public float darkHeightFogFalloff = 5.0f;
    public Color darkFogColor = new Color(0.02f, 0.02f, 0.02f);
    public float darkDistFogDensity = 0.95f;
    public float darkDistFogStart = 1f;
    public float darkDistFogEnd = 1f;

    [Header("Estado Iluminado (com energia)")]
    public float lightGlobalDarkness = 0.1f;
    public float lightHeightFogDensity = 0.15f;
    public float lightHeightFogFalloff = 2.0f;
    public Color lightFogColor = new Color(0.05f, 0.05f, 0.08f);
    public float lightDistFogDensity = 0.4f;
    public float lightDistFogStart = 10f;
    public float lightDistFogEnd = 25f;

    [Header("Transicao")]
    public float transitionSpeed = 1.5f;

    [Header("Zonas Escuras Configuráveis")]
    public DarkZone[] darkZones;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private bool powerRestored = false;
    private bool inTransition = false;
    private float transitionT = 0f;

    private static readonly int ID_GlobalDarkness = Shader.PropertyToID("_GlobalDarkness");
    private static readonly int ID_HeightDensity = Shader.PropertyToID("_HeightFogDensity");
    private static readonly int ID_HeightFalloff = Shader.PropertyToID("_HeightFogFalloff");
    private static readonly int ID_HeightColor = Shader.PropertyToID("_HeightFogColor");
    private static readonly int ID_DistDensity = Shader.PropertyToID("_DistFogDensity");
    private static readonly int ID_DistStart = Shader.PropertyToID("_DistFogStart");
    private static readonly int ID_DistEnd = Shader.PropertyToID("_DistFogEnd");
    private static readonly int ID_DistColor = Shader.PropertyToID("_DistFogColor");

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (startWithDarkness)
            ApplyShader(darkGlobalDarkness, darkHeightFogDensity, darkHeightFogFalloff, darkFogColor,
                        darkDistFogDensity, darkDistFogStart, darkDistFogEnd, darkFogColor);
        else
        {
            ApplyShader(lightGlobalDarkness, lightHeightFogDensity, lightHeightFogFalloff, lightFogColor,
                        lightDistFogDensity, lightDistFogStart, lightDistFogEnd, lightFogColor);
            powerRestored = true;
        }
    }

    void Update()
    {
        if (!inTransition) return;

        transitionT += Time.deltaTime * transitionSpeed;
        float t = Mathf.Clamp01(transitionT);

        ApplyShader(
            Mathf.Lerp(darkGlobalDarkness, lightGlobalDarkness, t),
            Mathf.Lerp(darkHeightFogDensity, lightHeightFogDensity, t),
            Mathf.Lerp(darkHeightFogFalloff, lightHeightFogFalloff, t),
            Color.Lerp(darkFogColor, lightFogColor, t),
            Mathf.Lerp(darkDistFogDensity, lightDistFogDensity, t),
            Mathf.Lerp(darkDistFogStart, lightDistFogStart, t),
            Mathf.Lerp(darkDistFogEnd, lightDistFogEnd, t),
            Color.Lerp(darkFogColor, lightFogColor, t)
        );

        if (t >= 1f) inTransition = false;
    }

    // ---------------------------------------------
    //  CHAMADO PELO LeverSystem
    // ---------------------------------------------
    public void OnPowerRestored()
    {
        if (powerRestored) return;
        powerRestored = true;
        transitionT = 0f;
        inTransition = true;
        Debug.Log("DarknessManager: a iluminar a cena...");
    }

    // ---------------------------------------------
    //  APLICAR AO SHADER
    // ---------------------------------------------
    void ApplyShader(float gDark, float hDensity, float hFalloff, Color hColor,
                     float dDensity, float dStart, float dEnd, Color dColor)
    {
        Shader.SetGlobalFloat(ID_GlobalDarkness, gDark);
        Shader.SetGlobalFloat(ID_HeightDensity, hDensity);
        Shader.SetGlobalFloat(ID_HeightFalloff, hFalloff);
        Shader.SetGlobalColor(ID_HeightColor, hColor);
        Shader.SetGlobalFloat(ID_DistDensity, dDensity);
        Shader.SetGlobalFloat(ID_DistStart, dStart);
        Shader.SetGlobalFloat(ID_DistEnd, dEnd);
        Shader.SetGlobalColor(ID_DistColor, dColor);
    }

    // ---------------------------------------------
    //  ZONAS ESCURAS MANUAIS
    // ---------------------------------------------
    public void SetDarkZone(string zoneID, bool dark)
    {
        foreach (DarkZone zone in darkZones)
            if (zone.zoneID == zoneID) { zone.SetDark(dark); return; }
    }

    // ---------------------------------------------
    //  ACESSO PUBLICO
    // ---------------------------------------------
    public bool IsDark() => !powerRestored;
}

// ---------------------------------------------
//  ESTRUTURA DE ZONA ESCURA
// ---------------------------------------------
[System.Serializable]
public class DarkZone
{
    public string zoneID = "Zone_A";
    public Light[] zoneLights;

    public void SetDark(bool dark)
    {
        foreach (Light l in zoneLights)
            if (l != null) l.enabled = !dark;
    }
}