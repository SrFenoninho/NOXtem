using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Crosshair RawImages")]
    public RawImage crosshairRawImage;
    public Texture normalCrosshair;     // mira padrão
    public Texture interactCrosshair;   // mira ao apontar para objeto interagível

    [Header("Crosshair Rotation Settings")]
    public float rotationSpeed = 5f;    // velocidade de rotação suave

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private float targetRotation = 0f;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Update()
    {
        if (crosshairRawImage == null) return;

        // Interpolar suavemente para o ângulo alvo
        float currentRotation = crosshairRawImage.rectTransform.eulerAngles.z;
        float newRotation = Mathf.LerpAngle(currentRotation, targetRotation, Time.deltaTime * rotationSpeed);
        crosshairRawImage.rectTransform.rotation = Quaternion.Euler(0f, 0f, newRotation);
    }

    // ---------------------------------------------
    //  ESTADOS DA MIRA
    // ---------------------------------------------
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
        targetRotation = 45f; // rodar 45° ao apontar para objeto interagível
    }
}
