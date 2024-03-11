using Model.Helpers;
using Model.Helpers.Changes;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Tectonic;

public interface IStrategyUser
{
    IReadOnlyTectonic Tectonic { get; }
    ReadOnlyBitSet16 PossibilitiesAt(Cell cell);
    ReadOnlyBitSet16 PossibilitiesAt(int row, int column)
    {
        return PossibilitiesAt(new Cell(row, column));
    }
    ReadOnlyBitSet16 ZonePositionsFor(int zone, int n);
    IChangeBuffer<IUpdatableTectonicSolvingState> ChangeBuffer { get; }
}