using UnityEngine;

public class StormLiquidTexture : MonoBehaviour
{




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("Movimento Principal")]

    public Vector2 scrollSpeed = new Vector2(0f, 0.5f); 
    [Header("Efeito Espiral")]
    public float rotationSpeed = 30f;




    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private MeshFilter mf;
    private Vector2[] originalUVs;
    private Vector2 currentScroll = Vector2.zero;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        mf = GetComponent<MeshFilter>();
        if (mf != null)
        {
            originalUVs = mf.mesh.uv;
        }
    }

    void Update()
    {
        if (mf == null || originalUVs == null) return;

        currentScroll += scrollSpeed * Time.deltaTime;
        currentScroll.x %= 1f;
        currentScroll.y %= 1f;

        float currentAngle = Time.time * rotationSpeed;
        float rad = currentAngle * Mathf.Deg2Rad;
        float s = Mathf.Sin(rad);
        float c = Mathf.Cos(rad);

        Vector2[] newUVs = new Vector2[originalUVs.Length];

        for (int i = 0; i < newUVs.Length; i++)
        {
            Vector2 uv = originalUVs[i] - new Vector2(0.5f, 0.5f);

            float rx = uv.x * c - uv.y * s;
            float ry = uv.x * s + uv.y * c;

            newUVs[i] = new Vector2(rx + 0.5f + currentScroll.x, ry + 0.5f + currentScroll.y);
        }

        mf.mesh.uv = newUVs;
    }
}
