using System.Collections.Generic;
using System.Threading.Tasks;
using Model.Solver.Helpers;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Logs;
using Model.Solver.Positions;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver;

public class Solver : IStrategyManager, IChangeManager, ILogHolder
{
    private Sudoku _sudoku;
    private readonly IPossibilities[,] _possibilities = new IPossibilities[9, 9];
    private readonly GridPositions[] _positions = new GridPositions[9];
    
    public IReadOnlySudoku Sudoku => _sudoku;
    public IStrategy[] Strategies { get; }
    public StrategyInfo[] StrategyInfos => GetStrategyInfo();
    public List<ISolverLog> Logs => _logManager.Logs;

    public bool LogsManaged { get; init; } = true;
    public bool StatisticsTracked { get; init; }
    public bool UniquenessDependantStrategiesAllowed { get; private set; } = true;

    public SolverState State => new(this);
    public SolverState StartState => _logManager.StartState;

    public delegate void OnNumberAdded(int row, int col);
    public event OnNumberAdded? NumberAdded;

    public delegate void OnPossibilityRemoved(int row, int col);
    public event OnPossibilityRemoved? PossibilityRemoved;

    public event LogManager.OnLogsUpdate? LogsUpdated;

    private int _currentStrategy = -1;
    private ulong _excludedStrategies;
    private ulong _lockedStrategies;
    private bool _changeWasMade;

    private readonly LogManager _logManager;

    public Solver(Sudoku s)
    {
        var strategyLoader = new StrategyLoader();
        strategyLoader.Load();
        Strategies = strategyLoader.Strategies;
        _excludedStrategies = strategyLoader.ExcludedStrategies;
        
        _sudoku = s;
        CallOnNewSudokuForEachStrategy();

        NumberAdded += (_, _) => _changeWasMade = true;
        PossibilityRemoved += (_, _) => _changeWasMade = true;

        Init();

        PreComputer = new PreComputer(this);

        GraphManager = new LinkGraphManager(this);
        
        _logManager = new LogManager(this);
        _logManager.LogsUpdated += logs => LogsUpdated?.Invoke(logs);
        
        ChangeBuffer = new ChangeBuffer(this);
    }

    //Solver------------------------------------------------------------------------------------------------------------
    
    public void SetSudoku(Sudoku s)
    {
        _sudoku = s;
        CallOnNewSudokuForEachStrategy();

        Reset();

        if (LogsManaged) _logManager.Clear();

        PreComputer.Reset();
    }
    
    
    public void SetSolutionByHand(int number, int row, int col)
    {
        _sudoku[row, col] = number;
        Reset();
        
        if(LogsManaged) _logManager.Clear();
    }

    public void RemovePossibilityByHand(int possibility, int row, int col)
    {
        if (!_possibilities[row, col].Remove(possibility)) return;
        _positions[possibility - 1].Remove(row, col);
        
        if (LogsManaged) _logManager.PossibilityRemovedByHand(possibility, row, col);
    }
    
    public void Solve(bool stopAtProgress = false)
    {
        for (_currentStrategy = 0; _currentStrategy < Strategies.Length; _currentStrategy++)
        {
            if (_sudoku.IsComplete()) return;
            if (!IsStrategyUsed(_currentStrategy)) continue;
            
            if (StatisticsTracked) Strategies[_currentStrategy].Tracker.StartUsing();
            Strategies[_currentStrategy].ApplyOnce(this);
            if (StatisticsTracked) Strategies[_currentStrategy].Tracker.StopUsing(_changeWasMade);

            if (!_changeWasMade) continue;
            
            _changeWasMade = false;
            _currentStrategy = -1;
            PreComputer.Reset();
            GraphManager.Clear();
            if(LogsManaged) _logManager.Push();

            if (stopAtProgress) return;
        }
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

    public static string StateToSudokuString(string state, SudokuTranslationType type)
    {
        var s = new Sudoku();
        int cursor = 0;
        int n = -1;
        
        while (cursor < state.Length)
        {
            char current = state[cursor];
            if (current is 'd' or 'p')
            {
                n++;

                if (current == 'd')
                {
                    s[n / 9, n % 9] = state[cursor + 1] - '0';
                }
            }

            cursor++;
        }

        return SudokuTranslator.Translate(s, type);
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

    public bool AddSolution(int number, int row, int col, IStrategy strategy)
    {
        if (!AddSolution(number, row, col)) return false;
        
        if (LogsManaged) _logManager.NumberAdded(number, row, col, strategy);
        return true;
    }
    
    public bool RemovePossibility(int possibility, int row, int col, IStrategy strategy)
    {
        if (!RemovePossibility(possibility, row, col)) return false;
        
        if (LogsManaged) _logManager.PossibilityRemoved(possibility, row, col, strategy);
        return true;
    }

    public ChangeBuffer ChangeBuffer { get; }
    public LinkGraphManager GraphManager { get; }
    public PreComputer PreComputer { get; }

    //ChangeManager-----------------------------------------------------------------------------------------------------
    
    public IPossibilitiesHolder TakePossibilitiesSnapshot()
    {
        return SolverSnapshot.TakeSnapshot(this);
    }
    
    public bool AddSolutionFromBuffer(int number, int row, int col)
    {
        return AddSolution(number, row, col);
    }

    public bool RemovePossibilityFromBuffer(int possibility, int row, int col)
    {
        return RemovePossibility(possibility, row, col);
    }

    public void PublishChangeReport(ChangeReport report, IStrategy strategy)
    {
        if (!LogsManaged) return;
        
        _logManager.ChangePushed(report, strategy);
    }

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
        if (_sudoku[row, col] != 0) return false;
        
        _sudoku[row, col] = number;
        UpdateAfterSolutionAdded(number, row, col);
        
        NumberAdded?.Invoke(row, col);
        return true;
    }

    private bool RemovePossibility(int possibility, int row, int col)
    {
        if (!_possibilities[row, col].Remove(possibility)) return false;
        _positions[possibility - 1].Remove(row, col);
        
        PossibilityRemoved?.Invoke(row, col);
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

public class SolverState : ITranslatable
{
    private readonly CellState[,] _cellStates = new CellState[9, 9];
    
    public SolverState(Solver solver)
    {
        for (int row = 0; row < 9; row++)
        {
            for(int col = 0; col < 9; col++)
            {
                if (solver.Sudoku[row, col] != 0) _cellStates[row, col] = new CellState(solver.Sudoku[row, col]);
                else _cellStates[row, col] = solver.PossibilitiesAt(row, col).ToCellState();
            }
        }
    }

    public CellState At(int row, int col)
    {
        return _cellStates[row, col];
    }

    public int this[int row, int col]
    {
        get
        {
            var current = _cellStates[row, col];
            return current.IsPossibilities ? 0 : current.AsNumber;
        } 
    }
}

public readonly struct CellState
{
    private readonly short _bits;

    public CellState(int solved)
    {
        _bits = (short) (solved << 9);
    }

    private CellState(short bits)
    {
        _bits = bits;
    }

    public static CellState FromBits(short bits)
    {
        return new CellState(bits);
    }

    public bool IsPossibilities => _bits <= 0x1FF;
    
    public IPossibilities AsPossibilities => BitPossibilities.FromBits(_bits);
    
    public int AsNumber => _bits >> 9;
}

