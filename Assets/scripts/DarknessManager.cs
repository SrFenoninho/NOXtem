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
    public Color darknessColor = new Color(0.02f, 0.02f, 0.02f); // quase preto
    public float ambientLight = 0.0f;   // 0 = preto total
    public float darkRadius = 0.5f;   // raio de visibilidade sem isqueiro
    public float darkSoftness = 0.5f;   // fade do circulo

    [Header("Estado Iluminado (com energia)")]
    public float lightRadius = 30f;    // raio grande - ve tudo
    public float lightSoftness = 10f;
    public float lightAmbient = 0.3f;   // alguma luz ambiente com energia

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

    private float currentRadius;
    private float currentSoftness;
    private float currentAmbient;

    private static readonly int ID_DarknessColor = Shader.PropertyToID("_DarknessColor");
    private static readonly int ID_AmbientLight = Shader.PropertyToID("_AmbientLight");
    private static readonly int ID_DarknessRadius = Shader.PropertyToID("_DarknessRadius");
    private static readonly int ID_DarknessSoftness = Shader.PropertyToID("_DarknessSoftness");

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
        Shader.SetGlobalColor(ID_DarknessColor, darknessColor);

        if (startWithDarkness)
        {
            currentRadius = darkRadius;
            currentSoftness = darkSoftness;
            currentAmbient = ambientLight;
        }
        else
        {
            currentRadius = lightRadius;
            currentSoftness = lightSoftness;
            currentAmbient = lightAmbient;
            powerRestored = true;
        }

        ApplyShader(currentRadius, currentSoftness, currentAmbient);
    }

    void Update()
    {
        if (!inTransition) return;

        transitionT += Time.deltaTime * transitionSpeed;
        float t = Mathf.Clamp01(transitionT);

        currentRadius = Mathf.Lerp(darkRadius, lightRadius, t);
        currentSoftness = Mathf.Lerp(darkSoftness, lightSoftness, t);
        currentAmbient = Mathf.Lerp(ambientLight, lightAmbient, t);

        ApplyShader(currentRadius, currentSoftness, currentAmbient);

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
    //  AUXILIARES
    // ---------------------------------------------
    void ApplyShader(float radius, float softness, float ambient)
    {
        Shader.SetGlobalFloat(ID_DarknessRadius, radius);
        Shader.SetGlobalFloat(ID_DarknessSoftness, softness);
        Shader.SetGlobalFloat(ID_AmbientLight, ambient);
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
    public float GetRadius() => currentRadius;
    public float GetSoftness() => currentSoftness;
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