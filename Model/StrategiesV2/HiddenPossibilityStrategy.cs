using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace Model.StrategiesV2;

public class HiddenPossibilityStrategy : IStrategy
{
    private readonly int _type;

    public HiddenPossibilityStrategy(int type)
    {
        _type = type;
    }
    
    public void ApplyOnce(ISolver solver)
    {
        //Rows
        for (int row = 0; row < 9; row++)
        {
            Dictionary<Positions, List<int>> possibilitiesToExamine = new();
            for (int number = 1; number <= 9; number++)
            {
                var positions = PossiblePositionsInRow(solver, row, number);
                if (positions.Count == _type)
                {
                    if (!possibilitiesToExamine.TryAdd(positions, new List<int> { number }))
                    {
                        possibilitiesToExamine[positions].Add(number);
                    }
                }
            }

            foreach (var entry in possibilitiesToExamine)
            {
                if (entry.Value.Count == _type)
                {
                    foreach (var col in entry.Key.All())
                    {
                        RemoveAllPossibilitiesExcept(solver, row, col, entry.Value);
                    }
                }
            }
        }
        
        //Columns
        for (int col = 0; col < 9; col++)
        {
            Dictionary<Positions, List<int>> possibilitiesToExamine = new();
            for (int number = 1; number <= 9; number++)
            {
                var positions = PossiblePositionsInColumn(solver, col, number);
                if (positions.Count == _type)
                {
                    if (!possibilitiesToExamine.TryAdd(positions, new List<int> { number }))
                    {
                        possibilitiesToExamine[positions].Add(number);
                    }
                }
            }
            
            foreach (var entry in possibilitiesToExamine)
            {
                if (entry.Value.Count == _type)
                {
                    foreach (var row in entry.Key.All())
                    {
                        RemoveAllPossibilitiesExcept(solver, row, col, entry.Value);
                    }
                }
            }
        }
        
        //MiniGrid
        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                Dictionary<Positions, List<int>> possibilitiesToExamine = new();
                for (int number = 1; number <= 9; number++)
                {
                    var positions = PossiblePositionsInMiniGrid(solver, miniRow, miniCol, number);
                    if (positions.Count == _type)
                    {
                        if (!possibilitiesToExamine.TryAdd(positions, new List<int> { number }))
                        {
                            possibilitiesToExamine[positions].Add(number);
                        }
                    }
                }
                
                foreach (var entry in possibilitiesToExamine)
                {
                    if (entry.Value.Count == _type)
                    {
                        foreach (var gridNumber in entry.Key.All())
                        {
                            int row = miniRow * 3 + gridNumber / 3;
                            int col = miniCol * 3 + gridNumber % 3;
                            
                            RemoveAllPossibilitiesExcept(solver, row, col, entry.Value);
                        }
                    }
                }
            }
        }
    }
    
    private Positions PossiblePositionsInRow(ISolver solver, int row, int number)
    {
        Positions result = new();
        for (int col = 0; col < 9; col++)
        {
            if (solver.Sudoku[row, col] == number) return new Positions();
            if (solver.Sudoku[row, col] == 0 &&
                solver.Possibilities[row, col].Peek(number)) result.Add(col);
        }

        return result;
    }

    private Positions PossiblePositionsInColumn(ISolver solver, int col, int number)
    {
        Positions result = new();
        for (int row = 0; row < 9; row++)
        {
            if (solver.Sudoku[row, col] == number) return new Positions();
            if (solver.Sudoku[row, col] == 0 &&
                solver.Possibilities[row, col].Peek(number)) result.Add(row);
        }

        return result;
    }
    
    private Positions PossiblePositionsInMiniGrid(ISolver solver, int miniRow, int miniCol, int number)
    {
        Positions result = new();
        for (int gridNumber = 0; gridNumber < 9; gridNumber++)
        {
            int row = miniRow * 3 + gridNumber / 3;
            int col = miniCol * 3 + gridNumber % 3;
            
            if (solver.Sudoku[row, col] == number) return new Positions();
            if (solver.Sudoku[row, col] == 0 &&
                solver.Possibilities[row, col].Peek(number)) result.Add(gridNumber);
        }

        return result;
    }

    private void RemoveAllPossibilitiesExcept(ISolver solver, int row, int col, List<int> except)
    {
        if (_type == 1) solver.AddDefinitiveNumber(except[0], row, col,
            new HiddenPossibilityLog(row, col, except[0], _type));
        for (int i = 1; i <= 9; i++)
        {
            if (!except.Contains(i))
            {
                solver.RemovePossibility(i, row, col, new HiddenPossibilityLog(row, col, i, _type));
            }
        }
    }
}

public class Positions
{
    private int _pos;
    public int Count { private set; get; }

    public void Add(int pos)
    {
        _pos |= 1 << pos;
        Count++;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Positions pos) return false;
        return _pos == pos._pos;
    }

    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return _pos;
    }

    public List<int> All()
    {
        List<int> result = new();
        for (int i = 0; i < 9; i++)
        {
            if(((_pos >> i) & 1) > 0) result.Add(i);
        }

        return result;
    }
}

public class HiddenPossibilityLog : ISolverLog
{
    public string AsString { get; }
    public StrategyLevel Level { get; }

    public HiddenPossibilityLog(int row, int col, int number, int type)
    {
        if (type == 1)
        {
            AsString = AsString = $"[{row + 1}, {col + 1}] {number} added as definitive because of hidden {type}";
            Level = StrategyLevel.Basic;
        }
        else
        {
            AsString = $"[{row + 1}, {col + 1}] {number} removed from possibilities because of hidden {type}";
            Level = (StrategyLevel)type;
        }
        
    }
}