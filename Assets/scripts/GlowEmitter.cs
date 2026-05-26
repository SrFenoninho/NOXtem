using UnityEngine;

public class GlowEmitter : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Glow Settings")]
    public bool enableGlow = false;
    public Color glowColor = Color.white;
    
    [Header("Particle System Settings")]
    [Range(1, 40)] public int emissionRate = 10;
    [Range(0.02f, 0.4f)] public float particleSize = 0.25f;
    [Range(0.5f, 4f)] public float particleLifetime = 2.5f;
    
    [Header("Dispersão")]
    [Range(0.05f, 1f)] public float dispersalSpeed = 1.0f;
    
    [Header("Animation")]
    [Range(0.5f, 3f)] public float pulseSpeed = 1.5f;
    [Range(0.3f, 1f)] public float pulseMin = 0.3f;
    [Range(0.8f, 2f)] public float pulseMax = 1.2f;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private GameObject particleObj;
    private ParticleSystem particles;
    private ParticleSystem.EmissionModule emission;
    private float pulseTimer = 0f;
    private bool glowActive = true;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        if (enableGlow)
            InitializeGlow();
    }

    void Update()
    {
        if (enableGlow && glowActive && particles != null)
            UpdatePulse();
    }

    // ---------------------------------------------
    //  INICIALIZACAO DO GLOW
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

        particles = particleObj.AddComponent<ParticleSystem>();
        
        // Configurar main
        var main = particles.main;
        main.startColor = glowColor;
        main.startSize = new ParticleSystem.MinMaxCurve(particleSize * 0.8f, particleSize * 1.2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0f);
        main.startLifetime = particleLifetime;
        main.maxParticles = 150;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // Sem shape - as partículas nascem no centro
        var shape = particles.shape;
        shape.enabled = false;

        // Opacity fade
        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(glowColor, 0f),
                new GradientColorKey(glowColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        // Size over lifetime
        var sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(0.5f, 0.8f);
        sizeCurve.AddKey(1f, 0.3f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // Velocity over lifetime - partículas saem em grande raio
        var velocity = particles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(-dispersalSpeed, dispersalSpeed);
        velocity.y = new ParticleSystem.MinMaxCurve(-dispersalSpeed * 0.5f, dispersalSpeed * 1.5f);
        velocity.z = new ParticleSystem.MinMaxCurve(-dispersalSpeed, dispersalSpeed);
        velocity.space = ParticleSystemSimulationSpace.World;

        // Emission
        emission = particles.emission;
        emission.rateOverTime = emissionRate;

        // Renderer
        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.SetColor("_Color", glowColor);
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

    }

    // ---------------------------------------------
    //  ANIMACAO (PULSACAO)
    // ---------------------------------------------
    void UpdatePulse()
    {
        pulseTimer += Time.deltaTime * pulseSpeed;

        float pulse = Mathf.Lerp(
            pulseMin,
            pulseMax,
            (Mathf.Sin(pulseTimer) + 1f) / 2f
        );

        emission.rateOverTime = emissionRate * pulse;
    }

    // ---------------------------------------------
    //  CONTROLE DO GLOW
    // ---------------------------------------------
    public void DisableGlow()
    {
        glowActive = false;
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

        glowActive = true;
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