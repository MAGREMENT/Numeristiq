using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Model.Logs;
using Model.Positions;
using Model.Possibilities;
using Model.Strategies;
using Model.Strategies.AlternatingChains;
using Model.Strategies.AlternatingChains.ChainAlgorithms;
using Model.Strategies.AlternatingChains.ChainTypes;
using Model.Strategies.ForcingNets;
using Model.StrategiesUtil;

namespace Model;

public class Solver : IStrategyManager, IChangeManager, ILogHolder //TODO : improve UI, solve memory problems, improve classes access to things (possibilities for example)
{
    public IPossibilities[,] Possibilities { get; } = new IPossibilities[9, 9];
    public List<ISolverLog> Logs => _logManager.Logs;

    public Sudoku Sudoku { get; private set; }
    public IStrategy[] Strategies { get; }

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
    
    private PreComputer _pre;
    private LogManager _logManager;
    private ChangeBuffer _changeBuffer;

    public Solver(Sudoku s, params IStrategy[] strategies)
    {
        Strategies = strategies.Length > 0 ? strategies : BasicStrategies();
        
        Sudoku = s;

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
        _changeBuffer = new ChangeBuffer(this);
    }
    
    private Solver(Sudoku s, IPossibilities[,] p, IStrategy[] t, PreComputer pre)
    {
        Sudoku = s;
        Possibilities = p;
        Strategies = t;
        _pre = pre;
        _logManager = new LogManager(this);
        _changeBuffer = new ChangeBuffer(this);
        
        State = GetState();
        
        NumberAdded += (_, _) => _changeWasMade = true;
        PossibilityRemoved += (_, _) => _changeWasMade = true;
    }
    
    //Solver------------------------------------------------------------------------------------------------------------
    
    public void SetSudoku(Sudoku s)
    {
        Sudoku = s;

        ResetPossibilities();

        if (LogsManaged)
        {
            _logManager = new LogManager(this);
            State = GetState();
        }

        _pre = new PreComputer(this);
    }
    
    
    public void SetDefinitiveNumberByHand(int number, int row, int col)
    {
        Sudoku[row, col] = number;
        ResetPossibilities();
        if(LogsManaged) Logs.Clear();
    }

    public void RemovePossibilityByHand(int possibility, int row, int col)
    {
        Possibilities[row, col].Remove(possibility);
        
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

    public ChangeBuffer ChangeBuffer => _changeBuffer;

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
        return new Solver(Sudoku.Copy(), possCopy, stratCopy, _pre);
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

    private static IStrategy[] BasicStrategies()
    {
        return new IStrategy[]{
            new NakedSingleStrategy(),
            new HiddenSingleStrategy(),
            new NakedPossibilitiesStrategy(2),
            new HiddenPossibilitiesStrategy(2),
            new BoxLineReductionStrategy(),
            new PointingPossibilitiesStrategy(),
            new NakedPossibilitiesStrategy(3),
            new HiddenPossibilitiesStrategy(3),
            new NakedPossibilitiesStrategy(4),
            new HiddenPossibilitiesStrategy(4),
            new XWingStrategy(),
            new XYWingStrategy(),
            new XYZWingStrategy(),
            new GridFormationStrategy(3),
            new GridFormationStrategy(4),
            new SimpleColoringStrategy(),
            new BUGStrategy(),
            new FinnedXWingStrategy(),
            new FinnedGridFormationStrategy(3),
            //new FinnedGridFormationStrategy(4),
            new FireworksStrategy(),
            new UniqueRectanglesStrategy(),
            new XYChainStrategy(),
            new ThreeDimensionMedusaStrategy(),
            new AlignedPairExclusionStrategy(4),
            new AlternatingChainGeneralization<ILinkGraphElement>(new GroupedXCycles(),
                new AlternatingChainAlgorithmV1<ILinkGraphElement>(20)),
            //new AlternatingChainGeneralization<PossibilityCoordinate>(new NormalAIC(),
                //new AlternatingChainAlgorithmV1<PossibilityCoordinate>(20)),
                new DigitForcingNetStrategy(),
            new CellForcingNetStrategy(4),
            new UnitForcingNetStrategy(4),
            new NishioForcingNetStrategy(),
            new AlmostLockedSetsStrategy()
            //new TrialAndMatchStrategy(2)
        };
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

