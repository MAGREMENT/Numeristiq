using System;
using System.Collections.Generic;
using Model.Possibilities;
using Model.Strategies;
using Model.Strategies.LocalizedPossibility;
using Model.Strategies.SamePossibilities;
using Model.Strategies.SinglePossibility;
using Model.StrategiesV2;

namespace Model;

public class Solver : ISolver
{
    public IPossibilities[,] Possibilities { get; }
    public List<ISolverLog> Logs { get; } = new();
    public Sudoku Sudoku { get; }

    private List<IStrategy> Strategies { get; } = new()
    {
        /*new SinglePossibilityStrategyPackage(),
        new SamePossibilitiesStrategyPackage(),
        new LocalizedPossibilityStrategyPackage(),*/
        new NakedPossibilitiesStrategy(1),
        new HiddenPossibilityStrategy(1),
        new NakedPossibilitiesStrategy(2),
        new HiddenPossibilityStrategy(2),
        new LocalizedPossibilityStrategyPackage(),
        new NakedPossibilitiesStrategy(3),
        new HiddenPossibilityStrategy(3),
        new NakedPossibilitiesStrategy(4),
        new HiddenPossibilityStrategy(4),
        new XWingStrategy(),
        new YWingStrategy()
        //new TrialAndMatchStrategy(2)
    };

    public delegate void OnNumberAdded(int row, int col);
    public event OnNumberAdded? NumberAdded;

    public delegate void OnPossibilityRemoved(int row, int col);
    public event OnPossibilityRemoved? PossibilityRemoved;

    private readonly List<int[]> _listOfChanges = new();
    private bool _changeWasMade;

    public Solver(Sudoku s)
    {
        Possibilities = new IPossibilities[9, 9];
        Sudoku = s;

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                Possibilities[i, j] = new BoolArrayPossibilities();
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

        NumberAdded += (_, _) => _changeWasMade = true;
        PossibilityRemoved += (_, _) => _changeWasMade = true;
    }
    
    public bool AddDefinitiveNumber(int number, int row, int col, ISolverLog? log = null)
    {
        if (Sudoku[row, col] != 0) return false;
        Sudoku[row, col] = number;
        UpdatePossibilitiesAfterDefinitiveNumberAdded(number, row, col);
        Logs.Add(log ?? new BasicNumberAddedLog(number, row, col));
        NumberAdded?.Invoke(row, col);
        return true;
    }

    public bool RemovePossibility(int possibility, int row, int col, ISolverLog? log = null)
    {
        bool buffer = Possibilities[row, col].Remove(possibility);
        if (buffer)
        {
            Logs.Add(log ?? new BasicPossibilityRemovedLog(possibility, row, col));
            PossibilityRemoved?.Invoke(row, col);
        }
        return buffer;
    }

    private void UpdatePossibilitiesAfterDefinitiveNumberAdded(int number, int row, int col)
    {
        Possibilities[row, col].RemoveAll();
        for (int i = 0; i < 9; i++)
        {
            Possibilities[row, i].Remove(number);
            Possibilities[i, col].Remove(number);
        }
        
        int startRow = (row / 3) * 3;
        int startColumn = (col / 3) * 3;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Possibilities[startRow + i, startColumn + j].Remove(number);
            }
        }
    }

    public void ExcludeStrategy(Type type)
    {
        for (int i = 0; i < Strategies.Count; i++)
        {
            if (Strategies[i].GetType() == type) Strategies.RemoveAt(i);
        }
    }
    
    public Positions PossiblePositionsInRow(int row, int number)
    {
        Positions result = new();
        for (int col = 0; col < 9; col++)
        {
            if (Sudoku[row, col] == number) return new Positions();
            if (Possibilities[row, col].Peek(number)) result.Add(col);
        }
        return result;
    }
    
    public Positions PossiblePositionsInColumn(int col, int number)
    {
        Positions result = new();
        for (int row = 0; row < 9; row++)
        {
            if (Sudoku[row, col] == number) return new Positions();
            if (Possibilities[row, col].Peek(number)) result.Add(row);
        }

        return result;
    }
    
    public List<int[]> PossiblePositionsInMiniGrid(int miniRow, int miniCol, int number)
    {
        List<int[]> result = new();
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                var realRow = miniRow * 3 + i;
                var realCol = miniCol * 3 + j;
                
                if (Sudoku[realRow, realCol] == number) return new List<int[]>();
                if (Possibilities[realRow, realCol].Peek(number)) result.Add(new[]{realRow, realCol});
            }
        }

        return result;
    }

    public void Solve()
    {
        for (int i = 0; i < Strategies.Count; i++)
        {
            if (Sudoku.IsComplete()) return;
            Strategies[i].ApplyOnce(this);
            if (_changeWasMade)
            {
                _changeWasMade = false;
                i = -1;
            }
        }
    }

    public void RunAllStrategiesOnce()
    {
        foreach (var strategy in Strategies)
        {
            strategy.ApplyOnce(this);
        }
    }

    public List<int[]> RunUntilProgress()
    {
        _listOfChanges.Clear();
        NumberAdded += AddToListOfChanges;
        PossibilityRemoved += AddToListOfChanges;
        
        foreach (var strategy in Strategies)
        {
            strategy.ApplyOnce(this);
            if (_changeWasMade)
            {
                _changeWasMade = false;
                break;
            }
        }
        
        NumberAdded -= AddToListOfChanges;
        PossibilityRemoved -= AddToListOfChanges;
        
        return _listOfChanges;
    }

    private void AddToListOfChanges(int row, int col)
    {
        _listOfChanges.Add(new[] {row, col});
    }
    
    

}

