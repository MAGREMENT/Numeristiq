using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.Possibility;
using Model.Solver.PossibilityPosition;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Strategies;

public class AlignedPairExclusionStrategy : AbstractStrategy //TODO add Aligned triple exclusion
{
    public const string OfficialName = "Aligned Pair Exclusion";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    private readonly int _maxAlzSize; //TODO

    public AlignedPairExclusionStrategy(int maxAlsSize) : base(OfficialName,  StrategyDifficulty.Hard, DefaultBehavior)
    {
        _maxAlzSize = maxAlsSize;
    }

    public override void Apply(IStrategyManager strategyManager)
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
        var shared = new List<Cell>(Cells.SharedSeenEmptyCells(strategyManager, row1, col1, row2, col2));

        var poss1 = strategyManager.PossibilitiesAt(row1, col1);
        var poss2 = strategyManager.PossibilitiesAt(row2, col2);
        
        if (shared.Count < poss1.Count || shared.Count < poss2.Count) return false;

        var inSameUnit = Cells.ShareAUnit(row1, col1, row2, col2);

        Dictionary<int, Possibilities> one = new();
        foreach (var possibility in poss1)
        {
            var copy = poss2.Copy();
            if (inSameUnit) copy.Remove(possibility);
            one[possibility] = copy;
        }
        
        Dictionary<int, Possibilities> two = new();
        foreach (var possibility in poss2)
        {
            var copy = poss1.Copy();
            if (inSameUnit) copy.Remove(possibility);
            two[possibility] = copy;
        }

        List<IPossibilitiesPositions> usefulAls = new();

        foreach (var als in strategyManager.AlmostNakedSetSearcher.InCells(shared))
        {
            bool useful = false;
            foreach (var possibility in als.Possibilities)
            {
                if (one.TryGetValue(possibility, out var other1))
                {
                    foreach (var possibility2 in als.Possibilities)
                    {
                        if (possibility2 == possibility) continue;

                        if (other1.Remove(possibility2)) useful = true;
                    }

                    if (other1.Count == 0)
                    {
                        strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, row1, col1);
                    }
                }
                
                if (two.TryGetValue(possibility, out var other2))
                {
                    foreach (var possibility2 in als.Possibilities)
                    {
                        if (possibility2 == possibility) continue;

                        if (other2.Remove(possibility2)) useful = true;
                    }

                    if (other2.Count == 0)
                    {
                        strategyManager.ChangeBuffer.ProposePossibilityRemoval(possibility, row2, col2);
                    }
                }
            }

            if (useful) usefulAls.Add(als);
        }

        return strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this, 
            new AlignedPairExclusionReportBuilder(usefulAls, row1, col1, row2, col2))
                && OnCommitBehavior == OnCommitBehavior.Return;
    }
}

public class AlignedPairExclusionReportBuilder : IChangeReportBuilder
{
    private readonly List<IPossibilitiesPositions> _als;
    private readonly int _row1;
    private readonly int _col1;
    private readonly int _row2;
    private readonly int _col2;

    public AlignedPairExclusionReportBuilder(List<IPossibilitiesPositions> als, int row1, int col1, int row2, int col2)
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
                foreach (var coord in als.EachCell())
                {
                    lighter.HighlightCell(coord.Row, coord.Col, (ChangeColoration) color);
                }

                color++;
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}

