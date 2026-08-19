using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CreditsScreen : MonoBehaviour
{



    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("Design (Opcional)")]



    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    [Tooltip("Uma imagem para ficar de fundo (arrasta do projeto)")]

    public Texture imagemDeFundo;

    [Tooltip("A fonte para o título João Rodrigues")]
    public TMP_FontAsset fonteTextoCima;

    [Tooltip("A fonte para os agradecimentos das fontes")]
    public TMP_FontAsset fonteTextoBaixo;

    [Tooltip("A fonte para o botão do Menu")]
    public TMP_FontAsset fonteBotao;

    [Header("Caminho de Volta")]
    public string cenaDeVolta = "MainMenu";





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
        }

        GameObject canvasObj = new GameObject("Credits_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 900;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject fundoObj = new GameObject("Background");
        fundoObj.transform.SetParent(canvasObj.transform, false);
        RawImage bg = fundoObj.AddComponent<RawImage>();
        bg.rectTransform.anchorMin = Vector2.zero;
        bg.rectTransform.anchorMax = Vector2.one;
        bg.rectTransform.offsetMin = Vector2.zero;
        bg.rectTransform.offsetMax = Vector2.zero;
        if (imagemDeFundo != null) bg.texture = imagemDeFundo;
        else bg.color = Color.black;

        GameObject t1Obj = new GameObject("Texto_Ideia");
        t1Obj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI t1 = t1Obj.AddComponent<TextMeshProUGUI>();
        t1.text = "Game designed by:\nSr. Fenoninho (A.K.A. João Rodrigues)";
        if (fonteTextoCima != null) t1.font = fonteTextoCima;
        t1.alignment = TextAlignmentOptions.Center;
        t1.fontSize = 65;
        t1.color = Color.black;
        t1.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        t1.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        t1.rectTransform.anchoredPosition = new Vector2(0, 200);
        t1.rectTransform.sizeDelta = new Vector2(1200, 200);

        GameObject t2Obj = new GameObject("Texto_Fontes");
        t2Obj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI t2 = t2Obj.AddComponent<TextMeshProUGUI>();
        t2.text = "Fonts created by:\nMia's Scribblings: Amelia McVinnie\nBoiled Pasta: baltdev";
        if (fonteTextoBaixo != null) t2.font = fonteTextoBaixo;
        t2.alignment = TextAlignmentOptions.Center;
        t2.fontSize = 50;
        t2.color = Color.black;
        t2.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        t2.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        t2.rectTransform.anchoredPosition = new Vector2(0, -100);
        t2.rectTransform.sizeDelta = new Vector2(1200, 300);

        GameObject btnObj = new GameObject("BotaoVoltar");
        btnObj.transform.SetParent(canvasObj.transform, false);
        Image btnBg = btnObj.AddComponent<Image>();
        btnBg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnBg;

        btnBg.rectTransform.anchorMin = new Vector2(0.5f, 0f);
        btnBg.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        btnBg.rectTransform.anchoredPosition = new Vector2(0, 150);
        btnBg.rectTransform.sizeDelta = new Vector2(400, 80);

        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "MAIN MENU";
        if (fonteBotao != null) btnText.font = fonteBotao;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.fontSize = 35;
        btnText.color = Color.white;
        btnText.rectTransform.anchorMin = Vector2.zero;
        btnText.rectTransform.anchorMax = Vector2.one;
        btnText.rectTransform.offsetMin = Vector2.zero;
        btnText.rectTransform.offsetMax = Vector2.zero;

        btn.onClick.AddListener(CliqueNoBotaoVoltar);
    }





    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    private void CliqueNoBotaoVoltar()
    {
        LoadingManager.Carregar(cenaDeVolta);
    }
}
