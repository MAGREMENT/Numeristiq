namespace Model.Sudoku.Generator;

public interface ISudokuPuzzleGenerator
{
    public Sudoku Generate();

    public Sudoku[] Generate(int count);

    public void Generate(OnNewPuzzleGenerated handler, int count = 1);
}

public delegate void OnNewPuzzleGenerated(Sudoku sudoku);