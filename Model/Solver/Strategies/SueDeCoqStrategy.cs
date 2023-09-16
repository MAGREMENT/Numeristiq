using System.Collections.Generic;
using Model.Solver.Helpers;
using Model.Solver.Helpers.Changes;
using Model.Solver.Positions;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Strategies;

public class SueDeCoqStrategy : IStrategy
{
    public string Name => "Sue-De-Coq";
    public StrategyLevel Difficulty => StrategyLevel.Extreme;
    public StatisticsTracker Tracker { get; } = new();
    
    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                LinePositions cols = new();

                for (int gridCol = 0; gridCol < 3; gridCol++)
                {
                    int col = miniCol * 3 + gridCol;

                    if (strategyManager.Sudoku[row, col] == 0) cols.Add(col);
                }

                if (cols.Count < 2) continue;
                TrySearchRow(strategyManager, cols, row);
                
                if(cols.Count != 3) continue;
                foreach (var col in cols)
                {
                    var copy = cols.Copy();
                    copy.Remove(col);
                    
                    TrySearchRow(strategyManager, copy, row);
                }
            }
        }

        for (int col = 0; col < 9; col++)
        {
            for (int miniRow = 0; miniRow < 3; miniRow++)
            {
                LinePositions rows = new();

                for (int gridRow = 0; gridRow < 3; gridRow++)
                {
                    int row = miniRow * 3 + gridRow;

                    if (strategyManager.Sudoku[row, col] == 0) rows.Add(row);
                }

                if (rows.Count < 2) continue;
                TrySearchColumn(strategyManager, rows, col);

                if (rows.Count != 3) continue;
                foreach (var row in rows)
                {
                    var copy = rows.Copy();
                    copy.Remove(row);

                    TrySearchColumn(strategyManager, copy, col);
                }
            }
        }
        
    }

    private void TrySearchRow(IStrategyManager strategyManager, IReadOnlyLinePositions cols, int row)
    {
        IPossibilities possibilities = IPossibilities.NewEmpty();
        foreach (var col in cols)
        {
            possibilities.Add(strategyManager.PossibilitiesAt(row, col));
        }

        if (possibilities.Count - 2 < cols.Count) return;

        SearchRow(strategyManager, cols, possibilities, row);
    }

    private void SearchRow(IStrategyManager strategyManager, IReadOnlyLinePositions cols,
        IReadOnlyPossibilities possibilities, int row)
    {
        List<Cell> rowCoords = new();
        List<Cell> miniCoords = new();
        for (int col = 0; col < 9; col++)
        {
            if (strategyManager.Sudoku[row, col] != 0 || cols.Peek(col)) continue;

            rowCoords.Add(new Cell(row, col));
        }

        int miniRow = row / 3;
        int miniCol = cols.GetFirst() / 3;

        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            for (int gridCol = 0; gridCol < 3; gridCol++)
            {
                int searchedRow = miniRow * 3 + gridRow;
                int searchedCol = miniCol * 3 + gridCol;
                if (strategyManager.Sudoku[searchedRow, searchedCol] != 0
                    || (searchedRow == row && cols.Peek(searchedCol))) continue;

                miniCoords.Add(new Cell(searchedRow, searchedCol));
            }
        }

        var rowAls = AlmostLockedSet.SearchForAls(strategyManager, rowCoords, 4);
        var miniAls = AlmostLockedSet.SearchForAls(strategyManager, miniCoords, 4);

        foreach (var rAls in rowAls)
        {
            foreach (var mAls in miniAls)
            {
                if (rAls.Possibilities.Or(mAls.Possibilities).Equals(possibilities))
                    ProcessSueDeCoq(strategyManager, row, cols, rAls, mAls, Unit.Row);
            }
        }
    }
    
    private void TrySearchColumn(IStrategyManager strategyManager, IReadOnlyLinePositions rows, int col)
    {
        IPossibilities possibilities = IPossibilities.NewEmpty();
        foreach (var row in rows)
        {
            possibilities.Add(strategyManager.PossibilitiesAt(row, col));
        }

        if (possibilities.Count - 2 < rows.Count) return;

        SearchColumn(strategyManager, rows, possibilities, col);
    }

    private void SearchColumn(IStrategyManager strategyManager, IReadOnlyLinePositions rows,
        IReadOnlyPossibilities possibilities, int col)
    {
        List<Cell> colCoords = new();
        List<Cell> miniCoords = new();
        for (int row = 0; row < 9; row++)
        {
            if (strategyManager.Sudoku[row, col] != 0 || rows.Peek(row)) continue;

            colCoords.Add(new Cell(row, col));
        }

        int miniCol = col / 3;
        int miniRow = rows.GetFirst() / 3;
        for (int gridCol = 0; gridCol < 3; gridCol++)
        { 
            for (int gridRow = 0; gridRow < 3; gridRow++)
            {
                int searchedCol = miniCol * 3 + gridCol;
                int searchedRow = miniRow * 3 + gridRow;
                if (strategyManager.Sudoku[searchedRow, searchedCol] != 0 
                    || (searchedCol == col && rows.Peek(searchedRow))) continue;

                miniCoords.Add(new Cell(searchedRow, searchedCol));
            }
        }

        var colAls = AlmostLockedSet.SearchForAls(strategyManager, colCoords, 4);
        var miniAls = AlmostLockedSet.SearchForAls(strategyManager, miniCoords, 4);

        foreach (var cAls in colAls)
        {
            foreach (var mAls in miniAls)
            {
                if (cAls.Possibilities.Or(mAls.Possibilities).Equals(possibilities))
                    ProcessSueDeCoq(strategyManager, col, rows, cAls, mAls, Unit.Column);
            }
        }
    }

    private void ProcessSueDeCoq(IStrategyManager strategyManager, int unitNumber, IReadOnlyLinePositions center,
       AlmostLockedSet unitAls, AlmostLockedSet miniAls, Unit unit)
    {
        for (int other = 0; other < 9; other++)
        {
            Cell current = unit == Unit.Row
                ? new Cell(unitNumber, other)
                : new Cell(other, unitNumber);
            if(unitAls.Contains(current) || center.Peek(other)) continue;

            foreach (var possibility in unitAls.Possibilities)
            {
                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, current.Row, current.Col);
            }
        }

        var unitStart = unitNumber / 3 * 3;
        var otherStart = center.GetFirst() / 3 * 3;
        for (int gridUnit = 0; gridUnit < 3; gridUnit++)
        {
            if (gridUnit + unitStart == unitNumber) continue;
            for (int gridOther = 0; gridOther < 3; gridOther++)
            {
                Cell current = unit == Unit.Row 
                    ? new Cell(unitStart + gridUnit, otherStart + gridOther) 
                    : new Cell(otherStart + gridOther, unitStart + gridUnit);
                if (miniAls.Contains(current)) continue;
                
                foreach (var possibility in miniAls.Possibilities)
                {
                    strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, current.Row, current.Col);
                }
            }
        }

        strategyManager.ChangeBuffer.Push(this, new SueDeCoqReportBuilder(unitNumber, center, miniAls, unitAls, unit));
    }
}

public class SueDeCoqReportBuilder : IChangeReportBuilder
{
    private readonly int _unitNumber;
    private readonly IReadOnlyLinePositions _positions;
    private readonly AlmostLockedSet _miniAls;
    private readonly AlmostLockedSet _unitAls;
    private readonly Unit _unit;

    public SueDeCoqReportBuilder(int unitNumber, IReadOnlyLinePositions positions, AlmostLockedSet miniAls,
        AlmostLockedSet unitAls, Unit unit)
    {
        _unitNumber = unitNumber;
        _positions = positions;
        _miniAls = miniAls;
        _unitAls = unitAls;
        _unit = unit;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        List<Cell> center = new(_positions.Count);
        foreach (var other in _positions)
        {
            center.Add(_unit == Unit.Row ?
                new Cell(_unitNumber, other) :
                new Cell(other, _unitNumber));
        }
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "",
            lighter =>
            {
                foreach (var coord in center)
                {
                    lighter.HighlightCell(coord.Row, coord.Col, ChangeColoration.Neutral);
                }

                foreach (var coord in _unitAls.Coordinates)
                {
                    lighter.HighlightCell(coord.Row, coord.Col, ChangeColoration.CauseOffOne);
                }

                foreach (var coord in _miniAls.Coordinates)
                {
                    lighter.HighlightCell(coord.Row, coord.Col, ChangeColoration.CauseOffTwo);
                }
                IChangeReportBuilder.HighlightChanges(lighter, changes);
            });
    }
}