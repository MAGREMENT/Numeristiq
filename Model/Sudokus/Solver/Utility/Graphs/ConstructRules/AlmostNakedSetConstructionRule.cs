using System.Collections.Generic;
using Model.Core.Graphs;
using Model.Sudokus.Solver.Utility.AlmostLockedSets;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Utility.Graphs.ConstructRules;

public class AlmostNakedSetConstructionRule : IConstructionRule<ISudokuSolverData, IGraph<ISudokuElement, LinkStrength>>
{
    public static AlmostNakedSetConstructionRule Instance { get; } = new();
    
    private AlmostNakedSetConstructionRule(){}
    
    public int ID { get; } = UniqueConstructionRuleID.Next();

    public void Apply(IGraph<ISudokuElement, LinkStrength> linkGraph, ISudokuSolverData data)
    {
        foreach (var als in data.PreComputer.AlmostLockedSets())
        {
            foreach (var p in als.Possibilities.EnumeratePossibilities())
            {
                var rowBuffer = -1;
                var colBuffer = -1;
                var soloRow = true;
                var soloCol = true;
                
                List<Cell> notIn = new();
                List<CellPossibilities> cps = new();
                
                foreach (var cell in als.EnumerateCells())
                {
                    var possibilities = als.PossibilitiesInCell(cell);
                    if (possibilities.Contains(p))
                    {
                        notIn.Add(cell);
                        if (soloRow)
                        {
                            if (rowBuffer == -1) rowBuffer = cell.Row;
                            else if (rowBuffer != cell.Row) soloRow = false;
                        }
                        if (soloCol)
                        {
                            if (colBuffer == -1) colBuffer = cell.Column;
                            else if (colBuffer != cell.Column) soloCol = false;
                        }
                    }
                    
                    possibilities -= p;
                    cps.Add(new CellPossibilities(cell, possibilities));
                }

                var nakedSet = new NakedSet(cps.ToArray());
                if (notIn.Count == 1)
                {
                    linkGraph.Add(new CellPossibility(notIn[0], p), nakedSet,
                        LinkStrength.Strong, LinkType.MonoDirectional);
                }
                else
                {
                    ISudokuElement element;
                    if (soloRow && rowBuffer != -1) element = new PointingRow(p, notIn);
                    else if (soloCol && colBuffer != -1) element = new PointingColumn(p, notIn);
                    else element = new CellsPossibility(p, notIn.ToArray()); //TODO useless ?

                    linkGraph.Add(element, nakedSet, LinkStrength.Strong, LinkType.MonoDirectional);

                    foreach (var ssc in SudokuUtility.SharedSeenCells(notIn))
                    {
                        if (data.PossibilitiesAt(ssc).Contains(p)) linkGraph.Add(new CellPossibility(ssc, p),
                                element, LinkStrength.Weak, LinkType.MonoDirectional);
                    }
                }
                
                foreach (var possibility in als.Possibilities.EnumeratePossibilities())
                {
                    if (possibility == p) continue;
                    var cells = new List<Cell>(als.EnumerateCells(possibility));

                    foreach (var ssc in SudokuUtility.SharedSeenCells(cells))
                    {
                        if(data.PossibilitiesAt(ssc).Contains(possibility)) linkGraph.Add(nakedSet,
                            new CellPossibility(ssc, possibility), LinkStrength.Weak, LinkType.MonoDirectional);
                    }
                }
            }
        }
    }
}