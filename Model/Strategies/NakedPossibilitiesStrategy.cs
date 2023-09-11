using System.Collections.Generic;
using System.Text;
using Model.Changes;
using Model.Positions;
using Model.Possibilities;
using Model.Solver;
using Model.StrategiesUtil;

namespace Model.Strategies;

/// <summary>
/// Naked possibilities, also called locked possibilities, happens when n cells shares n candidates in the same unit,
/// without any cell having any extra possibility. In that case, each cell must have one of the n candidates as a
/// solution. Therefor, any cell in that unit that is not in the n cells cannot have one of the n possibilities.
/// </summary>
public class NakedPossibilitiesStrategy : IStrategy
{
    public string Name { get; }
    public StrategyLevel Difficulty { get; }
    public StatisticsTracker Tracker { get; } = new();

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
        for (int row = 0; row < 9; row++)
        {
            var possibleCols = EveryRowCellWithLessPossibilities(strategyManager, row, _type + 1);
            RecursiveRowMashing(strategyManager, IPossibilities.NewEmpty(), possibleCols, -1, row, new LinePositions());
        }
        
        for (int col = 0; col < 9; col++)
        {
            var possibleRows = EveryColumnCellWithLessPossibilities(strategyManager, col, _type + 1);
            RecursiveColumnMashing(strategyManager, IPossibilities.NewEmpty(), possibleRows, -1, col, new LinePositions());
        }
        
        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                var possibleGridNumbers = EveryMiniGridCellWithLessPossibilities(strategyManager, miniRow, miniCol, _type + 1);
                RecursiveMiniGridMashing(strategyManager, IPossibilities.NewEmpty(), possibleGridNumbers, -1, miniRow, miniCol,
                    new MiniGridPositions(miniRow, miniCol));
            }
        }
    }

    private LinePositions EveryRowCellWithLessPossibilities(IStrategyManager strategyManager, int row, int than)
    {
        LinePositions result = new();
        for (int col = 0; col < 9; col++)
        {
            if (strategyManager.Sudoku[row, col] == 0 && strategyManager.PossibilitiesAt(row, col).Count < than)
                result.Add(col);
        }

        return result;
    }

    private void RecursiveRowMashing(IStrategyManager strategyManager, IPossibilities current,
        LinePositions possibleCols, int cursor, int row, LinePositions visited)
    {
        int col;
        while ((col = possibleCols.Next(ref cursor)) != -1)
        {
            var possibilities = strategyManager.PossibilitiesAt(row, col);
            if(possibilities.Count > _type) continue;
            
            var newCurrent = current.Or(possibilities);
            if (newCurrent.Count > _type) continue;
            
            var newVisited = visited.Copy();
            newVisited.Add(col);
            
            if (newVisited.Count == _type && newCurrent.Count == _type)
                RemovePossibilitiesFromRow(strategyManager, row, newCurrent, newVisited);
            else if (newVisited.Count < _type)
                RecursiveRowMashing(strategyManager, newCurrent, possibleCols, cursor, row, newVisited);
            
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
    
    private LinePositions EveryColumnCellWithLessPossibilities(IStrategyManager strategyManager, int col, int than)
    {
        LinePositions result = new();
        for (int row = 0; row < 9; row++)
        {
            if (strategyManager.Sudoku[row, col] == 0 && strategyManager.PossibilitiesAt(row, col).Count < than) 
                result.Add(row);
        }

        return result;
    }
    
    private void RecursiveColumnMashing(IStrategyManager strategyManager, IPossibilities current,
        LinePositions possibleRows, int cursor, int col, LinePositions visited)
    {
        int row;
        while((row = possibleRows.Next(ref cursor)) != -1)
        {
            var possibilities = strategyManager.PossibilitiesAt(row, col);
            if(possibilities.Count > _type) continue;
            
            var newCurrent = current.Or(possibilities);
            if (newCurrent.Count > _type) continue;
            
            var newVisited = visited.Copy();
            newVisited.Add(row);

            if (newVisited.Count == _type && newCurrent.Count == _type)
                RemovePossibilitiesFromColumn(strategyManager, col, newCurrent, newVisited);
            else if (newVisited.Count < _type)
                RecursiveColumnMashing(strategyManager, newCurrent, possibleRows, cursor, col, newVisited);
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
    
    private MiniGridPositions EveryMiniGridCellWithLessPossibilities(IStrategyManager strategyManager, int miniRow, int miniCol, int than)
    {
        MiniGridPositions result = new(miniRow, miniCol);
        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            for (int gridCol = 0; gridCol < 3; gridCol++)
            {
                int row = miniRow * 3 + gridRow;
                int col = miniCol * 3 + gridCol;
            
                if (strategyManager.Sudoku[row, col] == 0 && strategyManager.PossibilitiesAt(row, col).Count < than) 
                    result.Add(gridRow, gridCol);
            }
        }
        
        return result;
    }
    
    private void RecursiveMiniGridMashing(IStrategyManager strategyManager, IPossibilities current,
        MiniGridPositions possiblePos, int cursor, int miniRow, int miniCol, MiniGridPositions visited)
    {
        Cell pos;
        while((pos = possiblePos.Next(ref cursor)).Row != -1)
        {
            var possibilities = strategyManager.PossibilitiesAt(pos.Row, pos.Col);
            if(possibilities.Count > _type) continue;
            
            var newCurrent = current.Or(possibilities);
            if (newCurrent.Count > _type) continue;
            
            var newVisited = visited.Copy();
            newVisited.Add(pos.Row % 3, pos.Col % 3);
            
            if (newVisited.Count == _type && newCurrent.Count == _type)
                RemovePossibilitiesFromMiniGrid(strategyManager, miniRow, miniCol, newCurrent, newVisited);
            else if (newVisited.Count < _type)
                RecursiveMiniGridMashing(strategyManager, newCurrent, possiblePos, cursor, miniRow, miniCol, newVisited);
        }
    }
    
    private void RemovePossibilitiesFromMiniGrid(IStrategyManager strategyManager, int miniRow, int miniCol, IPossibilities toRemove,
        MiniGridPositions except)
    {
        foreach (var n in toRemove)
        {
            for (int gridRow = 0; gridRow < 3; gridRow++)
            {
                for (int gridCol = 0; gridCol < 3; gridCol++)
                {
                    int row = miniRow * 3 + gridRow;
                    int col = miniCol * 3 + gridCol;
                
                    if (!except.Peek(gridRow, gridCol)) strategyManager.ChangeBuffer.AddPossibilityToRemove(n, row, col);
                }
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

    public ChangeReport Build(List<SolverChange> changes, ISolver snapshot)
    {
        var coords = new List<CellPossibility>();
        foreach (var other in _linePos)
        {
            foreach (var possibility in _possibilities)
            {
                switch (_unit)
                {
                    case Unit.Row :
                        if(snapshot.PossibilitiesAt(_unitNumber, other).Peek(possibility))
                            coords.Add(new CellPossibility(_unitNumber, other, possibility));
                        break;
                    case Unit.Column :
                        if(snapshot.PossibilitiesAt(other, _unitNumber).Peek(possibility))
                            coords.Add(new CellPossibility(other, _unitNumber, possibility));
                        break;
                }
            }
        }

        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Explanation(), lighter =>
        {
            foreach (var coord in coords)
            {
                lighter.HighlightPossibility(coord.Possibility, coord.Row, coord.Col, ChangeColoration.CauseOffOne);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }

    private string Explanation()
    {
        var builder = new StringBuilder("The cells (");
        foreach (var other in _linePos)
        {
            switch (_unit)
            {
                case Unit.Row :
                    builder.Append(new Cell(_unitNumber, other) + " ");
                    break;
                case Unit.Column :
                    builder.Append(new Cell(other, _unitNumber) + " ");
                    break;
            }
        }
        
        return builder.ToString()[..^1] + $") only contains the possibilities ({_possibilities}). Any other cell in" +
               $" {_unit.ToString().ToLower()} {_unitNumber + 1} cannot contains these possibilities";
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
    
    public ChangeReport Build(List<SolverChange> changes, ISolver snapshot)
    {
        var coords = new List<CellPossibility>();
        foreach (var pos in _miniPos)
        {
            foreach (var possibility in _possibilities)
            {
                if(snapshot.PossibilitiesAt(pos.Row, pos.Col).Peek(possibility))
                    coords.Add(new CellPossibility(pos.Row, pos.Col, possibility));
            }
        }
        
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Explanation(), lighter =>
        {
            foreach (var coord in coords)
            {
                lighter.HighlightPossibility(coord.Possibility, coord.Row, coord.Col, ChangeColoration.CauseOffOne);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
    
    private string Explanation()
    {
        var builder = new StringBuilder("The cells (");
        foreach (var coord in _miniPos)
        {
            builder.Append(coord);
        }
        
        return builder.ToString()[..^1] + $") only contains the possibilities ({_possibilities}). Any other cell in" +
               $" mini grid {_miniPos.MiniGridNumber()} cannot contains these possibilities";
    }
}