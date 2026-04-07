using UnityEngine;
using System.Collections.Generic;

public class TextureRandomRotation : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Offset Aleatorio")]
    public float offsetRange = 0.3f;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null) return;

        Mesh mesh = Instantiate(mf.mesh);
        mf.mesh = mesh;

        int[] tris = mesh.triangles;
        Vector2[] uvs = mesh.uv;

        if (uvs.Length != mesh.vertexCount)
            uvs = new Vector2[mesh.vertexCount];

        // Agrupar triangulos em quads (pares que partilham uma aresta)
        // Cada face do cubo = 2 triangulos consecutivos no array
        // Blockbench exporta sempre em pares: tri0+tri1 = face, tri2+tri3 = face...
        for (int i = 0; i < tris.Length; i += 6)
        {
            if (i + 5 >= tris.Length) break;

            // Os 4 vertices unicos do quad
            var verts = new HashSet<int>();
            for (int t = 0; t < 6; t++) verts.Add(tris[i + t]);
            var vertList = new List<int>(verts);

            // Rotacao aleatoria - mesma para os 2 triangulos da face
            float angle = Random.Range(0, 4) * 90f * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            float ox = Random.Range(-offsetRange, offsetRange);
            float oy = Random.Range(-offsetRange, offsetRange);

            // Centro dos UVs do quad
            Vector2 center = Vector2.zero;
            foreach (int v in vertList) center += uvs[v];
            center /= vertList.Count;

            // Aplicar a todos os vertices do quad
            foreach (int v in vertList)
                uvs[v] = RotateUV(uvs[v], center, cos, sin, ox, oy);
        }

        mesh.uv = uvs;
        mesh.UploadMeshData(false);
    }

    // ---------------------------------------------
    //  AUXILIAR
    // ---------------------------------------------
    Vector2 RotateUV(Vector2 uv, Vector2 center, float cos, float sin, float ox, float oy)
    {
        float u = uv.x - center.x;
        float v = uv.y - center.y;
        return new Vector2(cos * u - sin * v + center.x + ox,
                           sin * u + cos * v + center.y + oy);
    }
}