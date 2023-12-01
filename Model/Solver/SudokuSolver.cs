using System.Collections.Generic;
using Global.Enums;
using Model.Solver.Helpers;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Logs;
using Model.Solver.Position;
using Model.Solver.Possibility;
using Model.Solver.StrategiesUtility.AlmostLockedSets;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver;

//TODO => Documentation + Explanation + Review highlighting for each strategy
//BIG TODO => For each strategy using old als, revamp
public class SudokuSolver : ISolver, IStrategyManager, IChangeManager, ILogHolder
{
    private Sudoku _sudoku;
    private readonly Possibilities[,] _possibilities = new Possibilities[9, 9];
    private readonly GridPositions[] _positions = new GridPositions[9];
    
    public IReadOnlySudoku Sudoku => _sudoku;
    public IReadOnlyList<ISolverLog> Logs => LogManager.Logs;
    
    public bool LogsManaged
    {
        get => LogManager.IsEnabled;
        init => LogManager.IsEnabled = value;
    }
    public bool StatisticsTracked { get; init; }
    public bool UniquenessDependantStrategiesAllowed { get; private set; } = true;

    public SolverState CurrentState => new(this);
    public SolverState StartState { get; private set; }

    public delegate void SolutionAddition(int number, int row, int col);
    public event SolutionAddition? SolutionAdded;

    public delegate void PossibilityRemoval(int number, int row, int col);
    public event PossibilityRemoval? PossibilityRemoved;
    
    public event OnCurrentStrategyChange? CurrentStrategyChanged;

    public event OnLogsUpdate? LogsUpdated;

    private IReadOnlyList<IStrategy> Strategies => _strategyLoader.Strategies;
    private int _currentStrategy = -1;
    
    private int _solutionAddedBuffer;
    private int _possibilityRemovedBuffer;

    private bool _startedSolving;

    private readonly StrategyLoader _strategyLoader;

    public SudokuSolver(IRepository<List<StrategyDAO>> repository) : this(new Sudoku(), repository) { }

    private SudokuSolver(Sudoku s, IRepository<List<StrategyDAO>> repository)
    {
        _strategyLoader = new StrategyLoader(repository);
        _strategyLoader.Load();
        
        _sudoku = s;
        CallOnNewSudokuForEachStrategy();

        SolutionAdded += (_, _, _) => _solutionAddedBuffer++;
        PossibilityRemoved += (_, _, _) => _possibilityRemovedBuffer++;

        Init();
        StartState = new SolverState(this);

        PreComputer = new PreComputer(this);

        GraphManager = new LinkGraphManager(this);
        
        LogManager = new LogManager(this);
        LogManager.LogsUpdated += () => LogsUpdated?.Invoke();
        
        ChangeBuffer = new ChangeBuffer(this);

        AlmostHiddenSetSearcher = new AlmostHiddenSetSearcher(this);
        AlmostNakedSetSearcher = new AlmostNakedSetSearcher(this);
    }

    //Solver------------------------------------------------------------------------------------------------------------

    public IStrategyLoader StrategyLoader => _strategyLoader;

    public void SetSudoku(Sudoku s)
    {
        _sudoku = s;
        CallOnNewSudokuForEachStrategy();

        Reset();
        StartState = new SolverState(this);

        LogManager.Clear();

        _startedSolving = false;
    }

    public void SetState(SolverState state)
    {
        _sudoku = SudokuTranslator.TranslateToSudoku(state);
        CallOnNewSudokuForEachStrategy();
        
        Reset();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var at = state.At(row, col);
                if (!at.IsPossibilities) continue;

                var asPoss = at.AsPossibilities;
                foreach (var p in PossibilitiesAt(row, col))
                {
                    if (!asPoss.Peek(p)) RemovePossibility(p, row, col, false);
                }
            }
        }
        StartState = new SolverState(this);

        LogManager.Clear();

        _startedSolving = false;
    }
    
    public void SetSolutionByHand(int number, int row, int col)
    {
        if (!_startedSolving && _sudoku[row, col] != 0) RemoveSolution(row, col);
        if (!AddSolution(number, row, col, false)) return;

        if (_startedSolving) LogManager.AddByHand(number, row, col, ChangeType.Solution);
        else StartState = new SolverState(this);
    }

    public void RemoveSolutionByHand(int row, int col)
    {
        if (_startedSolving) return;

        RemoveSolution(row, col);
        StartState = new SolverState(this);
    }

    public void RemovePossibilityByHand(int possibility, int row, int col)
    {
        if (!_startedSolving) return;

        if (!RemovePossibility(possibility, row, col, false)) return;

        LogManager.AddByHand(possibility, row, col, ChangeType.Possibility);
    }
    
    public void Solve(bool stopAtProgress = false)
    {
        _startedSolving = true;
        
        for (_currentStrategy = 0; _currentStrategy < Strategies.Count; _currentStrategy++)
        {
            if (!_strategyLoader.IsStrategyUsed(_currentStrategy)) continue;

            CurrentStrategyChanged?.Invoke(_currentStrategy);
            var current = Strategies[_currentStrategy];

            if (StatisticsTracked) current.Tracker.StartUsing();
            current.Apply(this);
            ChangeBuffer.Push(current);
            if (StatisticsTracked) current.Tracker.StopUsing(_solutionAddedBuffer, _possibilityRemovedBuffer);

            if (_solutionAddedBuffer + _possibilityRemovedBuffer == 0) continue;

            _solutionAddedBuffer = 0;
            _possibilityRemovedBuffer = 0;
            
            _currentStrategy = -1;
            CurrentStrategyChanged?.Invoke(_currentStrategy);

            PreComputer.Reset();
            GraphManager.Clear();

            if (stopAtProgress || _sudoku.IsComplete()) return;
        }

        _currentStrategy = -1;
        CurrentStrategyChanged?.Invoke(_currentStrategy);
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

    public void ExcludeStrategy(int number)
    {
        _strategyLoader.ExcludeStrategy(number);
    }

    public void UseStrategy(int number)
    {
        _strategyLoader.UseStrategy(number);
    }

    public void AllowUniqueness(bool yes)
    {
        UniquenessDependantStrategiesAllowed = yes;
        _strategyLoader.AllowUniqueness(yes);
    }

    public string FullScan()
    {
        var oldBehavior = new List<OnCommitBehavior>();
        
        for (int i = 0; i < Strategies.Count; i++)
        {
            var current = Strategies[i];
            oldBehavior.Add(current.OnCommitBehavior);
            if (!_strategyLoader.IsStrategyUsed(i)) continue;

            current.OnCommitBehavior = OnCommitBehavior.WaitForAll;
            current.Apply(this);

            if (_sudoku.IsComplete()) break;
        }

        for (int i = 0; i < oldBehavior.Count; i++)
        {
            Strategies[i].OnCommitBehavior = oldBehavior[i];
        }
        
        return "FULL SCAN :\n\n" + SudokuTranslator.TranslateToGrid(CurrentState) + "\n" + ChangeBuffer.CommitDump();
    }
    
    public StrategyInformation[] GetStrategyInfo()
    {
        return _strategyLoader.GetStrategiesInformation();
    }
    
    //PossibilityHolder-------------------------------------------------------------------------------------------------
    
    public IReadOnlyPossibilities PossibilitiesAt(int row, int col)
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

    //StrategyManager---------------------------------------------------------------------------------------------------

    public ChangeBuffer ChangeBuffer { get; }
    public LinkGraphManager GraphManager { get; }
    public PreComputer PreComputer { get; }
    public AlmostHiddenSetSearcher AlmostHiddenSetSearcher { get; }
    public AlmostNakedSetSearcher AlmostNakedSetSearcher { get; }

    //ChangeManager-----------------------------------------------------------------------------------------------------
    
    public IPossibilitiesHolder TakeSnapshot()
    {
        return SolverSnapshot.TakeSnapshot(this);
    }

    public bool ExecuteChange(SolverChange change)
    {
        return change.ChangeType == ChangeType.Solution
            ? AddSolution(change.Number, change.Row, change.Column)
            : RemovePossibility(change.Number, change.Row, change.Column);
    }

    public LogManager LogManager { get; }

    //Private-----------------------------------------------------------------------------------------------------------

    private void CallOnNewSudokuForEachStrategy()
    {
        foreach (var s in Strategies)
        {
            s.OnNewSudoku(_sudoku);
        }
    }
    
    private bool AddSolution(int number, int row, int col, bool callEvents = true)
    {
        if (!_possibilities[row, col].Peek(number)) return false;
        
        _sudoku[row, col] = number;
        UpdateAfterSolutionAdded(number, row, col);
        if(callEvents) SolutionAdded?.Invoke(number, row, col);
        
        return true;
    }

    private void RemoveSolution(int row, int col)
    {
        if (_sudoku[row, col] == 0) return;

        _sudoku[row, col] = 0;
        Reset();
    }

    private bool RemovePossibility(int possibility, int row, int col, bool callEvents = true)
    {
        if (!_possibilities[row, col].Remove(possibility)) return false;
        
        _positions[possibility - 1].Remove(row, col);
        if(callEvents) PossibilityRemoved?.Invoke(possibility, row, col);
        
        return true;
    }

    private void Init()
    {
        InitPossibilities();
        InitPositions();
    }

    private void Reset()
    {
        ResetPossibilities();
        ResetPositions();
    }

    private void UpdateAfterSolutionAdded(int number, int row, int col)
    {
        UpdatePossibilitiesAfterSolutionAdded(number, row, col);
        UpdatePositionsAfterSolutionAdded(number, row, col);
    }

    private void InitPossibilities()
    {
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                _possibilities[i, j] = new Possibilities();
            }
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
                _possibilities[i, j].Reset();
            }
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
        _possibilities[row, col].RemoveAll();
        
        for (int i = 0; i < 9; i++)
        {
            _possibilities[row, i].Remove(number);
            _possibilities[i, col].Remove(number);
        }
        
        int startRow = row / 3 * 3;
        int startColumn = col / 3 * 3;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                _possibilities[startRow + i, startColumn + j].Remove(number);
            }
        }
    }
    
    private void InitPositions()
    {
        for (int i = 0; i < 9; i++)
        {
            _positions[i] = new GridPositions();
            _positions[i].Fill();
        }
        
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (_sudoku[i, j] != 0)
                {
                    UpdatePositionsAfterSolutionAdded(_sudoku[i, j], i, j);
                }
            }
        }
    }

    private void ResetPositions()
    {
        for (int i = 0; i < 9; i++)
        {
            _positions[i].Fill();
        }
        
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (_sudoku[i, j] != 0)
                {
                    UpdatePositionsAfterSolutionAdded(_sudoku[i, j], i, j);
                }
            }
        }
    }

    private void UpdatePositionsAfterSolutionAdded(int number, int row, int col)
    {
        for (int i = 0; i < 9; i++)
        {
            _positions[i].Remove(row, col);
        }
        
        var pos = _positions[number - 1];
        pos.VoidRow(row);
        pos.VoidColumn(col);
        pos.VoidMiniGrid(row / 3, col / 3);
    }
}

