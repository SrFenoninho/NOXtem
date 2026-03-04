using UnityEngine;

public class TextureRandomRotation : MonoBehaviour
{
    // ---------------------------------------------
    //  ROTAÇÃO E DESLOCAMENTO ALEATÓRIO DE TEXTURA
    // ---------------------------------------------
    // Aplicado no Start para variar visualmente tiles repetidos no mapa
    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null) return;

        Material mat = renderer.material;

        // Escolher aleatoriamente uma de 4 rotações (0°, 90°, 180°, 270°)
        int rotations = Random.Range(0, 4);

        if (rotations == 1)         // 90°
        {
            mat.SetTextureScale("_MainTex", new Vector2(1, 1));
            mat.SetTextureOffset("_MainTex", new Vector2(0, 0));
        }
        else if (rotations == 2)    // 180°
        {
            mat.SetTextureScale("_MainTex", new Vector2(-1, -1));
            mat.SetTextureOffset("_MainTex", new Vector2(1, 1));
        }
        else if (rotations == 3)    // 270°
        {
            mat.SetTextureScale("_MainTex", new Vector2(-1, 1));
            mat.SetTextureOffset("_MainTex", new Vector2(1, 0));
        }

        // Aplicar escala aleatória para mais variação visual
        float scale = Random.Range(0.8f, 1.5f);
        Vector2 currentScale = mat.GetTextureScale("_MainTex");
        mat.SetTextureScale("_MainTex", currentScale * scale);

        // Aplicar deslocamento aleatório para evitar alinhamento óbvio
        float offsetX = Random.Range(-0.5f, 0.5f);
        float offsetY = Random.Range(-0.5f, 0.5f);
        Vector2 currentOffset = mat.GetTextureOffset("_MainTex");
        mat.SetTextureOffset("_MainTex", currentOffset + new Vector2(offsetX, offsetY));
    }
}
