using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    // A Instância global para ser chamada em qualquer lado
    public static LoadingManager Instancia;

    [Header("Estilo do Ecrã (Personaliza Aqui)")]
    [Tooltip("A textura que vai ficar por trás do ecrã de loading (tal como no Main Menu)")]
    public Texture texturaDoFundo;
    
    [Tooltip("A fonte TextMeshPro que queres usar para as letras")]
    public TMP_FontAsset fonteDoTexto;
    
    [Tooltip("A cor da barra de progresso (a parte que vai enchendo)")]
    public Color corDaBarra = Color.red;
    
    [Tooltip("Cor do fundo da barra (a parte vazia)")]
    public Color corFundoDaBarra = Color.black;

    private bool estaACarregar = false; // ANTI-BUG: Garante que não carregas 2 vezes seguidas

    void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ---------------------------------------------------------
    // ATALHOS GLOBAIS SEGURANÇA (EVITA CRASHES DURANTE TESTES)
    // ---------------------------------------------------------
    public static void Carregar(string nomeCena)
    {
        if (Instancia != null) Instancia.CarregarCena(nomeCena);
        else SceneManager.LoadScene(nomeCena); // Fallback para se estiveres a testar uma cena solta!
    }

    public static void Carregar(int indexCena)
    {
        if (Instancia != null) Instancia.CarregarCena(indexCena);
        else SceneManager.LoadScene(indexCena);
    }

    // Função para carregar pelo NOME da cena (Interna)
    public void CarregarCena(string nomeDaCena)
    {
        if (estaACarregar) return; 
        StartCoroutine(RotinaDeLoading(nomeDaCena, -1));
    }

    // Função para carregar pelo NÚMERO (Build Index) da cena (Interna)
    public void CarregarCena(int indexDaCena)
    {
        if (estaACarregar) return; 
        StartCoroutine(RotinaDeLoading(null, indexDaCena));
    }

    private IEnumerator RotinaDeLoading(string nomeDaCena, int indexDaCena)
    {
        estaACarregar = true;

        // ---------------------------------------------------------
        // 1. CRIAR O CANVAS E A UI INTEIRAMENTE POR CÓDIGO
        // ---------------------------------------------------------
        
        // Objeto Canvas Principal
        GameObject canvasObj = new GameObject("Canvas_De_Loading_Automatico");
        DontDestroyOnLoad(canvasObj); 
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; 

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // ANTI-BUG: Este Raycaster absorve cliques no fundo, para ninguém poder interagir com botões ocultos!
        canvasObj.AddComponent<GraphicRaycaster>();

        // Fundo (RawImage)
        GameObject fundoObj = new GameObject("Fundo");
        fundoObj.transform.SetParent(canvasObj.transform, false);
        RawImage imagemFundoUI = fundoObj.AddComponent<RawImage>();
        imagemFundoUI.raycastTarget = true; // Bloqueia cliques do rato

        imagemFundoUI.rectTransform.anchorMin = Vector2.zero;
        imagemFundoUI.rectTransform.anchorMax = Vector2.one;
        imagemFundoUI.rectTransform.offsetMin = Vector2.zero;
        imagemFundoUI.rectTransform.offsetMax = Vector2.zero;
        
        if (texturaDoFundo != null)
            imagemFundoUI.texture = texturaDoFundo;
        else
            imagemFundoUI.color = Color.black; 

        // Texto "A Carregar..."
        GameObject textoObj = new GameObject("TextoLoading");
        textoObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI textoUI = textoObj.AddComponent<TextMeshProUGUI>();
        textoUI.text = "LOADING...";
        textoUI.alignment = TextAlignmentOptions.Center;
        textoUI.fontSize = 70;
        textoUI.color = Color.white;
        if (fonteDoTexto != null) textoUI.font = fonteDoTexto;
        
        textoUI.rectTransform.anchorMin = new Vector2(0.5f, 0f);
        textoUI.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        textoUI.rectTransform.anchoredPosition = new Vector2(0, 200); 
        textoUI.rectTransform.sizeDelta = new Vector2(600, 100);

        // Fundo da Barra
        GameObject fundoBarraObj = new GameObject("FundoDaBarra");
        fundoBarraObj.transform.SetParent(canvasObj.transform, false);
        Image fundoBarraUI = fundoBarraObj.AddComponent<Image>();
        fundoBarraUI.color = corFundoDaBarra;
        fundoBarraUI.rectTransform.anchorMin = new Vector2(0.5f, 0f);
        fundoBarraUI.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        fundoBarraUI.rectTransform.anchoredPosition = new Vector2(0, 100);
        fundoBarraUI.rectTransform.sizeDelta = new Vector2(800, 30); 

        // Preenchimento da Barra (Progresso)
        GameObject barraCheiaObj = new GameObject("BarraDeProgresso");
        barraCheiaObj.transform.SetParent(fundoBarraObj.transform, false); 
        Image barraProgressoUI = barraCheiaObj.AddComponent<Image>();
        barraProgressoUI.color = corDaBarra;
        
        barraProgressoUI.type = Image.Type.Filled;
        barraProgressoUI.fillMethod = Image.FillMethod.Horizontal;
        barraProgressoUI.fillOrigin = (int)Image.OriginHorizontal.Left;
        barraProgressoUI.fillAmount = 0f;
        
        barraProgressoUI.rectTransform.anchorMin = Vector2.zero;
        barraProgressoUI.rectTransform.anchorMax = Vector2.one;
        barraProgressoUI.rectTransform.offsetMin = Vector2.zero;
        barraProgressoUI.rectTransform.offsetMax = Vector2.zero;

        // ---------------------------------------------------------
        // 2. INICIAR O CARREGAMENTO ASÍNCRONO
        // ---------------------------------------------------------
        
        yield return null; 

        AsyncOperation operacao = null;
        
        // ANTI-BUG: Impede erros caso te enganes no nome de uma cena!
        try
        {
            if (!string.IsNullOrEmpty(nomeDaCena))
            {
                operacao = SceneManager.LoadSceneAsync(nomeDaCena);
            }
            else
            {
                operacao = SceneManager.LoadSceneAsync(indexDaCena);
            }
        }
        catch (System.Exception e)
        {
            // Debug.LogError("Erro de Loading: " + e.Message);
        }

        // Se a cena não existe no Unity (engano no nome), saímos para não congelar o jogo para sempre
        if (operacao == null)
        {
            // Debug.LogError("A CENA NÃO FOI ENCONTRADA. VAI AO 'BUILD SETTINGS' VER SE O NOME ESTÁ CERTO!");
            textoUI.text = "ERROR: SCENE NOT FOUND!";
            textoUI.color = Color.red;
            yield return new WaitForSeconds(3f);
            Destroy(canvasObj);
            estaACarregar = false;
            yield break;
        }
        
        operacao.allowSceneActivation = false; 

        while (!operacao.isDone)
        {
            float progressoAtual = Mathf.Clamp01(operacao.progress / 0.9f);
            barraProgressoUI.fillAmount = progressoAtual;

            if (operacao.progress >= 0.9f)
            {
                operacao.allowSceneActivation = true;
            }

            yield return null;
        }

        Destroy(canvasObj);
        estaACarregar = false;
    }
}
