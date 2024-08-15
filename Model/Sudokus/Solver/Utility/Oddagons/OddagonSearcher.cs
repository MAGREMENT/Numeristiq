using System;
using System.Collections.Generic;
using Model.Core;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Sudokus.Solver.Utility.Graphs.ConstructRules;
using Model.Sudokus.Solver.Utility.Oddagons.Algorithms;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Utility.Oddagons;

public static class OddagonSearcher
{
    private static readonly IOddagonSearchAlgorithm Algorithm = new OddagonSearchAlgorithmV1();

    public static List<AlmostOddagon> Search(ISudokuSolverData solverData, int maxLength, int maxGuardians)
    {
        solverData.PreComputer.SimpleGraph.Construct(CellStrongLinkConstructRule.Instance,
            CellWeakLinkConstructRule.Instance,
            UnitStrongLinkConstructRule.Instance, UnitWeakLinkConstructRule.Instance);

        Algorithm.MaxLength = maxLength;
        Algorithm.MaxGuardians = maxGuardians;
        
        return Algorithm.Search(solverData, solverData.PreComputer.SimpleGraph.Graph);
    }

    public static IEnumerable<CellPossibility> FindGuardians(ISudokuSolvingState holder, CellPossibility one, CellPossibility two)
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
            foreach (var p in holder.PossibilitiesAt(one.Row, one.Column).EnumeratePossibilities())
            {
                if (p == one.Possibility || p == two.Possibility) continue;

                yield return new CellPossibility(one.Row, one.Column, p);
            }
        }
        else throw new Exception();
    }
}