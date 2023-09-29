using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.Positions;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Strategies;

/// <summary>
/// http://forum.enjoysudoku.com/using-multi-sector-locked-sets-t31222.html
/// </summary>
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

                        Try(strategyManager, possibilities, possibilities.Invert());
                    }
                }
            }
        }
    }

    private void Try(IStrategyManager strategyManager, IPossibilities home, IPossibilities away)
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
                
                if (home.Peek(solved))
                {
                    gpRow.VoidRow(row);
                    gpCol.VoidColumn(col);
                }
                
                if(away.Peek(solved))
                {
                    gpRow.VoidColumn(col);
                    gpCol.VoidRow(row);
                }
            }
        }

        Try(strategyManager, gpRow, home, away, Unit.Row);
        Try(strategyManager, gpCol, home, away, Unit.Column);
    }

    private void Try(IStrategyManager strategyManager, GridPositions gp, IPossibilities home, IPossibilities away, Unit unit)
    {
        if (gp.Count < 9) return;

        IPossibilities one;
        IPossibilities two;
        if (unit == Unit.Row)
        {
            one = home;
            two = away;
        }
        else
        {
            one = away;
            two = home;
        }
        
        int count = 0;
        
        for (int row = 0; row < 9; row++)
        {
            if (gp.RowCount(row) == 0) continue;

            int n = 0;
            for (int col = 0; col < 9; col++)
            {
                if (two.Peek(strategyManager.Sudoku[row, col])) n++;
            }

            count += two.Count - n;
        }

        for (int col = 0; col < 9; col++)
        {
            if (gp.ColumnCount(col) == 0) continue;

            int n = 0;
            for (int row = 0; row < 9; row++)
            {
                if (one.Peek(strategyManager.Sudoku[row, col])) n++;
            }
            
            count += one.Count - n;
        }

        Process(strategyManager, gp, home, away, count, unit);
    }

    private void Process(IStrategyManager strategyManager, GridPositions gp, IPossibilities home, IPossibilities away, int count, Unit unit)
    {
        if (count != gp.Count) return;
        
        IPossibilities one;
        IPossibilities two;
        if (unit == Unit.Row)
        {
            one = home;
            two = away;
        }
        else
        {
            one = away;
            two = home;
        }
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var rowCount = gp.RowCount(row);
                var colCount = gp.ColumnCount(col);

                switch (rowCount, colCount)
                {
                    case (> 0, 0) :
                        foreach (var possibility in two)
                        {
                            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
                        }

                        break;
                    case (0, > 0) :
                        foreach (var possibility in one)
                        {
                            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
                        }

                        break;
                }
            }
        }

        strategyManager.ChangeBuffer.Push(this, new MultiSectorLockedSetsReportBuilder(gp, home, away));
    }
}

public class MultiSectorLockedSetsReportBuilder : IChangeReportBuilder
{
    private readonly GridPositions _gp;
    private readonly IPossibilities _home;
    private readonly IPossibilities _away;

    public MultiSectorLockedSetsReportBuilder(GridPositions gp, IPossibilities home, IPossibilities away)
    {
        _gp = gp;
        _home = home;
        _away = away;
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

                if (_home.Peek(solved)) homeSet.Add(new Cell(row, col));
                if (_away.Peek(solved)) awaySet.Add(new Cell(row, col));
            }
        }

        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Explanation(), lighter =>
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

    private string Explanation()
    {
        return $"Multi-Sector Locked Sets found :\n" +
               $"Home possibilities : {_home}\n" +
               $"Away possibilities : {_away}";
    }
}