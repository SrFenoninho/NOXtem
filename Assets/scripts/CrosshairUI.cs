using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour
{
    [Header("Crosshair RawImages")]
    public RawImage crosshairRawImage;
    public Texture normalCrosshair;
    public Texture interactCrosshair; 

    [Header("Crosshair Rotation Settings")]
    public float rotationSpeed = 5f;
    private float targetRotation = 0f;

    private void Update()
    {
        if (crosshairRawImage == null) return;
        float currentRotation = crosshairRawImage.rectTransform.eulerAngles.z;
        float newRotation = Mathf.LerpAngle(currentRotation, targetRotation, Time.deltaTime * rotationSpeed);
        crosshairRawImage.rectTransform.rotation = Quaternion.Euler(0f, 0f, newRotation); 
    }
    public void SetNormal()
    {
        if (crosshairRawImage == null) return;

        if (normalCrosshair != null)
            crosshairRawImage.texture = normalCrosshair;
        targetRotation = 0f;
    }
    public void SetInteract()
    {
        if (crosshairRawImage == null) return;

        if (interactCrosshair != null)
            crosshairRawImage.texture = interactCrosshair;
        targetRotation = 45f;
    }
}