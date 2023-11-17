using System.Collections.Generic;
using Global;
using Model.Solver.Position;
using Model.Solver.Possibility;

namespace Model.Solver.StrategiesUtility.Exocet;

public static class JuniorExocetSearcher
{
    private const int Max = 4;
    
    public static List<JuniorExocet> FullGrid(IStrategyManager strategyManager)
    {
        var result = new List<JuniorExocet>();

        for (int unit = 0; unit < 9; unit++)
        {
            for (int otherStart = 0; otherStart < 9; otherStart += 3)
            {
                for (int o1 = 0; o1 < 2; o1++)
                {
                    for (int o2 = o1 + 1; o2 < 3; o2++)
                    {
                        //Rule 1
                        var base1 = new Cell(unit, otherStart + o1);
                        var base2 = new Cell(unit, otherStart + o2);

                        var poss1 = strategyManager.PossibilitiesAt(base1);
                        var poss2 = strategyManager.PossibilitiesAt(base2);
                        var or = poss1.Or(poss2);

                        if (poss1.Count != 0 && poss2.Count != 0 && or.Count <= Max)
                            TryOfBase(strategyManager, base1, base2, or, result);
                        
                        base1 = new Cell(otherStart + o1, unit);
                        base2 = new Cell(otherStart + o2, unit);

                        poss1 = strategyManager.PossibilitiesAt(base1);
                        poss2 = strategyManager.PossibilitiesAt(base2);
                        or = poss1.Or(poss2);

                        if (poss1.Count != 0 && poss2.Count != 0 && or.Count <= Max)
                            TryOfBase(strategyManager, base1, base2, or, result);
                    }
                }
            }
        }

        return result;
    }

    private static void TryOfBase(IStrategyManager strategyManager, Cell base1, Cell base2, Possibilities basePossibilities,
        List<JuniorExocet> result)
    {
        var isRowJe = base1.Row == base2.Row;
        var targetCandidates = isRowJe
            ? RowTargetCandidates(strategyManager, base1, basePossibilities)
            : ColumnTargetCandidates(strategyManager, base1, basePossibilities);

        for (int i = 0; i < targetCandidates.Count; i++)
        {
            for (int j = i + 1; j < targetCandidates.Count; j++)
            {
                //Rule 4
                var t1 = targetCandidates[i];
                var t2 = targetCandidates[j];

                if (!strategyManager.PossibilitiesAt(t1).Or(strategyManager.PossibilitiesAt(t2))
                    .PeekAll(basePossibilities) || (t1.Row / 3 == t2.Row / 3 && t1.Col / 3 == t2.Col / 3) ) continue;

                Cell[] t1MirrorNodes;
                Cell[] t2MirrorNodes;

                if (isRowJe) ComputeRowMirrorNode(out t1MirrorNodes, out t2MirrorNodes, t1, t2, base1);
                else ComputeColumnMirrorNode(out t1MirrorNodes, out t2MirrorNodes, t1, t2, base1);

                bool notOk = true;

                foreach (var cell in t1MirrorNodes)
                {
                    if (strategyManager.ContainsAny(cell.Row, cell.Col, basePossibilities)) notOk = false;
                }
                
                foreach (var cell in t2MirrorNodes)
                {
                    if (strategyManager.ContainsAny(cell.Row, cell.Col, basePossibilities)) notOk = false;
                }

                if (notOk) continue;
                
                //Rule 3
                var sCellsPositions = new Dictionary<int, GridPositions>();
                foreach (var possibility in basePossibilities)
                {
                    sCellsPositions.Add(possibility, new GridPositions());
                }

                if (isRowJe) FillRowSCells(strategyManager, sCellsPositions, base1, base2, t1, t2, basePossibilities);
                else FillColumnSCells(strategyManager, sCellsPositions, base1, base2, t1, t2, basePossibilities);

                foreach (var gp in sCellsPositions.Values)
                {
                    if (!gp.CanBeCoveredByLines(2, Unit.Row, Unit.Column))
                    {
                        notOk = true;
                        break;
                    }
                }

                if (notOk) continue;

                var escapeCell = isRowJe
                    ? new Cell(base1.Row, OtherUnitInBox(base1.Col, base2.Col))
                    : new Cell(OtherUnitInBox(base1.Row, base2.Row), base1.Col);
                result.Add(new JuniorExocet(base1, base2, t1, t2, basePossibilities, escapeCell,
                    t1MirrorNodes, t2MirrorNodes, sCellsPositions));
            }
        }
    }

    private static List<Cell> RowTargetCandidates(IStrategyManager strategyManager, Cell base1,
        Possibilities basePossibilities)
    {
        //Rule 2
        List<Cell> result = new();
        var rows = OtherUnitsInBox(base1.Row);
        var baseGridCol = base1.Col / 3;

        for (int gridCol = 0; gridCol < 3; gridCol++)
        {
            if (gridCol == baseGridCol) continue;

            for (int c = 0; c < 3; c++)
            {
                var col = gridCol * 3 + c;

                var poss1 = strategyManager.PossibilitiesAt(rows[0], col);
                var poss2 = strategyManager.PossibilitiesAt(rows[1], col);

                if (poss1.PeekAny(basePossibilities) && !strategyManager.ContainsAny(rows[1], col, basePossibilities))
                    result.Add(new Cell(rows[0], col));
                else if (poss2.PeekAny(basePossibilities) && !strategyManager.ContainsAny(rows[0], col, basePossibilities))
                    result.Add(new Cell(rows[1], col));
            }
        }

        return result;
    }
    
    private static List<Cell> ColumnTargetCandidates(IStrategyManager strategyManager, Cell base1,
        Possibilities basePossibilities)
    {
        //Rule 2
        List<Cell> result = new();
        var cols = OtherUnitsInBox(base1.Col);
        var baseGridRow = base1.Row / 3;

        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            if (gridRow == baseGridRow) continue;

            for (int r = 0; r < 3; r++)
            {
                var row = gridRow * 3 + r;

                var poss1 = strategyManager.PossibilitiesAt(row, cols[0]);
                var poss2 = strategyManager.PossibilitiesAt(row, cols[1]);

                if (poss1.PeekAny(basePossibilities) && !strategyManager.ContainsAny(row, cols[1], basePossibilities))
                    result.Add(new Cell(row, cols[0]));
                else if (poss2.PeekAny(basePossibilities) && !strategyManager.ContainsAny(row, cols[0], basePossibilities))
                    result.Add(new Cell(row, cols[1]));
            }
        }

        return result;
    }

    private static void ComputeRowMirrorNode(out Cell[] t1MirrorCells, out Cell[]
        t2MirrorCells, Cell t1, Cell t2, Cell baseC)
    {
        var t1StartCol = t1.Col / 3 * 3;
        var t2StartCol = t2.Col / 3 * 3;
                
        t1MirrorCells = new Cell[2];
        t2MirrorCells = new Cell[2];

        var cursor1 = 0;
        var cursor2 = 0;
                
        if (t1.Row == t2.Row)
        {
            var lastRow = OtherUnitInBox(t1.Row, baseC.Row);

            for (int c = 0; c < 3; c++)
            {
                var col1 = t1StartCol + c;
                var col2 = t2StartCol + c;

                if (col1 != t1.Col) t2MirrorCells[cursor2++] = new Cell(lastRow, col1);
                if (col2 != t2.Col) t1MirrorCells[cursor1++] = new Cell(lastRow, col2);
            }
        }
        else
        {
            for (int c = 0; c < 3; c++)
            {
                var col1 = t1StartCol + c;
                var col2 = t2StartCol + c;

                if (col1 != t1.Col) t2MirrorCells[cursor2++] = new Cell(t1.Row, col1);
                if (col2 != t2.Col) t1MirrorCells[cursor1++] = new Cell(t2.Row, col2);
            }
        }
    }
    
    private static void ComputeColumnMirrorNode(out Cell[] t1MirrorCells, out Cell[]
        t2MirrorCells, Cell t1, Cell t2, Cell baseC)
    {
        var t1StartRow = t1.Row / 3 * 3;
        var t2StartRow = t2.Row / 3 * 3;
                
        t1MirrorCells = new Cell[2];
        t2MirrorCells = new Cell[2];

        var cursor1 = 0;
        var cursor2 = 0;
                
        if (t1.Col == t2.Col)
        {
            var lastCol = OtherUnitInBox(t1.Col, baseC.Col);

            for (int r = 0; r < 3; r++)
            {
                var row1 = t1StartRow + r;
                var row2 = t2StartRow + r;

                if (row1 != t1.Row) t2MirrorCells[cursor2++] = new Cell(row1, lastCol);
                if (row2 != t2.Row) t1MirrorCells[cursor1++] = new Cell(row2, lastCol);
            }
        }
        else
        {
            for (int r = 0; r < 3; r++)
            {
                var row1 = t1StartRow + r;
                var row2 = t2StartRow + r;

                if (row1 != t1.Row) t2MirrorCells[cursor2++] = new Cell(row1, t1.Col);
                if (row2 != t2.Row) t1MirrorCells[cursor1++] = new Cell(row2, t2.Col);
            }
        }
    }

    private static void FillRowSCells(IStrategyManager strategyManager, Dictionary<int, GridPositions> sCellsPositions,
        Cell base1, Cell base2, Cell t1, Cell t2, Possibilities basePossibilities)
    {
        var lastBaseCol = OtherUnitInBox(base1.Col, base2.Col);
        for (int row = 0; row < 9; row++)
        {
            if (row / 3 == base1.Row / 3) continue;

            foreach (var possibility in basePossibilities)
            {
                if(strategyManager.Contains(row, lastBaseCol, possibility))
                    sCellsPositions[possibility].Add(row, lastBaseCol);
                        
                if(strategyManager.Contains(row, t1.Col, possibility))
                    sCellsPositions[possibility].Add(row, t1.Col);
                        
                if(strategyManager.Contains(row, t2.Col, possibility))
                    sCellsPositions[possibility].Add(row, t2.Col);
            }
        }
    }
    
    private static void FillColumnSCells(IStrategyManager strategyManager, Dictionary<int, GridPositions> sCellsPositions,
        Cell base1, Cell base2, Cell t1, Cell t2, Possibilities basePossibilities)
    {
        var lastBaseRow = OtherUnitInBox(base1.Row, base2.Row);
        for (int col = 0; col < 9; col++)
        {
            if (col / 3 == base1.Col / 3) continue;

            foreach (var possibility in basePossibilities)
            {
                if(strategyManager.Contains(lastBaseRow, col, possibility))
                    sCellsPositions[possibility].Add(lastBaseRow, col);
                        
                if(strategyManager.Contains(t1.Row, col, possibility))
                    sCellsPositions[possibility].Add(t1.Row, col);
                        
                if(strategyManager.Contains(t2.Row, col, possibility))
                    sCellsPositions[possibility].Add(t2.Row, col);
            }
        }
    }

    private static int[] OtherUnitsInBox(int except)
    {
        var result = new int[2];
        int cursor = 0;
        var start = except / 3 * 3;

        for (int i = 0; i < 3; i++)
        {
            var u = start + i;
            if (u == except) continue;

            result[cursor] = u;
            cursor++;
        }

        return result;
    }

    private static int OtherUnitInBox(int except1, int except2)
    {
        var start = except1 / 3 * 3;
        for (int i = 0; i < 3; i++)
        {
            var u = start + i;
            if (u == except1 || u == except2) continue;

            return u;
        }

        return -1;
    }
}