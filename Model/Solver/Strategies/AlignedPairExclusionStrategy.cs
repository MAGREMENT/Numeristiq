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

                var usc = Cells.UnitSharedCount(row1, col1, row2, col2);
                switch (usc)
                {
                    case 1 : 
                        continue;
                    case 0 :
                        if (row1 / 3 != row2 / 3 && col1 / 3 != col2 / 3) continue;
                        break;
                }
                
                if (Search(strategyManager, row1, col1, row2, col2)) return;
            }
        }
    }

    private bool Search(IStrategyManager strategyManager, int row1, int col1, int row2, int col2)
    {
        var shared = new List<Cell>(Cells.SharedSeenEmptyCells(strategyManager, row1, col1, row2, col2));

        var poss1 = strategyManager.PossibilitiesAt(row1, col1);
        var poss2 = strategyManager.PossibilitiesAt(row2, col2);
        var or = poss1.Or(poss2);
        
        if (shared.Count < poss1.Count || shared.Count < poss2.Count) return false;

        var inSameUnit = Cells.ShareAUnit(row1, col1, row2, col2);
        
        List<IPossibilitiesPositions> usefulAls = new();
        HashSet<BiValue> forbidden = new();

        foreach (var als in strategyManager.AlmostNakedSetSearcher.InCells(shared))
        {
            int i = 0;
            bool useful = false;
            while (als.Possibilities.Next(ref i))
            {
                if (!or.Peek(i)) continue;
                
                int j = i;
                while (als.Possibilities.Next(ref j))
                {
                    if (!or.Peek(j)) continue;
                    
                    if(forbidden.Add(new BiValue(i, j))) useful = true;
                }
            }

            if (useful) usefulAls.Add(als);
        }

        SearchForElimination(strategyManager, poss1, poss2, forbidden, row1, col1, inSameUnit);
        SearchForElimination(strategyManager, poss2, poss1, forbidden, row2, col2, inSameUnit);
        
        return strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this, 
            new AlignedPairExclusionReportBuilder(usefulAls, row1, col1, row2, col2))
                && OnCommitBehavior == OnCommitBehavior.Return;
    }

    private void SearchForElimination(IStrategyManager strategyManager, IReadOnlyPossibilities poss1,
        IReadOnlyPossibilities poss2, HashSet<BiValue> forbidden, int row, int col, bool inSameUnit)
    {
        foreach (var p1 in poss1)
        {
            bool toDelete = true;
            foreach (var p2 in poss2)
            {
                if (p1 == p2 && inSameUnit) continue;
                if (!forbidden.Contains(new BiValue(p1, p2)))
                {
                    toDelete = false;
                    break;
                }
            }
            
            if(toDelete) strategyManager.ChangeBuffer.ProposePossibilityRemoval(p1, row, col);
        }
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

            Possibilities removed = new Possibilities();
            foreach (var change in changes) removed.Add(change.Number);
            
            int color = (int) ChangeColoration.CauseOffOne;
            foreach (var als in _als)
            {
                if (!removed.PeekAny(als.Possibilities)) continue;
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

