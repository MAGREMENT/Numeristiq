using System;
using System.Collections.Generic;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;
using Model.Sudoku.Solver.StrategiesUtility.Oddagons.Algorithms;

namespace Model.Sudoku.Solver.StrategiesUtility.Oddagons;

public static class OddagonSearcher
{
    private static readonly IOddagonSearchAlgorithm Algorithm = new OddagonSearchAlgorithmV3(7, 3);

    public static List<AlmostOddagon> Search(IStrategyManager strategyManager)
    {
        strategyManager.GraphManager.ConstructSimple(ConstructRule.CellStrongLink, ConstructRule.CellWeakLink,
            ConstructRule.UnitStrongLink, ConstructRule.UnitWeakLink);
        return Algorithm.Search(strategyManager, strategyManager.GraphManager.SimpleLinkGraph);
    }

    public static IEnumerable<CellPossibility> FindGuardians(IPossibilitiesHolder holder, CellPossibility one, CellPossibility two)
    {
        if (one.Possibility == two.Possibility)
        {
            if (one.Row == two.Row)
            {
                foreach (var col in holder.RowPositionsAt(one.Row, one.Possibility))
                {
                    if (col == one.Column || col == two.Column) continue;

                    yield return new CellPossibility(one.Row, col, one.Possibility);
                }
            }
            else if (one.Column == two.Column)
            {
                foreach (var row in holder.ColumnPositionsAt(one.Column, one.Possibility))
                {
                    if (row == one.Row || row == two.Row) continue;

                    yield return new CellPossibility(row, one.Column, one.Possibility);
                }
            }
            else if (one.Row / 3 == two.Row / 3 && one.Column / 3 == two.Column / 3)
            {
                foreach (var cell in holder.MiniGridPositionsAt(one.Row / 3, one.Column / 3, one.Possibility))
                {
                    if (cell == one.ToCell() || cell == two.ToCell()) continue;
                        
                    yield return new CellPossibility(cell, one.Possibility);
                }
            }
            else throw new Exception();
        }
        else if (one.Row == two.Row && one.Column == two.Column)
        {
            foreach (var p in holder.PossibilitiesAt(one.Row, one.Column))
            {
                if (p == one.Possibility || p == two.Possibility) continue;

                yield return new CellPossibility(one.Row, one.Column, p);
            }
        }
        else throw new Exception();
    }
}