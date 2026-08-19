using UnityEngine;

public class GlowEmitter : MonoBehaviour
{




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("Glow Settings")]
    public bool enableGlow = false;
    public Color glowColor = Color.white;

    public Texture particleTexture; 

    [Header("Particle System Settings")]

    public Vector3 particleOffset = new Vector3(0f, 0f, -0.5f);
    [Range(1, 40)] public int emissionRate = 10;
    [Range(0.02f, 0.4f)] public float particleSize = 0.25f;
    [Range(0.5f, 10f)] public float particleLifetime = 3f;
    public float particleRotationSpeed = 0.5f;

    [Header("Dispersão")]
    [Range(0.05f, 1f)] public float dispersalSpeed = 1.0f;




    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private GameObject particleObj;
    private ParticleSystem particles;
    private ParticleSystem.EmissionModule emission;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        if (enableGlow)
            InitializeGlow();
    }




    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    void InitializeGlow()
    {
        particleObj = new GameObject("GlowParticles");
        particleObj.transform.SetParent(transform);
        particleObj.transform.localRotation = Quaternion.identity;
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
            particleObj.transform.position = rend.bounds.center;
        else
            particleObj.transform.localPosition = Vector3.zero;

        particleObj.transform.Translate(particleOffset, Space.Self);

        particles = particleObj.AddComponent<ParticleSystem>();

        var main = particles.main;
        main.startColor = glowColor;
        main.startSize = particleSize;
        main.startSpeed = 0f;
        main.startLifetime = particleLifetime;
        main.maxParticles = 1;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var shape = particles.shape;
        shape.enabled = false;

        var rotOverLifetime = particles.rotationOverLifetime;
        rotOverLifetime.enabled = true;
        rotOverLifetime.z = new ParticleSystem.MinMaxCurve(particleRotationSpeed);

        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(glowColor, 0f),
                new GradientColorKey(glowColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.15f),
                new GradientAlphaKey(1f, 0.85f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        var sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.01f);
        sizeCurve.AddKey(0.15f, 1f);
        sizeCurve.AddKey(0.85f, 1f);
        sizeCurve.AddKey(1f, 0.01f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 1, 1, 0, particleLifetime) });

        particles.Play();

        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (particleShader == null) particleShader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");

        renderer.material = new Material(particleShader);
        if (particleTexture != null)
        {
            if (renderer.material.HasProperty("_BaseMap")) renderer.material.SetTexture("_BaseMap", particleTexture);
            else renderer.material.mainTexture = particleTexture;
        }

        if (renderer.material.HasProperty("_BaseColor"))
            renderer.material.SetColor("_BaseColor", glowColor);
        else if (renderer.material.HasProperty("_TintColor"))
            renderer.material.SetColor("_TintColor", glowColor);
        else if (renderer.material.HasProperty("_Color"))
            renderer.material.color = glowColor;

        renderer.renderMode = ParticleSystemRenderMode.Billboard;
    }




    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void DisableGlow()
    {
        if (particles != null)
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (particleObj != null)
            particleObj.SetActive(false);
    }

    public void EnableGlow()
    {
        if (particleObj == null)
        {
            enableGlow = true;
            InitializeGlow();
        }

        if (particleObj != null)
            particleObj.SetActive(true);

        if (particles != null)
            particles.Play();
    }

    public void TriggerGlowEnd()
    {
        DisableGlow();
    }
}
