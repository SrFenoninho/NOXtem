using UnityEngine;
using UnityEngine.UI;

public class FlowFreeCell : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    public int x;
    public int y;
    public Image bgImage;       // fundo branco da celula
    public Image shapeImage;    // circulo (endpoint) ou quadrado (caminho)

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private FlowFreeGame game;

    // ---------------------------------------------
    //  INICIALIZAcaO
    // ---------------------------------------------
    // Chamado pelo FlowFreeUI ao criar a grelha
    public void Initialize(int gridX, int gridY, FlowFreeGame gameManager)
    {
        x = gridX;
        y = gridY;
        game = gameManager;

        if (bgImage == null)
            bgImage = GetComponent<Image>();

        game.RegisterCell(this);
        UpdateVisual();
    }

    // ---------------------------------------------
    //  VISUAL
    // ---------------------------------------------
    // Atualiza o aspeto da celula com base no estado atual da grelha
    public void UpdateVisual()
    {
        if (game == null) return;

        Color cellColor = game.GetCellColor(x, y);
        bool endpoint = game.IsEndpoint(x, y);

        // Fundo cinzento escuro elegante com transparência para estilo terminal futurista
        bgImage.color = new Color(0.12f, 0.12f, 0.12f, 0.85f);

        if (cellColor == Color.clear)
        {
            // Celula vazia - esconder forma
            if (shapeImage != null)
                shapeImage.gameObject.SetActive(false);
            return;
        }

        if (shapeImage != null)
        {
            shapeImage.gameObject.SetActive(true);
            shapeImage.color = cellColor;

            RectTransform rt = shapeImage.GetComponent<RectTransform>();

            if (endpoint)
            {
                // Endpoints sao circulos maiores (50x50 para caber melhor na celula de 80)
                shapeImage.sprite = GetCircleSprite();
                rt.sizeDelta = new Vector2(50f, 50f);
            }
            else
            {
                // Caminho e um circulo mais pequeno e elegante (estilo fluxo de energia circular)
                shapeImage.sprite = GetCircleSprite();
                rt.sizeDelta = new Vector2(25f, 25f);
            }
        }
    }

    // ---------------------------------------------
    //  GERAcaO DE SPRITE CIRCULAR
    // ---------------------------------------------
    // Gera um sprite circular em runtime - sem assets externos necessarios
    Sprite GetCircleSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        float radius = size / 2f;
        Color transparent = new Color(0, 0, 0, 0);

        for (int py = 0; py < size; py++)
            for (int px = 0; px < size; px++)
            {
                float dist = Vector2.Distance(new Vector2(px, py), new Vector2(radius, radius));
                tex.SetPixel(px, py, dist <= radius ? Color.white : transparent);
            }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
