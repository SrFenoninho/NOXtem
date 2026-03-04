using UnityEngine;
using UnityEngine.UI;

public class FlowFreeCell : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    public int x;
    public int y;
    public Image bgImage;       // fundo branco da célula
    public Image shapeImage;    // círculo (endpoint) ou quadrado (caminho)

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private FlowFreeGame game;

    // ---------------------------------------------
    //  INICIALIZAÇÃO
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
    // Atualiza o aspeto da célula com base no estado atual da grelha
    public void UpdateVisual()
    {
        if (game == null) return;

        Color cellColor = game.GetCellColor(x, y);
        bool endpoint = game.IsEndpoint(x, y);

        bgImage.color = Color.white;

        if (cellColor == Color.clear)
        {
            // Célula vazia — esconder forma
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
                // Endpoints são círculos maiores
                shapeImage.sprite = GetCircleSprite();
                rt.sizeDelta = new Vector2(60f, 60f);
            }
            else
            {
                // Caminho é um quadrado mais pequeno
                shapeImage.sprite = null;
                rt.sizeDelta = new Vector2(40f, 40f);
            }
        }
    }

    // ---------------------------------------------
    //  GERAÇÃO DE SPRITE CIRCULAR
    // ---------------------------------------------
    // Gera um sprite circular em runtime — sem assets externos necessários
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
