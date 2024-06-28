﻿using System.Collections.Generic;
using Model.Core.Changes;
using Model.Sudokus.Solver.Position;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Core;

//TODO remove the principle of updatable solving state and just ask solver for state after emulating change

public interface INumericSolvingState
{
    int this[int row, int col] { get; }
    ReadOnlyBitSet16 PossibilitiesAt(int row, int col);
    ReadOnlyBitSet16 PossibilitiesAt(Cell cell) => PossibilitiesAt(cell.Row, cell.Column);
}

public interface IDichotomousSolvingState
{
    bool this[int row, int col] { get; }
    bool IsAvailable(int row, int col);
}

public interface IUpdatableDichotomousSolvingState : IDichotomousSolvingState
{
    public IUpdatableDichotomousSolvingState Apply(IEnumerable<DichotomousChange> progresses);
    public IUpdatableDichotomousSolvingState Apply(DichotomousChange progress);
}

public interface IUpdatableNumericSolvingState : INumericSolvingState
{
    public IUpdatableNumericSolvingState Apply(IEnumerable<NumericChange> progresses);
    public IUpdatableNumericSolvingState Apply(NumericChange progress);
}

public interface ISudokuSolvingState : INumericSolvingState
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

public interface IUpdatableSudokuSolvingState : ISudokuSolvingState, IUpdatableNumericSolvingState
{
    
}

public interface ITectonicSolvingState : INumericSolvingState
{
    
}

public interface IUpdatableTectonicSolvingState : ITectonicSolvingState, IUpdatableNumericSolvingState
{
    
}