using UnityEngine;
using UnityEngine.UI;

public class FlowFreeUI : MonoBehaviour
{
    [Header("Grid Settings")]
    public GameObject cellPrefab;
    public Transform gridContainer;
    public float cellSize = 80f;
    public float cellSpacing = 10f;

    private FlowFreeCell[,] cells = new FlowFreeCell[5, 5];
    private FlowFreeGame game;

    void Awake()
    {
        game = GetComponent<FlowFreeGame>();
    }

    public void DrawGrid(PuzzleData puzzle)
    {
        ClearGrid();

        // Calcula offset para centrar a grelha
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

                FlowFreeCell cell = cellObj.GetComponent<FlowFreeCell>();
                if (cell == null)
                    cell = cellObj.AddComponent<FlowFreeCell>();

                cell.Initialize(x, y, game);
                cells[x, y] = cell;
            }
        }

        // Atualiza visuais dos endpoints
        foreach (ColorPair pair in puzzle.pairs)
        {
            cells[pair.start.x, pair.start.y].UpdateVisual();
            cells[pair.end.x, pair.end.y].UpdateVisual();
        }
    }

    void ClearGrid()
    {
        if (gridContainer == null) return;
        foreach (Transform child in gridContainer)
            Destroy(child.gameObject);
    }
}