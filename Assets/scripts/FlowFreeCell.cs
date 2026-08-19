using UnityEngine;
using UnityEngine.UI;

public class FlowFreeCell : MonoBehaviour
{





    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    public int x;
    public int y;
    public Image bgImage;
    public Image shapeImage;





    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private FlowFreeGame game;





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
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

    public void UpdateVisual()
    {
        if (game == null) return;

        Color cellColor = game.GetCellColor(x, y);
        bool endpoint = game.IsEndpoint(x, y);

        bgImage.color = new Color(0.12f, 0.12f, 0.12f, 0.85f);

        if (cellColor == Color.clear)
        {
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
                shapeImage.sprite = GetCircleSprite();
                rt.sizeDelta = new Vector2(50f, 50f);
            }
            else
            {
                shapeImage.sprite = GetCircleSprite();
                rt.sizeDelta = new Vector2(25f, 25f);
            }
        }
    }




    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
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
