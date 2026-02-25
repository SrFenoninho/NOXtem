using UnityEngine;

[System.Serializable]
public class ColorPair
{
    public Color color;
    public Vector2Int start;
    public Vector2Int end;
}

[System.Serializable]
public class PuzzleData
{
    public ColorPair[] pairs;
}

public class FlowFreePuzzle : MonoBehaviour
{
    public PuzzleData[] puzzles;

    void Awake()
    {
        CreatePuzzles();
    }

    void CreatePuzzles()
    {
        puzzles = new PuzzleData[6];
        Color orange = new Color(1f, 0.5f, 0f);

        // Puzzle 1
        // YGGGG
        // YGOBB
        // YYOOB
        // RYROB
        // RRROB
        puzzles[0] = new PuzzleData { pairs = new ColorPair[5] };
        puzzles[0].pairs[0] = new ColorPair { color = Color.yellow, start = new Vector2Int(0, 0), end = new Vector2Int(1, 3) };
        puzzles[0].pairs[1] = new ColorPair { color = Color.green, start = new Vector2Int(4, 0), end = new Vector2Int(1, 1) };
        puzzles[0].pairs[2] = new ColorPair { color = orange, start = new Vector2Int(2, 1), end = new Vector2Int(3, 4) };
        puzzles[0].pairs[3] = new ColorPair { color = Color.blue, start = new Vector2Int(3, 1), end = new Vector2Int(4, 4) };
        puzzles[0].pairs[4] = new ColorPair { color = Color.red, start = new Vector2Int(0, 3), end = new Vector2Int(2, 3) };

        // Puzzle 2
        // OBBBG
        // OBRGG
        // OBRGY
        // ORRGY
        // ORYYY
        puzzles[1] = new PuzzleData { pairs = new ColorPair[5] };
        puzzles[1].pairs[0] = new ColorPair { color = orange, start = new Vector2Int(0, 0), end = new Vector2Int(0, 4) };
        puzzles[1].pairs[1] = new ColorPair { color = Color.blue, start = new Vector2Int(3, 0), end = new Vector2Int(1, 2) };
        puzzles[1].pairs[2] = new ColorPair { color = Color.green, start = new Vector2Int(4, 0), end = new Vector2Int(3, 3) };
        puzzles[1].pairs[3] = new ColorPair { color = Color.red, start = new Vector2Int(2, 1), end = new Vector2Int(1, 4) };
        puzzles[1].pairs[4] = new ColorPair { color = Color.yellow, start = new Vector2Int(4, 2), end = new Vector2Int(2, 4) };

        // Puzzle 3
        // OYYYY
        // OOOOY
        // GGGRB
        // GRRRB
        // GRBBB
        puzzles[2] = new PuzzleData { pairs = new ColorPair[5] };
        puzzles[2].pairs[0] = new ColorPair { color = orange, start = new Vector2Int(0, 0), end = new Vector2Int(3, 1) };
        puzzles[2].pairs[1] = new ColorPair { color = Color.yellow, start = new Vector2Int(1, 0), end = new Vector2Int(4, 1) };
        puzzles[2].pairs[2] = new ColorPair { color = Color.green, start = new Vector2Int(2, 2), end = new Vector2Int(0, 4) };
        puzzles[2].pairs[3] = new ColorPair { color = Color.red, start = new Vector2Int(3, 2), end = new Vector2Int(1, 4) };
        puzzles[2].pairs[4] = new ColorPair { color = Color.blue, start = new Vector2Int(4, 2), end = new Vector2Int(2, 4) };

        // Puzzle 4
        // OOORG
        // ORRRG
        // ORGGG
        // BBBBY
        // BYYYY
        puzzles[3] = new PuzzleData { pairs = new ColorPair[5] };
        puzzles[3].pairs[0] = new ColorPair { color = orange, start = new Vector2Int(2, 0), end = new Vector2Int(0, 2) };
        puzzles[3].pairs[1] = new ColorPair { color = Color.red, start = new Vector2Int(3, 0), end = new Vector2Int(1, 2) };
        puzzles[3].pairs[2] = new ColorPair { color = Color.green, start = new Vector2Int(4, 0), end = new Vector2Int(2, 2) };
        puzzles[3].pairs[3] = new ColorPair { color = Color.blue, start = new Vector2Int(3, 3), end = new Vector2Int(0, 4) };
        puzzles[3].pairs[4] = new ColorPair { color = Color.yellow, start = new Vector2Int(4, 3), end = new Vector2Int(1, 4) };

        // Puzzle 5
        // RBBBB
        // RRRRB
        // OOOGG
        // OGGGY
        // OYYYY
        puzzles[4] = new PuzzleData { pairs = new ColorPair[5] };
        puzzles[4].pairs[0] = new ColorPair { color = Color.red, start = new Vector2Int(0, 0), end = new Vector2Int(3, 1) };
        puzzles[4].pairs[1] = new ColorPair { color = Color.blue, start = new Vector2Int(1, 0), end = new Vector2Int(4, 1) };
        puzzles[4].pairs[2] = new ColorPair { color = orange, start = new Vector2Int(2, 2), end = new Vector2Int(0, 4) };
        puzzles[4].pairs[3] = new ColorPair { color = Color.green, start = new Vector2Int(4, 2), end = new Vector2Int(1, 3) };
        puzzles[4].pairs[4] = new ColorPair { color = Color.yellow, start = new Vector2Int(4, 3), end = new Vector2Int(1, 4) };

        // Puzzle 6
        // YYYYY
        // BBBRR
        // BRRRO
        // BGGGO
        // GGOOO
        puzzles[5] = new PuzzleData { pairs = new ColorPair[5] };
        puzzles[5].pairs[0] = new ColorPair { color = Color.yellow, start = new Vector2Int(0, 0), end = new Vector2Int(4, 0) };
        puzzles[5].pairs[1] = new ColorPair { color = Color.blue, start = new Vector2Int(2, 1), end = new Vector2Int(0, 3) };
        puzzles[5].pairs[2] = new ColorPair { color = Color.red, start = new Vector2Int(4, 1), end = new Vector2Int(1, 2) };
        puzzles[5].pairs[3] = new ColorPair { color = orange, start = new Vector2Int(4, 2), end = new Vector2Int(2, 4) };
        puzzles[5].pairs[4] = new ColorPair { color = Color.green, start = new Vector2Int(3, 3), end = new Vector2Int(0, 4) };
    }

    public PuzzleData GetRandomPuzzle()
    {
        if (puzzles == null || puzzles.Length == 0)
            CreatePuzzles();
        return puzzles[Random.Range(0, puzzles.Length)];
    }
}