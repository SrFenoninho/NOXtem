using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class LoadingManager : MonoBehaviour
{




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    public static LoadingManager Instancia;

    [Header("Estilo do Ecrã (Personaliza Aqui)")]



    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    [Tooltip("A textura que vai ficar por trás do ecrã de loading (tal como no Main Menu)")]
    public Texture texturaDoFundo;

    [Tooltip("A fonte TextMeshPro que queres usar para as letras")]
    public TMP_FontAsset fonteDoTexto;

    [Tooltip("A cor da barra de progresso (a parte que vai enchendo)")]
    public Color corDaBarra = Color.red;

    [Tooltip("Cor do fundo da barra (a parte vazia)")]
    public Color corFundoDaBarra = Color.black;


    private bool estaACarregar = false;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
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





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public static void Carregar(string nomeCena)
    {
        if (Instancia != null) Instancia.CarregarCena(nomeCena);
        else SceneManager.LoadScene(nomeCena);
    }

    public static void Carregar(int indexCena)
    {
        if (Instancia != null) Instancia.CarregarCena(indexCena);
        else SceneManager.LoadScene(indexCena);
    }

    public void CarregarCena(string nomeDaCena)
    {
        if (estaACarregar) return; 
        StartCoroutine(RotinaDeLoading(nomeDaCena, -1));
    }

    public void CarregarCena(int indexDaCena)
    {
        if (estaACarregar) return; 
        StartCoroutine(RotinaDeLoading(null, indexDaCena));
    }





    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    private IEnumerator RotinaDeLoading(string nomeDaCena, int indexDaCena)
    {
        estaACarregar = true;

        GameObject canvasObj = new GameObject("Canvas_De_Loading_Automatico");
        DontDestroyOnLoad(canvasObj); 

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; 

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject fundoObj = new GameObject("Fundo");
        fundoObj.transform.SetParent(canvasObj.transform, false);
        RawImage imagemFundoUI = fundoObj.AddComponent<RawImage>();
        imagemFundoUI.raycastTarget = true;

        imagemFundoUI.rectTransform.anchorMin = Vector2.zero;
        imagemFundoUI.rectTransform.anchorMax = Vector2.one;
        imagemFundoUI.rectTransform.offsetMin = Vector2.zero;
        imagemFundoUI.rectTransform.offsetMax = Vector2.zero;

        if (texturaDoFundo != null)
            imagemFundoUI.texture = texturaDoFundo;
        else
            imagemFundoUI.color = Color.black; 

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

        GameObject fundoBarraObj = new GameObject("FundoDaBarra");
        fundoBarraObj.transform.SetParent(canvasObj.transform, false);
        Image fundoBarraUI = fundoBarraObj.AddComponent<Image>();
        fundoBarraUI.color = corFundoDaBarra;
        fundoBarraUI.rectTransform.anchorMin = new Vector2(0.5f, 0f);
        fundoBarraUI.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        fundoBarraUI.rectTransform.anchoredPosition = new Vector2(0, 100);
        fundoBarraUI.rectTransform.sizeDelta = new Vector2(800, 30); 

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

        yield return null; 

        AsyncOperation operacao = null;

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
        }

        if (operacao == null)
        {
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
