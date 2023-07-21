using System.Collections.Generic;
using System.Linq;
using Model.Positions;
using Model.Possibilities;

namespace Model.Strategies;

public class NakedPossibilitiesStrategy : IStrategy
{
    public string Name { get; }

    public StrategyLevel Difficulty { get; }
    public int Score { get; set; }

    private readonly int _type;

    public NakedPossibilitiesStrategy(int type)
    {
        _type = type;
        switch (type)
        {
            case 1 : Name = "Naked single";
                Difficulty = StrategyLevel.Basic;
                break;
            case 2 : Name = "Naked double";
                Difficulty = StrategyLevel.Easy;
                break;
            case 3 : Name = "Naked triple";
                Difficulty = StrategyLevel.Easy;
                break;
            case 4 : Name = "Naked quad";
                Difficulty = StrategyLevel.Medium;
                break;
            default : Name = "Naked unknown";
                Difficulty = StrategyLevel.None;
                break;
        }
    }
    
    
    public void ApplyOnce(ISolverView solverView)
    {
        //Rows
        for (int row = 0; row < 9; row++)
        {
            IPossibilities empty = IPossibilities.New();
            empty.RemoveAll();
            var possibleCols = EveryRowCellWithLessPossibilities(solverView, row, _type + 1);
            RecursiveRowMashing(solverView, empty, possibleCols, row, _type, new LinePositions());
        }
        
        //Cols
        for (int col = 0; col < 9; col++)
        {
            IPossibilities empty = IPossibilities.New();
            empty.RemoveAll();
            var possibleRows = EveryColumnCellWithLessPossibilities(solverView, col, _type + 1);
            RecursiveColumnMashing(solverView, empty, possibleRows, col, _type, new LinePositions());
        }
        
        //MiniGrid
        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                IPossibilities empty = IPossibilities.New();
                empty.RemoveAll();
                var possibleGridNumbers = EveryMiniGridCellWithLessPossibilities(solverView, miniRow, miniCol, _type + 1);
                RecursiveMiniGridMashing(solverView, empty, possibleGridNumbers, miniRow, miniCol,
                    _type, new MiniGridPositions(miniRow, miniCol));
            }
        }
    }
    
    private Queue<int> EveryRowCellWithLessPossibilities(ISolverView solverView, int row, int than)
    {
        Queue<int> result = new();
        for (int col = 0; col < 9; col++)
        {
            if (solverView.Sudoku[row, col] == 0 && solverView.Possibilities[row, col].Count < than) 
                result.Enqueue(col);
        }

        return result;
    }

    private void RecursiveRowMashing(ISolverView solverView, IPossibilities current,
        Queue<int> possibleCols, int row, int count, LinePositions visited)
    {
        while (possibleCols.Count > 0)
        {
            int col = possibleCols.Dequeue();

            IPossibilities newCurrent = current.Mash(solverView.Possibilities[row, col]);
            var newVisited = visited.Copy();
            newVisited.Add(col);
            
            if (newCurrent.Count <= _type)
            {
                if (count - 1 == 0 && newCurrent.Count == _type)
                {
                    if (_type == 1) solverView.AddDefinitiveNumber(newCurrent.First(), row, col, this);
                    else RemovePossibilitiesFromRow(solverView, row, newCurrent, newVisited);
                }
                else
                {
                    RecursiveRowMashing(solverView, newCurrent,
                        new Queue<int>(possibleCols), row, count - 1, newVisited);
                }
            }
        }
    }

    private void RemovePossibilitiesFromRow(ISolverView solverView, int row, IPossibilities toRemove, LinePositions except)
    {
        foreach (var n in toRemove)
        {
            for (int col = 0; col < 9; col++)
            {
                if (solverView.Sudoku[row, col] == 0 && !except.Peek(col))
                    solverView.RemovePossibility(n, row, col, this);
            }
        }
    }
    
    private Queue<int> EveryColumnCellWithLessPossibilities(ISolverView solverView, int col, int than)
    {
        Queue<int> result = new();
        for (int row = 0; row < 9; row++)
        {
            if (solverView.Sudoku[row, col] == 0 && solverView.Possibilities[row, col].Count < than) 
                result.Enqueue(row);
        }

        return result;
    }
    
    private void RecursiveColumnMashing(ISolverView solverView, IPossibilities current,
        Queue<int> possibleRows, int col, int count, LinePositions visited)
    {
        while(possibleRows.Count > 0)
        {
            int row = possibleRows.Dequeue();

            IPossibilities newCurrent = current.Mash(solverView.Possibilities[row, col]);
            var newVisited = visited.Copy();
            newVisited.Add(row);
            
            if (newCurrent.Count <= _type)
            {
                if (count - 1 == 0 && newCurrent.Count == _type)
                {
                    if (_type == 1) solverView.AddDefinitiveNumber(newCurrent.First(), row, col, this);
                    else RemovePossibilitiesFromColumn(solverView, col, newCurrent, newVisited);
                }
                else
                {
                    RecursiveColumnMashing(solverView, newCurrent, new Queue<int>(possibleRows),
                        col, count - 1, newVisited);
                }
            }
        }
    }

    private void RemovePossibilitiesFromColumn(ISolverView solverView, int col, IPossibilities toRemove, LinePositions except)
    {
        foreach (var n in toRemove)
        {
            for (int row = 0; row < 9; row++)
            {
                if (solverView.Sudoku[row, col] == 0 && !except.Peek(row))
                    solverView.RemovePossibility(n, row, col, this);
            }
        }
    }
    
    private Queue<int> EveryMiniGridCellWithLessPossibilities(ISolverView solverView, int miniRow, int miniCol, int than)
    {
        Queue<int> result = new();
        for (int gridNumber = 0; gridNumber < 9; gridNumber++)
        {
            int row = miniRow * 3 + gridNumber / 3;
            int col = miniCol * 3 + gridNumber % 3;
            
            if (solverView.Sudoku[row, col] == 0 && solverView.Possibilities[row, col].Count < than) 
                result.Enqueue(gridNumber);
        }
        
        return result;
    }
    
    private void RecursiveMiniGridMashing(ISolverView solverView, IPossibilities current,
        Queue<int> possibleGridNumbers, int miniRow, int miniCol, int count, MiniGridPositions visited)
    {
        while(possibleGridNumbers.Count > 0)
        {
            int gridNumber = possibleGridNumbers.Dequeue();
            
            int row = miniRow * 3 + gridNumber / 3;
            int col = miniCol * 3 + gridNumber % 3;
            
            IPossibilities newCurrent = current.Mash(solverView.Possibilities[row, col]);
            var newVisited = visited.Copy();
            newVisited.Add(gridNumber);

            if (newCurrent.Count <= _type)
            {
                if (count - 1 == 0 && newCurrent.Count == _type)
                {
                    if (_type == 1) solverView.AddDefinitiveNumber(newCurrent.First(), row, col, this);
                    else RemovePossibilitiesFromMiniGrid(solverView, miniRow, miniCol, newCurrent, newVisited);
                }
                else
                {
                    RecursiveMiniGridMashing(solverView, newCurrent, new Queue<int>(possibleGridNumbers),
                        miniRow, miniCol, count - 1, newVisited);
                }
            }
        }
    }
    
    private void RemovePossibilitiesFromMiniGrid(ISolverView solverView, int miniRow, int miniCol, IPossibilities toRemove,
        MiniGridPositions except)
    {
        foreach (var n in toRemove)
        {
            for (int gridNumber = 0; gridNumber < 9; gridNumber++)
            {
                int row = miniRow * 3 + gridNumber / 3;
                int col = miniCol * 3 + gridNumber % 3;
                
                if (solverView.Sudoku[row, col] == 0 && !except.PeekFromGridPositions(gridNumber))
                    solverView.RemovePossibility(n, row, col, this);
            }
        }
    }
}