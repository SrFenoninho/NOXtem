using UnityEngine;
using System.Collections;

public class BossVFX : MonoBehaviour
{
    [Header("Particle System de Efeitos em Área")]
    public Color aoeParticleColor = Color.gray;
    public Texture aoeParticleTexture;
    [Range(0.5f, 5f)]
    public float aoeParticleSize = 2.0f;
    private ParticleSystem aoeParticles;
    private float jumpAttackRadius = 6f; 

    private BossController boss;

    public void Initialize(BossController controller)
    {
        boss = controller;
        BossCombat combat = GetComponent<BossCombat>();
        if(combat != null) jumpAttackRadius = combat.jumpAttackRadius;
        
        CreateAoeParticleSystem();
        if (aoeParticles != null) 
        {
            aoeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void PlayAoeParticles()
    {
        if (aoeParticles != null) aoeParticles.Play();
    }

    public void TriggerCameraShake(float duration, float magnitude)
    {
        StartCoroutine(ShakeCamera(duration, magnitude));
    }

    private IEnumerator ShakeCamera(float duration, float magnitude)
    {
        Camera mainCam = Camera.main;
        if(mainCam == null) yield break;
        
        Vector3 originalPos = mainCam.transform.localPosition;
        float elapsed = 0.0f;
        
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            
            mainCam.transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        mainCam.transform.localPosition = originalPos;
    }

    private void CreateAoeParticleSystem()
    {
        GameObject particleObj = new GameObject("AoeParticles");
        particleObj.transform.SetParent(transform);
        particleObj.transform.localPosition = new Vector3(0, 0.5f, 0); 

        aoeParticles = particleObj.AddComponent<ParticleSystem>();

        var main = aoeParticles.main;
        main.startColor = aoeParticleColor;
        main.startSize = aoeParticleSize;
        main.startSpeed = 15f; 
        main.startLifetime = 1.5f;
        main.maxParticles = 80;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.loop = false;

        var emission = aoeParticles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 40, 80, 1, 0.01f) });

        var shape = aoeParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = jumpAttackRadius * 0.8f; 

        var sizeOverLifetime = aoeParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.01f);
        sizeCurve.AddKey(0.15f, 1f);
        sizeCurve.AddKey(0.85f, 1f);
        sizeCurve.AddKey(1f, 0.01f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var colorOverLifetime = aoeParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(aoeParticleColor, 0f),
                new GradientColorKey(aoeParticleColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.8f),
                new GradientAlphaKey(1f, 0.8f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        var renderer = aoeParticles.GetComponent<ParticleSystemRenderer>();
        Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (particleShader == null) particleShader = Shader.Find("Legacy Shaders/Particles/Alpha Blended"); // Fallback Clássico com Alpha

        renderer.material = new Material(particleShader);
        if (aoeParticleTexture != null)
        {
            if (renderer.material.HasProperty("_BaseMap")) renderer.material.SetTexture("_BaseMap", aoeParticleTexture);
            else renderer.material.mainTexture = aoeParticleTexture;
        }
        
        if (renderer.material.HasProperty("_BaseColor"))
            renderer.material.SetColor("_BaseColor", aoeParticleColor);
        else if (renderer.material.HasProperty("_TintColor"))
            renderer.material.SetColor("_TintColor", aoeParticleColor);
        else if (renderer.material.HasProperty("_Color"))
            renderer.material.color = aoeParticleColor;

        renderer.renderMode = ParticleSystemRenderMode.Billboard;
    }
}
