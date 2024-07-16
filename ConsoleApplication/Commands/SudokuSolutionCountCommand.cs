using ConsoleApplication.Commands.Abstracts;
using Model.Core.BackTracking;
using Model.Sudokus;
using Model.Sudokus.Generator;

namespace ConsoleApplication.Commands;

public class SudokuSolutionCountCommand : SolutionCountCommand<Sudoku, IPossibilitiesGiver>
{
    public SudokuSolutionCountCommand() : base("Sudoku")
    {
    }

    protected override BackTracker<Sudoku, IPossibilitiesGiver> BackTracker { get; }
        = new SudokuBackTracker(new Sudoku(), ConstantPossibilitiesGiver.Instance);
    
    protected override void SetBackTracker(string s)
    {
        BackTracker.Set(SudokuTranslator.TranslateLineFormat(s));
    }

    protected override string ToString(Sudoku puzzle)
    {
        return SudokuTranslator.TranslateLineFormat(puzzle, SudokuLineFormatEmptyCellRepresentation.Points);
    }
}