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
    ReadOnlyBitSet8 PossibilitiesAt(Cell cell);
    ReadOnlyBitSet8 PossibilitiesAt(int row, int column)
    {
        return PossibilitiesAt(new Cell(row, column));
    }
    ReadOnlyBitSet8 ZonePositionsFor(int zone, int n);
    ReadOnlyBitSet8 ZonePositionsFor(IZone zone, int n);
    IChangeBuffer<IUpdatableTectonicSolvingState, ITectonicHighlighter> ChangeBuffer { get; }
    LinkGraphManager<ITectonicStrategyUser, ITectonicElement> Graphs { get; }
}