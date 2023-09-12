using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Model.Changes;
using Model.Logs;
using Model.Positions;
using Model.Possibilities;
using Model.StrategiesUtil;
using Model.StrategiesUtil.LinkGraph;

namespace Model.Solver;

//TODO : improve UI, solve memory problems, improve classes access to things (possibilities for example)
public class Solver : IStrategyManager, IChangeManager, ILogHolder, IStrategyHolder 
{
    private Sudoku _sudoku;
    public IReadOnlySudoku Sudoku => _sudoku;
    public IPossibilities[,] Possibilities { get; } = new IPossibilities[9, 9];
    public IStrategy[] Strategies { get; private set; } = Array.Empty<IStrategy>();
    public StrategyInfo[] StrategyInfos => _strategyLoader.Infos;
    public List<ISolverLog> Logs => _logManager.Logs;

    public bool LogsManaged { get; init; } = true;
    public bool StatisticsTracked { get; init; }

    public string State => GetState();
    public string StartState => _logManager.StartState;

    public delegate void OnNumberAdded(int row, int col);
    public event OnNumberAdded? NumberAdded;

    public delegate void OnPossibilityRemoved(int row, int col);
    public event OnPossibilityRemoved? PossibilityRemoved;

    public event LogManager.OnLogsUpdate? LogsUpdated;

    private int _currentStrategy = -1;
    private ulong _excludedStrategies;
    private bool _changeWasMade;
    
    private readonly PreComputer _pre;
    private readonly LogManager _logManager;
    private readonly StrategyLoader _strategyLoader;

    public Solver(Sudoku s)
    {
        _sudoku = s;
        SetOriginalBoardForStrategies();

        NumberAdded += (_, _) => _changeWasMade = true;
        PossibilityRemoved += (_, _) => _changeWasMade = true;

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                Possibilities[i, j] = IPossibilities.New();
            }
        }
        
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (_sudoku[i, j] != 0)
                {
                    UpdatePossibilitiesAfterSolutionAdded(s[i, j], i, j);
                }
            }
        }

        _strategyLoader = new StrategyLoader(this);
        _strategyLoader.Load();
        
        _pre = new PreComputer(this);
        
        _logManager = new LogManager(this);
        _logManager.LogsUpdated += logs => LogsUpdated?.Invoke(logs);
        
        ChangeBuffer = new ChangeBuffer(this);
    }
    
    private Solver(Sudoku s, IPossibilities[,] p, IStrategy[] t)
    {
        _sudoku = s;
        SetOriginalBoardForStrategies();
        
        Possibilities = p;

        NumberAdded += (_, _) => _changeWasMade = true;
        PossibilityRemoved += (_, _) => _changeWasMade = true;
        
        _strategyLoader = new StrategyLoader(this);
        Strategies = t;
        _pre = new PreComputer(this);
        _logManager = new LogManager(this);
        _logManager.LogsUpdated += logs => LogsUpdated?.Invoke(logs);
        ChangeBuffer = new ChangeBuffer(this);
    }
    
    //Solver------------------------------------------------------------------------------------------------------------
    
    public void SetSudoku(Sudoku s)
    {
        _sudoku = s;
        SetOriginalBoardForStrategies();

        ResetPossibilities();

        if (LogsManaged) _logManager.Clear();

        _pre.Reset();
    }
    
    
    public void SetSolutionByHand(int number, int row, int col)
    {
        _sudoku[row, col] = number;
        ResetPossibilities();
        if(LogsManaged) _logManager.Clear();
    }

    public void RemovePossibilityByHand(int possibility, int row, int col)
    {
        if (!Possibilities[row, col].Remove(possibility)) return;
        
        if (LogsManaged) _logManager.PossibilityRemovedByHand(possibility, row, col);
    }
    
    public void Solve(bool stopAtProgress = false)
    {
        for (_currentStrategy = 0; _currentStrategy < Strategies.Length; _currentStrategy++)
        {
            if (_sudoku.IsComplete()) return;
            if(((_excludedStrategies >> _currentStrategy) & 1) > 0) continue;
            
            if(StatisticsTracked) Strategies[_currentStrategy].Tracker.StartUsing();
            Strategies[_currentStrategy].ApplyOnce(this);
            if(StatisticsTracked) Strategies[_currentStrategy].Tracker.StopUsing(_changeWasMade);

            if (!_changeWasMade) continue;
            _changeWasMade = false;
            _currentStrategy = -1;
            _pre.Reset();
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
                if (_sudoku[row, col] == 0 && Possibilities[row, col].Count == 0) return true;
            }
        }

        return false;
    }

    public void ExcludeStrategy(int number)
    {
        _excludedStrategies |= 1ul << number;
    }

    public void ExcludeStrategies(StrategyLevel level)
    {
        for (int i = 0; i < Strategies.Length; i++)
        {
            if (Strategies[i].Difficulty == level) ExcludeStrategy(i);
        }
    }

    public void UseStrategy(int number)
    {
        _excludedStrategies &= ~(1ul << number);
    }
    
    //PossibilityHolder-------------------------------------------------------------------------------------------------
    
    public IReadOnlyPossibilities PossibilitiesAt(int row, int col)
    {
        return Possibilities[row, col];
    }

    //StrategyManager---------------------------------------------------------------------------------------------------
    
    public bool AddSolution(int number, int row, int col, IStrategy strategy)
    {
        if (_sudoku[row, col] != 0) return false;
        
        _sudoku[row, col] = number;
        UpdatePossibilitiesAfterSolutionAdded(number, row, col);
        if (LogsManaged) _logManager.NumberAdded(number, row, col, strategy);
        
        NumberAdded?.Invoke(row, col);
        return true;
    }
    
    public bool RemovePossibility(int possibility, int row, int col, IStrategy strategy)
    {
        bool buffer = Possibilities[row, col].Remove(possibility);
        if (!buffer) return false;
        
        if (LogsManaged) _logManager.PossibilityRemoved(possibility, row, col, strategy);
        
        PossibilityRemoved?.Invoke(row, col);
        return true;
    }

    public ChangeBuffer ChangeBuffer { get; }

    public LinePositions RowPositionsAt(int row, int number)
    {
        return _pre.RowPositions(row, number);
    }
    
    public LinePositions ColumnPositionsAt(int col, int number)
    {
        return _pre.ColumnPositions(col, number);
    }
    
    public MiniGridPositions MiniGridPositionsAt(int miniRow, int miniCol, int number)
    {
        return _pre.MiniGridPositions(miniRow, miniCol, number);
    }

    public List<AlmostLockedSet> AlmostLockedSets()
    {
        return _pre.AlmostLockedSets();
    }

    public LinkGraph<ILinkGraphElement> LinkGraph()
    {
        return _pre.LinkGraph();
    }

    public Dictionary<ILinkGraphElement, Coloring> OnColoring(int row, int col, int possibility)
    {
        return _pre.OnColoring(row, col, possibility);
    }

    public Dictionary<ILinkGraphElement, Coloring> OffColoring(int row, int col, int possibility)
    {
        return _pre.OffColoring(row, col, possibility);
    }
    
    public Solver Copy()
    {
        IPossibilities[,] possCopy = new IPossibilities[9, 9];
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                possCopy[row, col] = Possibilities[row, col].Copy();
            }
        }

        IStrategy[] stratCopy = new IStrategy[Strategies.Length];
        Array.Copy(Strategies, stratCopy, Strategies.Length);
        return new Solver(_sudoku.Copy(), possCopy, stratCopy);
    }
    
    //ChangeManager-----------------------------------------------------------------------------------------------------
    
    public ISolver TakePossibilitiesSnapshot()
    {
        return PossibilitiesSnapshot.TakeSnapshot(this);
    }
    
    public bool AddSolution(int number, int row, int col)
    {
        if (_sudoku[row, col] != 0) return false;
        
        _sudoku[row, col] = number;
        UpdatePossibilitiesAfterSolutionAdded(number, row, col);

        NumberAdded?.Invoke(row, col);
        return true;
    }

    public bool RemovePossibility(int possibility, int row, int col)
    {
        bool buffer = Possibilities[row, col].Remove(possibility);
        if (!buffer) return false;
        
        PossibilityRemoved?.Invoke(row, col);
        return true;
    }

    public void PushChangeReportLog(ChangeReport report, IStrategy strategy)
    {
        if (!LogsManaged) return;
        
        _logManager.ChangePushed(report, strategy);
    }
    
    //StrategyHolder----------------------------------------------------------------------------------------------------
    
    public void SetStrategies(IStrategy[] strategies)
    {
        Strategies = strategies;
    }

    public void SetExcludedStrategies(ulong excluded)
    {
        _excludedStrategies = excluded;
    }
    
    //Private-----------------------------------------------------------------------------------------------------------

    private void ResetPossibilities()
    {
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                Possibilities[i, j].Reset();
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
        Possibilities[row, col].RemoveAll();
        for (int i = 0; i < 9; i++)
        {
            Possibilities[row, i].Remove(number);
            Possibilities[i, col].Remove(number);
        }
        
        int startRow = row / 3 * 3;
        int startColumn = col / 3 * 3;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Possibilities[startRow + i, startColumn + j].Remove(number);
            }
        }
    }

    private void SetOriginalBoardForStrategies()
    {
        foreach (var strategy in Strategies)
        {
            if (strategy is IOriginalBoardNeededStrategy originalBoardNeededStrategy)
            {
                originalBoardNeededStrategy.SetOriginalBoard(_sudoku.Copy());
            }
        }
    }

    private string GetState()
    {
        string result = "";
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (_sudoku[row, col] != 0) result += "d" + _sudoku[row, col];
                else
                {
                    result += "p";
                    foreach (var possibility in Possibilities[row, col])
                    {
                        result += possibility;
                    }
                }
            }
        }

        return result;
    }
}

