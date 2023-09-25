using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Strategies;

public class AlignedPairExclusionStrategy : AbstractStrategy
{
    private readonly int _maxAlzSize;

    public AlignedPairExclusionStrategy(int maxAlsSize) : base("Aligned pair exclusion",  StrategyDifficulty.Hard)
    {
        _maxAlzSize = maxAlsSize;
    }

    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        for(int i = 0; i < 81; i++)
        {
            int row1 = i / 9;
            int col1 = i % 9;
            if(strategyManager.Sudoku[row1, col1] != 0) continue;

            for (int j = i + 1; j < 81; j++)
            {
                int row2 = j / 9;
                int col2 = j % 9;

                if (strategyManager.Sudoku[row2, col2] != 0) continue;
                if (Search(strategyManager, row1, col1, row2, col2)) return;
            }
        }
    }

    private bool Search(IStrategyManager strategyManager, int row1, int col1, int row2, int col2)
    {
        List<Cell> shared = new List<Cell>(
            Cells.SharedSeenEmptyCells(strategyManager, row1, col1, row2, col2));

        var poss1 = strategyManager.PossibilitiesAt(row1, col1);
        var poss2 = strategyManager.PossibilitiesAt(row2, col2);
        
        if (shared.Count < poss1.Count || shared.Count < poss2.Count) return false;

        var inSameUnit = Cells.ShareAUnit(row1, col1, row2, col2);

        Dictionary<int, IPossibilities> one = new();
        foreach (var possibility in poss1)
        {
            var copy = poss2.Copy();
            if (inSameUnit) copy.Remove(possibility);
            one[possibility] = copy;
        }
        
        Dictionary<int, IPossibilities> two = new();
        foreach (var possibility in poss2)
        {
            var copy = poss1.Copy();
            if (inSameUnit) copy.Remove(possibility);
            two[possibility] = copy;
        }

        HashSet<AlmostLockedSet> usefulAls = new();

        foreach (var als in AlmostLockedSetSearcher.InCells(strategyManager, shared, _maxAlzSize))
        {
            foreach (var possibility in als.Possibilities)
            {
                if (one.TryGetValue(possibility, out var other1))
                {
                    foreach (var possibility2 in als.Possibilities)
                    {
                        if (possibility2 == possibility) continue;
                        
                        if(other1.Remove(possibility2)) usefulAls.Add(als);
                    }

                    if (other1.Count == 0)
                    {
                        strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row1, col1);
                        strategyManager.ChangeBuffer.Push(this, 
                            new AlignedPairExclusionReportBuilder(usefulAls, row1, col1, row2, col2));
                        return true;
                    }
                }
                
                if (two.TryGetValue(possibility, out var other2))
                {
                    foreach (var possibility2 in als.Possibilities)
                    {
                        if (possibility2 == possibility) continue;
                        
                        if(other2.Remove(possibility2)) usefulAls.Add(als);
                    }

                    if (other2.Count == 0)
                    {
                        strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row2, col2);
                        strategyManager.ChangeBuffer.Push(this,
                            new AlignedPairExclusionReportBuilder(usefulAls, row1, col1, row2, col2));
                        return true;
                    }
                }
            }
        }

        return false;
    }
}

public class AlignedPairExclusionReportBuilder : IChangeReportBuilder
{
    private readonly HashSet<AlmostLockedSet> _als;
    private readonly int _row1;
    private readonly int _col1;
    private readonly int _row2;
    private readonly int _col2;

    public AlignedPairExclusionReportBuilder(HashSet<AlmostLockedSet> als, int row1, int col1, int row2, int col2)
    {
        _als = als;
        _row1 = row1;
        _col1 = col1;
        _row2 = row2;
        _col2 = col2;
    }
    
    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            lighter.HighlightCell(_row1, _col1, ChangeColoration.Neutral);
            lighter.HighlightCell(_row2, _col2, ChangeColoration.Neutral);
            
            int color = (int) ChangeColoration.CauseOffOne;
            foreach (var als in _als)
            {
                foreach (var coord in als.Coordinates)
                {
                    lighter.HighlightCell(coord.Row, coord.Col, (ChangeColoration) color);
                }

                color++;
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}

