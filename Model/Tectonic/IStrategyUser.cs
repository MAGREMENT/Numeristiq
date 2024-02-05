using Model.Helpers.Changes;
using Model.Sudoku.Solver.BitSets;
using Model.Utility;

namespace Model.Tectonic;

public interface IStrategyUser
{
    IReadOnlyTectonic Tectonic { get; }
    ReadOnlyBitSet16 PossibilitiesAt(Cell cell);
    ReadOnlyBitSet16 ZonePositionsFor(int zone, int n);
    IChangeBuffer ChangeBuffer { get; }
}