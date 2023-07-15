using System;
using System.Collections.Generic;
using Model.Strategies.ChainingStrategiesUtil;

namespace Model.Strategies;

// ReSharper disable once InconsistentNaming
public class XYWingStrategy : IStrategy
{
    public void ApplyOnce(ISolver solver)
    {
        var toSearch = AllCellsWith2Possibilities(solver);
        foreach(var current in toSearch)
        {
            var unitsDispersion = MatchingUnitDispersion(current, toSearch);

            //Rows
            while (unitsDispersion[0].Count > 0)
            {
                Coordinate one = unitsDispersion[0][0];
                unitsDispersion[0].RemoveAt(0);
                
                if (ShareAtLeastOne(solver.Possibilities[one.Row, one.Col],
                        solver.Possibilities[current.Row, current.Col]))
                {
                    foreach (var two in unitsDispersion[1])
                    {
                        if(IsYWing(solver, current, one, two)
                           && ProcessYWing(solver, current, one, two))
                            return;
                    }
                    
                    foreach (var two in unitsDispersion[2])
                    {
                        if(IsYWing(solver, current, one, two)
                           && ProcessYWing(solver, current, one, two))
                            return;
                    }
                }
            }
            
            //Columns
            while (unitsDispersion[1].Count > 0)
            {
                Coordinate one = unitsDispersion[1][0];
                unitsDispersion[1].RemoveAt(0);
                
                if (ShareAtLeastOne(solver.Possibilities[one.Row, one.Col],
                        solver.Possibilities[current.Row, current.Col]))
                {
                    foreach (var two in unitsDispersion[2])
                    {
                        if (IsYWing(solver, current, one, two)
                            && ProcessYWing(solver, current, one, two))
                            return;
                    }
                }
            }
        }
    }

    private List<Coordinate>[] MatchingUnitDispersion(Coordinate coord, List<Coordinate> toSee)
    {
        List<Coordinate>[] result = { new(), new(), new() };
        foreach (var c in toSee)
        {
            if (c.Row == coord.Row)
            {
                result[0].Add(c);
            }
            if (c.Col == coord.Col)
            {
                result[1].Add(c);
            }
            if (c.Row / 3 == coord.Row / 3 && c.Col / 3 == coord.Col / 3)
            {
                result[2].Add(c);
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
    /// <param name="solver"></param>
    /// <param name="opposite"></param>
    /// <param name="one"></param>
    /// <param name="two"></param>
    /// <returns></returns>
    private static bool IsYWing(ISolver solver, Coordinate opposite, Coordinate one, Coordinate two)
    {
        if (AreAllInSameUnit(opposite, one, two)) return false;
        var oppositePoss = solver.Possibilities[opposite.Row, opposite.Col];
        var onePoss = solver.Possibilities[one.Row, one.Col];
        var twoPoss = solver.Possibilities[two.Row, two.Col];
        
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
    
    private static bool AreAllInSameUnit(Coordinate one, Coordinate two, Coordinate three)
    {
        return (one.Row == two.Row && one.Row == three.Row) || (one.Col == two.Col && one.Col == three.Col) ||
               (one.Row / 3 == two.Row / 3 && one.Col / 3 == two.Col / 3 && one.Row / 3 == three.Row / 3 &&
                one.Col / 3 == three.Col / 3);
    }

    private static bool ProcessYWing(ISolver solver, Coordinate opposite, Coordinate one, Coordinate two)
    {
        bool wasProgressMade = false;

        int toRemove = Minus(solver.Possibilities[one.Row, one.Col],
            solver.Possibilities[opposite.Row, opposite.Col]);
        foreach (var coord in MatchingCells(one, two))
        {
            if (solver.RemovePossibility(toRemove, coord.Row, coord.Col,
                    new XYWingLog(toRemove, coord.Row, coord.Col,
                        opposite, one, two))) wasProgressMade = true;
        }

        return wasProgressMade;
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
    private static IEnumerable<Coordinate> MatchingCells(Coordinate one, Coordinate two)
    {
        if (one.Row / 3 == two.Row / 3)
        {
            int startColOne = one.Col / 3;
            for (int i = 0; i < 3; i++)
            {
                yield return new Coordinate(two.Row, startColOne * 3 + i);
            }

            int startColTwo = two.Col / 3;
            for (int i = 0; i < 3; i++)
            {
                yield return new Coordinate(one.Row, startColTwo * 3 + i);
            }
        }
        else if (one.Col / 3 == two.Col / 3)
        {
            int startRowOne = one.Row / 3;
            for (int i = 0; i < 3; i++)
            {
                yield return new Coordinate(startRowOne * 3 + i, two.Col);
            }

            int startRowTwo = two.Row / 3;
            for (int i = 0; i < 3; i++)
            {
                yield return new Coordinate(startRowTwo * 3 + i, one.Col);
            }
        }
        else
        {
            yield return new Coordinate(one.Row, two.Col);
            yield return new Coordinate(two.Row, one.Col);
        }
    }

    private static List<Coordinate> AllCellsWith2Possibilities(ISolver solver)
    {
        List<Coordinate> result = new();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (solver.Possibilities[row, col].Count == 2) result.Add(new Coordinate(row, col));
            }
        }

        return result;
    }
}

// ReSharper disable once InconsistentNaming
public class XYWingLog : ISolverLog
{
    public string AsString { get; }
    public StrategyLevel Level { get; } = StrategyLevel.Hard;

    public XYWingLog(int number, int row, int col, Coordinate one, Coordinate two, Coordinate three)
    {
        AsString = $"[{row + 1}, {col + 1}] {number} removed from possibilities because of XY-Wings" +
                   $" at {one}, {two} and {three}";
    }
}