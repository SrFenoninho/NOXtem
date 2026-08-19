using UnityEngine;

public class DarknessManager : MonoBehaviour
{





    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    public static DarknessManager Instance { get; private set; }

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
    //  PRIVATE STATE
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
        if (!powerRestored || inDarkZone)
        {
            if (lighter != null)
            {
                currentRadius = lighter.GetCurrentRadius();
                currentSoftness = lighter.GetCurrentSoftness();
            }
            else
            {
                currentRadius = darkRadius;
                currentSoftness = darkSoftness;
            }

            currentAmbient = ambientLight;
        }
        else
        {
            currentRadius = Mathf.Lerp(currentRadius, lightRadius, Time.deltaTime * transitionSpeed);
            currentSoftness = Mathf.Lerp(currentSoftness, lightSoftness, Time.deltaTime * transitionSpeed);
            currentAmbient = Mathf.Lerp(currentAmbient, lightAmbient, Time.deltaTime * transitionSpeed);
        }

        ApplyToShader();
    }




    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    void ApplyToShader()
    {
        Shader.SetGlobalColor("_DarknessColor", darknessColor);
        Shader.SetGlobalFloat("_DarknessRadius", currentRadius);
        Shader.SetGlobalFloat("_DarknessSoftness", currentSoftness);
        Shader.SetGlobalFloat("_AmbientLight", currentAmbient);
    }




    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void OnPowerRestored()
    {
        powerRestored = true;

        CeilingLight[] ceilingLights = Object.FindFirstObjectByType<CeilingLight>() != null 
            ? Object.FindObjectsByType<CeilingLight>(FindObjectsSortMode.None) 
            : new CeilingLight[0];

        foreach (CeilingLight light in ceilingLights)
        {
            if (light != null) light.TurnOn();
        }
    }

    public void SetInDarkZone(bool state)
    {
        inDarkZone = state;

        if (!inDarkZone && powerRestored)
        {
        }
    }

    public bool IsDark()
    {
        return !powerRestored;
    }

    public bool IsInDarkZone()
    {
        return inDarkZone;
    }
}
