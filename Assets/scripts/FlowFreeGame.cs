using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class FlowFreeGame : MonoBehaviour
{
    [Header("References")]
    public FlowFreePuzzle puzzleData;
    public FlowFreeTerminal terminal;
    public FlowFreeUI gameUI;

    private PuzzleData currentPuzzle;
    private Color[,] grid = new Color[5, 5];
    private bool[,] isEndpoint = new bool[5, 5];
    private Color currentDragColor = Color.clear;
    private FlowFreeCell[,] cellRefs = new FlowFreeCell[5, 5];

    void Awake()
    {
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (puzzleData != null)
            StartNewGame();
    }

    public void RegisterCell(FlowFreeCell cell)
    {
        cellRefs[cell.x, cell.y] = cell;
    }

    public void StartNewGame()
    {
        if (puzzleData == null) return;
        currentPuzzle = puzzleData.GetRandomPuzzle();
        currentDragColor = Color.clear;
        InitializeGrid();
        if (gameUI != null)
            gameUI.DrawGrid(currentPuzzle);
        Debug.Log("New Flow Free puzzle loaded!");
    }

    void InitializeGrid()
    {
        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 5; y++)
            {
                grid[x, y] = Color.clear;
                isEndpoint[x, y] = false;
            }

        foreach (ColorPair pair in currentPuzzle.pairs)
        {
            grid[pair.start.x, pair.start.y] = pair.color;
            grid[pair.end.x, pair.end.y] = pair.color;
            isEndpoint[pair.start.x, pair.start.y] = true;
            isEndpoint[pair.end.x, pair.end.y] = true;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            FlowFreeCell cell = GetCellUnderMouse();
            if (cell != null)
            {
                if (isEndpoint[cell.x, cell.y])
                {
                    currentDragColor = grid[cell.x, cell.y];
                }
                else if (grid[cell.x, cell.y] != Color.clear)
                {
                    currentDragColor = grid[cell.x, cell.y];
                }
            }
        }

        if (Input.GetMouseButton(0) && currentDragColor != Color.clear)
        {
            FlowFreeCell cell = GetCellUnderMouse();
            if (cell != null && !isEndpoint[cell.x, cell.y])
            {

                if (HasAdjacentColor(cell.x, cell.y, currentDragColor))
                {
                    grid[cell.x, cell.y] = currentDragColor;
                    cell.UpdateVisual();
                    CheckCompletion();
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            currentDragColor = Color.clear;
        }
    }

    bool HasAdjacentColor(int x, int y, Color color)
    {
        if (x > 0 && grid[x - 1, y] == color) return true;
        if (x < 4 && grid[x + 1, y] == color) return true;
        if (y > 0 && grid[x, y - 1] == color) return true;
        if (y < 4 && grid[x, y + 1] == color) return true;
        return false;
    }

    FlowFreeCell GetCellUnderMouse()
    {
        if (EventSystem.current == null) return null;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            FlowFreeCell cell = result.gameObject.GetComponent<FlowFreeCell>();
            if (cell != null) return cell;
        }
        return null;
    }

    public Color GetCellColor(int x, int y) => grid[x, y];
    public bool IsEndpoint(int x, int y) => isEndpoint[x, y];

    void CheckCompletion()
    {
        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 5; y++)
                if (grid[x, y] == Color.clear) return;

        Debug.Log("Puzzle completed!");
        if (terminal != null)
            terminal.OnGameComplete();
    }
}
