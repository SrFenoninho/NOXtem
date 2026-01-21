using UnityEngine;

public class TextureRandomRotation : MonoBehaviour
{
    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null) return;

        Material mat = renderer.material;

        int rotations = Random.Range(0, 4);

        if (rotations == 1) // 90°
        {
            mat.SetTextureScale("_MainTex", new Vector2(1, 1));
            mat.SetTextureOffset("_MainTex", new Vector2(0, 0));
        }
        else if (rotations == 2) // 180°
        {
            mat.SetTextureScale("_MainTex", new Vector2(-1, -1));
            mat.SetTextureOffset("_MainTex", new Vector2(1, 1));
        }
        else if (rotations == 3) // 270°
        {
            mat.SetTextureScale("_MainTex", new Vector2(-1, 1));
            mat.SetTextureOffset("_MainTex", new Vector2(1, 0));
        }

        float scale = Random.Range(0.8f, 1.5f); // Random scale between 0.8 and 1.5
        Vector2 currentScale = mat.GetTextureScale("_MainTex");
        mat.SetTextureScale("_MainTex", currentScale * scale);

        float offsetX = Random.Range(-0.5f, 0.5f); // Random offset between -0.5 and 0.5
        float offsetY = Random.Range(-0.5f, 0.5f);
        Vector2 currentOffset = mat.GetTextureOffset("_MainTex");
        mat.SetTextureOffset("_MainTex", currentOffset + new Vector2(offsetX, offsetY));
    }
}