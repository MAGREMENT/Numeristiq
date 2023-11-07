using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.Positions;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.AlmostLockedSets;

namespace Model.Solver.Strategies;

public class SueDeCoqStrategy : AbstractStrategy
{
    public const string OfficialName = "Sue-De-Coq";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public SueDeCoqStrategy() : base(OfficialName, StrategyDifficulty.Extreme, DefaultBehavior) {}
    
    public override void Apply(IStrategyManager strategyManager)
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
                if (TrySearchRow(strategyManager, cols, row)) return;
                
                if(cols.Count != 3) continue;
                foreach (var col in cols)
                {
                    var copy = cols.Copy();
                    copy.Remove(col);

                    if (TrySearchRow(strategyManager, copy, row)) return;
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
                if (TrySearchColumn(strategyManager, rows, col)) return;

                if (rows.Count != 3) continue;
                foreach (var row in rows)
                {
                    var copy = rows.Copy();
                    copy.Remove(row);

                    if (TrySearchColumn(strategyManager, copy, col)) return;
                }
            }
        }
        
    }

    private bool TrySearchRow(IStrategyManager strategyManager, IReadOnlyLinePositions cols, int row)
    {
        IPossibilities possibilities = IPossibilities.NewEmpty();
        foreach (var col in cols)
        {
            possibilities.Add(strategyManager.PossibilitiesAt(row, col));
        }

        if (possibilities.Count - 2 < cols.Count) return false;

        return SearchRow(strategyManager, cols, possibilities, row);
    }

    private bool SearchRow(IStrategyManager strategyManager, IReadOnlyLinePositions cols,
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
        int miniCol = cols.First() / 3;

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

        var rowAls = strategyManager.AlmostNakedSetSearcher.InCells(rowCoords);
        var miniAls = strategyManager.AlmostNakedSetSearcher.InCells(miniCoords);

        foreach (var rAls in rowAls)
        {
            foreach (var mAls in miniAls)
            {
                if (rAls.Possibilities.Or(mAls.Possibilities).Equals(possibilities) && ProcessSueDeCoq(strategyManager,
                        row, cols, rAls, mAls, Unit.Row)) return true;
            }
        }

        return false;
    }
    
    private bool TrySearchColumn(IStrategyManager strategyManager, IReadOnlyLinePositions rows, int col)
    {
        IPossibilities possibilities = IPossibilities.NewEmpty();
        foreach (var row in rows)
        {
            possibilities.Add(strategyManager.PossibilitiesAt(row, col));
        }

        if (possibilities.Count - 2 < rows.Count) return false;

        return SearchColumn(strategyManager, rows, possibilities, col);
    }

    private bool SearchColumn(IStrategyManager strategyManager, IReadOnlyLinePositions rows,
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
        int miniRow = rows.First() / 3;
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

        var colAls = strategyManager.AlmostNakedSetSearcher.InCells(colCoords);
        var miniAls = strategyManager.AlmostNakedSetSearcher.InCells(miniCoords);

        foreach (var cAls in colAls)
        {
            foreach (var mAls in miniAls)
            {
                if (cAls.Possibilities.Or(mAls.Possibilities).Equals(possibilities) && ProcessSueDeCoq(strategyManager,
                        col, rows, cAls, mAls, Unit.Column)) return true;
            }
        }

        return false;
    }

    private bool ProcessSueDeCoq(IStrategyManager strategyManager, int unitNumber, IReadOnlyLinePositions center,
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
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, current.Row, current.Col);
            }
        }

        var unitStart = unitNumber / 3 * 3;
        var otherStart = center.First() / 3 * 3;
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
                    strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, current.Row, current.Col);
                }
            }
        }

        return strategyManager.ChangeBuffer.Commit(this, new SueDeCoqReportBuilder(unitNumber, center, miniAls,
            unitAls, unit)) && OnCommitBehavior == OnCommitBehavior.Return;
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

                foreach (var coord in _unitAls.Cells)
                {
                    lighter.HighlightCell(coord.Row, coord.Col, ChangeColoration.CauseOffOne);
                }

                foreach (var coord in _miniAls.Cells)
                {
                    lighter.HighlightCell(coord.Row, coord.Col, ChangeColoration.CauseOffTwo);
                }
                IChangeReportBuilder.HighlightChanges(lighter, changes);
            });
    }
}