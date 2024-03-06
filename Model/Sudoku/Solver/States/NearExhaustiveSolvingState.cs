using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Sudoku.Solver.BitSets;
using Model.Sudoku.Solver.Position;

namespace Model.Sudoku.Solver.States;

public class NearExhaustiveSolvingState : IUpdatableSudokuSolvingState
{
    private readonly Sudoku _sudoku;
    private readonly ReadOnlyBitSet16[,] _possibilities;
    private readonly GridPositions[] _positions;

    public NearExhaustiveSolvingState(ISudokuSolvingState state)
    {
        var possibilities = new ReadOnlyBitSet16[9, 9];
        var positions = new GridPositions[9];
        var sudoku = new Sudoku();
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                possibilities[row, col] = state.PossibilitiesAt(row, col);
                sudoku[row, col] = state[row, col];
            }
        }

        for (int number = 1; number <= 9; number++)
        {
            positions[number - 1] = state.PositionsFor(number).Copy();
        }
        
        _sudoku = sudoku;
        _positions = positions;
        _possibilities = possibilities;
    }

    private NearExhaustiveSolvingState(Sudoku sudoku, ReadOnlyBitSet16[,] possibilities, GridPositions[] positions)
    {
        _sudoku = sudoku;
        _positions = positions;
        _possibilities = possibilities;
    }

    public int this[int row, int col] => _sudoku[row, col];

    public ReadOnlyBitSet16 PossibilitiesAt(int row, int col)
    {
        return _possibilities[row, col];
    }
    
    public IReadOnlyLinePositions RowPositionsAt(int row, int number)
    {
        return _positions[number - 1].RowPositions(row);
    }

    public IReadOnlyLinePositions ColumnPositionsAt(int col, int number)
    {
        return _positions[number - 1].ColumnPositions(col);
    }

    public IReadOnlyMiniGridPositions MiniGridPositionsAt(int miniRow, int miniCol, int number)
    {
        return _positions[number - 1].MiniGridPositions(miniRow, miniCol);
    }

    public IReadOnlyGridPositions PositionsFor(int number)
    {
        return _positions[number - 1];
    }

    public IUpdatableSolvingState Apply(IReadOnlyList<SolverProgress> progresses)
    {
        throw new System.NotImplementedException();
    }

    public IUpdatableSolvingState Apply(SolverProgress progress)
    {
        throw new System.NotImplementedException();
    }
}