using System.Collections.Generic;
using System.Linq;
using Model.Positions;
using Model.Possibilities;
using Model.StrategiesUtil;

namespace Model.Strategies;

public class SueDeCoqStrategy : IStrategy
{
    public string Name => "Sue-De-Coq";
    public StrategyLevel Difficulty => StrategyLevel.Extreme;
    public int Score { get; set; }
    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                for (int gridRow = 0; gridRow < 3; gridRow++)
                {
                    int row = miniRow * 3 + gridRow;
                    IPossibilities possibilities = IPossibilities.NewEmpty();
                    LinePositions cols = new LinePositions();

                    for (int gridCol = 0; gridCol < 3; gridCol++)
                    {
                        int col = miniCol * 3 + gridCol;
                        if (strategyManager.Sudoku[row, col] != 0) continue;
                        
                        cols.Add(col);
                        foreach (var possibility in strategyManager.Possibilities[row, col])
                        {
                            possibilities.Add(possibility);
                        }
                    }

                    if (cols.Count < 2) continue;
                    if(possibilities.Count - 2 < cols.Count) continue;

                    List<Coordinate> rowCoords = new();
                    List<Coordinate> miniCoords = new();
                    for (int col = 0; col < 9; col++)
                    {
                        if (col / 3 == miniCol) continue;
                        if (strategyManager.Sudoku[row, col] != 0) continue;

                        rowCoords.Add(new Coordinate(row, col));
                    }

                    for (int gridRow2 = 0; gridRow2 < 3; gridRow2++)
                    {
                        if (gridRow2 == gridRow) continue;
                        for (int gridCol = 0; gridCol < 3; gridCol++)
                        {
                            int row2 = miniRow * 3 + gridRow2;
                            int col = miniCol * 3 + gridCol;
                            if (strategyManager.Sudoku[row2, col] != 0) continue;

                            miniCoords.Add(new Coordinate(row2, col));
                        }
                    }

                    var rowAls = AlmostLockedSet.SearchForAls(strategyManager, rowCoords, 4);
                    var miniAls = AlmostLockedSet.SearchForAls(strategyManager, miniCoords, 4);

                    foreach (var rAls in rowAls)
                    {
                        foreach (var mAls in miniAls)
                        {
                            if (rAls.Possibilities.Mash(mAls.Possibilities).Equals(possibilities))
                                ProcessSueDeCoq(strategyManager, row, cols, possibilities, rAls, mAls, Unit.Row);
                        }
                    }
                }
                
                //TODO columns
            }
        }
    }

    private void ProcessSueDeCoq(IStrategyManager strategyManager, int unitNumber, LinePositions center,
        IPossibilities centerPossibilities, AlmostLockedSet unitAls, AlmostLockedSet miniAls, Unit unit)
    {
        for (int other = 0; other < 9; other++)
        {
            Coordinate current = unit == Unit.Row ?
                new Coordinate(unitNumber, other) : new Coordinate(other, unitNumber);
            if(unitAls.Contains(current)) continue;
            if(center.Peek(other)) continue;

            foreach (var possibility in unitAls.Possibilities)
            {
                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, current.Row, current.Col);
            }
        }

        var unitStart = unitNumber / 3 * 3;
        var otherStart = center.First() / 3 * 3;
        for (int gridUnit = 0; gridUnit < 3; gridUnit++)
        {
            if (gridUnit + unitStart == unitNumber) continue;
            for (int gridOther = 0; gridOther < 3; gridOther++)
            {
                Coordinate current = unit == Unit.Row ?
                    new Coordinate(unitStart + gridUnit, otherStart + gridOther) :
                    new Coordinate(otherStart + gridOther, unitStart + gridUnit);
                if (miniAls.Contains(current)) continue;
                
                foreach (var possibility in miniAls.Possibilities)
                {
                    strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, current.Row, current.Col);
                }
            }
        }

        strategyManager.ChangeBuffer.Push(this, new SueDeCoqReportBuilder());
    }
}

public class SueDeCoqReportBuilder : IChangeReportBuilder
{
    public ChangeReport Build(List<SolverChange> changes, IChangeManager manager)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes),
            lighter => IChangeReportBuilder.HighlightChanges(lighter, changes), "");
    }
}