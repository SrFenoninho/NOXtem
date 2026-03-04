using UnityEngine;
using UnityEngine.UI;

public class Lighter : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Input")]
    public KeyCode toggleKey = KeyCode.F;

    [Header("Viewmodel")]
    [Tooltip("GameObject filho com o modelo 3D do isqueiro — posicionado na câmara")]
    public GameObject lighterModel;
    [Tooltip("Posição local do modelo quando guardado")]
    public Vector3 hiddenPosition = new Vector3(0.4f, -0.5f, 0.6f);
    [Tooltip("Posição local do modelo quando visível")]
    public Vector3 visiblePosition = new Vector3(0.25f, -0.25f, 0.5f);
    public float drawSpeed = 8f;  // velocidade de tirar/guardar

    [Header("Luz")]
    [Tooltip("Point Light filho do lighterModel — arrastar aqui")]
    public Light lighterLight;
    public float lightRange = 5f;
    public float lightIntensity = 1.8f;
    public Color lightColor = new Color(1f, 0.85f, 0.55f); // laranja quente

    [Header("Cintilação")]
    public bool useFlicker = true;
    public float flickerSpeed = 8f;
    public float flickerAmount = 0.25f;   // variação máxima de intensidade

    [Header("Audio")]
    public AudioClip igniteSound;   // som ao acender
    public AudioClip extinguishSound;
    private AudioSource audioSource;

    [Header("Vinheta UI")]
    [Tooltip("Imagem de vinheta no Canvas — deve ser uma textura radial escura nas bordas")]
    public Image vignetteImage;
    [Tooltip("Alpha da vinheta quando as luzes estão desligadas e o isqueiro está apagado")]
    public float darkVignetteAlpha = 0.95f;
    [Tooltip("Alpha da vinheta com o isqueiro aceso")]
    public float litVignetteAlpha = 0.55f;
    [Tooltip("Alpha da vinheta com energia restaurada")]
    public float poweredVignetteAlpha = 0f;
    public float vignetteSpeed = 3f;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private bool isLit = false;
    private bool isVisible = false;
    private float baseIntensity;
    private float targetVignetteAlpha;
    private Vector3 targetModelPos;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Configurar a luz
        if (lighterLight != null)
        {
            lighterLight.type = LightType.Point;
            lighterLight.range = lightRange;
            lighterLight.intensity = lightIntensity;
            lighterLight.color = lightColor;
            lighterLight.enabled = false;
            baseIntensity = lightIntensity;
        }

        // Esconder o modelo no início
        if (lighterModel != null)
        {
            lighterModel.SetActive(true);
            lighterModel.transform.localPosition = hiddenPosition;
        }

        targetModelPos = hiddenPosition;

        // Vinheta começa cheia (escuridão total)
        targetVignetteAlpha = darkVignetteAlpha;
        SetVignetteAlpha(darkVignetteAlpha);
    }

    void Update()
    {
        HandleInput();
        HandleModelAnimation();
        HandleFlicker();
        HandleVignette();
    }

    // ---------------------------------------------
    //  INPUT
    // ---------------------------------------------
    void HandleInput()
    {
        if (Input.GetKeyDown(toggleKey))
            Toggle();
    }

    void Toggle()
    {
        // Se a energia já foi restaurada, não precisa do isqueiro
        // (mas ainda pode usá-lo — escolha de design, podes remover este if se quiseres)
        isLit = !isLit;
        isVisible = isLit;

        targetModelPos = isVisible ? visiblePosition : hiddenPosition;

        if (lighterLight != null)
            lighterLight.enabled = isLit;

        // Áudio
        if (isLit && igniteSound != null)
            audioSource.PlayOneShot(igniteSound);
        else if (!isLit && extinguishSound != null)
            audioSource.PlayOneShot(extinguishSound);

        // Atualizar vinheta
        UpdateVignetteTarget();

        Debug.Log(isLit ? "Isqueiro aceso" : "Isqueiro apagado");
    }

    // ---------------------------------------------
    //  ANIMAÇÃO DO MODELO
    // ---------------------------------------------
    void HandleModelAnimation()
    {
        if (lighterModel == null) return;

        lighterModel.transform.localPosition = Vector3.Lerp(
            lighterModel.transform.localPosition,
            targetModelPos,
            Time.deltaTime * drawSpeed);
    }

    // ---------------------------------------------
    //  CINTILAÇÃO DA CHAMA
    // ---------------------------------------------
    void HandleFlicker()
    {
        if (!useFlicker || lighterLight == null || !isLit) return;

        // Ruído Perlin para cintilação orgânica
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        lighterLight.intensity = baseIntensity + (noise - 0.5f) * flickerAmount * 2f;
    }

    // ---------------------------------------------
    //  VINHETA
    // ---------------------------------------------
    void HandleVignette()
    {
        if (vignetteImage == null) return;

        // Se a energia foi restaurada, a vinheta desaparece
        if (DarknessManager.Instance != null && !DarknessManager.Instance.IsDark())
            targetVignetteAlpha = poweredVignetteAlpha;

        Color c = vignetteImage.color;
        c.a = Mathf.Lerp(c.a, targetVignetteAlpha, Time.deltaTime * vignetteSpeed);
        vignetteImage.color = c;
    }

    void UpdateVignetteTarget()
    {
        // Não alterar se a energia já foi restaurada
        if (DarknessManager.Instance != null && !DarknessManager.Instance.IsDark()) return;

        targetVignetteAlpha = isLit ? litVignetteAlpha : darkVignetteAlpha;
    }

    void SetVignetteAlpha(float alpha)
    {
        if (vignetteImage == null) return;
        Color c = vignetteImage.color;
        c.a = alpha;
        vignetteImage.color = c;
    }

    // ---------------------------------------------
    //  ACESSO PÚBLICO
    // ---------------------------------------------
    public bool IsLit() => isLit;
}