using System.Collections.Generic;
using Model.Changes;
using Model.Positions;
using Model.Possibilities;
using Model.Solver;
using Model.StrategiesUtil;

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
            case 2 : Name = "Naked double";
                Difficulty = StrategyLevel.Easy;
                break;
            case 3 : Name = "Naked triple";
                Difficulty = StrategyLevel.Easy;
                break;
            case 4 : Name = "Naked quad";
                Difficulty = StrategyLevel.Easy;
                break;
            default : Name = "Naked unknown";
                Difficulty = StrategyLevel.None;
                break;
        }
    }
    
    
    public void ApplyOnce(IStrategyManager strategyManager)
    {
        //Rows
        for (int row = 0; row < 9; row++)
        {
            IPossibilities empty = IPossibilities.New();
            empty.RemoveAll();
            var possibleCols = EveryRowCellWithLessPossibilities(strategyManager, row, _type + 1);
            RecursiveRowMashing(strategyManager, empty, possibleCols, row, _type, new LinePositions());
        }
        
        //Cols
        for (int col = 0; col < 9; col++)
        {
            IPossibilities empty = IPossibilities.New();
            empty.RemoveAll();
            var possibleRows = EveryColumnCellWithLessPossibilities(strategyManager, col, _type + 1);
            RecursiveColumnMashing(strategyManager, empty, possibleRows, col, _type, new LinePositions());
        }
        
        //MiniGrid
        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                IPossibilities empty = IPossibilities.New();
                empty.RemoveAll();
                var possibleGridNumbers = EveryMiniGridCellWithLessPossibilities(strategyManager, miniRow, miniCol, _type + 1);
                RecursiveMiniGridMashing(strategyManager, empty, possibleGridNumbers, miniRow, miniCol,
                    _type, new MiniGridPositions(miniRow, miniCol));
            }
        }
    }

    private Queue<int> EveryRowCellWithLessPossibilities(IStrategyManager strategyManager, int row, int than)
    {
        Queue<int> result = new();
        for (int col = 0; col < 9; col++)
        {
            if (strategyManager.Sudoku[row, col] == 0 && strategyManager.Possibilities[row, col].Count < than) 
                result.Enqueue(col);
        }

        return result;
    }

    private void RecursiveRowMashing(IStrategyManager strategyManager, IPossibilities current,
        Queue<int> possibleCols, int row, int count, LinePositions visited)
    {
        while (possibleCols.Count > 0)
        {
            int col = possibleCols.Dequeue();

            IPossibilities newCurrent = current.Mash(strategyManager.Possibilities[row, col]);
            var newVisited = visited.Copy();
            newVisited.Add(col);
            
            if (newCurrent.Count <= _type)
            {
                if (count - 1 == 0 && newCurrent.Count == _type)
                {
                    RemovePossibilitiesFromRow(strategyManager, row, newCurrent, newVisited);
                }
                else
                {
                    RecursiveRowMashing(strategyManager, newCurrent,
                        new Queue<int>(possibleCols), row, count - 1, newVisited);
                }
            }
        }
    }

    private void RemovePossibilitiesFromRow(IStrategyManager strategyManager, int row, IPossibilities toRemove, LinePositions except)
    {
        foreach (var n in toRemove)
        {
            for (int col = 0; col < 9; col++)
            {
                if (!except.Peek(col)) strategyManager.ChangeBuffer.AddPossibilityToRemove(n, row, col);
            }
        }
        
        strategyManager.ChangeBuffer.Push(this, new LineNakedPossibilitiesReportBuilder(toRemove, except, row, Unit.Row));
    }
    
    private Queue<int> EveryColumnCellWithLessPossibilities(IStrategyManager strategyManager, int col, int than)
    {
        Queue<int> result = new();
        for (int row = 0; row < 9; row++)
        {
            if (strategyManager.Sudoku[row, col] == 0 && strategyManager.Possibilities[row, col].Count < than) 
                result.Enqueue(row);
        }

        return result;
    }
    
    private void RecursiveColumnMashing(IStrategyManager strategyManager, IPossibilities current,
        Queue<int> possibleRows, int col, int count, LinePositions visited)
    {
        while(possibleRows.Count > 0)
        {
            int row = possibleRows.Dequeue();

            IPossibilities newCurrent = current.Mash(strategyManager.Possibilities[row, col]);
            var newVisited = visited.Copy();
            newVisited.Add(row);
            
            if (newCurrent.Count <= _type)
            {
                if (count - 1 == 0 && newCurrent.Count == _type)
                {
                    RemovePossibilitiesFromColumn(strategyManager, col, newCurrent, newVisited);
                }
                else
                {
                    RecursiveColumnMashing(strategyManager, newCurrent, new Queue<int>(possibleRows),
                        col, count - 1, newVisited);
                }
            }
        }
    }

    private void RemovePossibilitiesFromColumn(IStrategyManager strategyManager, int col, IPossibilities toRemove, LinePositions except)
    {
        foreach (var n in toRemove)
        {
            for (int row = 0; row < 9; row++)
            {
                if (!except.Peek(row)) strategyManager.ChangeBuffer.AddPossibilityToRemove(n, row, col);
            }
        }
        
        strategyManager.ChangeBuffer.Push(this, new LineNakedPossibilitiesReportBuilder(toRemove, except, col, Unit.Column));
    }
    
    private Queue<int> EveryMiniGridCellWithLessPossibilities(IStrategyManager strategyManager, int miniRow, int miniCol, int than)
    {
        Queue<int> result = new();
        for (int gridNumber = 0; gridNumber < 9; gridNumber++)
        {
            int row = miniRow * 3 + gridNumber / 3;
            int col = miniCol * 3 + gridNumber % 3;
            
            if (strategyManager.Sudoku[row, col] == 0 && strategyManager.Possibilities[row, col].Count < than) 
                result.Enqueue(gridNumber);
        }
        
        return result;
    }
    
    private void RecursiveMiniGridMashing(IStrategyManager strategyManager, IPossibilities current,
        Queue<int> possibleGridNumbers, int miniRow, int miniCol, int count, MiniGridPositions visited)
    {
        while(possibleGridNumbers.Count > 0)
        {
            int gridNumber = possibleGridNumbers.Dequeue();
            
            int row = miniRow * 3 + gridNumber / 3;
            int col = miniCol * 3 + gridNumber % 3;
            
            IPossibilities newCurrent = current.Mash(strategyManager.Possibilities[row, col]);
            var newVisited = visited.Copy();
            newVisited.Add(gridNumber);

            if (newCurrent.Count <= _type)
            {
                if (count - 1 == 0 && newCurrent.Count == _type)
                {
                    RemovePossibilitiesFromMiniGrid(strategyManager, miniRow, miniCol, newCurrent, newVisited);
                }
                else
                {
                    RecursiveMiniGridMashing(strategyManager, newCurrent, new Queue<int>(possibleGridNumbers),
                        miniRow, miniCol, count - 1, newVisited);
                }
            }
        }
    }
    
    private void RemovePossibilitiesFromMiniGrid(IStrategyManager strategyManager, int miniRow, int miniCol, IPossibilities toRemove,
        MiniGridPositions except)
    {
        foreach (var n in toRemove)
        {
            for (int gridNumber = 0; gridNumber < 9; gridNumber++)
            {
                int row = miniRow * 3 + gridNumber / 3;
                int col = miniCol * 3 + gridNumber % 3;
                
                if (!except.Peek(gridNumber)) strategyManager.ChangeBuffer.AddPossibilityToRemove(n, row, col);
            }
        }
        
        strategyManager.ChangeBuffer.Push(this, new MiniGridNakedPossibilitiesReportBuilder(toRemove, except));
    }
}

public class LineNakedPossibilitiesReportBuilder : IChangeReportBuilder
{
    private readonly IPossibilities _possibilities;
    private readonly LinePositions _linePos;
    private readonly int _unitNumber;
    private readonly Unit _unit;


    public LineNakedPossibilitiesReportBuilder(IPossibilities possibilities, LinePositions linePos, int unitNumber, Unit unit)
    {
        _possibilities = possibilities;
        _linePos = linePos;
        _unitNumber = unitNumber;
        _unit = unit;
    }

    public ChangeReport Build(List<SolverChange> changes, IChangeManager manager)
    {
        var coords = new List<PossibilityCoordinate>();
        switch (_unit)
        {
            case Unit.Row :
                foreach (var col in _linePos)
                {
                    foreach (var possibility in _possibilities)
                    {
                        if(manager.Possibilities[_unitNumber, col].Peek(possibility))
                            coords.Add(new PossibilityCoordinate(_unitNumber, col, possibility));
                    }
                }
                break;
            case Unit.Column :
                foreach (var row in _linePos)
                {
                    foreach (var possibility in _possibilities)
                    {
                        if(manager.Possibilities[row, _unitNumber].Peek(possibility))
                            coords.Add(new PossibilityCoordinate(row, _unitNumber, possibility));
                    }
                }
                break;
        }
        
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var coord in coords)
            {
                lighter.HighlightPossibility(coord.Possibility, coord.Row, coord.Col, ChangeColoration.CauseOffOne);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}

public class MiniGridNakedPossibilitiesReportBuilder : IChangeReportBuilder
{
    private readonly IPossibilities _possibilities;
    private readonly MiniGridPositions _miniPos;

    public MiniGridNakedPossibilitiesReportBuilder(IPossibilities possibilities, MiniGridPositions miniPos)
    {
        _possibilities = possibilities;
        _miniPos = miniPos;
    }
    
    public ChangeReport Build(List<SolverChange> changes, IChangeManager manager)
    {
        var coords = new List<PossibilityCoordinate>();
        foreach (var pos in _miniPos)
        {
            foreach (var possibility in _possibilities)
            {
                if(manager.Possibilities[pos[0], pos[1]].Peek(possibility))
                    coords.Add(new PossibilityCoordinate(pos[0], pos[1], possibility));
            }
        }
        
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var coord in coords)
            {
                lighter.HighlightPossibility(coord.Possibility, coord.Row, coord.Col, ChangeColoration.CauseOffOne);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}