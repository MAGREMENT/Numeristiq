using Model.Core;
using Model.Core.Generators;

namespace Model.Sudokus.Generator;

public class GeneratedSudokuPuzzle : GeneratedPuzzle<Sudoku>
{
    public int Id { get; }

    public GeneratedSudokuPuzzle(int id, Sudoku sudoku) : base(sudoku)
    {
        Id = id;
    }
    
    public GeneratedSudokuPuzzle(Sudoku sudoku) : base(sudoku)
    {
        Id = 0;
    }

    public override string AsString() =>
        SudokuTranslator.TranslateLineFormat(Puzzle, SudokuLineFormatEmptyCellRepresentation.Zeros);
}