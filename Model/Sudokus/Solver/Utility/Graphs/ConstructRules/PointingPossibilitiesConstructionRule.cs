﻿using System.Collections.Generic;
using Model.Core.Graphs;
using Model.Sudokus.Solver.Position;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility.Graphs.ConstructRules;

public class PointingPossibilitiesConstructionRule : IConstructionRule<ISudokuSolverData, IGraph<ISudokuElement, LinkStrength>>
{
    public static PointingPossibilitiesConstructionRule Instance { get; } = new();
    
    private PointingPossibilitiesConstructionRule() {}
    
    public int ID { get; } = UniqueConstructionRuleID.Next();
    
    public void Apply(IGraph<ISudokuElement, LinkStrength> linkGraph, ISudokuSolverData data)
    {
        for (int n = 1; n <= 9; n++)
        {
            for (int miniRow = 0; miniRow < 3; miniRow++)
            {
                for (int miniCol = 0; miniCol < 3; miniCol++)
                {
                    var ppimn = data.MiniGridPositionsAt(miniRow, miniCol, n);
                    if (ppimn.Count < 2) continue;
                    SearchForPointingInMiniGrid(data, linkGraph, ppimn, miniRow, miniCol, n);
                }
            }
        }
    }

    private void SearchForPointingInMiniGrid(ISudokuSolverData solverData, IGraph<ISudokuElement, LinkStrength> linkGraph,
        IReadOnlyBoxPositions ppimn, int miniRow, int miniCol, int numba)
    {
        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            var colPos = ppimn.OnGridRow(gridRow);
            if (colPos.Count > 1)
            {
                List<CellPossibility> singles = new();
                List<PointingColumn> pcs = new();
                for (int gridCol = 0; gridCol < 3; gridCol++)
                {
                    int buffer = -1;
                    for (int a = 0; a < 3; a++)
                    {
                        if (a == gridRow) continue;
                        if (ppimn.Contains(a, gridCol))
                        {
                            singles.Add(new CellPossibility(miniRow * 3 + a, miniCol * 3 + gridCol, numba));
                            if (buffer == -1) buffer = a;
                            else pcs.Add(new PointingColumn(numba, miniCol * 3 + gridCol,
                                miniRow * 3 + a, miniRow * 3 + buffer));
                        }
                    }
                }

                var singleStrength = singles.Count == 1 ? LinkStrength.Strong : LinkStrength.Weak;
                var pcsStrength = pcs.Count == 1 && singles.Count == pcs[0].Count ? LinkStrength.Strong : LinkStrength.Weak;
                var current = new PointingRow(numba, miniRow * 3 + gridRow, colPos);

                foreach (var single in singles)
                {
                    linkGraph.Add(current, single, singleStrength);
                }

                foreach (var pc in pcs)
                {
                    linkGraph.Add(current, pc, pcsStrength);
                }
                
                singles.Clear();
                var prs = new List<PointingRow>();

                for (int miniCol2 = 0; miniCol2 < 3; miniCol2++)
                {
                    if (miniCol == miniCol2) continue;

                    List<CellPossibility> aligned = new();
                    for (int gridCol = 0; gridCol < 3; gridCol++)
                    {
                        int row = miniRow * 3 + gridRow;
                        int col = miniCol2 * 3 + gridCol;

                        if (solverData.PossibilitiesAt(row, col).Contains(numba)) aligned.Add(new CellPossibility(row, col, numba));
                    }
                    
                    singles.AddRange(aligned);
                    if(aligned.Count > 1) prs.Add(new PointingRow(numba, aligned));
                }
                
                singleStrength = singles.Count == 1 ? LinkStrength.Strong : LinkStrength.Weak;
                var prsStrength = prs.Count == 1 && singles.Count == prs[0].Count ? LinkStrength.Strong : LinkStrength.Weak;

                foreach (var single in singles)
                {
                    linkGraph.Add(current, single, singleStrength);
                }

                foreach (var pr in prs)
                {
                    linkGraph.Add(current, pr, prsStrength);
                }
            }
        }
        
        for (int gridCol = 0; gridCol < 3; gridCol++)
        {
            var rowPos = ppimn.OnGridColumn(gridCol);
            if (rowPos.Count > 1)
            {
                List<CellPossibility> singles = new();
                List<PointingRow> prs = new();
                for (int gridRow = 0; gridRow < 3; gridRow++)
                {
                    int buffer = -1;
                    for (int a = 0; a < 3; a++)
                    {
                        if (a == gridCol) continue;
                        if (ppimn.Contains(gridRow, a))
                        {
                            singles.Add(new CellPossibility(miniRow * 3 + gridRow, miniCol * 3 + a, numba));
                            if (buffer == -1) buffer = a;
                            else prs.Add(new PointingRow(numba, miniRow * 3 + gridRow,
                                miniCol * 3 + a, miniCol * 3 + buffer));
                        }
                    }
                }

                var singleStrength = singles.Count == 1 ? LinkStrength.Strong : LinkStrength.Weak;
                var prsStrength = prs.Count == 1 && singles.Count == prs[0].Count ? LinkStrength.Strong : LinkStrength.Weak;
                var current = new PointingColumn(numba, miniCol * 3 + gridCol, rowPos);

                foreach (var single in singles)
                {
                    linkGraph.Add(current, single, singleStrength);
                }

                foreach (var pc in prs)
                {
                    linkGraph.Add(current, pc, prsStrength);
                }
                
                singles.Clear();
                var pcs = new List<PointingColumn>();

                for (int miniRow2 = 0; miniRow2 < 3; miniRow2++)
                {
                    if (miniRow == miniRow2) continue;

                    List<CellPossibility> aligned = new();
                    for (int gridRow = 0; gridRow < 3; gridRow++)
                    {
                        int row = miniRow2 * 3 + gridRow;
                        int col = miniCol * 3 + gridCol;

                        if (solverData.PossibilitiesAt(row, col).Contains(numba)) aligned.Add(new CellPossibility(row, col, numba));
                    }
                    
                    singles.AddRange(aligned);
                    if(aligned.Count > 1) pcs.Add(new PointingColumn(numba, aligned));
                }
                
                singleStrength = singles.Count == 1 ? LinkStrength.Strong : LinkStrength.Weak;
                var pcsStrength = pcs.Count == 1 && singles.Count == pcs[0].Count ? LinkStrength.Strong : LinkStrength.Weak;

                foreach (var single in singles)
                {
                    linkGraph.Add(current, single, singleStrength);
                }

                foreach (var pc in pcs)
                {
                    linkGraph.Add(current, pc, pcsStrength);
                }
            }
        }
    }
}