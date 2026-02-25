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
        return Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
    }
}