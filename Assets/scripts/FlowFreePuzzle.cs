using UnityEngine;

[System.Serializable]
public class ColorPair
{




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    public Color color;
    public Vector2Int start;
    public Vector2Int end;
}

[System.Serializable]
public class PuzzleData
{
    public ColorPair[] pairs;
    public string[] solution;
}

public class FlowFreePuzzle : MonoBehaviour
{

    public PuzzleData[] puzzles;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Awake()
    {
        CreatePuzzles();
    }




    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    void CreatePuzzles()
    {
        puzzles = new PuzzleData[6];
        Color orange = new Color(1f, 0.5f, 0f);

        puzzles[0] = new PuzzleData { pairs = new ColorPair[5] };
        puzzles[0].pairs[0] = new ColorPair { color = Color.yellow, start = new Vector2Int(0, 0), end = new Vector2Int(1, 3) };
        puzzles[0].pairs[1] = new ColorPair { color = Color.green,  start = new Vector2Int(4, 0), end = new Vector2Int(1, 1) };
        puzzles[0].pairs[2] = new ColorPair { color = orange,       start = new Vector2Int(2, 1), end = new Vector2Int(3, 4) };
        puzzles[0].pairs[3] = new ColorPair { color = Color.blue,   start = new Vector2Int(3, 1), end = new Vector2Int(4, 4) };
        puzzles[0].pairs[4] = new ColorPair { color = Color.red,    start = new Vector2Int(0, 3), end = new Vector2Int(2, 3) };
        puzzles[0].solution = new string[] { "YGGGG", "YGOBB", "YYOOB", "RYROB", "RRROB" };

        puzzles[1] = new PuzzleData { pairs = new ColorPair[5] };
        puzzles[1].pairs[0] = new ColorPair { color = orange,       start = new Vector2Int(0, 0), end = new Vector2Int(0, 4) };
        puzzles[1].pairs[1] = new ColorPair { color = Color.blue,   start = new Vector2Int(3, 0), end = new Vector2Int(1, 2) };
        puzzles[1].pairs[2] = new ColorPair { color = Color.green,  start = new Vector2Int(4, 0), end = new Vector2Int(3, 3) };
        puzzles[1].pairs[3] = new ColorPair { color = Color.red,    start = new Vector2Int(2, 1), end = new Vector2Int(1, 4) };
        puzzles[1].pairs[4] = new ColorPair { color = Color.yellow, start = new Vector2Int(4, 2), end = new Vector2Int(2, 4) };
        puzzles[1].solution = new string[] { "OBBBG", "OBRGG", "OBRGY", "ORRGY", "ORYYY" };

        puzzles[2] = new PuzzleData { pairs = new ColorPair[5] };
        puzzles[2].pairs[0] = new ColorPair { color = orange,       start = new Vector2Int(0, 0), end = new Vector2Int(3, 1) };
        puzzles[2].pairs[1] = new ColorPair { color = Color.yellow, start = new Vector2Int(1, 0), end = new Vector2Int(4, 1) };
        puzzles[2].pairs[2] = new ColorPair { color = Color.green,  start = new Vector2Int(2, 2), end = new Vector2Int(0, 4) };
        puzzles[2].pairs[3] = new ColorPair { color = Color.red,    start = new Vector2Int(3, 2), end = new Vector2Int(1, 4) };
        puzzles[2].pairs[4] = new ColorPair { color = Color.blue,   start = new Vector2Int(4, 2), end = new Vector2Int(2, 4) };
        puzzles[2].solution = new string[] { "OYYYY", "OOOOY", "GGGRB", "GRRRB", "GRBBB" };

        puzzles[3] = new PuzzleData { pairs = new ColorPair[5] };
        puzzles[3].pairs[0] = new ColorPair { color = orange,       start = new Vector2Int(2, 0), end = new Vector2Int(0, 2) };
        puzzles[3].pairs[1] = new ColorPair { color = Color.red,    start = new Vector2Int(3, 0), end = new Vector2Int(1, 2) };
        puzzles[3].pairs[2] = new ColorPair { color = Color.green,  start = new Vector2Int(4, 0), end = new Vector2Int(2, 2) };
        puzzles[3].pairs[3] = new ColorPair { color = Color.blue,   start = new Vector2Int(3, 3), end = new Vector2Int(0, 4) };
        puzzles[3].pairs[4] = new ColorPair { color = Color.yellow, start = new Vector2Int(4, 3), end = new Vector2Int(1, 4) };
        puzzles[3].solution = new string[] { "OOORG", "ORRRG", "ORGGG", "BBBBY", "BYYYY" };

        puzzles[4] = new PuzzleData { pairs = new ColorPair[5] };
        puzzles[4].pairs[0] = new ColorPair { color = Color.red,    start = new Vector2Int(0, 0), end = new Vector2Int(3, 1) };
        puzzles[4].pairs[1] = new ColorPair { color = Color.blue,   start = new Vector2Int(1, 0), end = new Vector2Int(4, 1) };
        puzzles[4].pairs[2] = new ColorPair { color = orange,       start = new Vector2Int(2, 2), end = new Vector2Int(0, 4) };
        puzzles[4].pairs[3] = new ColorPair { color = Color.green,  start = new Vector2Int(4, 2), end = new Vector2Int(1, 3) };
        puzzles[4].pairs[4] = new ColorPair { color = Color.yellow, start = new Vector2Int(4, 3), end = new Vector2Int(1, 4) };
        puzzles[4].solution = new string[] { "RBBBB", "RRRRB", "OOOGG", "OGGGY", "OYYYY" };

        puzzles[5] = new PuzzleData { pairs = new ColorPair[5] };
        puzzles[5].pairs[0] = new ColorPair { color = Color.yellow, start = new Vector2Int(0, 0), end = new Vector2Int(4, 0) };
        puzzles[5].pairs[1] = new ColorPair { color = Color.blue,   start = new Vector2Int(2, 1), end = new Vector2Int(0, 3) };
        puzzles[5].pairs[2] = new ColorPair { color = Color.red,    start = new Vector2Int(4, 1), end = new Vector2Int(1, 2) };
        puzzles[5].pairs[3] = new ColorPair { color = orange,       start = new Vector2Int(4, 2), end = new Vector2Int(2, 4) };
        puzzles[5].pairs[4] = new ColorPair { color = Color.green,  start = new Vector2Int(3, 3), end = new Vector2Int(0, 4) };
        puzzles[5].solution = new string[] { "YYYYY", "BBBRR", "BRRRO", "BGGGO", "GGOOO" };
    }





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public PuzzleData GetRandomPuzzle()
    {
        if (puzzles == null || puzzles.Length == 0)
            CreatePuzzles();
        return puzzles[Random.Range(0, puzzles.Length)];
    }
}
