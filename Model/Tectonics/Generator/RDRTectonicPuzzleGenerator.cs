using Model.Core.Generators;
using Model.Utility;

namespace Model.Tectonics.Generator;

public class RDRTectonicPuzzleGenerator : RDRPuzzleGenerator<ITectonic>
{
    public RDRTectonicPuzzleGenerator(IFilledPuzzleGenerator<ITectonic> filledGenerator) : base(filledGenerator)
    {
    }

    protected override int GetSolutionCount(ITectonic puzzle, int stopAt)
    {
        return BackTracking.Count(puzzle, new TectonicPossibilitiesGiver(puzzle), stopAt);
    }

    protected override Cell GetSymmetricCell(ITectonic puzzle, Cell cell)
    {
        return new Cell(puzzle.RowCount - 1 - cell.Row, puzzle.ColumnCount - 1 - cell.Column);
    }
}