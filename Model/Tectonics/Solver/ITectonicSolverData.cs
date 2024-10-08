﻿using Model.Core;
using Model.Core.BackTracking;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Core.Highlighting;
using Model.Tectonics.Solver.Utility;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Tectonics.Solver;

public interface ITectonicSolverData : IPossibilitiesGiver
{
    IReadOnlyTectonic Tectonic { get; }
    ReadOnlyBitSet8 PossibilitiesAt(Cell cell);
    ReadOnlyBitSet8 PossibilitiesAt(int row, int column)
    {
        return PossibilitiesAt(new Cell(row, column));
    }
    ReadOnlyBitSet8 ZonePositionsFor(int zone, int n);
    ReadOnlyBitSet8 ZonePositionsFor(IZone zone, int n);
    NumericChangeBuffer<INumericSolvingState, ITectonicHighlighter> ChangeBuffer { get; }
    ConstructedGraph<ITectonicSolverData, IGraph<ITectonicElement, LinkStrength>> ManagedGraph { get; }
}