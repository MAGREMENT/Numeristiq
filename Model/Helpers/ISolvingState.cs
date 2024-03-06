using System.Collections.Generic;
using Model.Helpers.Changes;
using Model.Sudoku.Solver.BitSets;
using Model.Utility;

namespace Model.Helpers;

public interface ISolvingState
{
    int this[int row, int col] { get; }
    ReadOnlyBitSet16 PossibilitiesAt(int row, int col);
    ReadOnlyBitSet16 PossibilitiesAt(Cell cell) => PossibilitiesAt(cell.Row, cell.Column);
}

public interface IUpdatableSolvingState : ISolvingState
{
    public IUpdatableSolvingState Apply(IReadOnlyList<SolverProgress> progresses);
    public IUpdatableSolvingState Apply(SolverProgress progress);
}