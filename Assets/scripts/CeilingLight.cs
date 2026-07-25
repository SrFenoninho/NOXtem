using System.Collections;
using UnityEngine;

public class CeilingLight : MonoBehaviour
{
    [Header("Configuração da Lâmpada do Teto")]
    public bool startPowered = false;
    public float targetIntensity = 3.0f;
    public float targetRange = 10.0f;
    public Color lightColor = new Color(1.0f, 0.95f, 0.8f);

    [Header("Efeito ao Ligar a Energia")]
    public bool flickerOnPowerUp = true;
    public AudioClip powerUpFlickerSound;

    private Light lampLight;
    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock propBlock;
    private static readonly int EmissionColorProperty = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        lampLight = GetComponent<Light>() ?? GetComponentInChildren<Light>();
        meshRenderer = GetComponent<MeshRenderer>() ?? GetComponentInChildren<MeshRenderer>();
        propBlock = new MaterialPropertyBlock();
    }

    void Start()
    {
        if (lampLight != null)
        {
            lampLight.color = lightColor;
            lampLight.shadows = LightShadows.Soft;
            lampLight.shadowBias = 0.005f;
            lampLight.shadowNormalBias = 0.05f;
            lampLight.shadowNearPlane = 0.02f;
        }

        // Garantir que a câmara principal tem fundo preto em vez do céu azul por defeito da Unity
        if (Camera.main != null)
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = Color.black;
        }

        if (startPowered || (DarknessManager.Instance != null && !DarknessManager.Instance.IsDark()))
        {
            TurnOnImmediate();
        }
        else
        {
            TurnOffImmediate();
        }
    }

    public void TurnOn()
    {
        if (flickerOnPowerUp)
        {
            StartCoroutine(FlickerRoutine());
        }
        else
        {
            TurnOnImmediate();
        }
    }

    public void TurnOff()
    {
        TurnOffImmediate();
    }

    void TurnOnImmediate()
    {
        if (lampLight != null)
        {
            lampLight.enabled = true;
            lampLight.intensity = targetIntensity;
            lampLight.range = targetRange;
        }

        SetMaterialEmission(true);
    }

    void TurnOffImmediate()
    {
        if (lampLight != null)
        {
            lampLight.enabled = false;
            lampLight.intensity = 0f;
        }

        SetMaterialEmission(false);
    }

    IEnumerator FlickerRoutine()
    {
        if (lampLight == null) yield break;

        lampLight.enabled = true;
        AudioSource audio = GetComponent<AudioSource>();

        if (audio != null && powerUpFlickerSound != null)
        {
            audio.PlayOneShot(powerUpFlickerSound);
        }

        // Piscar 3 vezes estilo lâmpada fluorescente a acender (sem zerar completamente para não piscar azul)
        for (int i = 0; i < 3; i++)
        {
            lampLight.intensity = targetIntensity * 0.3f;
            SetMaterialEmission(true);
            yield return new WaitForSeconds(Random.Range(0.05f, 0.12f));

            lampLight.intensity = 0.05f;
            SetMaterialEmission(false);
            yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
        }

        TurnOnImmediate();
    }

    void SetMaterialEmission(bool active)
    {
        if (meshRenderer != null)
        {
            meshRenderer.GetPropertyBlock(propBlock);
            if (active)
            {
                propBlock.SetColor(EmissionColorProperty, lightColor * 2f);
            }
            else
            {
                propBlock.SetColor(EmissionColorProperty, Color.black);
            }
            meshRenderer.SetPropertyBlock(propBlock);
        }
    }
}
