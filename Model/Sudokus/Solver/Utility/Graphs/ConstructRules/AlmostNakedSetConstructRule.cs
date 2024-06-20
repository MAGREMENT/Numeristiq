using System.Collections.Generic;
using Model.Core.Graphs;
using Model.Sudokus.Solver.Utility.AlmostLockedSets;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Utility.Graphs.ConstructRules;

public class AlmostNakedSetConstructRule : IConstructRule<ISudokuSolverData, ISudokuElement>
{
    public void Apply(ILinkGraph<ISudokuElement> linkGraph, ISudokuSolverData solverData)
    {
        foreach (var als in solverData.PreComputer.AlmostLockedSets())
        {
            foreach (var p in als.Possibilities.EnumeratePossibilities())
            {
                var rowBuffer = -1;
                var colBuffer = -1;
                var soloRow = true;
                var soloCol = true;
                
                List<Cell> notIn = new();
                List<CellPossibilities> cps = new();
                
                foreach (var cell in als.EachCell())
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

                    foreach (var ssc in SudokuCellUtility.SharedSeenCells(notIn))
                    {
                        if (solverData.PossibilitiesAt(ssc).Contains(p)) linkGraph.Add(new CellPossibility(ssc, p),
                                element, LinkStrength.Weak, LinkType.MonoDirectional);
                    }
                }
                
                foreach (var possibility in als.Possibilities.EnumeratePossibilities())
                {
                    if (possibility == p) continue;
                    var cells = new List<Cell>(als.EachCell(possibility));

                    foreach (var ssc in SudokuCellUtility.SharedSeenCells(cells))
                    {
                        if(solverData.PossibilitiesAt(ssc).Contains(possibility)) linkGraph.Add(nakedSet,
                            new CellPossibility(ssc, possibility), LinkStrength.Weak, LinkType.MonoDirectional);
                    }
                }
            }
        }
    }

    public void Apply(ILinkGraph<CellPossibility> linkGraph, ISudokuSolverData solverData)
    {
        
    }
}