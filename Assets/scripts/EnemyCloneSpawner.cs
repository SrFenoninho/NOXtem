using UnityEngine;
using System.Collections;

public class EnemyCloneSpawner : MonoBehaviour
{
    // ---------------------------------------------
    //  DADOS PASSADOS PELO ENEMYAI AO MORRER
    // ---------------------------------------------
    [HideInInspector] public GameObject enemyPrefab;
    [HideInInspector] public GameObject cloneSpawnerPrefab;
    [HideInInspector] public int generation;
    [HideInInspector] public int maxGeneration;
    [HideInInspector] public System.Action OnCloneDeath;

    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Spawn Settings")]
    public float spawnDelay = 1.5f;     // tempo de espera antes de criar o clone

    [Header("Particle Circle")]
    public float circleRadius = 1.2f;
    public Color particleColor = new Color(0.5f, 0f, 1f, 1f);
    public float particleSize = 0.2f;
    [Range(5, 50)]
    public int emissionRate = 30;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private ParticleSystem particles;

    // ---------------------------------------------
    //  INICIALIZAcaO
    // ---------------------------------------------
    // Chamado pelo EnemyAI ao morrer - configura e inicia o processo de spawn
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

    // ---------------------------------------------
    //  EFEITO DE PARTiCULAS
    // ---------------------------------------------
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

        // Circulo horizontal no chao
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = circleRadius;
        shape.radiusThickness = 0.05f;
        shape.rotation = new Vector3(90f, 0f, 0f);

        // Gradiente de cor ao longo da vida da particula
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

        // Diminuir ao longo do tempo
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

    // ---------------------------------------------
    //  SPAWN DO CLONE
    // ---------------------------------------------
    IEnumerator SpawnCloneAfterDelay()
    {
        yield return new WaitForSeconds(spawnDelay);

        if (particles != null)
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (enemyPrefab == null) yield break;

        // Criar clone na posicao deste objeto (ja calculada pelo EnemyAI antes de morrer)
        GameObject clone = Instantiate(enemyPrefab, transform.position, Quaternion.identity);

        EnemyAI ai = clone.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.isOriginal = false;
            ai.generation = generation;
            ai.maxGeneration = maxGeneration;
            ai.enemyPrefab = enemyPrefab;
            ai.cloneSpawnerPrefab = cloneSpawnerPrefab; // passar adiante para geracoes futuras
            ai.OnDeath += () => OnCloneDeath?.Invoke();
        }

        Destroy(gameObject);
    }
}
