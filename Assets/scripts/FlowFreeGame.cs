using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class FlowFreeGame : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("References")]
    public FlowFreePuzzle puzzleData;
    public FlowFreeTerminal terminal;
    public FlowFreeUI gameUI;

    [Header("UI Feedback")]
    public TextMeshProUGUI winText;
    public Button resetButton;
    public Button exitButton;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private PuzzleData currentPuzzle;
    private Color[,] grid = new Color[5, 5];
    private bool[,] isEndpoint = new bool[5, 5];
    private Color currentDragColor = Color.clear;
    private FlowFreeCell[,] cellRefs = new FlowFreeCell[5, 5];
    private bool puzzleComplete = false;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Awake()
    {
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (puzzleData != null)
            StartNewGame();

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetPuzzle);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitMinigame);
    }

    void OnDisable()
    {
        if (resetButton != null)
            resetButton.onClick.RemoveListener(ResetPuzzle);

        if (exitButton != null)
            exitButton.onClick.RemoveListener(ExitMinigame);
    }

    // ---------------------------------------------
    //  REGISTO DE CELULAS
    // ---------------------------------------------
    public void RegisterCell(FlowFreeCell cell)
    {
        cellRefs[cell.x, cell.y] = cell;
    }

    // ---------------------------------------------
    //  JOGO
    // ---------------------------------------------
    public void StartNewGame()
    {
        if (puzzleData == null) return;

        currentPuzzle = puzzleData.GetRandomPuzzle();
        currentDragColor = Color.clear;
        puzzleComplete = false;

        if (winText != null)
        {
            winText.text = "";
            winText.gameObject.SetActive(false);
        }

        InitializeGrid();

        if (gameUI != null)
            gameUI.DrawGrid(currentPuzzle);
    }

    public void ResetPuzzle()
    {
        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 5; y++)
                if (!isEndpoint[x, y])
                    grid[x, y] = Color.clear;

        puzzleComplete = false;
        currentDragColor = Color.clear;

        if (winText != null)
        {
            winText.text = "";
            winText.gameObject.SetActive(false);
        }

        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 5; y++)
                if (cellRefs[x, y] != null)
                    cellRefs[x, y].UpdateVisual();
    }

    // ---------------------------------------------
    //  SAIDA DO MINIJOGO
    // ---------------------------------------------
    public void ExitMinigame()
    {
        if (terminal != null)
            terminal.ForceClose();
    }

    // ---------------------------------------------
    //  INICIALIZACAO DA GRELHA
    // ---------------------------------------------
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

    // ---------------------------------------------
    //  INPUT DE ARRASTAR
    // ---------------------------------------------
    void Update()
    {
        if (puzzleComplete) return;

        if (Input.GetMouseButtonDown(0))
        {
            FlowFreeCell cell = GetCellUnderMouse();
            if (cell != null)
            {
                if (isEndpoint[cell.x, cell.y] || grid[cell.x, cell.y] != Color.clear)
                    currentDragColor = grid[cell.x, cell.y];
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
            currentDragColor = Color.clear;
    }

    // ---------------------------------------------
    //  LOGICA DA GRELHA
    // ---------------------------------------------
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

    // ---------------------------------------------
    //  VERIFICACAO DE CONCLUSAO
    // ---------------------------------------------
    void CheckCompletion()
    {
        if (currentPuzzle.solution == null) return;

        for (int y = 0; y < 5; y++)
            for (int x = 0; x < 5; x++)
            {
                char solChar = currentPuzzle.solution[y][x];
                Color expectedColor = GetColorFromSolutionChar(solChar);
                if (grid[x, y] != expectedColor) return;
            }

        puzzleComplete = true;

        if (winText != null)
        {
            winText.gameObject.SetActive(true);
            winText.text = "Puzzle Complete!";
        }

        StartCoroutine(CompleteAfterDelay(0.25f));
    }

    System.Collections.IEnumerator CompleteAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (terminal != null)
            terminal.OnGameComplete();
    }

    Color GetColorFromSolutionChar(char c)
    {
        foreach (ColorPair pair in currentPuzzle.pairs)
        {
            char startChar = currentPuzzle.solution[pair.start.y][pair.start.x];
            if (startChar == c) return pair.color;
        }
        return Color.clear;
    }
}