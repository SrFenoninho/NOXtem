using UnityEngine;
using UnityEngine.UI;

public class FlowFreeCell : MonoBehaviour
{
    public int x;
    public int y;
    public Image cellImage;
    private FlowFreeGame game;

    public void Initialize(int gridX, int gridY, FlowFreeGame gameManager)
    {
        x = gridX;
        y = gridY;
        game = gameManager;
        if (cellImage == null)
            cellImage = GetComponent<Image>();

        game.RegisterCell(this);

        UpdateVisual();
    }

    public void UpdateVisual()
    {
        if (game == null) return;
        Color cellColor = game.GetCellColor(x, y);
        if (cellImage != null)
            cellImage.color = cellColor == Color.clear ? Color.white : cellColor;
    }
}
