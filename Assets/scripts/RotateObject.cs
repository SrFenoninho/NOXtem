using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [Header("Rotation Settings")]
    public Vector3 rotationSpeed = new Vector3(0, 0, 0);

    [Header("Particle System Settings")]
    public bool useParticles = true;
    public Color particleColor = Color.white;
    [Range(5, 50f)]
    public int emissionRate = 10;
    [Range(0.1f, 2f)]
    public float particleSize = 0.3f;

    private ParticleSystem particles;

    void Start()
    {
        if (useParticles)
        {
            CreateParticleSystem();
        }
    }

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }

    void CreateParticleSystem()
    {
        GameObject particleObj = new GameObject("Particles");
        particleObj.transform.SetParent(transform);

        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            particleObj.transform.position = rend.bounds.center;
        }
        else
        {
            particleObj.transform.localPosition = Vector3.zero;
        }

            particles = particleObj.AddComponent<ParticleSystem>();

        var main = particles.main;
        main.startColor = particleColor;
        main.startSize = particleSize;
        main.startSpeed = 0.5f;
        main.startLifetime = 1.5f;
        main.maxParticles = 50;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = particles.emission;
        emission.rateOverTime = emissionRate;

        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        var sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, 0f);
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
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Mobile/Particles/Additive"));
        renderer.material.SetColor("_TintColor", particleColor);
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
    }
}