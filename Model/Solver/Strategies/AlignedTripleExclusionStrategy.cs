using System;
using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.Possibility;
using Model.Solver.PossibilityPosition;
using Model.Solver.StrategiesUtility;

namespace Model.Solver.Strategies;

public class AlignedTripleExclusionStrategy : AbstractStrategy
{
    public const string OfficialName = "Aligned Triple Exclusion";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    protected override IStrategyArgument[] NonReadOnlyArguments { get; }

    private int _minSharedSeenCells;
    
    public AlignedTripleExclusionStrategy(int minSharedSeenCells) : base(OfficialName, StrategyDifficulty.Hard, DefaultBehavior)
    {
        _minSharedSeenCells = minSharedSeenCells;
        NonReadOnlyArguments = new IStrategyArgument[]
        {
            new IntStrategyArgument("Minimum shared seen cells", () => _minSharedSeenCells,
                i => _minSharedSeenCells = i, new SliderViewInterface(5, 12, 1))
        };
    }

    public override void Apply(IStrategyManager strategyManager)
    {
        for (int start1 = 0; start1 < 9; start1 += 3)
        {
            if (_minSharedSeenCells > 12) continue;
            
            //Aligned in unit & box => 12 ssc
            for (int start2 = 0; start2 < 9; start2 += 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    var c1 = new Cell(start1 + i, start2);
                    var c2 = new Cell(start1 + i, start2 + 1);
                    var c3 = new Cell(start1 + i, start2 + 2);
                    
                    if (strategyManager.Sudoku[c1.Row, c1.Column] == 0 && strategyManager.Sudoku[c2.Row, c2.Column] == 0 &&
                        strategyManager.Sudoku[c3.Row, c3.Column] == 0 && Search(strategyManager, c1, c2, c3)) return;

                    c1 = new Cell(start1, start2 + i);
                    c2 = new Cell(start1 + 1, start2 + i);
                    c3 = new Cell(start1 + 2, start2 + i);
                    
                    if (strategyManager.Sudoku[c1.Row, c1.Column] == 0 && strategyManager.Sudoku[c2.Row, c2.Column] == 0 &&
                        strategyManager.Sudoku[c3.Row, c3.Column] == 0 && Search(strategyManager, c1, c2, c3)) return;
                }
            }
            
            if (_minSharedSeenCells > 6) continue;
            
            //2 aligned boxes & 2 in same unit => 6 ssc
            for (int u = 0; u < 2; u++)
            {
                for (int v = u + 1; v < 3; v++)
                {
                    var unit1 = start1 + u;
                    var unit2 = start1 + v;

                    for (int i = 0; i < 9; i++)
                    {
                        int s = i / 3 * 3;
                        for (int j = s; j < s + 3; j++)
                        {
                            if (i == j) continue;
                            
                            if (strategyManager.Sudoku[unit1, i] == 0 && strategyManager.Sudoku[unit1, j] == 0)
                            {
                                for (int k = 0; k < 9; k++)
                                {
                                    if(k / 3 == i / 3 || strategyManager.Sudoku[unit2, k] != 0) continue;

                                    if (Search(strategyManager, new Cell(unit1, i), new Cell(unit1, j),
                                            new Cell(unit2, k))) return;
                                }
                            }
                            
                            if (strategyManager.Sudoku[unit2, i] == 0 && strategyManager.Sudoku[unit2, j] == 0)
                            {
                                for (int k = 0; k < 9; k++)
                                {
                                    if(k / 3 == i / 3 || strategyManager.Sudoku[unit1, k] != 0) continue;

                                    if (Search(strategyManager, new Cell(unit2, i), new Cell(unit2, j),
                                            new Cell(unit1, k))) return;
                                }
                            }
                            
                            if (strategyManager.Sudoku[i, unit1] == 0 && strategyManager.Sudoku[j, unit1] == 0)
                            {
                                for (int k = 0; k < 9; k++)
                                {
                                    if(k / 3 == i / 3 || strategyManager.Sudoku[k, unit2] != 0) continue;

                                    if (Search(strategyManager, new Cell(i, unit1), new Cell(j, unit1),
                                            new Cell(k, unit2))) return;
                                }
                            }
                            
                            if (strategyManager.Sudoku[i, unit2] == 0 && strategyManager.Sudoku[j, unit2] == 0)
                            {
                                for (int k = 0; k < 9; k++)
                                {
                                    if(k / 3 == i / 3 || strategyManager.Sudoku[k, unit1] != 0) continue;

                                    if (Search(strategyManager, new Cell(i, unit2), new Cell(j, unit2),
                                            new Cell(k, unit1))) return;
                                }
                            }
                        }
                    }
                }
            }
            
            if (_minSharedSeenCells > 5) continue;

            //2 aligned boxes & different units => 5 ssc
            for (int start2 = 0; start2 < 9; start2 += 3)
            {
                for (int u = 0; u < 2; u++)
                {
                    for (int v = u + 1; v < 3; v++)
                    {
                        var unit1 = start1 + u;
                        var unit2 = start1 + v;
                        var unit3 = start1 + LastUnitInBox(u, v);

                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                var other1 = start2 + i;
                                var other2 = start2 + j;

                                if (strategyManager.Sudoku[unit1, other1] == 0 & strategyManager.Sudoku[unit2, other2] == 0)
                                {
                                    for (int startOther = 0; startOther < 9; startOther += 3)
                                    {
                                        if (startOther == start2) continue;

                                        for (int k = 0; k < 3; k++)
                                        {
                                            var other3 = startOther + k;
                                            if(strategyManager.Sudoku[unit3, other3] != 0) continue;
                                            
                                            if(Search(strategyManager, new Cell(unit1, other1),
                                                   new Cell(unit2, other2), new Cell(unit3, other3))) return;
                                        }
                                    }
                                }
                                
                                if (strategyManager.Sudoku[other1, unit1] == 0 & strategyManager.Sudoku[other2, unit2] == 0)
                                {
                                    for (int startOther = 0; startOther < 3; startOther++)
                                    {
                                        if (startOther == start2) continue;

                                        for (int k = 0; k < 3; k++)
                                        {
                                            var other3 = startOther + k;
                                            if(strategyManager.Sudoku[other3, unit3] != 0) continue;
                                            
                                            if(Search(strategyManager, new Cell(other1, unit1),
                                                   new Cell(other2, unit2), new Cell(other3, unit3))) return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private int LastUnitInBox(int one, int two)
    {
        var add = one + two;
        switch (add)
        {
            case 1 : return 2;
            case 2 : return 1;
            case 3 : return 0;
            default: throw new ArgumentException();
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
                if (p1 == p2 && Cells.ShareAUnit(c1, c2)) continue;

                if (forbiddenBi.Contains(new BiValue(p1, p2))) continue;

                foreach (var p3 in poss3)
                {
                    if((p1 == p3 && Cells.ShareAUnit(c1, c3)) || (p2 == p3 && Cells.ShareAUnit(c2, c3))) continue;

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
                    lighter.HighlightCell(coord.Row, coord.Column, (ChangeColoration) color);
                }

                color++;
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}