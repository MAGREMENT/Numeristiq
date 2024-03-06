using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Changes.Buffers;
using Model.Helpers.Logs;
using Model.Sudoku.Solver.BitSets;
using Model.Sudoku.Solver.Position;
using Model.Sudoku.Solver.Possibility;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.AlmostLockedSets;
using Model.Sudoku.Solver.Trackers;

namespace Model.Sudoku.Solver;

//TODO => Documentation + Explanation + Review highlighting for each strategy
//TODO => For each strategy using old als, revamp
public class SudokuSolver : IStrategyUser, ILogManagedChangeProducer, ISolveResult, ISolvingState
{
    private Sudoku _sudoku;
    private readonly ReadOnlyBitSet16[,] _possibilities = new ReadOnlyBitSet16[9, 9];
    private readonly GridPositions[] _positions = new GridPositions[9];
    private readonly LinePositions[,] _rowsPositions = new LinePositions[9, 9];
    private readonly LinePositions[,] _colsPositions = new LinePositions[9, 9];
    private readonly MiniGridPositions[,,] _minisPositions = new MiniGridPositions[3,3,9];
    
    public IReadOnlySudoku Sudoku => _sudoku;
    public IReadOnlyList<ISolverLog> Logs => LogManager.Logs;

    public bool UniquenessDependantStrategiesAllowed => StrategyManager.UniquenessDependantStrategiesAllowed;
    public bool LogsManaged { get; private set; }

    public IUpdatableSolvingState CurrentState => new ArraySolvingState(this);
    public IUpdatableSolvingState StartState { get; private set; }

    private int _currentStrategy = -1;
    
    private int _solutionAddedBuffer;
    private int _possibilityRemovedBuffer;
    private bool _startedSolving;

    private IChangeBuffer _changeBuffer;
    private readonly TrackerManager _trackerManager;
    
    public SudokuSolver() : this(new Sudoku()) { }

    private SudokuSolver(Sudoku s)
    {
        StrategyManager = new StrategyManager();
        _changeBuffer = new FastChangeBuffer(this);
        _trackerManager = new TrackerManager(this);
        
        _sudoku = s;
        CallOnNewSudokuForEachStrategy();

        InitPossibilities();
        StartState = new ArraySolvingState(this);

        PreComputer = new PreComputer(this);
        
        LogManager = new LogManager();

        AlmostHiddenSetSearcher = new AlmostHiddenSetSearcher(this);
        AlmostNakedSetSearcher = new AlmostNakedSetSearcher(this);
    }

    public void AddTracker(Tracker tracker)
    {
        _trackerManager.AddTracker(tracker);
    }

    public void RemoveTracker(Tracker tracker)
    {
        _trackerManager.RemoveTracker(tracker);
    }

    //Solver------------------------------------------------------------------------------------------------------------

    public StrategyManager StrategyManager { get; }

    public void SetSudoku(Sudoku s)
    {
        _sudoku = s;
        CallOnNewSudokuForEachStrategy();

        ResetPossibilities();
        StartState = new ArraySolvingState(this);

        LogManager.Clear();
        PreComputer.Reset();

        _startedSolving = false;
    }

    public void SetState(ArraySolvingState state)
    {
        _sudoku = SudokuTranslator.TranslateSolvingState(state);
        CallOnNewSudokuForEachStrategy();
        
        ResetPossibilities();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var at = state.Get(row, col);
                if (!at.IsPossibilities) continue;

                var asPoss = at.AsPossibilities;
                foreach (var p in PossibilitiesAt(row, col).EnumeratePossibilities())
                {
                    if (!asPoss.Contains(p)) RemovePossibility(p, row, col, false);
                }
            }
        }
        StartState = new ArraySolvingState(this);
        
        LogManager.Clear();
        PreComputer.Reset();

        _startedSolving = false;
    }
    
    public void SetSolutionByHand(int number, int row, int col)
    {
        if (_sudoku[row, col] != 0) RemoveSolution(row, col);

        if (_startedSolving && LogsManaged)
        {
            var stateBefore = CurrentState;
            if (!AddSolution(number, row, col, false)) return;
            LogManager.AddByHand(number, row, col, ProgressType.SolutionAddition, stateBefore);
        }
        else if (!AddSolution(number, row, col, false)) return;
        
        if(!_startedSolving) StartState = new ArraySolvingState(this);
    }

    public void RemoveSolutionByHand(int row, int col)
    {
        if (_startedSolving) return;

        RemoveSolution(row, col);
        StartState = new ArraySolvingState(this);
    }

    public void RemovePossibilityByHand(int possibility, int row, int col)
    {
        if (_startedSolving && LogsManaged)
        {
            var stateBefore = CurrentState;
            if (!RemovePossibility(possibility, row, col, false)) return;
            LogManager.AddByHand(possibility, row, col, ProgressType.PossibilityRemoval, stateBefore);
        }
        else if (!RemovePossibility(possibility, row, col, false)) return;

        if (!_startedSolving) StartState = new ArraySolvingState(this);
    }
    
    public void Solve(bool stopAtProgress = false)
    {
        _startedSolving = true;
        
        for (_currentStrategy = 0; _currentStrategy < StrategyManager.Strategies.Count; _currentStrategy++)
        {
            var current = StrategyManager.Strategies[_currentStrategy];
            if (!current.Enabled) continue;

            _trackerManager.OnStrategyStart(current, _currentStrategy);
            current.Apply(this);
            ChangeBuffer.Push(current);
            _trackerManager.OnStrategyEnd(current, _currentStrategy, _solutionAddedBuffer, _possibilityRemovedBuffer);

            if (_solutionAddedBuffer + _possibilityRemovedBuffer == 0) continue;

            _solutionAddedBuffer = 0;
            _possibilityRemovedBuffer = 0;
            
            _currentStrategy = -1;

            PreComputer.Reset();

            if (stopAtProgress || _sudoku.IsComplete()) break;
        }

        _currentStrategy = -1;
        _trackerManager.OnSolveDone(this);
    }
    
    public void Solve(OnCommitBehavior behavior)
    {
        _startedSolving = true;
        
        for (_currentStrategy = 0; _currentStrategy < StrategyManager.Strategies.Count; _currentStrategy++)
        {
            var current = StrategyManager.Strategies[_currentStrategy];
            if (!current.Enabled) continue;
            
            var old = current.OnCommitBehavior;
            current.OnCommitBehavior = OnCommitBehavior.WaitForAll;

            _trackerManager.OnStrategyStart(current, _currentStrategy);
            current.Apply(this);
            ChangeBuffer.Push(current);
            _trackerManager.OnStrategyEnd(current, _currentStrategy, _solutionAddedBuffer, _possibilityRemovedBuffer);

            current.OnCommitBehavior = old;

            if (_solutionAddedBuffer + _possibilityRemovedBuffer == 0) continue;

            _solutionAddedBuffer = 0;
            _possibilityRemovedBuffer = 0;
            
            _currentStrategy = -1;

            PreComputer.Reset();

            if (_sudoku.IsComplete()) break;
        }

        _currentStrategy = -1;
        _trackerManager.OnSolveDone(this);
    }

    public BuiltChangeCommit[] EveryPossibleNextStep()
    {
        var oldBuffer = ChangeBuffer;
        ChangeBuffer = new NotExecutedChangeBuffer(this);
        
        for (_currentStrategy = 0; _currentStrategy < StrategyManager.Strategies.Count; _currentStrategy++)
        {
            var current = StrategyManager.Strategies[_currentStrategy];
            if (!current.Enabled) continue;

            var behavior = current.OnCommitBehavior;
            current.OnCommitBehavior = OnCommitBehavior.WaitForAll;
            
            _trackerManager.OnStrategyStart(current, _currentStrategy);
            current.Apply(this);
            ChangeBuffer.Push(current);
            _trackerManager.OnStrategyEnd(current, _currentStrategy, _solutionAddedBuffer, _possibilityRemovedBuffer);

            current.OnCommitBehavior = behavior;
            _solutionAddedBuffer = 0;
            _possibilityRemovedBuffer = 0;
        }
        
        _currentStrategy = -1;

        var result = ((NotExecutedChangeBuffer)ChangeBuffer).DumpCommits();
        ChangeBuffer = oldBuffer;
        return result;
    }
    
    public void ApplyCommit(BuiltChangeCommit commit)
    {
        ChangeBuffer.PushCommit(commit);
    }

    public bool IsWrong()
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
    
    //PossibilityHolder-------------------------------------------------------------------------------------------------

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

    //StrategyManager---------------------------------------------------------------------------------------------------

    public IChangeBuffer ChangeBuffer
    {
        get => _changeBuffer;
        set
        {
            _changeBuffer = value;
            LogsManaged = value.HandlesLog;
        } 
    }
    public PreComputer PreComputer { get; }
    public AlmostHiddenSetSearcher AlmostHiddenSetSearcher { get; }
    public AlmostNakedSetSearcher AlmostNakedSetSearcher { get; }

    //ChangeManager-----------------------------------------------------------------------------------------------------
    
    public IPossibilitiesHolder TakeSnapshot()
    {
        return SolverSnapshot.TakeSnapshot(this);
    }

    public bool CanRemovePossibility(CellPossibility cp)
    {
        return PossibilitiesAt(cp.Row, cp.Column).Contains(cp.Possibility);
    }

    public bool CanAddSolution(CellPossibility cp)
    {
        return Sudoku[cp.Row, cp.Column] == 0;
    }

    public bool ExecuteChange(SolverProgress progress)
    {
        return progress.ProgressType == ProgressType.SolutionAddition
            ? AddSolution(progress.Number, progress.Row, progress.Column)
            : RemovePossibility(progress.Number, progress.Row, progress.Column);
    }

    public LogManager LogManager { get; }

    //Private-----------------------------------------------------------------------------------------------------------

    private void CallOnNewSudokuForEachStrategy()
    {
        foreach (var s in StrategyManager.Strategies)
        {
            s.OnNewSudoku(_sudoku);
        }
    }
    
    private bool AddSolution(int number, int row, int col, bool callEvents = true)
    {
        if (!_possibilities[row, col].Contains(number)) return false;
        
        _sudoku[row, col] = number;
        UpdatePossibilitiesAfterSolutionAdded(number, row, col);
        
        if(callEvents) _solutionAddedBuffer++;
        return true;
    }

    private void RemoveSolution(int row, int col)
    {
        if (_sudoku[row, col] == 0) return;

        _sudoku[row, col] = 0;
        ResetPossibilities();
    }

    private bool RemovePossibility(int possibility, int row, int col, bool callEvents = true)
    {
        if (!_possibilities[row, col].Contains(possibility)) return false;

        _possibilities[row, col] -= possibility;
        _positions[possibility - 1].Remove(row, col);
        _rowsPositions[row, possibility - 1].Remove(col);
        _colsPositions[col, possibility - 1].Remove(row);
        _minisPositions[row / 3, col / 3, possibility - 1].Remove(row % 3, col % 3);
        
        if(callEvents) _possibilityRemovedBuffer++;
        return true;
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
        }
        
        for (int i = 0; i < 9; i++)
        {
            RemovePossibilityCheckLess(number, row, i);
            RemovePossibilityCheckLess(number, i, col);
            RemovePossibilityCheckLess(number,  startRow + i / 3, startCol + i % 3);
        }
    }
    
    private void RemovePossibilityCheckLess(int possibility, int row, int col)
    {
        _possibilities[row, col] -= possibility;
        _positions[possibility - 1].Remove(row, col);
        _rowsPositions[row, possibility - 1].Remove(col);
        _colsPositions[col, possibility - 1].Remove(row);
        _minisPositions[row / 3, col / 3, possibility - 1].Remove(row % 3, col % 3);
    }

    public IEnumerable<int> EnumeratePossibilitiesAt(int row, int col)
    {
        return _possibilities[row, col].EnumeratePossibilities();
    }
}

