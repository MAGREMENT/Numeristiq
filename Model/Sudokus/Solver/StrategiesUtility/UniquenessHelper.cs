using System;
using Model.Sudokus.Solver.Position;

namespace Model.Sudokus.Solver.StrategiesUtility;

public static class UniquenessHelper
{
    public static int SearchExceptionInUnit(Unit unit, int expected, GridPositions gp)
    {
        var result = -1;
        var methods = UnitMethods.Get(unit);

        for (int u = 0; u < 9; u++)
        {
            var count = methods.Count(gp, u);

            if (expected == count || count == 0) continue;

            if (expected == count + 1)
            {
                if (result == -1) result = u;
                else return -1;
            }
            else return -1;
        }

        return result;
    }

    public static int ComputeExpectedCount(Unit unit, params GridPositions[] gps)
    {
        var result = 0;
        var methods = UnitMethods.Get(unit);

        for (int u = 0; u < 9; u++)
        {
            var count = 0;

            foreach (var gp in gps)
            {
                if (methods.Count(gp, u) > 0) count++;
            }

            result = Math.Max(result, count);
        }

        return result;
    }
}