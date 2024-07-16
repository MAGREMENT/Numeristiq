using Model.Core.BackTracking;
using Model.Core.Generators;
using Model.Utility;

namespace Model.Tectonics.Generator;

public class RDRTectonicPuzzleGenerator : RDRPuzzleGenerator<ITectonic>
{
    private readonly TectonicBackTracker _backTracker = new();
    
    public RDRTectonicPuzzleGenerator(IFilledPuzzleGenerator<ITectonic> filledGenerator) : base(filledGenerator)
    {
    }

    protected override int GetSolutionCount(ITectonic puzzle, int stopAt)
    {
        _backTracker.StopAt = stopAt;
        _backTracker.Set(puzzle, new TectonicPossibilitiesGiver(puzzle));
        return _backTracker.Count();
    }

    protected override Cell GetSymmetricCell(ITectonic puzzle, Cell cell)
    {
        return new Cell(puzzle.RowCount - 1 - cell.Row, puzzle.ColumnCount - 1 - cell.Column);
    }
}