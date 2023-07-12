using System.Collections.Generic;

namespace Model.StrategiesV2;

public class NakedPossibilitiesStrategy : IStrategy
{
    private readonly int _type;

    public NakedPossibilitiesStrategy(int type)
    {
        _type = type;
    }
    
    
    public void ApplyOnce(ISolver solver)
    {
        //Rows
        for (int row = 0; row < 9; row++)
        {
            IPossibilities empty = new ArrayPossibilities();
            empty.RemoveAll();
            var possibleCols = EveryRowCellWithLessPossibilities(solver, row, _type + 1);
            RecursiveRowMashing(solver, empty, possibleCols, row, _type, new HashSet<int>());
        }
        
        //Cols
        for (int col = 0; col < 9; col++)
        {
            if (col == 7 && _type == 3)
            {
                int a = 0;
            }
            IPossibilities empty = new ArrayPossibilities();
            empty.RemoveAll();
            var possibleRows = EveryColumnCellWithLessPossibilities(solver, col, _type + 1);
            RecursiveColumnMashing(solver, empty, possibleRows, col, _type, new HashSet<int>());
        }
        
        //MiniGrid
        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                IPossibilities empty = new ArrayPossibilities();
                empty.RemoveAll();
                var possibleGridNumbers = EveryMiniGridCellWithLessPossibilities(solver, miniRow, miniCol, _type + 1);
                RecursiveMiniGridMashing(solver, empty, possibleGridNumbers, miniRow, miniCol, _type, new HashSet<int>());
            }
        }
    }
    
    private List<int> EveryRowCellWithLessPossibilities(ISolver solver, int row, int than)
    {
        List<int> result = new();
        for (int col = 0; col < 9; col++)
        {
            if (solver.Sudoku[row, col] == 0 && solver.Possibilities[row, col].Count < than) 
                result.Add(col);
        }

        return result;
    }

    private void RecursiveRowMashing(ISolver solver, IPossibilities current,
        List<int> possibleCols, int row, int count, HashSet<int> visited)
    {
        foreach (var col in possibleCols)
        {
            if (!visited.Contains(col))
            {
                IPossibilities newCurrent = current.Mash(solver.Possibilities[row, col]);
                var newVisited = new HashSet<int>(visited) { col };
                if (newCurrent.Count <= _type)
                {
                    if (count - 1 == 0 && newCurrent.Count == _type)
                    {
                        if (_type == 1) solver.AddDefinitiveNumber(newCurrent.GetFirst(), row, col,
                            new NakedPossibilityLog(row, col, newCurrent.GetFirst(), _type));
                        else RemovePossibilitiesFromRow(solver, row, newCurrent, newVisited);
                    }
                    else
                    {
                        RecursiveRowMashing(solver, newCurrent, possibleCols, row, count - 1, newVisited);
                    }
                }
            }
        }
    }

    private void RemovePossibilitiesFromRow(ISolver solver, int row, IPossibilities toRemove, HashSet<int> except)
    {
        foreach (var n in toRemove.GetPossibilities())
        {
            for (int col = 0; col < 9; col++)
            {
                if (solver.Sudoku[row, col] == 0 && !except.Contains(col))
                    solver.RemovePossibility(n, row, col, new NakedPossibilityLog(row, col, n, _type));
            }
        }
    }
    
    private List<int> EveryColumnCellWithLessPossibilities(ISolver solver, int col, int than)
    {
        List<int> result = new();
        for (int row = 0; row < 9; row++)
        {
            if (solver.Sudoku[row, col] == 0 && solver.Possibilities[row, col].Count < than) 
                result.Add(row);
        }

        return result;
    }
    
    private void RecursiveColumnMashing(ISolver solver, IPossibilities current,
        List<int> possibleRows, int col, int count, HashSet<int> visited)
    {
        foreach (var row in possibleRows)
        {
            if (!visited.Contains(row))
            {
                IPossibilities newCurrent = current.Mash(solver.Possibilities[row, col]);
                var newVisited = new HashSet<int>(visited) { row };
                if (newCurrent.Count <= _type)
                {
                    if (count - 1 == 0 && newCurrent.Count == _type)
                    {
                        if (_type == 1) solver.AddDefinitiveNumber(newCurrent.GetFirst(), row, col,
                            new NakedPossibilityLog(row, col, newCurrent.GetFirst(), _type));
                        else RemovePossibilitiesFromColumn(solver, col, newCurrent, newVisited);
                    }
                    else
                    {
                        RecursiveColumnMashing(solver, newCurrent, possibleRows, col, count - 1, newVisited);
                    }
                }
            }
        }
    }

    private void RemovePossibilitiesFromColumn(ISolver solver, int col, IPossibilities toRemove, HashSet<int> except)
    {
        foreach (var n in toRemove.GetPossibilities())
        {
            for (int row = 0; row < 9; row++)
            {
                if (solver.Sudoku[row, col] == 0 && !except.Contains(row))
                    solver.RemovePossibility(n, row, col, new NakedPossibilityLog(row, col, n, _type));
            }
        }
    }
    
    private List<int> EveryMiniGridCellWithLessPossibilities(ISolver solver, int miniRow, int miniCol, int than)
    {
        List<int> result = new();
        for (int gridNumber = 0; gridNumber < 9; gridNumber++)
        {
            int row = miniRow * 3 + gridNumber / 3;
            int col = miniCol * 3 + gridNumber % 3;
            
            if (solver.Sudoku[row, col] == 0 && solver.Possibilities[row, col].Count < than) 
                result.Add(gridNumber);
        }
        
        return result;
    }
    
    private void RecursiveMiniGridMashing(ISolver solver, IPossibilities current,
        List<int> possibleGridNumbers, int miniRow, int miniCol, int count, HashSet<int> visited)
    {
        foreach (var gridNumber in possibleGridNumbers)
        {
            if (!visited.Contains(gridNumber))
            {
                int row = miniRow * 3 + gridNumber / 3;
                int col = miniCol * 3 + gridNumber % 3;
                
                IPossibilities newCurrent = current.Mash(solver.Possibilities[row, col]);
                var newVisited = new HashSet<int>(visited) { gridNumber };

                if (newCurrent.Count <= _type)
                {
                    if (count - 1 == 0 && newCurrent.Count == _type)
                    {
                        if (_type == 1) solver.AddDefinitiveNumber(newCurrent.GetFirst(), row, col,
                            new NakedPossibilityLog(row, col, newCurrent.GetFirst(), _type));
                        else RemovePossibilitiesFromMiniGrid(solver, miniRow, miniCol, newCurrent, newVisited);
                    }
                    else
                    {
                        RecursiveMiniGridMashing(solver, newCurrent, possibleGridNumbers, miniRow, miniCol,
                            count - 1, newVisited);
                    }
                }
            }
        }
    }
    
    private void RemovePossibilitiesFromMiniGrid(ISolver solver, int miniRow, int miniCol, IPossibilities toRemove,
        HashSet<int> except)
    {
        foreach (var n in toRemove.GetPossibilities())
        {
            for (int gridNumber = 0; gridNumber < 9; gridNumber++)
            {
                int row = miniRow * 3 + gridNumber / 3;
                int col = miniCol * 3 + gridNumber % 3;
                
                if (solver.Sudoku[row, col] == 0 && !except.Contains(gridNumber))
                    solver.RemovePossibility(n, row, col, new NakedPossibilityLog(row, col, n, _type));
            }
        }
    }
}

public class NakedPossibilityLog : ISolverLog
{
    public string AsString { get; }
    public StrategyLevel Level { get; }

    public NakedPossibilityLog(int row, int col, int number, int type)
    {
        if (type == 1)
        {
            AsString = $"[{row + 1}, {col + 1}] {number} added as definitive because of hidden {type}";
            Level = StrategyLevel.Basic;
        }
        else
        {
            AsString = $"[{row + 1}, {col + 1}] {number} removed from possibilities because of naked {type}";
            Level = (StrategyLevel) type;
        }
        
    }
}