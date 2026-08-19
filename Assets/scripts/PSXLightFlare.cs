using UnityEngine;

[RequireComponent(typeof(Light))]
public class PSXLightFlare : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("Flare Settings")]
    public float flareSize = 1.2f;
    public float maxDistance = 25f;
    public LayerMask obstacleLayers = ~0; // Camadas que bloqueiam a luz (paredes, portas, etc.)
    public Texture2D customFlareTexture; // Opcional: Se quiseres arrastar uma imagem própria

    [Header("Fade & Occlusion")]
    public float fadeSpeed = 8f;
    public bool enableOcclusion = true;

    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private Light targetLight;
    private GameObject flareQuad;
    private MeshRenderer flareRenderer;
    private MaterialPropertyBlock propBlock;
    private static Texture2D defaultProceduralTexture;
    private static readonly int MainTexProp = Shader.PropertyToID("_MainTex");
    private static readonly int ColorProp   = Shader.PropertyToID("_Color");

    private float currentAlpha = 0f;
    private float targetAlpha  = 1f;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    private void Awake()
    {
        targetLight = GetComponent<Light>();
        propBlock   = new MaterialPropertyBlock();
        CreateFlareQuad();
    }

    private void OnEnable()
    {
        if (flareQuad != null) flareQuad.SetActive(true);
    }

    private void OnDisable()
    {
        if (flareQuad != null) flareQuad.SetActive(false);
    }

    private void OnDestroy()
    {
        if (flareQuad != null) Destroy(flareQuad);
    }

    private void LateUpdate()
    {
        if (targetLight == null || Camera.main == null || flareQuad == null) return;

        // Se a luz estiver desligada ou com intensidade 0, esconder o brilho
        if (!targetLight.enabled || targetLight.intensity <= 0.01f)
        {
            targetAlpha = 0f;
            UpdateFlareVisuals();
            return;
        }

        Transform camT = Camera.main.transform;
        float dist = Vector3.Distance(transform.position, camT.position);

        // Se estiver demasiado longe da câmara, desativar
        if (dist > maxDistance)
        {
            targetAlpha = 0f;
            UpdateFlareVisuals();
            return;
        }

        // 1. Verificar se a câmara está a olhar para a luz (campo de visão)
        Vector3 dirToLight = (transform.position - camT.position).normalized;
        float lookDot = Vector3.Dot(camT.forward, dirToLight);

        if (lookDot < 0.4f) // Luz fora do ecrã ou atrás da câmara
        {
            targetAlpha = 0f;
            UpdateFlareVisuals();
            return;
        }

        float lookAlpha = Mathf.Clamp01((lookDot - 0.4f) / 0.6f);

        // 2. Orientação Billboard: Olhar sempre para a câmara
        flareQuad.transform.position = transform.position;
        flareQuad.transform.rotation = Quaternion.LookRotation(camT.forward);

        // 3. Escala baseada no tamanho e ligeiro crescimento com a distância
        float scaleFactor = flareSize * (1f + dist * 0.05f);
        flareQuad.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);

        // 4. Verificação de Oclusão (Raycast da luz para a câmara)
        if (enableOcclusion)
        {
            Vector3 dirToCam = (camT.position - transform.position).normalized;
            if (Physics.Raycast(transform.position, dirToCam, out RaycastHit hit, dist, obstacleLayers))
            {
                bool visible = (hit.transform == camT || hit.transform.root == camT.root);
                targetAlpha = visible ? lookAlpha : 0f;
            }
            else
            {
                targetAlpha = lookAlpha;
            }
        }
        else
        {
            targetAlpha = lookAlpha;
        }

        UpdateFlareVisuals();
    }

    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    private void UpdateFlareVisuals()
    {
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);

        if (currentAlpha <= 0.001f)
        {
            if (flareRenderer.enabled) flareRenderer.enabled = false;
            return;
        }

        if (!flareRenderer.enabled) flareRenderer.enabled = true;

        flareRenderer.GetPropertyBlock(propBlock);

        // A cor do brilho acompanha a cor e intensidade da luz
        Color col = targetLight.color;
        float intensityRatio = Mathf.Clamp01(targetLight.intensity / 3.0f);
        col.a = currentAlpha * intensityRatio;

        propBlock.SetColor(ColorProp, col);
        flareRenderer.SetPropertyBlock(propBlock);
    }

    private void CreateFlareQuad()
    {
        flareQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        flareQuad.name = "PSX_Flare_Billboard";
        flareQuad.transform.SetParent(transform, false);

        // Remover o collider do quad para não interferir com a física
        Collider c = flareQuad.GetComponent<Collider>();
        if (c != null) DestroyImmediate(c);

        flareRenderer = flareQuad.GetComponent<MeshRenderer>();

        // Usar Shader Additive para o brilho adicionar luz sem criar o círculo cinzento escuro
        Shader addShader = Shader.Find("Mobile/Particles/Additive") 
                       ?? Shader.Find("Particles/Additive") 
                       ?? Shader.Find("Legacy Shaders/Particles/Additive")
                       ?? Shader.Find("Sprites/Default");

        Material mat = new Material(addShader);
        flareRenderer.sharedMaterial = mat;

        // Se não tiver imagem atribuída, gerar a textura da cruz pixelizada PSX por código
        Texture2D tex = (customFlareTexture != null) ? customFlareTexture : GetProceduralPSXTexture();
        flareRenderer.GetPropertyBlock(propBlock);
        propBlock.SetTexture(MainTexProp, tex);
        flareRenderer.SetPropertyBlock(propBlock);
    }

    private Texture2D GetProceduralPSXTexture()
    {
        if (defaultProceduralTexture != null) return defaultProceduralTexture;

        // Criar textura pixelizada 64x64 com estrela/cruz retro e halo suave
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode   = TextureWrapMode.Clamp;

        Color[] pixels = new Color[size * size];
        float center = (size - 1) / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - center) / center;
                float dy = (y - center) / center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                // Halo radial suave no centro
                float halo = Mathf.Exp(-dist * 3.8f);

                // Raios horizontal e vertical em cruz retro
                float rayH = Mathf.Exp(-Mathf.Abs(dx) * 14f) * Mathf.Exp(-Mathf.Abs(dy) * 2.2f);
                float rayV = Mathf.Exp(-Mathf.Abs(dy) * 14f) * Mathf.Exp(-Mathf.Abs(dx) * 2.2f);

                // Brilho máximo dos raios + centro
                float val = Mathf.Max(halo, Mathf.Max(rayH, rayV));

                // Quantização retro em 8 níveis para o aspeto pixelizado PS1
                val = Mathf.Floor(val * 8f) / 8f;
                val = Mathf.Clamp01(val);

                // Em modo Additive, os píxeis pretos (0,0,0) são transparentes
                pixels[y * size + x] = new Color(val, val * 0.95f, val * 0.82f, val);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        defaultProceduralTexture = tex;
        return defaultProceduralTexture;
    }
}
