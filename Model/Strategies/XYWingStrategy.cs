using System;
using System.Collections.Generic;
using Model.Changes;
using Model.Possibilities;
using Model.Solver;
using Model.StrategiesUtil;

namespace Model.Strategies;

public class XYWingStrategy : IStrategy
{
    public string Name => "XYWing";
    
    public StrategyLevel Difficulty => StrategyLevel.Medium;
    public StatisticsTracker Tracker { get; } = new();

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        var toSearch = AllCellsWith2Possibilities(strategyManager);
        foreach(var current in toSearch)
        {
            var unitsDispersion = MatchingUnitDispersion(current, toSearch);

            //Rows
            while (unitsDispersion[0].Count > 0)
            {
                Cell one = unitsDispersion[0].Dequeue();
                
                if (ShareAtLeastOne(strategyManager.Possibilities[one.Row, one.Col],
                        strategyManager.Possibilities[current.Row, current.Col]))
                {
                    foreach (var two in unitsDispersion[1])
                    {
                        if(IsYWing(strategyManager, current, one, two)
                           && ProcessXYWing(strategyManager, current, one, two))
                            return;
                    }
                    
                    foreach (var two in unitsDispersion[2])
                    {
                        if(IsYWing(strategyManager, current, one, two)
                           && ProcessXYWing(strategyManager, current, one, two))
                            return;
                    }
                }
            }
            
            //Columns
            while (unitsDispersion[1].Count > 0)
            {
                Cell one = unitsDispersion[1].Dequeue();
                
                if (ShareAtLeastOne(strategyManager.Possibilities[one.Row, one.Col],
                        strategyManager.Possibilities[current.Row, current.Col]))
                {
                    foreach (var two in unitsDispersion[2])
                    {
                        if (IsYWing(strategyManager, current, one, two)
                            && ProcessXYWing(strategyManager, current, one, two))
                            return;
                    }
                }
            }
        }
    }

    private Queue<Cell>[] MatchingUnitDispersion(Cell coord, List<Cell> toSee)
    {
        Queue<Cell>[] result = { new(), new(), new() };
        foreach (var c in toSee)
        {
            if (c.Row == coord.Row)
            {
                result[0].Enqueue(c);
            }
            if (c.Col == coord.Col)
            {
                result[1].Enqueue(c);
            }
            if (c.Row / 3 == coord.Row / 3 && c.Col / 3 == coord.Col / 3)
            {
                result[2].Enqueue(c);
            }
        }
        return result;
    }

    private static bool ShareAtLeastOne(IPossibilities one, IPossibilities two)
    {
        foreach (var poss in one)
        {
            if (two.Peek(poss)) return true;
        }

        return false;
    }

    /// <summary>
    /// Conditions for a YWing :
    /// 1) Possibilities of all coordinates must have a count of 2 => Not checked here
    /// 2) Coordinates one and two must both share a unit (row, col, miniGrid) with opposite => Not checked here
    /// 3) All coordinates cannot be in the same unit => Checked here
    /// 4) Coordinates one and two must each have one of the possibilities of opposite and share one possibility => Checked here
    /// </summary>
    /// <param name="strategyManager"></param>
    /// <param name="opposite"></param>
    /// <param name="one"></param>
    /// <param name="two"></param>
    /// <returns></returns>
    private static bool IsYWing(IStrategyManager strategyManager, Cell opposite, Cell one, Cell two)
    {
        if (AreAllInSameUnit(opposite, one, two)) return false;
        var oppositePoss = strategyManager.Possibilities[opposite.Row, opposite.Col];
        var onePoss = strategyManager.Possibilities[one.Row, one.Col];
        var twoPoss = strategyManager.Possibilities[two.Row, two.Col];
        
        foreach (var poss in oppositePoss)
        {
            if (onePoss.Peek(poss))
            {
                if (twoPoss.Peek(poss)) return false;
            }else if (twoPoss.Peek(poss))
            {
                if (onePoss.Peek(poss)) return false;
            }
            else return false;
        }

        foreach (var poss in onePoss)
        {
            if (twoPoss.Peek(poss)) return true;
        }

        return false;
    }
    
    private static bool AreAllInSameUnit(Cell one, Cell two, Cell three)
    {
        return (one.Row == two.Row && one.Row == three.Row) || (one.Col == two.Col && one.Col == three.Col) ||
               (one.Row / 3 == two.Row / 3 && one.Col / 3 == two.Col / 3 && one.Row / 3 == three.Row / 3 &&
                one.Col / 3 == three.Col / 3);
    }

    private bool ProcessXYWing(IStrategyManager strategyManager, Cell opposite, Cell one, Cell two)
    {
        int toRemove = Minus(strategyManager.Possibilities[one.Row, one.Col],
            strategyManager.Possibilities[opposite.Row, opposite.Col]);
        foreach (var coord in MatchingCells(one, two))
        {
            strategyManager.ChangeBuffer.AddPossibilityToRemove(toRemove, coord.Row, coord.Col);
        }

        return strategyManager.ChangeBuffer.Push(this, new XYWingReportBuilder(opposite, one, two));
    }

    private static int Minus(IPossibilities one, IPossibilities two)
    {
        foreach (var n in one)
        {
            if (!two.Peek(n)) return n;
        }
        
        throw new Exception("Wtf big problem");
    }

    /// <summary>
    /// Note : doesnt work on every set of coordinates only opposite corner of YWing
    /// </summary>
    /// <param name="one"></param>
    /// <param name="two"></param>
    /// <returns></returns>
    private static IEnumerable<Cell> MatchingCells(Cell one, Cell two)
    {
        if (one.Row / 3 == two.Row / 3)
        {
            int startColOne = one.Col / 3;
            for (int i = 0; i < 3; i++)
            {
                yield return new Cell(two.Row, startColOne * 3 + i);
            }

            int startColTwo = two.Col / 3;
            for (int i = 0; i < 3; i++)
            {
                yield return new Cell(one.Row, startColTwo * 3 + i);
            }
        }
        else if (one.Col / 3 == two.Col / 3)
        {
            int startRowOne = one.Row / 3;
            for (int i = 0; i < 3; i++)
            {
                yield return new Cell(startRowOne * 3 + i, two.Col);
            }

            int startRowTwo = two.Row / 3;
            for (int i = 0; i < 3; i++)
            {
                yield return new Cell(startRowTwo * 3 + i, one.Col);
            }
        }
        else
        {
            yield return new Cell(one.Row, two.Col);
            yield return new Cell(two.Row, one.Col);
        }
    }

    private static List<Cell> AllCellsWith2Possibilities(IStrategyManager strategyManager)
    {
        List<Cell> result = new();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.Possibilities[row, col].Count == 2) result.Add(new Cell(row, col));
            }
        }

        return result;
    }
}

public class XYWingReportBuilder : IChangeReportBuilder
{
    private readonly Cell _opposite;
    private readonly Cell _one;
    private readonly Cell _two;

    public XYWingReportBuilder(Cell opposite, Cell one, Cell two)
    {
        _opposite = opposite;
        _one = one;
        _two = two;
    }
    
    public ChangeReport Build(List<SolverChange> changes, ISolver snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            lighter.HighlightCell(_opposite.Row, _opposite.Col, ChangeColoration.CauseOffTwo);
            lighter.HighlightCell(_one.Row, _one.Col, ChangeColoration.CauseOffOne);
            lighter.HighlightCell(_two.Row, _two.Col, ChangeColoration.CauseOffOne);

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}