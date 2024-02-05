using System.Collections.Generic;
using Model.Helpers.Changes;
using Model.Sudoku.Solver.BitSets;
using Model.Sudoku.Solver.PossibilityPosition;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Utility;

namespace Model.Sudoku.Solver.Strategies;

public class AlignedPairExclusionStrategy : AbstractStrategy
{
    public const string OfficialName = "Aligned Pair Exclusion";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    public AlignedPairExclusionStrategy() : base(OfficialName,  StrategyDifficulty.Hard, DefaultBehavior) { }

    public override void Apply(IStrategyUser strategyUser)
    {
        for (int start1 = 0; start1 < 9; start1 += 3)
        {
            for (int start2 = 0; start2 < 9; start2 += 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        for (int k = j + 1; k < 3; k++)
                        {
                            var r = start1 + i;
                            var c1 = start2 + j;
                            var c2 = start2 + k;

                            if (strategyUser.Sudoku[r, c1] == 0 && strategyUser.Sudoku[r, c2] == 0 &&
                                Search(strategyUser, r, c1, r, c2)) return;

                            var c = start2 + i;
                            var r1 = start1 + j;
                            var r2 = start1 + k;

                            if (strategyUser.Sudoku[r1, c] == 0 && strategyUser.Sudoku[r2, c] == 0 &&
                                Search(strategyUser, r1, c, r2, c)) return;
                        }
                    }
                }
            }

            for (int u = 0; u < 2; u++)
            {
                for (int v = u + 1; v < 3; v++)
                {
                    var unit1 = start1 + u;
                    var unit2 = start1 + v;

                    for (int i = 0; i < 9; i++)
                    {
                        if (strategyUser.Sudoku[unit1, i] == 0)
                        {
                            for (int j = 0; j < 9; j++)
                            {
                                if (i / 3 == j / 3 || strategyUser.Sudoku[unit2, j] != 0) continue;

                                if (Search(strategyUser, unit1, i, unit2, j)) return;
                            }
                        }

                        if (strategyUser.Sudoku[i, unit1] == 0)
                        {
                            for (int j = 0; j < 9; j++)
                            {
                                if (i / 3 == j / 3 || strategyUser.Sudoku[j, unit2] != 0) continue;

                                if (Search(strategyUser, i, unit1, j, unit2)) return;
                            }
                        }
                    }
                }
            }
        }
    }

    private bool Search(IStrategyUser strategyUser, int row1, int col1, int row2, int col2)
    {
        var shared = new List<Cell>(Cells.SharedSeenEmptyCells(strategyUser, row1, col1, row2, col2));

        var poss1 = strategyUser.PossibilitiesAt(row1, col1);
        var poss2 = strategyUser.PossibilitiesAt(row2, col2);
        var or = poss1 | poss2;
        
        if (shared.Count < poss1.Count || shared.Count < poss2.Count) return false;

        var inSameUnit = Cells.ShareAUnit(row1, col1, row2, col2);
        
        List<IPossibilitiesPositions> usefulAls = new();
        HashSet<BiValue> forbidden = new();

        foreach (var als in strategyUser.AlmostNakedSetSearcher.InCells(shared))
        {
            int i = 0;
            bool useful = false;
            while (als.Possibilities.Next(ref i))
            {
                if (!or.Contains(i)) continue;
                
                int j = i;
                while (als.Possibilities.Next(ref j))
                {
                    if (!or.Contains(j)) continue;
                    
                    if(forbidden.Add(new BiValue(i, j))) useful = true;
                }
            }

            if (useful) usefulAls.Add(als);
        }

        SearchForElimination(strategyUser, poss1, poss2, forbidden, row1, col1, inSameUnit);
        SearchForElimination(strategyUser, poss2, poss1, forbidden, row2, col2, inSameUnit);
        
        return strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit( 
            new AlignedPairExclusionReportBuilder(usefulAls, row1, col1, row2, col2))
                && OnCommitBehavior == OnCommitBehavior.Return;
    }

    private void SearchForElimination(IStrategyUser strategyUser, ReadOnlyBitSet16 poss1,
        ReadOnlyBitSet16 poss2, HashSet<BiValue> forbidden, int row, int col, bool inSameUnit)
    {
        foreach (var p1 in poss1.EnumeratePossibilities())
        {
            bool toDelete = true;
            foreach (var p2 in poss2.EnumeratePossibilities())
            {
                if (p1 == p2 && inSameUnit) continue;
                if (!forbidden.Contains(new BiValue(p1, p2)))
                {
                    toDelete = false;
                    break;
                }
            }
            
            if(toDelete) strategyUser.ChangeBuffer.ProposePossibilityRemoval(p1, row, col);
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
    
    public ChangeReport Build(IReadOnlyList<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport("", lighter =>
        {
            lighter.HighlightCell(_row1, _col1, ChangeColoration.Neutral);
            lighter.HighlightCell(_row2, _col2, ChangeColoration.Neutral);

            var removed = new ReadOnlyBitSet16();
            foreach (var change in changes) removed += change.Number;
            
            int color = (int) ChangeColoration.CauseOffOne;
            foreach (var als in _als)
            {
                if (!removed.ContainsAny(als.Possibilities)) continue;
                foreach (var coord in als.EachCell())
                {
                    lighter.HighlightCell(coord.Row, coord.Column, (ChangeColoration) color);
                }

                color++;
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}

