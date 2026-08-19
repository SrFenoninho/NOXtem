using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// NoShadows - Desativa a projeção e/ou receção de sombras em qualquer objeto (e filhos).
/// Adiciona este script a qualquer objeto no Inspector para remover as suas sombras.
/// </summary>
public class NoShadows : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("Shadow Settings")]
    [Tooltip("Imede o objeto de projetar sombras no chão/paredes")]
    public bool disableCastShadows = true;

    [Tooltip("Impede o objeto de receber sombras de outros objetos")]
    public bool disableReceiveShadows = false;

    [Tooltip("Aplicar também a todos os objetos filhos em hierarquia")]
    public bool applyToChildren = true;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    private void Awake()
    {
        ApplyShadowSettings();
    }

    private void Start()
    {
        ApplyShadowSettings();
    }

    private void OnEnable()
    {
        ApplyShadowSettings();
    }

    private void OnValidate()
    {
        ApplyShadowSettings();
    }

    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void ApplyShadowSettings()
    {
        if (applyToChildren)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers)
            {
                UpdateRenderer(r);
            }
        }
        else
        {
            Renderer r = GetComponent<Renderer>();
            if (r != null) UpdateRenderer(r);
        }
    }

    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    private void UpdateRenderer(Renderer r)
    {
        if (r == null) return;

        if (disableCastShadows)
        {
            r.shadowCastingMode = ShadowCastingMode.Off;
        }

        if (disableReceiveShadows)
        {
            r.receiveShadows = false;
        }
    }
}
