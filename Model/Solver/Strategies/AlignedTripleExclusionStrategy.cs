using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.Possibility;
using Model.Solver.PossibilityPosition;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Strategies;

public class AlignedTripleExclusionStrategy : AbstractStrategy
{
    public const string OfficialName = "Aligned Triple Exclusion";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    public AlignedTripleExclusionStrategy() : base(OfficialName, StrategyDifficulty.Hard, DefaultBehavior)
    {
    }


    public override void Apply(IStrategyManager strategyManager)
    {
        for (int startRow = 0; startRow < 9; startRow += 3)
        {
            for (int startCol = 0; startCol < 9; startCol += 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    var c1 = new Cell(startRow + i, startCol);
                    var c2 = new Cell(startRow + i, startCol + 1);
                    var c3 = new Cell(startRow + i, startCol + 2);
                    
                    if (strategyManager.Sudoku[c1.Row, c1.Col] == 0 && strategyManager.Sudoku[c2.Row, c2.Col] == 0 &&
                        strategyManager.Sudoku[c3.Row, c3.Col] == 0 && Search(strategyManager, c1, c2, c3)) return;

                    c1 = new Cell(startRow, startCol + i);
                    c2 = new Cell(startRow + 1, startCol + i);
                    c3 = new Cell(startRow + 2, startCol + i);
                    
                    if (strategyManager.Sudoku[c1.Row, c1.Col] == 0 && strategyManager.Sudoku[c2.Row, c2.Col] == 0 &&
                        strategyManager.Sudoku[c3.Row, c3.Col] == 0 && Search(strategyManager, c1, c2, c3)) return;
                }
            }
        }
    }

    private bool Search(IStrategyManager strategyManager, Cell c1, Cell c2, Cell c3)
    {
        var ssc = Cells.SharedSeenEmptyCells(strategyManager, c1, c2, c3);

        var poss1 = strategyManager.PossibilitiesAt(c1);
        var poss2 = strategyManager.PossibilitiesAt(c2);
        var poss3 = strategyManager.PossibilitiesAt(c3);
        var or = poss1.Or(poss2).Or(poss3);

        if (ssc.Count < poss1.Count || ssc.Count < poss2.Count || ssc.Count < poss3.Count) return false;

        List<IPossibilitiesPositions> usefulThings = new();
        HashSet<TriValue> forbiddenTri = new();
        HashSet<BiValue> forbiddenBi = new();

        var searcher = strategyManager.AlmostNakedSetSearcher;

        searcher.Difference = 2;
        foreach (var aals in searcher.InCells(ssc))
        {
            int i = 0;
            bool useful = false;
            while (aals.Possibilities.Next(ref i))
            {
                if (!or.Peek(i)) continue;

                int j = i;
                while (aals.Possibilities.Next(ref j))
                {
                    if (!or.Peek(j)) continue;

                    int k = j;
                    while (aals.Possibilities.Next(ref k))
                    {
                        if (!or.Peek(k)) continue;

                        if (forbiddenTri.Add(new TriValue(i, j, k))) useful = true;
                    }
                }
            }

            if (useful) usefulThings.Add(aals);
        }

        searcher.Difference = 1;
        foreach (var als in searcher.InCells(ssc))
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

                    if (forbiddenBi.Add(new BiValue(i, j))) useful = true;
                }
            }

            if (useful) usefulThings.Add(als);
        }
        
        SearchForElimination(strategyManager, poss1, poss2, poss3, c1, c2, c3, forbiddenTri, forbiddenBi);
        SearchForElimination(strategyManager, poss2, poss1, poss3, c2, c1, c3, forbiddenTri, forbiddenBi);
        SearchForElimination(strategyManager, poss3, poss2, poss1, c3, c2, c1, forbiddenTri, forbiddenBi);

        return strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this, 
            new AlignedTripleExclusionReportBuilder(c1, c2, c3, usefulThings)) && OnCommitBehavior == OnCommitBehavior.Return;
    }

    private void SearchForElimination(IStrategyManager strategyManager, IReadOnlyPossibilities poss1, IReadOnlyPossibilities poss2,
        IReadOnlyPossibilities poss3, Cell c1, Cell c2, Cell c3, HashSet<TriValue> forbiddenTri, HashSet<BiValue> forbiddenBi)
    {
        foreach (var p1 in poss1)
        {
            var toDelete = true;
            foreach (var p2 in poss2)
            {
                if (p1 == p2 && c1.ShareAUnit(c2)) continue;

                if (forbiddenBi.Contains(new BiValue(p1, p2))) continue;

                foreach (var p3 in poss3)
                {
                    if((p1 == p3 && c1.ShareAUnit(c3)) || (p2 == p3 && c2.ShareAUnit(c3))) continue;

                    if (forbiddenBi.Contains(new BiValue(p1, p3)) 
                        || forbiddenBi.Contains(new BiValue(p2, p3))) continue;

                    if (!forbiddenTri.Contains(new TriValue(p1, p2, p3)))
                    {
                        toDelete = false;
                        break;
                    }
                }

                if (!toDelete) break;
            }

            if (toDelete) strategyManager.ChangeBuffer.ProposePossibilityRemoval(p1, c1);
        }
    }
}

public readonly struct TriValue
{
    public TriValue(int one, int two, int three)
    {
        One = one;
        Two = two;
        Three = three;
    }

    public int One { get; }
    public int Two { get; }
    public int Three { get; }

    public override int GetHashCode()
    {
        return One ^ Two ^ Three;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not TriValue tri) return false;
        int i = 1 << One | 1 << Two | 1 << Three;
        int j = 1 << tri.One | 1 << tri.Two | 1 << tri.Three;
        return i == j;
    }

    public static bool operator ==(TriValue left, TriValue right)
    {
        int i = 1 << left.One | 1 << left.Two | 1 << left.Three;
        int j = 1 << right.One | 1 << right.Two | 1 << right.Three;
        return i == j;
    }

    public static bool operator !=(TriValue left, TriValue right)
    {
        return !(left == right);
    }
}

public class AlignedTripleExclusionReportBuilder : IChangeReportBuilder
{
    private readonly Cell _c1;
    private readonly Cell _c2;
    private readonly Cell _c3;
    private readonly List<IPossibilitiesPositions> _useful;

    public AlignedTripleExclusionReportBuilder(Cell c1, Cell c2, Cell c3, List<IPossibilitiesPositions> useful)
    {
        _c1 = c1;
        _c2 = c2;
        _c3 = c3;
        _useful = useful;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            lighter.HighlightCell(_c1, ChangeColoration.Neutral);
            lighter.HighlightCell(_c2, ChangeColoration.Neutral);
            lighter.HighlightCell(_c3, ChangeColoration.Neutral);

            Possibilities removed = new Possibilities();
            foreach (var change in changes) removed.Add(change.Number);
            
            int color = (int) ChangeColoration.CauseOffOne;
            foreach (var als in _useful)
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