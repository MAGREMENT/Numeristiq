using Model.Core.Generators;
using Model.Utility;

namespace Model.Binairos;

public class RDRBinairoPuzzleGenerator : RDRPuzzleGenerator<Binairo>
{
    private readonly BinairoBackTracker _backTracker = new();
    
    public RDRBinairoPuzzleGenerator(IFilledPuzzleGenerator<Binairo> filledGenerator) : base(filledGenerator)
    {
    }

    protected override int GetSolutionCount(Binairo puzzle, int stopAt)
    {
        _backTracker.StopAt = stopAt;
        _backTracker.Set(puzzle);
        return _backTracker.Count();
    }

    protected override Cell GetSymmetricCell(Binairo puzzle, Cell cell)
    {
        return new Cell(puzzle.RowCount - 1 - cell.Row, puzzle.ColumnCount - 1 - cell.Column);
    }
}