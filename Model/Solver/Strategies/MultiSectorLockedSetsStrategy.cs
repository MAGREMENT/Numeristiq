using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.Positions;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Strategies;

public class MultiSectorLockedSetsStrategy : AbstractStrategy //TODO add other elims
{
    public const string OfficialName = "Multi-Sector Locked Sets";
    
    public MultiSectorLockedSetsStrategy() : base(OfficialName, StrategyDifficulty.Extreme) { }

    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int n = 1; n <= 9; n++)
        {
            for (int o = n + 1; o <= 9; o++)
            {
                for (int p = o + 1; p <= 9; p++)
                {
                    for (int q = p + 1; q <= 9; q++)
                    {
                        IPossibilities possibilities = IPossibilities.NewEmpty();
                        possibilities.Add(n);
                        possibilities.Add(o);
                        possibilities.Add(p);
                        possibilities.Add(q);

                        Try(strategyManager, possibilities);
                    }
                }
            }
        }
    }

    private void Try(IStrategyManager strategyManager, IPossibilities possibilities)
    {
        GridPositions gpRow = new GridPositions();
        GridPositions gpCol = new GridPositions();
        gpRow.Fill();
        gpCol.Fill();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var solved = strategyManager.Sudoku[row, col];
                if (solved == 0) continue;
                
                if (possibilities.Peek(solved))
                {
                    gpRow.VoidRow(row);
                    gpCol.VoidColumn(col);
                }
                else
                {
                    gpRow.VoidColumn(col);
                    gpCol.VoidRow(row);
                }
            }
        }

        TryRow(strategyManager, gpRow, possibilities);
        TryColumn(strategyManager, gpCol, possibilities);
    }

    private void TryRow(IStrategyManager strategyManager, GridPositions gp, IPossibilities possibilities)
    {
        if (gp.Count < possibilities.Count) return;
        
        int count = 0;
        
        for (int row = 0; row < 9; row++)
        {
            if (gp.RowCount(row) == 0) continue;

            int n = 0;
            for (int col = 0; col < 9; col++)
            {
                var solved = strategyManager.Sudoku[row, col];
                if (solved == 0) continue;

                if (!possibilities.Peek(solved)) n++;
            }

            count += 9 - possibilities.Count - n;
        }

        for (int col = 0; col < 9; col++)
        {
            if (gp.ColumnCount(col) == 0) continue;

            int n = 0;
            for (int row = 0; row < 9; row++)
            {
                var solved = strategyManager.Sudoku[row, col];
                if (solved == 0) continue;

                if (possibilities.Peek(solved)) n++;
            }
            
            count += possibilities.Count - n;
        }

        ProcessRow(strategyManager, gp, possibilities, count);
    }

    private void TryColumn(IStrategyManager strategyManager, GridPositions gp, IPossibilities possibilities)
    {
        if (gp.Count < possibilities.Count) return;
        
        int count = 0;
        
        for (int col = 0; col < 9; col++)
        {
            if (gp.ColumnCount(col) == 0) continue;

            int n = 0;
            for (int row = 0; row < 9; row++)
            {
                var solved = strategyManager.Sudoku[row, col];
                if (solved == 0) continue;

                if (!possibilities.Peek(solved)) n++;
            }
            
            count += 9 - possibilities.Count - n;
        }
        
        for (int row = 0; row < 9; row++)
        {
            if (gp.RowCount(row) == 0) continue;

            int n = 0;
            for (int col = 0; col < 9; col++)
            {
                var solved = strategyManager.Sudoku[row, col];
                if (solved == 0) continue;

                if (possibilities.Peek(solved)) n++;
            }

            count += possibilities.Count - n;
        }

        ProcessCol(strategyManager, gp, possibilities, count);
    }

    private void ProcessRow(IStrategyManager strategyManager, GridPositions gp, IPossibilities possibilities, int count)
    {
        if (count != gp.Count) return;
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var rowCount = gp.RowCount(row);
                var colCount = gp.ColumnCount(col);

                switch (rowCount)
                {
                    case > 0 when colCount == 0:
                        for (int possibility = 1; possibility <= 9; possibility++)
                        {
                            if (possibilities.Peek(possibility)) continue;

                            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
                        }

                        break;
                    case 0 when colCount > 0:
                        foreach (var possibility in possibilities)
                        {
                            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
                        }

                        break;
                }
            }
        }

        strategyManager.ChangeBuffer.Push(this, new MultiSectorLockedSetsReportBuilder(gp, possibilities));
    }
    
    private void ProcessCol(IStrategyManager strategyManager, GridPositions gp, IPossibilities possibilities, int count)
    {
        if (count != gp.Count) return;
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var rowCount = gp.RowCount(row);
                var colCount = gp.ColumnCount(col);

                switch (rowCount)
                {
                    case > 0 when colCount == 0:
                        foreach (var possibility in possibilities)
                        {
                            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
                        }

                        break;
                    case 0 when colCount > 0:
                        for (int possibility = 1; possibility <= 9; possibility++)
                        {
                            if (possibilities.Peek(possibility)) continue;

                            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
                        }

                        break;
                }
            }
        }

        strategyManager.ChangeBuffer.Push(this, new MultiSectorLockedSetsReportBuilder(gp, possibilities));
    }
}

public class MultiSectorLockedSetsReportBuilder : IChangeReportBuilder
{
    private readonly GridPositions _gp;
    private readonly IPossibilities _possibilities;

    public MultiSectorLockedSetsReportBuilder(GridPositions gp, IPossibilities possibilities)
    {
        _gp = gp;
        _possibilities = possibilities;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        List<Cell> homeSet = new();
        List<Cell> awaySet = new();

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var solved = snapshot.Sudoku[row, col];
                if (solved == 0) continue;

                if (_possibilities.Peek(solved)) homeSet.Add(new Cell(row, col));
                else awaySet.Add(new Cell(row, col));
            }
        }

        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), _possibilities.ToString()!, lighter =>
        {
            foreach (var cell in homeSet)
            {
                lighter.HighlightCell(cell, ChangeColoration.CauseOnOne);
            }

            foreach (var cell in awaySet)
            {
                lighter.HighlightCell(cell, ChangeColoration.CauseOffOne);
            }
            
            foreach (var cell in _gp)
            {
                lighter.HighlightCell(cell, ChangeColoration.Neutral);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}