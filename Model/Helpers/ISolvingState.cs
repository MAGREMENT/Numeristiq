using System.Collections.Generic;
using Model.Helpers.Changes;
using Model.Sudokus.Solver.Position;
using Model.Utility;
using Model.Utility.BitSets;

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

public interface ISudokuSolvingState : ISolvingState
{
    public bool ContainsAny(int row, int col, ReadOnlyBitSet16 possibilities)
    {
        var solved = this[row, col];
        return solved == 0 ? PossibilitiesAt(row, col).ContainsAny(possibilities) : possibilities.Contains(solved);
    }

    public bool ContainsAny(Cell cell, ReadOnlyBitSet16 possibilities)
    {
        return ContainsAny(cell.Row, cell.Column, possibilities);
    }

    public bool Contains(int row, int col, int possibility)
    {
        var solved = this[row, col];
        return solved == 0 ? PossibilitiesAt(row, col).Contains(possibility) : solved == possibility;
    }
    
    IReadOnlyLinePositions ColumnPositionsAt(int col, int number);

    IReadOnlyLinePositions RowPositionsAt(int row, int number);

    IReadOnlyMiniGridPositions MiniGridPositionsAt(int miniRow, int miniCol, int number);

    IReadOnlyGridPositions PositionsFor(int number);
}

public interface IUpdatableSudokuSolvingState : ISudokuSolvingState, IUpdatableSolvingState
{
    
}

public interface ITectonicSolvingState : ISolvingState
{
    
}

public interface IUpdatableTectonicSolvingState : ITectonicSolvingState, IUpdatableSolvingState
{
    
}