using Model.Sudoku.Solver.BitSets;
using Model.Utility;

namespace Model.Tectonic;

public interface IStrategyUser
{
    IReadOnlyTectonic Tectonic { get; }

    ReadOnlyBitSet16 PossibilitiesAt(Cell cell);
}