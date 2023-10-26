using System;
using System.Collections.Generic;
using System.Linq;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.AlmostLockedSets;

namespace Model.Solver.Strategies;

/// <summary>
/// A firework is a pattern where a candidate in an intersecting row and column is limited to a box, with one exception
/// in the row and one exception in the column. Theses exceptions are called wings and the cell at the middle of the
/// intersection is the cross. A firework means that the candidate must appear at least once in the wings and cross cells.
/// On its own, this pattern doesn't tell us much, but its power come from multiple fireworks on the same cells.
///
/// Example :
/// +-------+-------+-------+
/// | x . . | 1 2 3 | 4 5 x |
/// | . . . | . . . | . . . |
/// | . . . | . . . | . . . |
/// +-------+-------+-------+
/// | 6 . . | . . . | . . . |
/// | 7 . . | . . . | . . . |
/// | 1 . . | . . . | . . . |
/// +-------+-------+-------+
/// | 2 . . | . . . | . . . |
/// | 3 . . | . . . | . . . |
/// | x . . | . . . | . . . |
/// +-------+-------+-------+
///
/// 8 and 9 must be present at least once in the x-marked cells.
///
/// Moreover, a firework can miss a wing and still be useful as seen below. A firework with both wings is called a
/// strict firework. Fireworks with different possibilities but the same cross and the amount of shared wings
/// being equal to two is called a stack. The opposite of a stack is the cell on the opposite corner to the cross on
/// the square created by the stack.
///
/// Applications :
///
/// - Triple fireworks : This is about a non-strict stack of size 3. This works the same way as a hidden set.
/// 3 candidates must be present at least once in 3 cells, therefore discarding any other candidates in those cells.
///
/// - Quad fireworks : This is about 2 non-strict stacks of size 2. They must share the same same wings and not
/// have any possibility in common. In that case, it works, like above, like a hidden set. 4 candidates must be present
/// at least once in 4 cells, therefore discarding any other candidates in those cells
///
/// - W-wing : This is about a stack of size 2. If, for both wing, there is an almost locked set on the unit shared by
/// that wing and the opposite that contains both possibilities of the stack, then we can remove those possibilities
/// from the opposite.
/// 
/// </summary>
public class FireworksStrategy : AbstractStrategy
{
    public const string OfficialName = "Fireworks";

    public FireworksStrategy() : base(OfficialName, StrategyDifficulty.Hard){}
    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        List<FireworkStack> looseStacks = new();
        List<FireworkStack> strictStacks = new();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                List<Firework> cellFireworks = new();
                foreach (var possibility in strategyManager.PossibilitiesAt(row, col))
                {
                    AddIfFirework(strategyManager, cellFireworks, row, col, possibility);
                }

                if (cellFireworks.Count < 2) continue;
                
                for (int i = 0; i < cellFireworks.Count; i++)
                {
                    for (int j = i + 1; j < cellFireworks.Count; j++)
                    {
                        FireworkStack stack;
                        try
                        {
                            stack = new FireworkStack(cellFireworks[i], cellFireworks[j]);
                            looseStacks.Add(stack);
                            if (stack.IsStrict()) strictStacks.Add(stack);
                        }
                        catch (ArgumentException)
                        {
                            continue;
                        }

                        //Triple
                        for (int k = j + 1; k < cellFireworks.Count; k++)
                        {
                            var sharedWings = stack.MashWings(cellFireworks[k]);
                            if (sharedWings.Count == 2)
                                ProcessTripleFirework(strategyManager, sharedWings,
                                    cellFireworks[i], cellFireworks[j],
                                    cellFireworks[k]);
                        }
                    }
                }
            }
        }

        //Quad
        for (int i = 0; i < looseStacks.Count; i++)
        {
            for (int j = i + 1; j < looseStacks.Count; j++)
            {
                var one = looseStacks[i];
                var two = looseStacks[j];

                if (one.Cross.Row == two.Cross.Row || one.Cross.Col == two.Cross.Col
                                                   || one.Possibilities.PeekAny(two.Possibilities)) continue;

                var sharedWings = one.MashWings(two);
                if (sharedWings.Count == 2)
                {
                    RemoveAllExcept(strategyManager, one.Cross, one.Possibilities);
                    RemoveAllExcept(strategyManager, two.Cross, two.Possibilities);

                    var mashed = one.Possibilities.Or(two.Possibilities);
                    foreach (var coord in sharedWings)
                    {
                        RemoveAllExcept(strategyManager, coord, mashed);
                    }
                    
                    strategyManager.ChangeBuffer.Push(this, new FireworksStacksReportBuilder(one, two));
                }
            }
        }
        
        //W-wing
        foreach (var dual in strictStacks)
        {
            var opposite = dual.Opposite();
            if(!strategyManager.PossibilitiesAt(opposite.Row, opposite.Col).PeekAll(dual.Possibilities)) continue;
            
            List<AlmostLockedSet>[] als = new List<AlmostLockedSet>[2];
            bool nah = false;
            for (int i = 0; i < 2; i++)
            {
                var coord = dual.Wings[i];

                als[i] = AlmostLockedSetSearcher.InCells(strategyManager, CoordinatesToSearch(dual.Cross,
                    coord, new Cell(opposite.Row, opposite.Col)), 4);

                als[i].RemoveAll(singleAls => !singleAls.Possibilities.PeekAll(dual.Possibilities));

                if (als[i].Count == 0)
                {
                    nah = true;
                    break;
                }
            }
            
            if(nah) continue;

            foreach (var possibility in dual.Possibilities)
            {
                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, opposite.Row, opposite.Col);
            }
            strategyManager.ChangeBuffer.Push(this, new FireworksWithAlsReportBuilder(dual, 
                als[0][0], als[1][0]));
        }

        //L-wing
        foreach (var dual in strictStacks)
        {
            var opposite = dual.Opposite();

            foreach (var possibility in strategyManager.PossibilitiesAt(opposite.Row, opposite.Col))
            {
                if(dual.Possibilities.Peek(possibility)) continue;

                if (strategyManager.PossibilitiesAt(opposite.Row, dual.Cross.Col).Peek(possibility) &&
                    strategyManager.RowPositionsAt(opposite.Row, possibility).Count == 2)
                {
                    strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, dual.Cross.Row, opposite.Col);
                    if (strategyManager.ChangeBuffer.NotEmpty())
                        strategyManager.ChangeBuffer.Push(this, new FireworksWithStrongLinkReportBuilder(dual, possibility,
                                new Cell(opposite.Row, opposite.Col),
                                new Cell(opposite.Row, dual.Cross.Col)));
                }

                if (strategyManager.PossibilitiesAt(dual.Cross.Row, opposite.Col).Peek(possibility) &&
                    strategyManager.ColumnPositionsAt(opposite.Col, possibility).Count == 2)
                {
                    strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, opposite.Row, dual.Cross.Col);
                    if (strategyManager.ChangeBuffer.NotEmpty())
                        strategyManager.ChangeBuffer.Push(this, new FireworksWithStrongLinkReportBuilder(dual, possibility,
                            new Cell(opposite.Row, opposite.Col),
                            new Cell(dual.Cross.Row, opposite.Col)));
                }
            }
        }
        
        //Dual ALP
        for (int i = 0; i < strictStacks.Count; i++)
        {
            for (int j = i + 1; j < strictStacks.Count; j++)
            {
                if(!strictStacks[i].Possibilities.Equals(strictStacks[j].Possibilities)) continue;

                var oppositeI = strictStacks[i].Opposite();
                var oppositeJ = strictStacks[j].Opposite();
                
                if(oppositeI != oppositeJ || !strategyManager.PossibilitiesAt(oppositeI.Row, oppositeI.Col)
                       .Equals(strictStacks[i].Possibilities)) continue;

                RemoveAllExcept(strategyManager, strictStacks[i].Cross, strictStacks[i].Possibilities);
                RemoveAllExcept(strategyManager, strictStacks[j].Cross, strictStacks[j].Possibilities);

                for (int unit = 0; unit < 9; unit++)
                {
                    foreach (var possibility in strictStacks[i].Possibilities)
                    {
                        if (unit != strictStacks[i].Wings[0].Row 
                            && unit != strictStacks[i].Wings[1].Row 
                            && unit != strictStacks[j].Wings[0].Row 
                            && unit != strictStacks[j].Wings[1].Row)
                        {
                        
                            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, unit, oppositeI.Col);
                            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, unit, oppositeI.Col);
                        }
                    
                        if (unit != strictStacks[i].Wings[0].Col
                            && unit != strictStacks[i].Wings[1].Col
                            && unit != strictStacks[j].Wings[0].Col 
                            && unit != strictStacks[j].Wings[1].Col)
                        {
                            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, oppositeI.Row, unit);
                            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, oppositeI.Row, unit);
                        }    
                    }
                }

                strategyManager.ChangeBuffer.Push(this,
                    new OppositeFireworksReportBuilder(strictStacks[i], strictStacks[j]));
            }
        }
    }

    private List<Cell> CoordinatesToSearch(Cell cross, Cell wing, Cell opposite)
    {
        List<Cell> result = new();
        bool sameRow = cross.Row == wing.Row;

        for (int i = 0; i < 9; i++)
        {
            result.Add(sameRow ? new Cell(i, wing.Col) : new Cell(wing.Row, i));
        }

        result.Remove(wing);
        result.Remove(opposite);

        return result;
    }

    private void RemoveAllExcept(IStrategyManager strategyManager, Cell coord, IPossibilities except)
    {
        foreach (var possibility in strategyManager.PossibilitiesAt(coord.Row, coord.Col))
        {
            if(except.Peek(possibility)) continue;
            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, coord.Row, coord.Col);
        }
    }

    private void ProcessTripleFirework(IStrategyManager strategyManager, HashSet<Cell> wings,
        Firework one, Firework two, Firework three)
    {
        foreach (var possibility in strategyManager.PossibilitiesAt(one.Cross.Row, one.Cross.Col))
        {
            if (possibility != one.Possibility && possibility != two.Possibility && possibility != three.Possibility)
                strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, one.Cross.Row, one.Cross.Col);
        }

        foreach (var wing in wings)
        {
            foreach (var possibility in strategyManager.PossibilitiesAt(wing.Row, wing.Col))
            {
                if (possibility != one.Possibility && possibility != two.Possibility && possibility != three.Possibility) 
                    strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, wing.Row, wing.Col);
            }
        }

        strategyManager.ChangeBuffer.Push(this, new FireworksReportBuilder(one, two, three));
    }

    private void AddIfFirework(IStrategyManager strategyManager, List<Firework> fireworks, int row, int col, int possibility)
    {
        int miniRow = row / 3;
        int miniCol = col / 3;

        int rowCol = -1;
        int colRow = -1;
        
        //Check row
        for (int c = 0; c < 9; c++)
        {
            if (c / 3 != miniCol && strategyManager.PossibilitiesAt(row, c).Peek(possibility))
            {
                if (rowCol == -1) rowCol = c;
                else return;
            }
        }
        
        //Check col
        for (int r = 0; r < 9; r++)
        {
            if (r / 3 != miniRow && strategyManager.PossibilitiesAt(r, col).Peek(possibility))
            {
                if (colRow == -1) colRow = r;
                else return;
            }
        }

        if (rowCol != -1)
        {
            if (colRow != -1) fireworks.Add(new Firework(possibility, row, col,
                    row, rowCol, colRow, col));
            else fireworks.Add(new Firework(possibility, row, col,
                row, rowCol));
        }
        else if (colRow != -1) fireworks.Add(new Firework(possibility, row, col,
            colRow, col));
    }
}

public class Firework
{
    public int Possibility { get; }
    public Cell Cross { get; }
    public Cell[] Wings { get; }

    public Firework(int possibility, int crossRow, int crossCol, int wingRow, int wingCol)
    {
        Possibility = possibility;
        Cross = new Cell(crossRow, crossCol);
        Wings = new Cell[] { new(wingRow, wingCol) };
    }
    
    public Firework(int possibility, int crossRow, int crossCol, int wingRow1, int wingCol1, int wingRow2, int wingCol2)
    {
        Possibility = possibility;
        Cross = new Cell(crossRow, crossCol);
        Wings = new Cell[] { new(wingRow1, wingCol1), new(wingRow2, wingCol2) };
    }

    public HashSet<Cell> MashWings(params Firework[] fireworks)
    {
        HashSet<Cell> result = new(Wings);
        foreach (var firework in fireworks)
        {
            foreach (var wing in firework.Wings)
            {
                result.Add(wing);
            }
        }

        return result;
    }
}

public class FireworkStack {
    public IPossibilities Possibilities { get; }
    public Cell Cross { get; }
    public Cell[] Wings { get; }
    private readonly uint _presence;

    public FireworkStack(params Firework[] fireworks)
    {
        if (fireworks.Length < 1) throw new ArgumentException("Must be at least one firework");

        Possibilities = IPossibilities.NewEmpty();
        Cross = fireworks[0].Cross;
        Wings = new Cell[2];

        var wings = fireworks[0].MashWings(fireworks);
        if (wings.Count != 2) throw new ArgumentException("Not matching wings");
        var cursor = 0;
        foreach (var wing in wings)
        {
            Wings[cursor] = wing;
            cursor++;
        }

        foreach (var firework in fireworks)
        {
            if (firework.Cross != Cross) throw new ArgumentException("Not matching cross");
            Possibilities.Add(firework.Possibility);
            if (firework.Wings.Contains(Wings[0])) _presence |= (uint)1 << (2 * firework.Possibility);
            if (firework.Wings.Contains(Wings[1])) _presence |= (uint)2 << (2 * firework.Possibility);
        }
    }

    public bool IsStrict()
    {
        return Possibilities.Count * 2 == System.Numerics.BitOperations.PopCount(_presence);
    }

    public bool IsPresent(int wingNumber, int possibility)
    {
        return ((_presence >> (possibility * 2)) & 3) == wingNumber + 1;
    }
    
    public HashSet<Cell> MashWings(params Firework[] fireworks)
    {
        HashSet<Cell> result = new(Wings);
        foreach (var firework in fireworks)
        {
            foreach (var wing in firework.Wings)
            {
                result.Add(wing);
            }
        }

        return result;
    }
    
    public HashSet<Cell> MashWings(FireworkStack stack)
    {
        HashSet<Cell> result = new(Wings);
        foreach (var wing in stack.Wings)
        {
            result.Add(wing);
        }

        return result;
    }
    
    public Cell Opposite()
    {
        bool firstSameRow = Wings[0].Row == Cross.Row;
        return firstSameRow ? new Cell(Wings[1].Row, Wings[0].Col) : new Cell(Wings[0].Row, Wings[1].Col);
    }
}

public static class FireworksReportBuilderHelper
{
    public static int HighlightFirework(IHighlightable lighter, Firework[] fireworks, int start = 0)
    {
        int color = (int)ChangeColoration.CauseOffOne + start;
        foreach (var firework in fireworks)
        {
            lighter.HighlightPossibility(firework.Possibility, firework.Cross.Row,
                firework.Cross.Col, (ChangeColoration)color);
            foreach (var coord in firework.Wings)
            {
                lighter.HighlightPossibility(firework.Possibility, coord.Row, coord.Col, (ChangeColoration)color);
            }

            color++;
        }

        return color;
    }

    public static int HighlightFireworkStack(IHighlightable lighter, FireworkStack stack, int start = 0)
    {
        int color = (int)ChangeColoration.CauseOffOne + start;
        foreach (var possibility in stack.Possibilities)
        {
            lighter.HighlightPossibility(possibility, stack.Cross.Row,
                stack.Cross.Col, (ChangeColoration)color);
            for (int i = 0; i < stack.Wings.Length; i++)
            {
                if(stack.IsPresent(i, possibility)) 
                    lighter.HighlightPossibility(possibility, stack.Wings[i].Row, stack.Wings[i].Col, (ChangeColoration)color);
            }

            color++;
        }

        return color;
    }
}

public class FireworksReportBuilder : IChangeReportBuilder
{
    private readonly Firework[] _fireworks;

    public FireworksReportBuilder(params Firework[] fireworks)
    {
        _fireworks = fireworks;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            FireworksReportBuilderHelper.HighlightFirework(lighter, _fireworks);

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}

public class FireworksStacksReportBuilder : IChangeReportBuilder
{
    private readonly FireworkStack[] _stacks;

    public FireworksStacksReportBuilder(params FireworkStack[] fireworks)
    {
        _stacks = fireworks;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var stack in _stacks)
            {
                FireworksReportBuilderHelper.HighlightFireworkStack(lighter, stack);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}

public class FireworksWithAlsReportBuilder : IChangeReportBuilder
{
    private readonly FireworkStack _stack;
    private readonly AlmostLockedSet[] _als;

    public FireworksWithAlsReportBuilder(FireworkStack stack, params AlmostLockedSet[] als)
    {
        _stack = stack;
        _als = als;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            int color = FireworksReportBuilderHelper.HighlightFireworkStack(lighter, _stack) + 1;

            foreach (var als in _als)
            {
                var coloration = (ChangeColoration)color;

                foreach (var coord in als.Cells)
                {
                    lighter.HighlightCell(coord, coloration);
                }

                color++;
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}

public class FireworksWithStrongLinkReportBuilder : IChangeReportBuilder
{
    private readonly FireworkStack _stack;
    private readonly int _possibility;
    private readonly Cell _opposite;
    private readonly Cell _wing;

    public FireworksWithStrongLinkReportBuilder(FireworkStack stack, int possibility, Cell opposite, Cell wing)
    {
        _stack = stack;
        _possibility = possibility;
        _opposite = opposite;
        _wing = wing;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            FireworksReportBuilderHelper.HighlightFireworkStack(lighter, _stack);
            
            lighter.HighlightPossibility(_possibility, _opposite.Row, _opposite.Col, ChangeColoration.CauseOnOne);
            lighter.HighlightPossibility(_possibility, _wing.Row, _wing.Col, ChangeColoration.CauseOnOne);

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}

public class OppositeFireworksReportBuilder : IChangeReportBuilder
{
    private readonly FireworkStack _f1;
    private readonly FireworkStack _f2;

    public OppositeFireworksReportBuilder(FireworkStack f1, FireworkStack f2)
    {
        _f1 = f1;
        _f2 = f2;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            FireworksReportBuilderHelper.HighlightFireworkStack(lighter, _f2,
                FireworksReportBuilderHelper.HighlightFireworkStack(lighter, _f1));
            
            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}