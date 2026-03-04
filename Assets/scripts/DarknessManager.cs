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
    [Tooltip("Se verdadeiro, a escuridao ativa automaticamente ao iniciar a cena")]
    public bool startWithDarkness = true;

    [Header("Height Fog Base")]
    [Tooltip("Y do chao da cena - deve ser igual ao Y do chao no mundo")]
    public float heightFogYBase = -0.125f;

    [Header("Estado Escuro (sem energia)")]
    public float darkHeightFogDensity = 0.97f;
    public float darkHeightFogFalloff = 8.0f;
    public Color darkHeightFogColor = new Color(0.01f, 0.01f, 0.01f);
    public float darkDistFogDensity = 0.97f;
    public float darkDistFogStart = 3f;
    public float darkDistFogEnd = 8f;

    [Header("Estado Iluminado (com energia)")]
    public float lightHeightFogDensity = 0.1f;
    public float lightHeightFogFalloff = 2.0f;
    public Color lightHeightFogColor = new Color(0.05f, 0.05f, 0.08f);
    public float lightDistFogDensity = 0.2f;
    public float lightDistFogStart = 12f;
    public float lightDistFogEnd = 35f;

    [Header("Transicao")]
    public float transitionSpeed = 0.4f;

    [Header("Zonas Escuras Configuráveis")]
    [Tooltip("Zonas especificas que ficam sempre escuras mesmo com energia")]
    public DarkZone[] darkZones;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private bool powerRestored = false;
    private bool inTransition = false;
    private float transitionT = 0f;

    // IDs dos parametros do shader (mais rapido que strings)
    private static readonly int ID_HeightDensity = Shader.PropertyToID("_HeightFogDensity");
    private static readonly int ID_HeightFalloff = Shader.PropertyToID("_HeightFogFalloff");
    private static readonly int ID_HeightOffset = Shader.PropertyToID("_HeightFogOffset");
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
        {
            ApplyToMaterials(
                darkHeightFogDensity, darkHeightFogFalloff, darkHeightFogColor,
                darkDistFogDensity, darkDistFogStart, darkDistFogEnd, darkHeightFogColor
            );
        }
        else
        {
            ApplyToMaterials(
                lightHeightFogDensity, lightHeightFogFalloff, lightHeightFogColor,
                lightDistFogDensity, lightDistFogStart, lightDistFogEnd, lightHeightFogColor
            );
            powerRestored = true;
        }
    }

    void Update()
    {
        if (!inTransition) return;

        transitionT += Time.deltaTime * transitionSpeed;
        float t = Mathf.Clamp01(transitionT);

        // Interpolar todos os parametros do shader suavemente
        ApplyToMaterials(
            Mathf.Lerp(darkHeightFogDensity, lightHeightFogDensity, t),
            Mathf.Lerp(darkHeightFogFalloff, lightHeightFogFalloff, t),
            Color.Lerp(darkHeightFogColor, lightHeightFogColor, t),
            Mathf.Lerp(darkDistFogDensity, lightDistFogDensity, t),
            Mathf.Lerp(darkDistFogStart, lightDistFogStart, t),
            Mathf.Lerp(darkDistFogEnd, lightDistFogEnd, t),
            Color.Lerp(darkHeightFogColor, lightHeightFogColor, t)
        );

        if (t >= 1f)
            inTransition = false;
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
    //  APLICAR A TODOS OS MATERIAIS DO SHADER
    // ---------------------------------------------
    // Shader.SetGlobal aplica a TODOS os materiais que usam o PS1Effect
    // sem precisar de arrastar nada no Inspector
    void ApplyToMaterials(float hDensity, float hFalloff, Color hColor,
                          float dDensity, float dStart, float dEnd, Color dColor)
    {
        Shader.SetGlobalFloat(ID_HeightDensity, hDensity);
        Shader.SetGlobalFloat(ID_HeightFalloff, hFalloff);
        Shader.SetGlobalFloat(ID_HeightOffset, heightFogYBase);
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
        {
            if (zone.zoneID == zoneID)
            {
                zone.SetDark(dark);
                return;
            }
        }
    }

    // ---------------------------------------------
    //  ACESSO PUBLICO
    // ---------------------------------------------
    public bool IsDark() => !powerRestored;

    // Forcar escuridao manual em runtime - util para eventos de scripting
    public void SetDarkness(float density, Color color)
    {
        if (powerRestored) return;
        Shader.SetGlobalFloat(ID_HeightDensity, density);
        Shader.SetGlobalColor(ID_HeightColor, color);
    }
}

// ---------------------------------------------
//  ESTRUTURA DE ZONA ESCURA
// ---------------------------------------------
[System.Serializable]
public class DarkZone
{
    public string zoneID = "Zone_A";

    [Tooltip("Luzes a desligar nesta zona")]
    public Light[] zoneLights;

    public void SetDark(bool dark)
    {
        foreach (Light l in zoneLights)
            if (l != null) l.enabled = !dark;
    }
}