using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class PSXPixelateEffect : MonoBehaviour
{
    [Header("Resolução Retro PSX")]
    [Tooltip("Escala da resolução do ecrã (0.2 = 240p pixéis gigantes, 0.4 = 480p, 1.0 = HD Normal)")]
    [Range(0.1f, 1f)]
    public float pixelScale = 0.3f;

    [Header("Ativação")]
    public bool enablePixelation = true;

    void OnEnable()
    {
        ApplyPixelation();
    }

    void OnDisable()
    {
        ResetPixelation();
    }

    void Update()
    {
        ApplyPixelation();
    }

    void ApplyPixelation()
    {
        var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset != null)
        {
            if (enablePixelation)
            {
                urpAsset.renderScale = Mathf.Clamp(pixelScale, 0.1f, 1f);
                urpAsset.upscalingFilter = UpscalingFilterSelection.Point;
            }
            else
            {
                urpAsset.renderScale = 1f;
            }
        }
    }

    void ResetPixelation()
    {
        var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset != null)
        {
            urpAsset.renderScale = 1f;
        }
    }
}
