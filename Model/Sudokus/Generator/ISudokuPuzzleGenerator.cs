namespace Model.Sudokus.Generator;

public interface ISudokuPuzzleGenerator
{
    public bool KeepSymmetry { get; set; }
    
    public bool KeepUniqueness { get; set; }
    
    public Sudoku Generate();

    public Sudoku[] Generate(int count);
}