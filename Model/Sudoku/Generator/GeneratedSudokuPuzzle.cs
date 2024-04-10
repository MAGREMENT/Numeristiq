using Model.Sudoku.Solver;

namespace Model.Sudoku.Generator;

public class GeneratedSudokuPuzzle
{
    public int Id { get; }
    
    public Sudoku Sudoku { get;}
    
    public bool Evaluated { get; private set; }
    
    public double Rating { get; private set; }
    
    public SudokuStrategy? Hardest { get; private set; }

    public GeneratedSudokuPuzzle(int id, Sudoku sudoku)
    {
        Id = id;
        Sudoku = sudoku;
    }
    
    public GeneratedSudokuPuzzle(Sudoku sudoku)
    {
        Id = 0;
        Sudoku = sudoku;
    }

    public void SetEvaluation(double rating, SudokuStrategy? hardest)
    {
        Hardest = hardest;
        Rating = rating;
        Evaluated = true;
    }

    public string AsString() =>
        SudokuTranslator.TranslateLineFormat(Sudoku, SudokuLineFormatEmptyCellRepresentation.Zeros);
}