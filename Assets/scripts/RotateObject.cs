using UnityEngine;

public class RotateObject : MonoBehaviour
{




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("Rotation Settings")]

    public Vector3 rotationSpeed = new Vector3(0, 0, 0);

    [Header("Particle System Settings")]
    public bool useParticles = true;
    public Vector3 particleOffset = new Vector3(0f, 0f, -0.5f);
    public Color particleColor = Color.white;
    public Texture particleTexture;



    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    [Range(5, 50f)]
    public int emissionRate = 10;
    [Range(0.1f, 2f)]
    public float particleSize = 0.3f;
    public float particleLifetime = 3f;
    public float particleRotationSpeed = 0.5f;

    private ParticleSystem particles;
    private MeshRenderer meshRenderer;
    private bool effectsStopped = false;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (useParticles)
            CreateParticleSystem();
    }

    void Update()
    {
        if (!effectsStopped && meshRenderer != null && !meshRenderer.enabled)
        {
            StopEffects();
        }

        if (!effectsStopped)
            transform.Rotate(rotationSpeed * Time.deltaTime);
    }




    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void StopEffects()
    {
        effectsStopped = true;
        if (particles != null)
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }




    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    void CreateParticleSystem()
    {
        GameObject particleObj = new GameObject("Particles");
        particleObj.transform.SetParent(transform);

        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
            particleObj.transform.position = rend.bounds.center;
        else
            particleObj.transform.localPosition = Vector3.zero;

        particleObj.transform.Translate(particleOffset, Space.Self);

        particles = particleObj.AddComponent<ParticleSystem>();

        var main = particles.main;
        main.startColor = particleColor;
        main.startSize = particleSize;
        main.startSpeed = 0f;
        main.startLifetime = particleLifetime;
        main.maxParticles = 1;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 1, 1, 0, particleLifetime) });

        var shape = particles.shape;
        shape.enabled = false;

        var rotOverLifetime = particles.rotationOverLifetime;
        rotOverLifetime.enabled = true;
        rotOverLifetime.z = new ParticleSystem.MinMaxCurve(particleRotationSpeed);

        var sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.01f);
        sizeCurve.AddKey(0.15f, 1f);
        sizeCurve.AddKey(0.85f, 1f);
        sizeCurve.AddKey(1f, 0.01f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(particleColor, 0f),
                new GradientColorKey(particleColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.15f),
                new GradientAlphaKey(1f, 0.85f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

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
            renderer.material.SetColor("_BaseColor", particleColor);
        else if (renderer.material.HasProperty("_TintColor"))
            renderer.material.SetColor("_TintColor", particleColor);
        else if (renderer.material.HasProperty("_Color"))
            renderer.material.color = particleColor;

        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        particles.Play();
    }
}
