using System;
using System.Collections.Generic;
using Model.Logs;
using Model.Positions;
using Model.Possibilities;
using Model.Strategies;
using Model.Strategies.AIC;
using Model.Strategies.AlternatingChains.ChainAlgorithms;
using Model.Strategies.AlternatingChains.ChainTypes;
using Model.Strategies.IntersectionRemoval;
using Model.StrategiesUtil;

namespace Model;

public class Solver : ISolverView //TODO : Look into precomputation, improve logs, improve UI
{
    public IPossibilities[,] Possibilities { get; } = new IPossibilities[9, 9];
    public List<ISolverLog> Logs => _logManager.Logs;
    public Sudoku Sudoku { get; private set; }
    public IStrategy[] Strategies { get; }

    public bool LogsManaged { get; set; } = true;

    public delegate void OnNumberAdded(int row, int col);
    public event OnNumberAdded? NumberAdded;

    public delegate void OnPossibilityRemoved(int row, int col);
    public event OnPossibilityRemoved? PossibilityRemoved;

    private readonly List<int[]> _listOfChanges = new();
    private bool _changeWasMade;

    private int _strategyCount;

    //private PreComputer _pre = new();
    private readonly LogManager _logManager = new();

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
        
        //_pre.PrecomputePositions(this);
    }
    
    private Solver(Sudoku s, IPossibilities[,] p, IStrategy[] t) //TODO : Add precomputer
    {
        Sudoku = s;
        Possibilities = p;
        Strategies = t;
        
        NumberAdded += (_, _) => _changeWasMade = true;
        PossibilityRemoved += (_, _) => _changeWasMade = true;
    }

    public void SetSudoku(Sudoku s)
    {
        Sudoku = s;

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
                    UpdatePossibilitiesAfterDefinitiveNumberAdded(s[i, j], i, j);
                }
            }
        }
        
        //_pre.PrecomputePositions(this);
    }

    public bool AddDefinitiveNumber(int number, int row, int col, IStrategy strategy)
    {
        if (Sudoku[row, col] != 0) return false;
        
        Sudoku[row, col] = number;
        UpdatePossibilitiesAfterDefinitiveNumberAdded(number, row, col);
        //_pre.DeletePosition(number, row, col);
        strategy.Score += 1;
        if (LogsManaged) _logManager.NumberAdded(number, row, col, strategy, _strategyCount);
        NumberAdded?.Invoke(row, col);
        return true;
    }

    public void SetDefinitiveNumberByHand(int number, int row, int col)
    {
        Sudoku[row, col] = number;
        UpdatePossibilitiesAfterDefinitiveNumberAdded(number, row, col);
    }

    public bool RemovePossibility(int possibility, int row, int col, IStrategy strategy)
    {
        bool buffer = Possibilities[row, col].Remove(possibility);
        if (!buffer) return false;

        //_pre.RemovePosition(possibility, row, col);
        strategy.Score += 1;
        if(LogsManaged) _logManager.PossibilityRemoved(possibility, row, col, strategy, _strategyCount);
        PossibilityRemoved?.Invoke(row, col);
        return true;
    }

    public void RemovePossibilityByHand(int possibility, int row, int col)
    {
        Possibilities[row, col].Remove(possibility);
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

    public LinePositions PossibilityPositionsInRow(int row, int number)
    {
        LinePositions result = new();
        for (int col = 0; col < 9; col++)
        {
            if (Sudoku[row, col] == number) return new LinePositions();
            if (Possibilities[row, col].Peek(number)) result.Add(col);
        }
        return result;
    }
    
    public LinePositions PossibilityPositionsInColumn(int col, int number)
    {
        LinePositions result = new();
        for (int row = 0; row < 9; row++)
        {
            if (Sudoku[row, col] == number) return new LinePositions();
            if (Possibilities[row, col].Peek(number)) result.Add(row);
        }

        return result;
    }
    
    public MiniGridPositions PossibilityPositionsInMiniGrid(int miniRow, int miniCol, int number)
    {
        MiniGridPositions result = new(miniRow, miniCol);
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                var realRow = miniRow * 3 + i;
                var realCol = miniCol * 3 + j;

                if (Sudoku[realRow, realCol] == number) return new MiniGridPositions(miniRow, miniCol);
                if (Possibilities[realRow, realCol].Peek(number)) result.Add(i, j);
            }
        }

        return result;
    }

    public void Solve()
    {
        for (int i = 0; i < Strategies.Length; i++)
        {
            if (Sudoku.IsComplete()) return;
            
            Strategies[i].ApplyOnce(this);
            _strategyCount++;
            
            if (_changeWasMade)
            {
                _changeWasMade = false;
                i = -1;
            }
        }
        
        if(LogsManaged) _logManager.Push();
    }

    public List<int[]> RunUntilProgress()
    {
        _listOfChanges.Clear();
        NumberAdded += AddToListOfChanges;
        PossibilityRemoved += AddToListOfChanges;
        
        foreach (var strategy in Strategies)
        {
            strategy.ApplyOnce(this);
            _strategyCount++;
            
            if (_changeWasMade)
            {
                _changeWasMade = false;
                break;
            }
        }
        
        NumberAdded -= AddToListOfChanges;
        PossibilityRemoved -= AddToListOfChanges;
        
        if(LogsManaged) _logManager.Push();
        
        return _listOfChanges;
    }

    private void AddToListOfChanges(int row, int col)
    {
        _listOfChanges.Add(new[] {row, col});
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

    public void ExcludeStrategy(Type type)
    {
        for (int i = 0; i < Strategies.Length; i++)
        {
            if (Strategies[i].GetType() == type) Strategies[i] = new NoStrategy();
        }
    }

    public IStrategy GetStrategy(Type type)
    {
        foreach (var t in Strategies)
        {
            if (t.GetType() == type) return t;
        }

        return new NoStrategy();
    }
    
    public static IStrategy[] BasicStrategies()
    {
        return new IStrategy[]{
            new NakedPossibilitiesStrategy(1),
            new HiddenPossibilityStrategy(1),
            new NakedPossibilitiesStrategy(2),
            new HiddenPossibilityStrategy(2),
            new BoxLineReduction(),
            new PointingPossibilitiesStrategy(),
            new NakedPossibilitiesStrategy(3),
            new HiddenPossibilityStrategy(3),
            new NakedPossibilitiesStrategy(4),
            new HiddenPossibilityStrategy(4),
            new XWingStrategy(),
            new XYWingStrategy(),
            new XYZWingStrategy(),
            new SimpleColoringStrategy(),
            new BugStrategy(),
            new GridFormationStrategy(3),
            new GridFormationStrategy(4),
            new FireworksStrategy(),
            new UniqueRectanglesStrategy(),
            new XYChainStrategy(),
            new ThreeDimensionMedusaStrategy(),
            new AlignedPairExclusionStrategy(4),
            //new GroupedXCyclesStrategy(),
            //new XCyclesStrategy(),
            new AlternatingChainGeneralization<PossibilityCoordinate>(new NormalXCycles(),
                new AlternatingChainAlgorithmV1<PossibilityCoordinate>(200)),
            //new AlternatingInferenceChainStrategy(int.MaxValue, 20),
            new AlternatingChainGeneralization<PossibilityCoordinate>(new NormalAIC(),
                new AlternatingChainAlgorithmV1<PossibilityCoordinate>(20))
            //new TrialAndMatchStrategy(2)
        };
    }

    public string GetState()
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

