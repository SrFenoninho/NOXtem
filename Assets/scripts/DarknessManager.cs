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
    [Header("Darkness Settings")]
    public Color darknessColor = new Color(0.02f, 0.02f, 0.02f);
    public float ambientLight = 0f;

    [Header("Dark State Values")]
    public float darkRadius = 0.5f;
    public float darkSoftness = 0.5f;

    [Header("Light State Values")]
    public float lightRadius = 15f;
    public float lightSoftness = 5f;
    public float lightAmbient = 1f;

    [Header("Transition")]
    public float transitionSpeed = 1.5f;
    public bool startWithDarkness = true;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private bool powerRestored = false;
    private bool inDarkZone = false;

    private float currentRadius;
    private float currentSoftness;
    private float currentAmbient;

    private Lighter lighter;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        lighter = Object.FindFirstObjectByType<Lighter>();

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

        ApplyToShader();
    }

    void Update()
    {
        // Se a energia estiver desligada OU o jogador estiver numa DarkZone,
        // o isqueiro assume o controlo do raio de visão
        if (!powerRestored || inDarkZone)
        {
            if (lighter != null)
            {
                // Ler os valores que o Lighter.cs está a calcular
                currentRadius = lighter.GetCurrentRadius();
                currentSoftness = lighter.GetCurrentSoftness();
            }
            else
            {
                currentRadius = darkRadius;
                currentSoftness = darkSoftness;
            }

            // O ambiente em geral continua escuro
            currentAmbient = ambientLight;
        }
        else
        {
            // A energia voltou e estamos fora da DarkZone — tudo fica claro gradualmente
            currentRadius = Mathf.Lerp(currentRadius, lightRadius, Time.deltaTime * transitionSpeed);
            currentSoftness = Mathf.Lerp(currentSoftness, lightSoftness, Time.deltaTime * transitionSpeed);
            currentAmbient = Mathf.Lerp(currentAmbient, lightAmbient, Time.deltaTime * transitionSpeed);
        }

        // Aplica ao shader todos os frames para vermos o isqueiro a iluminar
        ApplyToShader();
    }

    // ---------------------------------------------
    //  SHADER
    // ---------------------------------------------
    void ApplyToShader()
    {
        Shader.SetGlobalColor("_DarknessColor", darknessColor);
        Shader.SetGlobalFloat("_DarknessRadius", currentRadius);
        Shader.SetGlobalFloat("_DarknessSoftness", currentSoftness);
        Shader.SetGlobalFloat("_AmbientLight", currentAmbient);
    }

    // ---------------------------------------------
    //  ENERGIA
    // ---------------------------------------------
    public void OnPowerRestored()
    {
        powerRestored = true;
        Debug.Log("DarknessManager: Power restored, illuminating scene...");
    }

    // ---------------------------------------------
    //  ZONA ESCURA
    // ---------------------------------------------
    public void SetInDarkZone(bool state)
    {
        inDarkZone = state;

        if (!inDarkZone && powerRestored)
        {
            Debug.Log("DarknessManager: Exited Dark Zone, restoring light transition...");
        }
    }

    // ---------------------------------------------
    //  CONSULTAS
    // ---------------------------------------------
    public bool IsDark()
    {
        return !powerRestored;
    }

    public bool IsInDarkZone()
    {
        return inDarkZone;
    }
}