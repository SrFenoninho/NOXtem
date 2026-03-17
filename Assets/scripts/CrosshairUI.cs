using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Crosshair RawImages")]
    public RawImage crosshairRawImage;
    public Texture normalCrosshair;
    public Texture interactCrosshair;

    // ---------------------------------------------
    //  ESTADOS DA MIRA
    // ---------------------------------------------
    public void SetNormal()
    {
        if (crosshairRawImage == null) return;
        if (normalCrosshair != null)
            crosshairRawImage.texture = normalCrosshair;
    }

    public void SetInteract()
    {
        if (crosshairRawImage == null) return;
        if (interactCrosshair != null)
            crosshairRawImage.texture = interactCrosshair;
    }
}