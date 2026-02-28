using UnityEngine;
using System.Collections;

public class EnemyCloneSpawner : MonoBehaviour
{
    [HideInInspector] public GameObject enemyPrefab;
    [HideInInspector] public GameObject cloneSpawnerPrefab;
    [HideInInspector] public int generation;
    [HideInInspector] public int maxGeneration;
    [HideInInspector] public System.Action OnCloneDeath;

    [Header("Spawn Settings")]
    public float spawnDelay = 1.5f;

    [Header("Particle Circle")]
    public float circleRadius = 1.2f;
    public Color particleColor = new Color(0.5f, 0f, 1f, 1f);
    public float particleSize = 0.2f;
    [Range(5, 50)]
    public int emissionRate = 30;

    private ParticleSystem particles;

    public void Initialize(GameObject prefab, GameObject spawnerPrefab, int gen, int maxGen, System.Action onCloneDeath)
    {
        enemyPrefab = prefab;
        cloneSpawnerPrefab = spawnerPrefab;
        generation = gen;
        maxGeneration = maxGen;
        OnCloneDeath = onCloneDeath;

        CreateCircleEffect();
        StartCoroutine(SpawnCloneAfterDelay());
    }

    void CreateCircleEffect()
    {
        GameObject particleObj = new GameObject("CloneCircle");
        particleObj.transform.SetParent(transform);
        particleObj.transform.localPosition = Vector3.zero;

        particles = particleObj.AddComponent<ParticleSystem>();

        var main = particles.main;
        main.startColor = particleColor;
        main.startSize = particleSize;
        main.startSpeed = 0.3f;
        main.startLifetime = 1f;
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = true;

        var emission = particles.emission;
        emission.rateOverTime = emissionRate;

        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = circleRadius;
        shape.radiusThickness = 0.05f;
        shape.rotation = new Vector3(90f, 0f, 0f);

        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.6f, 0f, 1f), 0f),
                new GradientColorKey(new Color(0.9f, 0.3f, 1f), 0.5f),
                new GradientColorKey(new Color(0.6f, 0f, 1f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.9f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        var sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.SetFloat("_Mode", 2);
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        particles.Play();
    }

    IEnumerator SpawnCloneAfterDelay()
    {
        yield return new WaitForSeconds(spawnDelay);

        if (particles != null)
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (enemyPrefab == null) yield break;

        // Spawn clone at this object's position (already set to the correct spot by EnemyAI)
        GameObject clone = Instantiate(enemyPrefab, transform.position, Quaternion.identity);

        EnemyAI ai = clone.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.isOriginal = false;
            ai.generation = generation;
            ai.maxGeneration = maxGeneration;
            ai.enemyPrefab = enemyPrefab;
            ai.cloneSpawnerPrefab = cloneSpawnerPrefab; // pass it forward for future generations
            ai.OnDeath += () => OnCloneDeath?.Invoke();
        }

        Destroy(gameObject);
    }
}