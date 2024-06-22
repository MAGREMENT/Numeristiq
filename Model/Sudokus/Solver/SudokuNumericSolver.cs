using System.Collections.Generic;
using Model.Core;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.States;
using Model.Sudokus.Solver.Utility.AlmostLockedSets;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver;

//TODO => Documentation + Explanation + Review highlighting for each strategy
//TODO => For each strategy using old als, revamp
public class SudokuSolver : NumericStrategySolver<SudokuStrategy, IUpdatableSudokuSolvingState, ISudokuHighlighter>,
    ISudokuSolverData
{
    private Sudoku _sudoku;
    private readonly ReadOnlyBitSet16[,] _possibilities = new ReadOnlyBitSet16[9, 9];
    private readonly GridPositions[] _positions = new GridPositions[9];
    private readonly LinePositions[,] _rowsPositions = new LinePositions[9, 9];
    private readonly LinePositions[,] _colsPositions = new LinePositions[9, 9];
    private readonly MiniGridPositions[,,] _minisPositions = new MiniGridPositions[3,3,9];
    
    public IReadOnlySudoku Sudoku => _sudoku;
    public bool UniquenessDependantStrategiesAllowed => StrategyManager.UniquenessDependantStrategiesAllowed;
    public PreComputer PreComputer { get; }
    public AlmostHiddenSetSearcher AlmostHiddenSetSearcher { get; }
    public AlmostNakedSetSearcher AlmostNakedSetSearcher { get; }

    public SudokuSolver() : this(new Sudoku()) { }

    private SudokuSolver(Sudoku s)
    {
        _sudoku = s;
        
        CallOnNewSudokuForEachStrategy();
        InitPossibilities();
        
        StartState = new StateArraySolvingState(this);
        PreComputer = new PreComputer(this);
        AlmostHiddenSetSearcher = new AlmostHiddenSetSearcher(this);
        AlmostNakedSetSearcher = new AlmostNakedSetSearcher(this);
    }

    public void SetSudoku(Sudoku s)
    {
        _sudoku = s;
        
        CallOnNewSudokuForEachStrategy();
        ResetPossibilities();
        
        PreComputer.Reset();
        OnNewSolvable(_sudoku.GetSolutionCount());
    }

    public void SetState(INumericSolvingState state)
    {
        _sudoku = SudokuTranslator.TranslateSolvingState(state);
        
        CallOnNewSudokuForEachStrategy();
        ResetPossibilities();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (state[row, col] != 0) continue;

                var asPoss = state.PossibilitiesAt(row, col);
                foreach (var p in PossibilitiesAt(row, col).EnumeratePossibilities())
                {
                    if (!asPoss.Contains(p)) RemovePossibility(p, row, col);
                }
            }
        }

        PreComputer.Reset();
        OnNewSolvable(_sudoku.GetSolutionCount());
    }
    
    public ReadOnlyBitSet16 RawPossibilitiesAt(int row, int col)
    {
        if (Sudoku[row, col] != 0) return new ReadOnlyBitSet16();
        
        var result = ReadOnlyBitSet16.Filled(1, 9);

        var startR = row / 3 * 3;
        var startC = col / 3 * 3;
        for (int u = 0; u < 9; u++)
        {
            if (u != row) result -= Sudoku[u, col];
            if (u != col) result -= Sudoku[row, u];

            var r = startR + u / 3;
            var c = startC + u % 3;
            if (r != row || c != col) result -= Sudoku[r, c];
        }

        return result;
    }
    
    public override bool CanRemovePossibility(CellPossibility cp)
    {
        return PossibilitiesAt(cp.Row, cp.Column).Contains(cp.Possibility);
    }

    public override bool CanAddSolution(CellPossibility cp)
    {
        return Sudoku[cp.Row, cp.Column] == 0;
    }

    #region IStrategyUser

    public int this[int row, int col] => _sudoku[row, col];

    public ReadOnlyBitSet16 PossibilitiesAt(int row, int col)
    {
        return _possibilities[row, col];
    }
    
    public IReadOnlyLinePositions RowPositionsAt(int row, int number)
    {
        return _rowsPositions[row, number - 1];
    }
    
    public IReadOnlyLinePositions ColumnPositionsAt(int col, int number)
    {
        return _colsPositions[col, number - 1];
    }
    
    public IReadOnlyMiniGridPositions MiniGridPositionsAt(int miniRow, int miniCol, int number)
    {
        return _minisPositions[miniRow, miniCol, number - 1];
    }

    public IReadOnlyGridPositions PositionsFor(int number)
    {
        return _positions[number - 1];
    }
    
    public IEnumerable<int> EnumeratePossibilitiesAt(int row, int col)
    {
        return _possibilities[row, col].EnumeratePossibilities();
    }

    #endregion

    protected override IUpdatableSudokuSolvingState GetSolvingState()
    {
        return new StateArraySolvingState(this);
    }

    public override bool IsResultCorrect()
    {
        return Sudoku.IsCorrect();
    }

    public override bool HasSolverFailed()
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (_sudoku[row, col] == 0 && _possibilities[row, col].Count == 0) return true;
            }
        }

        return false;
    }

    protected override bool AddSolution(int number, int row, int col)
    {
        if (!_possibilities[row, col].Contains(number)) return false;
        
        _currentState = null;
        _sudoku[row, col] = number;
        UpdatePossibilitiesAfterSolutionAdded(number, row, col);
        
        return true;
    }

    protected override bool RemoveSolution(int row, int col)
    {
        if (_sudoku[row, col] == 0) return false;

        _currentState = null;
        _sudoku[row, col] = 0;
        ResetPossibilities();
        return true;
    }

    protected override bool RemovePossibility(int possibility, int row, int col)
    {
        if (!_possibilities[row, col].Contains(possibility)) return false;

        _currentState = null;
        _possibilities[row, col] -= possibility;
        _positions[possibility - 1].Remove(row, col);
        _rowsPositions[row, possibility - 1].Remove(col);
        _colsPositions[col, possibility - 1].Remove(row);
        _minisPositions[row / 3, col / 3, possibility - 1].Remove(row % 3, col % 3);
        
        return true;
    }

    protected override void OnChangeMade()
    {
        PreComputer.Reset();
    }

    protected override void ApplyStrategy(SudokuStrategy strategy)
    {
        strategy.Apply(this);
    }

    protected override bool IsComplete()
    {
        return _solutionCount == 81;
    }

    #region Private
    
    private void RemovePossibilityCheckLess(int possibility, int row, int col)
    {
        _currentState = null;
        
        _possibilities[row, col] -= possibility;
        _positions[possibility - 1].Remove(row, col);
        _rowsPositions[row, possibility - 1].Remove(col);
        _colsPositions[col, possibility - 1].Remove(row);
        _minisPositions[row / 3, col / 3, possibility - 1].Remove(row % 3, col % 3);
    }

    private void CallOnNewSudokuForEachStrategy()
    {
        foreach (var s in StrategyManager.Strategies)
        {
            s.OnNewSudoku(_sudoku);
        }
    }

    private void InitPossibilities()
    {
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                _possibilities[i, j] = ReadOnlyBitSet16.Filled(1, 9);
                _rowsPositions[i, j] = LinePositions.Filled();
                _colsPositions[i, j] = LinePositions.Filled();
                _minisPositions[i / 3, i % 3, j] = MiniGridPositions.Filled(i / 3, i % 3);
            }
            
            _positions[i] = GridPositions.Filled();
        }
        
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (_sudoku[i, j] != 0)
                {
                    UpdatePossibilitiesAfterSolutionAdded(_sudoku[i, j], i, j);
                }
            }
        }
    }

    private void ResetPossibilities()
    {
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                _possibilities[i, j] = ReadOnlyBitSet16.Filled(1, 9);
                _rowsPositions[i, j].Fill();
                _colsPositions[i, j].Fill();
                _minisPositions[i / 3, i % 3, j].Fill();
            }
            
            _positions[i].Fill();
        }
        
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (_sudoku[i, j] != 0)
                {
                    UpdatePossibilitiesAfterSolutionAdded(_sudoku[i, j], i, j);
                }
            }
        }
    }

    private void UpdatePossibilitiesAfterSolutionAdded(int number, int row, int col)
    {
        int miniRow = row / 3,
            miniCol = col / 3,
            gridRow = row % 3,
            gridCol = col % 3,
            startRow = miniRow * 3,
            startCol = miniCol * 3;

        _possibilities[row, col] = new ReadOnlyBitSet16();
        for (int i = 0; i < 9; i++)
        {
            _positions[i].Remove(row, col);
            _rowsPositions[row, i].Remove(col);
            _colsPositions[col, i].Remove(row);
            _minisPositions[miniRow, miniCol, i].Remove(gridRow, gridCol);
            RemovePossibilityCheckLess(number, row, i); //TODO look into clearing this
            RemovePossibilityCheckLess(number, i, col);
            RemovePossibilityCheckLess(number,  startRow + i / 3, startCol + i % 3);
        }
    }

    #endregion
}

