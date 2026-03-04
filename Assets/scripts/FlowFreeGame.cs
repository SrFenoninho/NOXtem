using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
    public Text winText;
    public Button resetButton;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private PuzzleData currentPuzzle;
    private Color[,] grid = new Color[5, 5];        // cor atual de cada célula da grelha
    private bool[,] isEndpoint = new bool[5, 5];    // verdadeiro se a célula é um endpoint fixo
    private Color currentDragColor = Color.clear;   // cor do fluxo que está a ser arrastado
    private FlowFreeCell[,] cellRefs = new FlowFreeCell[5, 5]; // referências às células instanciadas
    private bool puzzleComplete = false;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Awake()
    {
        gameObject.SetActive(false); // começa inativo — ativado pelo terminal
    }

    void OnEnable()
    {
        if (puzzleData != null)
            StartNewGame();

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetPuzzle);
    }

    void OnDisable()
    {
        if (resetButton != null)
            resetButton.onClick.RemoveListener(ResetPuzzle);
    }

    // ---------------------------------------------
    //  REGISTO DE CÉLULAS
    // ---------------------------------------------
    // Chamado por cada FlowFreeCell ao ser inicializada
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

        Debug.Log("Novo puzzle Flow Free carregado!");
    }

    public void ResetPuzzle()
    {
        // Limpar todas as células não-endpoint
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

        // Atualizar visual de todas as células
        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 5; y++)
                if (cellRefs[x, y] != null)
                    cellRefs[x, y].UpdateVisual();

        Debug.Log("Puzzle reiniciado!");
    }

    // ---------------------------------------------
    //  INICIALIZAÇÃO DA GRELHA
    // ---------------------------------------------
    void InitializeGrid()
    {
        // Limpar tudo
        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 5; y++)
            {
                grid[x, y] = Color.clear;
                isEndpoint[x, y] = false;
            }

        // Definir os endpoints do puzzle atual
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

        // Início do arrasto — determinar qual cor está a ser puxada
        if (Input.GetMouseButtonDown(0))
        {
            FlowFreeCell cell = GetCellUnderMouse();
            if (cell != null)
            {
                if (isEndpoint[cell.x, cell.y] || grid[cell.x, cell.y] != Color.clear)
                    currentDragColor = grid[cell.x, cell.y];
            }
        }

        // Arrastar — pintar célula se adjacente à cor atual
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

        // Fim do arrasto
        if (Input.GetMouseButtonUp(0))
            currentDragColor = Color.clear;
    }

    // ---------------------------------------------
    //  LÓGICA DA GRELHA
    // ---------------------------------------------
    // Verifica se alguma célula adjacente (4 direções) tem a cor dada
    bool HasAdjacentColor(int x, int y, Color color)
    {
        if (x > 0 && grid[x - 1, y] == color) return true;
        if (x < 4 && grid[x + 1, y] == color) return true;
        if (y > 0 && grid[x, y - 1] == color) return true;
        if (y < 4 && grid[x, y + 1] == color) return true;
        return false;
    }

    // Detetar qual célula está sob o cursor do rato via EventSystem
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
    //  VERIFICAÇÃO DE CONCLUSÃO
    // ---------------------------------------------
    void CheckCompletion()
    {
        if (currentPuzzle.solution == null) return;

        // Comparar grelha atual com a solução célula a célula
        for (int y = 0; y < 5; y++)
            for (int x = 0; x < 5; x++)
            {
                char solChar = currentPuzzle.solution[y][x];
                Color expectedColor = GetColorFromSolutionChar(solChar);
                if (grid[x, y] != expectedColor) return;
            }

        puzzleComplete = true;
        Debug.Log("Puzzle concluído!");

        if (winText != null)
        {
            winText.gameObject.SetActive(true);
            winText.text = "Puzzle Complete!";
        }

        StartCoroutine(CompleteAfterDelay(2.5f));
    }

    IEnumerator CompleteAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (terminal != null)
            terminal.OnGameComplete();
    }

    // Converte o carácter da solução na cor correspondente do puzzle
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
