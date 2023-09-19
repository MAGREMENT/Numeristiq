using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Model.Solver.Helpers;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Logs;
using Model.Solver.Positions;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver;

//TODO : improve UI, solve memory problems, improve classes access to things (possibilities for example)
public class Solver : IStrategyManager, IChangeManager, ILogHolder, IStrategyHolder 
{
    private Sudoku _sudoku;
    private readonly IPossibilities[,] _possibilities = new IPossibilities[9, 9];
    private readonly GridPositions[] _positions = new GridPositions[9];
    
    public IReadOnlySudoku Sudoku => _sudoku;
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

    private readonly LogManager _logManager;
    private readonly StrategyLoader _strategyLoader;

    public Solver(Sudoku s)
    {
        _sudoku = s;
        SetOriginalBoardForStrategies();

        NumberAdded += (_, _) => _changeWasMade = true;
        PossibilityRemoved += (_, _) => _changeWasMade = true;

        Init();

        _strategyLoader = new StrategyLoader(this);
        _strategyLoader.Load();
        
        PreComputer = new PreComputer(this);

        GraphManager = new LinkGraphManager(this);
        
        _logManager = new LogManager(this);
        _logManager.LogsUpdated += logs => LogsUpdated?.Invoke(logs);
        
        ChangeBuffer = new ChangeBuffer(this);
    }
    
    private Solver(Sudoku s, IPossibilities[,] p, GridPositions[] g, IStrategy[] t)
    {
        _sudoku = s;
        SetOriginalBoardForStrategies();
        
        _possibilities = p;
        _positions = g;

        NumberAdded += (_, _) => _changeWasMade = true;
        PossibilityRemoved += (_, _) => _changeWasMade = true;
        
        _strategyLoader = new StrategyLoader(this);
        Strategies = t;
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
        SetOriginalBoardForStrategies();

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
            if(((_excludedStrategies >> _currentStrategy) & 1) > 0) continue;
            
            if(StatisticsTracked) Strategies[_currentStrategy].Tracker.StartUsing();
            Strategies[_currentStrategy].ApplyOnce(this);
            if(StatisticsTracked) Strategies[_currentStrategy].Tracker.StopUsing(_changeWasMade);

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

    public Solver Copy()
    {
        IPossibilities[,] possCopy = new IPossibilities[9, 9];
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                possCopy[row, col] = _possibilities[row, col].Copy();
            }
        }

        GridPositions[] gpCopy = new GridPositions[9];
        for (int i = 0; i < 9; i++)
        {
            gpCopy[0] = _positions[i].Copy();
        }

        IStrategy[] strategiesCopy = new IStrategy[Strategies.Length];
        Array.Copy(Strategies, strategiesCopy, Strategies.Length);
        
        return new Solver(_sudoku.Copy(), possCopy, gpCopy, strategiesCopy);
    }
    
    //ChangeManager-----------------------------------------------------------------------------------------------------
    
    public IPossibilitiesHolder TakePossibilitiesSnapshot()
    {
        return PossibilitiesSnapshot.TakeSnapshot(this);
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
                    foreach (var possibility in _possibilities[row, col])
                    {
                        result += possibility;
                    }
                }
            }
        }

        return result;
    }
}

