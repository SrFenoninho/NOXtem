using UnityEngine;

public class TextureRandomRotation : MonoBehaviour
{
    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null) return;

        Material mat = renderer.material;

        int rotations = Random.Range(0, 3);
        float angle = rotations * 90f;

        float rad = angle * Mathf.Deg2Rad;

        Vector2 center = new Vector2(0.5f, 0.5f);

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
    }
}