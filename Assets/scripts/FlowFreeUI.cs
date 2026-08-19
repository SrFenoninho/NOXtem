using UnityEngine;
using UnityEngine.UI;

public class FlowFreeUI : MonoBehaviour
{




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("Grid Settings")]

    public GameObject cellPrefab;
    public Transform gridContainer;
    public float cellSize = 80f;
    public float cellSpacing = 10f;





    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private FlowFreeCell[,] cells = new FlowFreeCell[5, 5];
    private FlowFreeGame game;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Awake()
    {
        game = GetComponent<FlowFreeGame>();
    }





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void DrawGrid(PuzzleData puzzle)
    {
        ClearGrid();

        float totalSize = 5 * cellSize + 4 * cellSpacing;
        float offset = -(totalSize / 2f) + (cellSize / 2f);

        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                GameObject cellObj = Instantiate(cellPrefab, gridContainer);
                RectTransform rect = cellObj.GetComponent<RectTransform>();

                float posX = offset + x * (cellSize + cellSpacing);
                float posY = -offset - y * (cellSize + cellSpacing);
                rect.anchoredPosition = new Vector2(posX, posY);
                rect.sizeDelta = new Vector2(cellSize, cellSize);

                Image bg = cellObj.GetComponent<Image>();
                if (bg != null) bg.color = Color.white;

                GameObject shapeObj = new GameObject("Shape", typeof(RectTransform), typeof(Image));
                shapeObj.transform.SetParent(cellObj.transform, false);
                RectTransform shapeRect = shapeObj.GetComponent<RectTransform>();
                shapeRect.anchoredPosition = Vector2.zero;
                shapeRect.sizeDelta = new Vector2(40f, 40f);

                FlowFreeCell cell = cellObj.GetComponent<FlowFreeCell>();
                if (cell == null)
                    cell = cellObj.AddComponent<FlowFreeCell>();

                cell.bgImage = bg;
                cell.shapeImage = shapeObj.GetComponent<Image>();
                cell.shapeImage.raycastTarget = false;

                cell.Initialize(x, y, game);
                cells[x, y] = cell;
            }
        }

        foreach (ColorPair pair in puzzle.pairs)
        {
            cells[pair.start.x, pair.start.y].UpdateVisual();
            cells[pair.end.x, pair.end.y].UpdateVisual();
        }
    }




    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    void ClearGrid()
    {
        if (gridContainer == null) return;
        foreach (Transform child in gridContainer)
            Destroy(child.gameObject);
    }
}
