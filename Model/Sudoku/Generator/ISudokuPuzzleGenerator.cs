namespace Model.Sudoku.Generator;

public interface ISudokuPuzzleGenerator
{
    public Sudoku Generate();

    public Sudoku[] Generate(int count);
}