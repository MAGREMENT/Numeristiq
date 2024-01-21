using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.Solver.StrategiesUtility.AlmostLockedSets;

namespace Model.Solver.StrategiesUtility.Graphs.ConstructRules;

public class AlmostNakedSetConstructRule : IConstructRule
{
    public void Apply(ILinkGraph<ISudokuElement> linkGraph, IStrategyManager strategyManager)
    {
        foreach (var als in strategyManager.PreComputer.AlmostLockedSets())
        {
            foreach (var p in als.Possibilities)
            {
                var rowBuffer = -1;
                var colBuffer = -1;
                var soloRow = true;
                var soloCol = true;
                
                List<Cell> notIn = new();
                List<CellPossibilities> cps = new();
                
                foreach (var cell in als.EachCell())
                {
                    var possibilities = als.PossibilitiesInCell(cell).Copy();
                    if (possibilities.Peek(p))
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
                    
                    possibilities.Remove(p);
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
                    else element = new CellsPossibility(p, notIn.ToArray());

                    linkGraph.Add(element, nakedSet, LinkStrength.Strong, LinkType.MonoDirectional);

                    foreach (var ssc in Cells.SharedSeenCells(notIn))
                    {
                        if (strategyManager.PossibilitiesAt(ssc).Peek(p)) linkGraph.Add(new CellPossibility(ssc, p),
                                element, LinkStrength.Weak, LinkType.MonoDirectional);
                    }
                }
                
                foreach (var possibility in als.Possibilities)
                {
                    if (possibility == p) continue;
                    List<Cell> cells = new List<Cell>(als.EachCell(possibility));

                    foreach (var ssc in Cells.SharedSeenCells(cells))
                    {
                        if(strategyManager.PossibilitiesAt(ssc).Peek(possibility)) linkGraph.Add(nakedSet,
                            new CellPossibility(ssc, possibility), LinkStrength.Weak, LinkType.MonoDirectional);
                    }
                }
            }
        }
    }

    public void Apply(ILinkGraph<CellPossibility> linkGraph, IStrategyManager strategyManager)
    {
        
    }
}