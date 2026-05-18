using UnityEngine;

public class GlowEmitter : MonoBehaviour
{
    // Tipos de forma
    public enum GlowShape { Cube, Sphere, Custom }

    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Glow Settings")]
    public bool enableGlow = true;
    public Color glowColor = Color.cyan;
    public GlowShape glowShape = GlowShape.Cube;
    
    [Header("Size & Opacity")]
    public bool autoFitToCollider = true;
    [Range(0.1f, 5f)] public float glowScale = 1.3f;
    [Range(0f, 1f)] public float glowOpacity = 0.4f;
    
    [Header("Glow Animation")]
    [Range(0.5f, 3f)] public float glowIntensityMin = 0.3f;
    [Range(1f, 5f)] public float glowIntensityMax = 1.0f;
    [Range(0.5f, 3f)] public float glowSpeed = 1.5f;

    // Optional: mesh customizada
    [Header("Custom Mesh (Optional)")]
    public Mesh customMesh;

    // Optional: desativar glow quando um evento acontece
    [Header("Auto Disable")]
    public bool autoDisableWhenTriggered = false;

    // ← NOVO: Sistema de partículas
    [Header("Particle System Settings")]
    public bool useParticles = true;
    public Color particleColor = Color.cyan;
    [Range(5, 50f)] public int emissionRate = 10;
    [Range(0.1f, 2f)] public float particleSize = 0.3f;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private GameObject glowObject;
    private MeshRenderer glowRenderer;
    private ParticleSystem particles;
    private float glowTimer = 0f;
    private bool glowActive = true;
    private Material glowMaterial;
    private Vector3 colliderSize = Vector3.one;

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
        if (enableGlow && glowActive && glowMaterial != null && glowObject != null)
            UpdateGlow();
    }

    // ---------------------------------------------
    //  SETUP DO GLOW
    // ---------------------------------------------
    void InitializeGlow()
    {
        // Cria um objeto filho com a forma do glow
        glowObject = new GameObject("GlowEmitter");
        glowObject.transform.SetParent(transform);
        glowObject.transform.localPosition = Vector3.zero;
        glowObject.transform.localRotation = Quaternion.identity;

        // Adiciona componentes
        MeshFilter meshFilter = glowObject.AddComponent<MeshFilter>();
        glowRenderer = glowObject.AddComponent<MeshRenderer>();

        // Define a mesh baseada na forma escolhida
        if (glowShape == GlowShape.Custom && customMesh != null)
        {
            meshFilter.mesh = customMesh;
        }
        else if (glowShape == GlowShape.Cube)
        {
            meshFilter.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        }
        else // Sphere
        {
            meshFilter.mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
        }

        // Ajusta tamanho automático ao collider
        if (autoFitToCollider)
        {
            AdjustScaleToCollider();
        }
        else
        {
            glowObject.transform.localScale = Vector3.one * glowScale;
        }

        // Cria material do glow
        glowMaterial = new Material(Shader.Find("Sprites/Default"));
        glowMaterial.name = "GlowMaterial_" + gameObject.name;
        
        // Blending transparente
        glowMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        glowMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        glowMaterial.SetInt("_ZWrite", 0);
        glowMaterial.renderQueue = 3000;

        UpdateGlowMaterial();
        glowRenderer.material = glowMaterial;

        // Desativa shadows
        glowRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        glowRenderer.receiveShadows = false;

        // Cria sistema de partículas
        if (useParticles)
            CreateParticleSystem();

        Debug.Log("GlowEmitter initialized on " + gameObject.name);
    }

    // Cria sistema de partículas como no RotateObject
    void CreateParticleSystem()
    {
        GameObject particleObj = new GameObject("GlowParticles");
        particleObj.transform.SetParent(transform);
        particleObj.transform.localPosition = Vector3.zero;

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

        // Particulas diminuem ao longo do tempo
        var sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // Fade de opacidade ao longo do tempo
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

    void AdjustScaleToCollider()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();

        if (boxCollider != null)
        {
            colliderSize = boxCollider.size;
            glowObject.transform.localScale = boxCollider.size * glowScale;
            glowObject.transform.localPosition = boxCollider.center;
            Debug.Log("Glow fitted to BoxCollider: " + glowObject.transform.localScale);
        }
        else if (sphereCollider != null)
        {
            float diameter = sphereCollider.radius * 2f;
            colliderSize = Vector3.one * diameter;
            glowObject.transform.localScale = Vector3.one * diameter * glowScale;
            glowObject.transform.localPosition = sphereCollider.center;
            Debug.Log("Glow fitted to SphereCollider: " + glowObject.transform.localScale);
        }
        else if (capsuleCollider != null)
        {
            float height = capsuleCollider.height;
            float radius = capsuleCollider.radius * 2f;
            colliderSize = new Vector3(radius, height, radius);
            glowObject.transform.localScale = colliderSize * glowScale;
            glowObject.transform.localPosition = capsuleCollider.center;
            Debug.Log("Glow fitted to CapsuleCollider: " + glowObject.transform.localScale);
        }
        else
        {
            glowObject.transform.localScale = Vector3.one * glowScale;
            glowObject.transform.localPosition = Vector3.zero;
            Debug.LogWarning("No collider found on " + gameObject.name);
        }
    }

    void UpdateGlow()
    {
        glowTimer += Time.deltaTime * glowSpeed;

        float glowIntensity = Mathf.Lerp(
            glowIntensityMin,
            glowIntensityMax,
            (Mathf.Sin(glowTimer) + 1f) / 2f
        );

        UpdateGlowMaterial(glowIntensity);
    }

    void UpdateGlowMaterial(float intensity = 1f)
    {
        if (glowMaterial == null) return;

        Color color = glowColor;
        color.a = glowOpacity * intensity;
        glowMaterial.SetColor("_Color", color);
    }

    // Para os efeitos
    void StopEffects()
    {
        if (particles != null)
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    // CONTROLO PÚBLICO
    public void DisableGlow()
    {
        glowActive = false;
        if (glowObject != null)
            glowObject.SetActive(false);
        
        // Para as partículas se autoDisableWhenTriggered
        if (autoDisableWhenTriggered)
            StopEffects();
    }

    public void EnableGlow()
    {
        glowActive = true;
        if (glowObject != null)
            glowObject.SetActive(true);
    }

    public void TriggerGlowEnd()
    {
        if (autoDisableWhenTriggered)
            DisableGlow();
    }
}