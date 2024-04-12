namespace Model.Sudoku.Generator;

public class ConstantFilledSudokuGenerator : IFilledSudokuGenerator
{
    public Sudoku Sudoku { get; set; }
    
    public ConstantFilledSudokuGenerator(Sudoku sudoku)
    {
        Sudoku = sudoku;
    }
    
    public Sudoku Generate()
    {
        return Sudoku.Copy();
    }
}