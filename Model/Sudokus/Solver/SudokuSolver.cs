using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Changes.Buffers;
using Model.Helpers.Highlighting;
using Model.Helpers.Steps;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.States;
using Model.Sudokus.Solver.Trackers;
using Model.Sudokus.Solver.Utility.AlmostLockedSets;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver;

//TODO => Documentation + Explanation + Review highlighting for each strategy
//TODO => For each strategy using old als, revamp
public class SudokuSolver : ISudokuStrategyUser, IStepManagingChangeProducer<IUpdatableSudokuSolvingState, ISudokuHighlighter>, ISolveResult
{
    private Sudoku _sudoku;
    private readonly ReadOnlyBitSet16[,] _possibilities = new ReadOnlyBitSet16[9, 9];
    private readonly GridPositions[] _positions = new GridPositions[9];
    private readonly LinePositions[,] _rowsPositions = new LinePositions[9, 9];
    private readonly LinePositions[,] _colsPositions = new LinePositions[9, 9];
    private readonly MiniGridPositions[,,] _minisPositions = new MiniGridPositions[3,3,9];
    
    public IReadOnlySudoku Sudoku => _sudoku;

    public bool UniquenessDependantStrategiesAllowed => StrategyManager.UniquenessDependantStrategiesAllowed;
    public bool StepsManaged { get; private set; }

    private IUpdatableSudokuSolvingState? _currentState;

    public IUpdatableSudokuSolvingState CurrentState
    {
        get
        {
            _currentState ??= new StateArraySolvingState(this);
            return _currentState;
        }
    }
    public IUpdatableSolvingState StartState { get; private set; }
    
    private int _solutionAddedBuffer;
    private int _possibilityRemovedBuffer;
    private bool _changeWasMade;
    
    public bool StartedSolving { get; private set; }

    private IChangeBuffer<IUpdatableSudokuSolvingState, ISudokuHighlighter> _changeBuffer;
    private readonly TrackerManager _trackerManager;

    public SudokuSolver() : this(new Sudoku()) { }

    private SudokuSolver(Sudoku s)
    {
        StrategyManager = new StrategyManager();
        _changeBuffer = new FastChangeBuffer<IUpdatableSudokuSolvingState, ISudokuHighlighter>(this);
        _trackerManager = new TrackerManager(this);
        
        _sudoku = s;
        CallOnNewSudokuForEachStrategy();

        InitPossibilities();
        StartState = new StateArraySolvingState(this);

        PreComputer = new PreComputer(this);
        
        StepHistory = new StepHistory<ISudokuHighlighter>();

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

    public StrategyManager StrategyManager { get; init; }

    public void SetSudoku(Sudoku s)
    {
        _sudoku = s;
        CallOnNewSudokuForEachStrategy();

        ResetPossibilities();
        StartState = new StateArraySolvingState(this);

        StepHistory.Clear();
        PreComputer.Reset();

        StartedSolving = false;
    }

    public void SetState(ISolvingState state)
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
                    if (!asPoss.Contains(p)) RemovePossibility(p, row, col, false);
                }
            }
        }
        StartState = new StateArraySolvingState(this);
        
        StepHistory.Clear();
        PreComputer.Reset();

        StartedSolving = false;
    }
    
    public void SetSolutionByHand(int number, int row, int col)
    {
        if (_sudoku[row, col] != 0) RemoveSolution(row, col);

        var before = CurrentState;
        if (!AddSolution(number, row, col, false)) return;
        
        if(!StartedSolving) StartState = new StateArraySolvingState(this);
        else if (StepsManaged) StepHistory.AddByHand(number, row, col, ProgressType.SolutionAddition, before);
    }

    public void RemoveSolutionByHand(int row, int col)
    {
        if (StartedSolving) return;

        RemoveSolution(row, col);
        StartState = new StateArraySolvingState(this);
    }

    public void RemovePossibilityByHand(int possibility, int row, int col)
    {
        if (StartedSolving && StepsManaged)
        {
            var stateBefore = CurrentState;
            if (!RemovePossibility(possibility, row, col, false)) return;
            StepHistory.AddByHand(possibility, row, col, ProgressType.PossibilityRemoval, stateBefore);
        }
        else if (!RemovePossibility(possibility, row, col, false)) return;

        if (!StartedSolving) StartState = new StateArraySolvingState(this);
    }
    
    public void Solve(bool stopAtProgress = false)
    {
        StartedSolving = true;
        _trackerManager.OnSolveStart();
        
        for (int i = 0; i < StrategyManager.Strategies.Count; i++)
        {
            var current = StrategyManager.Strategies[i];
            if (!current.Enabled) continue;

            _trackerManager.OnStrategyStart(current, i);
            current.Apply(this);
            ChangeBuffer.Push(current);
            _trackerManager.OnStrategyEnd(current, i, _solutionAddedBuffer, _possibilityRemovedBuffer);

            if (!_changeWasMade) continue;

            ResetChangeTrackingVariables();
            i = -1;
            PreComputer.Reset();

            if (stopAtProgress || _sudoku.IsComplete()) break; //TODO optimize isComplete with buffer with abstract class ?
        }

        _trackerManager.OnSolveDone(this);
    }

    public BuiltChangeCommit<ISudokuHighlighter>[] EveryPossibleNextStep()
    {
        var oldBuffer = ChangeBuffer;
        ChangeBuffer = new NotExecutedChangeBuffer<IUpdatableSudokuSolvingState, ISudokuHighlighter>(this);
        
        for (int i = 0; i < StrategyManager.Strategies.Count; i++)
        {
            var current = StrategyManager.Strategies[i];
            if (!current.Enabled) continue;

            var handling = current.InstanceHandling;
            current.InstanceHandling = InstanceHandling.UnorderedAll; //TODO to track manager
            
            _trackerManager.OnStrategyStart(current, i);
            current.Apply(this);
            ChangeBuffer.Push(current);
            _trackerManager.OnStrategyEnd(current, i, _solutionAddedBuffer, _possibilityRemovedBuffer);

            current.InstanceHandling = handling;
            ResetChangeTrackingVariables();
        }
        
        var result = ((NotExecutedChangeBuffer<IUpdatableSudokuSolvingState,
            ISudokuHighlighter>)ChangeBuffer).DumpCommits();
        ChangeBuffer = oldBuffer;
        return result;
    }

    public Clue<ISudokuHighlighter>? NextClue()
    {
        var oldBuffer = ChangeBuffer;
        ChangeBuffer = new ClueGetterChangeBuffer<IUpdatableSudokuSolvingState, ISudokuHighlighter>(this);
        
        for (int i = 0; i < StrategyManager.Strategies.Count; i++)
        {
            var current = StrategyManager.Strategies[i];
            if (!current.Enabled) continue;

            _trackerManager.OnStrategyStart(current, i);
            current.Apply(this);
            ChangeBuffer.Push(current);
            _trackerManager.OnStrategyEnd(current, i, _solutionAddedBuffer, _possibilityRemovedBuffer);

            if(!_changeWasMade) continue;
            
            ResetChangeTrackingVariables();
            break;
        }

        var result = ((ClueGetterChangeBuffer<IUpdatableSudokuSolvingState,
            ISudokuHighlighter>)ChangeBuffer).CurrentClue;
        ChangeBuffer = oldBuffer;
        return result;
    }
    
    public void ApplyCommit(BuiltChangeCommit<ISudokuHighlighter> commit)
    {
        StartedSolving = true;
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

    public IChangeBuffer<IUpdatableSudokuSolvingState, ISudokuHighlighter> ChangeBuffer
    {
        get => _changeBuffer;
        set
        {
            _changeBuffer = value;
            StepsManaged = value.IsManagingSteps;
        } 
    }
    public PreComputer PreComputer { get; }
    public AlmostHiddenSetSearcher AlmostHiddenSetSearcher { get; }
    public AlmostNakedSetSearcher AlmostNakedSetSearcher { get; }
    public IEnumerable<int> EnumeratePossibilitiesAt(int row, int col)
    {
        return _possibilities[row, col].EnumeratePossibilities();
    }

    #endregion

    #region ChangeManager

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

    public void FakeChange()
    {
        _changeWasMade = true;
    }

    public StepHistory<ISudokuHighlighter> StepHistory { get; }

    #endregion

    #region PossibilityAndSolutionSetters

    private bool AddSolution(int number, int row, int col, bool fromSolving = true)
    {
        if (!_possibilities[row, col].Contains(number)) return false;
        
        _currentState = null;
        _sudoku[row, col] = number;
        UpdatePossibilitiesAfterSolutionAdded(number, row, col);

        if (fromSolving)
        {
            _solutionAddedBuffer++;
            _changeWasMade = true;
        }
        return true;
    }

    private void RemoveSolution(int row, int col)
    {
        if (_sudoku[row, col] == 0) return;

        _currentState = null;
        _sudoku[row, col] = 0;
        ResetPossibilities();
    }

    private bool RemovePossibility(int possibility, int row, int col, bool fromSolving = true)
    {
        if (!_possibilities[row, col].Contains(possibility)) return false;

        _currentState = null;
        _possibilities[row, col] -= possibility;
        _positions[possibility - 1].Remove(row, col);
        _rowsPositions[row, possibility - 1].Remove(col);
        _colsPositions[col, possibility - 1].Remove(row);
        _minisPositions[row / 3, col / 3, possibility - 1].Remove(row % 3, col % 3);

        if (fromSolving)
        {
            _possibilityRemovedBuffer++;
            _changeWasMade = true;
        }
        return true;
    }
    
    private void RemovePossibilityCheckLess(int possibility, int row, int col)
    {
        _currentState = null;
        
        _possibilities[row, col] -= possibility;
        _positions[possibility - 1].Remove(row, col);
        _rowsPositions[row, possibility - 1].Remove(col);
        _colsPositions[col, possibility - 1].Remove(row);
        _minisPositions[row / 3, col / 3, possibility - 1].Remove(row % 3, col % 3);
    }

    #endregion

    #region Private

    private void ResetChangeTrackingVariables()
    {
        _possibilityRemovedBuffer = 0;
        _solutionAddedBuffer = 0;
        _changeWasMade = false;
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

