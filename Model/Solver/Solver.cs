using System.Collections.Generic;
using System.Threading.Tasks;
using Model.Solver.Helpers;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Logs;
using Model.Solver.Positions;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil.AlmostLockedSets;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver;

//TODO => Documentation + Explanation + Review highlighting for each strategy
public class Solver : IStrategyManager, IChangeManager, ILogHolder
{
    private Sudoku _sudoku;
    private readonly IPossibilities[,] _possibilities = new IPossibilities[9, 9];
    private readonly GridPositions[] _positions = new GridPositions[9];
    
    public IReadOnlySudoku Sudoku => _sudoku;
    public IStrategy[] Strategies { get; }
    public StrategyInfo[] StrategyInfos => GetStrategyInfo();
    public List<ISolverLog> Logs => LogManager.Logs;
    
    public bool LogsManaged
    {
        get => LogManager.IsEnabled;
        init => LogManager.IsEnabled = value;
    }
    public bool StatisticsTracked { get; init; }
    public bool UniquenessDependantStrategiesAllowed { get; private set; } = true;

    public SolverState State => new(this);
    public SolverState StartState { get; private set; }

    public delegate void SolutionAddition(int number, int row, int col);
    public event SolutionAddition? GoingToAddSolution;

    public delegate void PossibilityRemoval(int number, int row, int col);
    public event PossibilityRemoval? GoingToRemovePossibility;

    public delegate void OnCurrentStrategyChange(int index);
    public event OnCurrentStrategyChange? CurrentStrategyChanged;

    public event LogManager.OnLogsUpdated? LogsUpdated;

    private int _currentStrategy = -1;
    private ulong _excludedStrategies;
    private ulong _lockedStrategies;
    
    private int _solutionAddedBuffer;
    private int _possibilityRemovedBuffer;

    private readonly StrategyLoader _strategyLoader = new();

    public Solver() : this(new Sudoku()) { }

    public Solver(Sudoku s)
    {
        _strategyLoader.Load();
        Strategies = _strategyLoader.Strategies;
        _excludedStrategies = _strategyLoader.ExcludedStrategies;
        
        _sudoku = s;
        CallOnNewSudokuForEachStrategy();

        GoingToAddSolution += (_, _, _) => _solutionAddedBuffer++;
        GoingToRemovePossibility += (_, _, _) => _possibilityRemovedBuffer++;

        Init();
        StartState = new SolverState(this);

        PreComputer = new PreComputer(this);

        GraphManager = new LinkGraphManager(this);
        
        LogManager = new LogManager(this);
        LogManager.LogsUpdated += logs => LogsUpdated?.Invoke(logs);
        
        ChangeBuffer = new ChangeBuffer(this);

        AlmostHiddenSetSearcher = new AlmostHiddenSetSearcher(this);
        AlmostLockedSetSearcher = new AlmostLockedSetSearcher(this);
    }

    //Solver------------------------------------------------------------------------------------------------------------
    
    public void SetSudoku(Sudoku s)
    {
        _sudoku = s;
        CallOnNewSudokuForEachStrategy();

        Reset();
        StartState = new SolverState(this);

        LogManager.Clear();
        //PreComputer.Reset();
        //GraphManager.Clear();
    }
    
    
    public void SetSolutionByHand(int number, int row, int col) //TODO rework the by hand methods and add remove solution
    {
        if (!_possibilities[row, col].Peek(number)) return;
        
        _sudoku[row, col] = number;
        Reset();
        LogManager.Clear();
    }

    public void RemovePossibilityByHand(int possibility, int row, int col)
    {
        if (!_possibilities[row, col].Remove(possibility)) return;
        
        _positions[possibility - 1].Remove(row, col);
        LogManager.AddByHand(possibility, row, col);
    }
    
    public void Solve(bool stopAtProgress = false)
    {
        for (_currentStrategy = 0; _currentStrategy < Strategies.Length; _currentStrategy++)
        {
            if (!IsStrategyUsed(_currentStrategy)) continue;

            CurrentStrategyChanged?.Invoke(_currentStrategy);
            var current = Strategies[_currentStrategy];

            if (StatisticsTracked) current.Tracker.StartUsing();
            current.ApplyOnce(this);
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

    public Task SolveAsync(bool stopAtProgress = false)
    {
        return Task.Run(() => Solve(stopAtProgress));
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
        if (IsStrategyLocked(number)) return;
        _excludedStrategies |= 1ul << number;
    }

    public void ExcludeStrategies(StrategyDifficulty difficulty)
    {
        for (int i = 0; i < Strategies.Length; i++)
        {
            if (Strategies[i].Difficulty == difficulty && !IsStrategyLocked(i)) ExcludeStrategy(i);
        }
    }

    public void UseStrategy(int number)
    {
        if (IsStrategyLocked(number)) return;
        _excludedStrategies &= ~(1ul << number);
    }

    public bool IsStrategyUsed(int number)
    {
        return ((_excludedStrategies >> number) & 1) == 0;
    }

    public bool IsStrategyLocked(int number)
    {
        return ((_lockedStrategies >> number) & 1) > 0;
    }

    public void AllowUniqueness(bool yes)
    {
        UniquenessDependantStrategiesAllowed = yes;
        
        for (int i = 0; i < Strategies.Length; i++)
        {
            if (Strategies[i].UniquenessDependency != UniquenessDependency.FullyDependent) continue;
            
            if (yes) UnLockStrategy(i);
            else
            {
                ExcludeStrategy(i);
                LockStrategy(i);
            }
        }
    }

    public void SetOnInstanceFound(OnInstanceFound found)
    {
        foreach (var strategy in Strategies)
        {
            switch (found)
            {
                case OnInstanceFound.Return :
                    strategy.OnCommitBehavior = OnCommitBehavior.Return;
                    break;
                case OnInstanceFound.WaitForAll :
                    strategy.OnCommitBehavior = OnCommitBehavior.WaitForAll;
                    break;
                case OnInstanceFound.ChooseBest :
                    strategy.OnCommitBehavior = OnCommitBehavior.ChooseBest;
                    break;
                case OnInstanceFound.Default :
                    strategy.OnCommitBehavior = strategy.DefaultOnCommitBehavior;
                    break;
                case OnInstanceFound.Customized :
                    strategy.OnCommitBehavior = _strategyLoader.CustomizedOnInstanceFound.TryGetValue(strategy.Name,
                        out var behavior) ? behavior : strategy.DefaultOnCommitBehavior;

                    break;
            }
        }
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
    public AlmostLockedSetSearcher AlmostLockedSetSearcher { get; }

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
    
    private bool AddSolution(int number, int row, int col)
    {
        if (_sudoku[row, col] != 0 || !_possibilities[row, col].Peek(number)) return false;
        
        GoingToAddSolution?.Invoke(number, row, col);
        _sudoku[row, col] = number;
        UpdateAfterSolutionAdded(number, row, col);
        
        return true;
    }

    private bool RemovePossibility(int possibility, int row, int col)
    {
        if (!_possibilities[row, col].Remove(possibility)) return false;
        
        GoingToRemovePossibility?.Invoke(possibility, row, col);
        _positions[possibility - 1].Remove(row, col);
        
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
                _possibilities[i, j] = IPossibilities.New();
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

    private StrategyInfo[] GetStrategyInfo()
    {
        StrategyInfo[] result = new StrategyInfo[Strategies.Length];

        for (int i = 0; i < Strategies.Length; i++)
        {
            result[i] = new StrategyInfo(Strategies[i], IsStrategyUsed(i), IsStrategyLocked(i));
        }

        return result;
    }

    private void LockStrategy(int n)
    {
        _lockedStrategies |= 1ul << n;
    }

    private void UnLockStrategy(int n)
    {
        _lockedStrategies &= ~(1ul << n);
    }
}

public class StrategyInfo
{
    public string StrategyName { get; }
    public StrategyDifficulty Difficulty { get; }
    public bool Used { get; }
    public bool Locked { get; }

    public StrategyInfo(IStrategy strategy, bool used, bool locked)
    {
        StrategyName = strategy.Name;
        Difficulty = strategy.Difficulty;
        Used = used;
        Locked = locked;
    }
}

public enum OnInstanceFound
{
    Default, Return, WaitForAll, ChooseBest, Customized
}

