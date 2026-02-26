using UnityEngine;
using UnityEngine.UI;

public class FlowFreeCell : MonoBehaviour
{
    public int x;
    public int y;
    public Image bgImage;
    public Image shapeImage;

    private FlowFreeGame game;

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
        bool isEndpoint = game.IsEndpoint(x, y);

        bgImage.color = Color.white;

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

            if (isEndpoint)
            {
                shapeImage.sprite = GetCircleSprite();
                rt.sizeDelta = new Vector2(60f, 60f);
            }
            else
            {
                shapeImage.sprite = null;
                rt.sizeDelta = new Vector2(40f, 40f);
            }
        }
    }

    Sprite GetCircleSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        float radius = size / 2f;
        Color transparent = new Color(0, 0, 0, 0);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius));
                tex.SetPixel(x, y, dist <= radius ? Color.white : transparent);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}