using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Model.Logs;
using Model.Positions;
using Model.Possibilities;
using Model.StrategiesUtil;

namespace Model;

public class Solver : IStrategyManager, IChangeManager, ILogHolder, IStrategyHolder //TODO : improve UI, solve memory problems, improve classes access to things (possibilities for example)
{
    public IPossibilities[,] Possibilities { get; } = new IPossibilities[9, 9];
    public List<ISolverLog> Logs => _logManager.Logs;

    public Sudoku Sudoku { get; private set; }
    public IStrategy[] Strategies { get; private set; } = null!;
    public StrategyInfo[] StrategyInfos => _strategyLoader.Infos;

    public bool LogsManaged { get; init; } = true;
    public int StrategyCount { get; private set; }
    public string State { get; private set; }

    public delegate void OnNumberAdded(int row, int col);
    public event OnNumberAdded? NumberAdded;

    public delegate void OnPossibilityRemoved(int row, int col);
    public event OnPossibilityRemoved? PossibilityRemoved;

    private int _currentStrategy = -1;
    private int _excludedStrategies;
    private bool _changeWasMade;
    
    private readonly PreComputer _pre;
    private readonly LogManager _logManager;
    private readonly StrategyLoader _strategyLoader;

    public Solver(Sudoku s)
    {
        _strategyLoader = new StrategyLoader(this);
        _strategyLoader.Load();
        
        Sudoku = s;
        SetOriginalBoard();

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
                if (Sudoku[i, j] != 0)
                {
                    UpdatePossibilitiesAfterDefinitiveNumberAdded(s[i, j], i, j);
                }
            }
        }
        
        State = GetState();
        
        _pre = new PreComputer(this);
        _logManager = new LogManager(this);
        ChangeBuffer = new ChangeBuffer(this);
    }
    
    private Solver(Sudoku s, IPossibilities[,] p, IStrategy[] t)
    {
        Sudoku = s;
        SetOriginalBoard();
        Possibilities = p;
        Strategies = t;
        _pre = new PreComputer(this);
        _logManager = new LogManager(this);
        ChangeBuffer = new ChangeBuffer(this);
        _strategyLoader = new StrategyLoader(this);
        
        State = GetState();
        
        NumberAdded += (_, _) => _changeWasMade = true;
        PossibilityRemoved += (_, _) => _changeWasMade = true;
    }
    
    //Solver------------------------------------------------------------------------------------------------------------
    
    public void SetSudoku(Sudoku s)
    {
        Sudoku = s;
        SetOriginalBoard();

        ResetPossibilities();

        if (LogsManaged)
        {
            _logManager.Clear();
            State = GetState();
        }

        _pre.Reset();
    }
    
    
    public void SetDefinitiveNumberByHand(int number, int row, int col)
    {
        Sudoku[row, col] = number;
        ResetPossibilities();
        if(LogsManaged) _logManager.Clear();
    }

    public void RemovePossibilityByHand(int possibility, int row, int col)
    {
        if (!Possibilities[row, col].Remove(possibility)) return;
        
        if (!LogsManaged) return;
        
        _logManager.PossibilityRemovedByHand(possibility, row, col);
        State = GetState();
    }
    
    public void Solve(bool stopAtProgress = false)
    {
        for (_currentStrategy = 0; _currentStrategy < Strategies.Length; _currentStrategy++)
        {
            if (Sudoku.IsComplete()) return;
            if(((_excludedStrategies >> _currentStrategy) & 1) > 0) continue;
            
            Strategies[_currentStrategy].ApplyOnce(this);
            StrategyCount++;

            if (!_changeWasMade) continue;
            _changeWasMade = false;
            _currentStrategy = -1;
            _pre.Reset();

            if (!stopAtProgress) continue;
            if(LogsManaged) _logManager.Push();
            return;
        }
        
        if(LogsManaged) _logManager.Push();
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
                if (Sudoku[row, col] == 0 && Possibilities[row, col].Count == 0) return true;
            }
        }

        return false;
    }

    public void ExcludeStrategy(int number)
    {
        _excludedStrategies |= 1 << number;
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
        _excludedStrategies &= ~(1 << number);
    }

    //StrategyManager---------------------------------------------------------------------------------------------------
    
    public bool AddDefinitiveNumber(int number, int row, int col, IStrategy strategy)
    {
        if (Sudoku[row, col] != 0) return false;
        
        Sudoku[row, col] = number;
        UpdatePossibilitiesAfterDefinitiveNumberAdded(number, row, col);
        strategy.Score += 1;
        if (LogsManaged)
        {
            _logManager.NumberAdded(number, row, col, strategy);
            State = GetState();
        }
        NumberAdded?.Invoke(row, col);
        return true;
    }
    
    public bool RemovePossibility(int possibility, int row, int col, IStrategy strategy)
    {
        bool buffer = Possibilities[row, col].Remove(possibility);
        if (!buffer) return false;
        
        strategy.Score += 1;
        if (LogsManaged)
        {
            _logManager.PossibilityRemoved(possibility, row, col, strategy);
            State = GetState();
        }
        PossibilityRemoved?.Invoke(row, col);
        return true;
    }

    public ChangeBuffer ChangeBuffer { get; }

    public LinePositions PossibilityPositionsInRow(int row, int number)
    {
        return _pre.PossibilityPositionsInRow(row, number);
    }
    
    public LinePositions PossibilityPositionsInColumn(int col, int number)
    {
        return _pre.PossibilityPositionsInColumn(col, number);
    }
    
    public MiniGridPositions PossibilityPositionsInMiniGrid(int miniRow, int miniCol, int number)
    {
        return _pre.PossibilityPositionsInMiniGrid(miniRow, miniCol, number);
    }

    public List<AlmostLockedSet> AllAls()
    {
        return _pre.AllAls();
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
        return new Solver(Sudoku.Copy(), possCopy, stratCopy);
    }
    
    //ChangeManager-----------------------------------------------------------------------------------------------------
    
    public bool AddDefinitive(int number, int row, int col)
    {
        if (Sudoku[row, col] != 0) return false;
        
        Sudoku[row, col] = number;
        UpdatePossibilitiesAfterDefinitiveNumberAdded(number, row, col);
        Strategies[_currentStrategy].Score += 1;
        
        NumberAdded?.Invoke(row, col);
        return true;
    }

    public bool RemovePossibility(int possibility, int row, int col)
    {
        bool buffer = Possibilities[row, col].Remove(possibility);
        if (!buffer) return false;

        Strategies[_currentStrategy].Score += 1;
        
        PossibilityRemoved?.Invoke(row, col);
        return true;
    }

    public void PushChangeReportLog(ChangeReport report, IStrategy strategy)
    {
        if (!LogsManaged) return;
        
        _logManager.ChangePushed(report, strategy);
        State = GetState();
    }
    
    //StrategyHolder----------------------------------------------------------------------------------------------------
    
    public void SetStrategies(IStrategy[] strategies)
    {
        Strategies = strategies;
    }

    public void SetExcludedStrategies(int excluded)
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
                if (Sudoku[i, j] != 0)
                {
                    UpdatePossibilitiesAfterDefinitiveNumberAdded(Sudoku[i, j], i, j);
                }
            }
        }
    }

    private void UpdatePossibilitiesAfterDefinitiveNumberAdded(int number, int row, int col)
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

    private void SetOriginalBoard()
    {
        foreach (var strategy in Strategies)
        {
            if (strategy is IOriginalBoardNeededStrategy originalBoardNeededStrategy)
            {
                originalBoardNeededStrategy.SetOriginalBoard(Sudoku.Copy());
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
                if (Sudoku[row, col] != 0) result += "d" + Sudoku[row, col];
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

