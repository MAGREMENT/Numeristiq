using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Graphs;
using Model.Helpers.Highlighting;
using Model.Tectonics.Utility;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Tectonics;

public interface ITectonicStrategyUser
{
    IReadOnlyTectonic Tectonic { get; }
    ReadOnlyBitSet16 PossibilitiesAt(Cell cell);
    ReadOnlyBitSet16 PossibilitiesAt(int row, int column)
    {
        return PossibilitiesAt(new Cell(row, column));
    }
    ReadOnlyBitSet16 ZonePositionsFor(int zone, int n);
    ReadOnlyBitSet16 ZonePositionsFor(IZone zone, int n);
    IChangeBuffer<IUpdatableTectonicSolvingState, ITectonicHighlighter> ChangeBuffer { get; }
    LinkGraphManager<ITectonicStrategyUser, ITectonicElement> Graphs { get; }
}