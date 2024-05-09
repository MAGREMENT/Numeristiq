using System.Collections.Generic;
using Model.Sudokus.Solver.Position;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Utility.Exocet;

public static class ExocetSearcher
{
    private const int Max = 4;

    public static List<SeniorExocet> SearchSeniors(ISudokuStrategyUser strategyUser)
    {
        var result = new List<SeniorExocet>();

        foreach (var (c1, c2, p) in EnumerateBaseCandidates(strategyUser))
        {
            TryOfBase(strategyUser, c1, c2, p, result);
        }

        return result;
    }
    
    public static List<JuniorExocet> SearchJuniors(ISudokuStrategyUser strategyUser)
    {
        var result = new List<JuniorExocet>();

        foreach (var (c1, c2, p) in EnumerateBaseCandidates(strategyUser))
        {
            TryOfBase(strategyUser, c1, c2, p, result);
        }

        return result;
    }

    public static List<SingleTargetExocet> SearchSingleTargets(ISudokuStrategyUser strategyUser)
    {
        var result = new List<SingleTargetExocet>();

        foreach (var (c1, c2, p) in EnumerateBaseCandidates(strategyUser))
        {
            TryOfBase(strategyUser, c1, c2, p, result);
        }

        return result;
    }
    
    private static IEnumerable<(Cell, Cell, ReadOnlyBitSet16)> EnumerateBaseCandidates(ISudokuStrategyUser strategyUser)
    {
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

                        var poss1 = strategyUser.PossibilitiesAt(base1);
                        var poss2 = strategyUser.PossibilitiesAt(base2);
                        var or = poss1 | poss2;

                        if (poss1.Count != 0 && poss2.Count != 0 && or.Count <= Max) yield return (base1, base2, or);
                        
                        base1 = new Cell(otherStart + o1, unit);
                        base2 = new Cell(otherStart + o2, unit);

                        poss1 = strategyUser.PossibilitiesAt(base1);
                        poss2 = strategyUser.PossibilitiesAt(base2);
                        or = poss1 | poss2;

                        if (poss1.Count != 0 && poss2.Count != 0 && or.Count <= Max) yield return (base1, base2, or);
                    }
                }
            }
        }
    }

    private static void TryOfBase(ISudokuStrategyUser strategyUser, Cell base1, Cell base2,
        ReadOnlyBitSet16 basePossibilities, List<SeniorExocet> result)
    {
        var isRowJe = base1.Row == base2.Row;
        var escapeCross = isRowJe
            ? new House(Unit.Column, OtherUnitInBox(base1.Column, base2.Column))
            : new House(Unit.Row, OtherUnitInBox(base1.Row, base2.Row));

        var u = OtherUnitsInBox(isRowJe ? base1.Column / 3 : base1.Row / 3);
        for (int i = 0; i < 3; i++)
        {
            var cross1 = new House(isRowJe ? Unit.Column : Unit.Row, u[0] * 3 + i);
            for (int j = 0; j < 3; j++)
            {
                var cross2 = new House(isRowJe ? Unit.Column : Unit.Row, u[1] * 3 + j);
                var targetCandidates = TargetCandidates(strategyUser, base1, escapeCross,
                    cross1, cross2, basePossibilities);

                for (int k = 0; k < targetCandidates.Count; k++)
                {
                    for (int l = k + 1; l < targetCandidates.Count; l++)
                    {
                        var t1 = targetCandidates[k];
                        var t2 = targetCandidates[l];
                        
                        if(t1.Row == t2.Row || t1.Column == t2.Column || 
                           !(strategyUser.PossibilitiesAt(t1) | strategyUser.PossibilitiesAt(t2)).ContainsAll(basePossibilities)) continue;

                        Dictionary<int, GridPositions> sCells = new();
                        foreach (var p in basePossibilities.EnumeratePossibilities())
                        {
                            var gp = new GridPositions();
                            FillSCells(strategyUser, gp, base1, escapeCross, p, t1, t2, isRowJe);
                            FillSCells(strategyUser, gp, base1, cross1, p, t1, t2, isRowJe);
                            FillSCells(strategyUser, gp, base1, cross2, p, t1, t2, isRowJe);

                            sCells.Add(p, gp);
                        }

                        bool notOk = false;
                        foreach (var gp in sCells.Values)
                        {
                            if (!gp.CanBeCoveredByUnits(2, Unit.Row, Unit.Column))
                            {
                                notOk = true;
                                break;
                            }
                        }

                        if (notOk) continue;
                        
                        result.Add(new SeniorExocet(base1, base2, t1, t2, basePossibilities,
                            escapeCross, cross1, cross2, sCells));
                    }
                }
            }
        }
    }

    private static void FillSCells(ISudokuStrategyUser strategyUser, GridPositions gp, Cell base1,
        House house, int possibility, Cell t1, Cell t2, bool isRowJe)
    {
        foreach (var cell in house.EnumerateCells())
        {
            if (isRowJe)
            {
                if (cell.Row == t1.Row || cell.Row == t2.Row) continue;
            }
            else if (cell.Column == t1.Column || cell.Column == t2.Column) continue;
            if (SudokuCellUtility.ShareAUnit(cell, base1)) continue;
            if(strategyUser.Contains(cell.Row, cell.Column, possibility)) gp.Add(cell);
        }
    }

    private static List<Cell> TargetCandidates(ISudokuStrategyUser strategyUser, Cell base1,
        House escapeCross, House cross1, House cross2, ReadOnlyBitSet16 basePossibilities)
    {
        var result = new List<Cell>();
        var buffer = new List<Cell>();

        for (int u = 0; u < 9; u++)
        {
            if (escapeCross.Unit == Unit.Column)
            {
                if (u == base1.Row) continue;
            }
            else if (u == base1.Column) continue;
            
            buffer.Add(cross1.GetCellAt(u));
            buffer.Add(cross2.GetCellAt(u));
            var e = escapeCross.GetCellAt(u);
            if (e.Row / 3 != base1.Row / 3 || e.Column / 3 != base1.Column / 3) buffer.Add(e);

            foreach (var c in buffer)
            {
                if (!strategyUser.PossibilitiesAt(c).ContainsAny(basePossibilities)) continue;
                bool ok = true;
                foreach (var other in buffer)
                {
                    if(other == c) continue;
                    if (strategyUser.ContainsAny(other, basePossibilities))
                    {
                        ok = false;
                        break;
                    }
                }
                
                if(ok) result.Add(c);
            }
            
            buffer.Clear();
        }

        return result;
    }
    
    private static void TryOfBase(ISudokuStrategyUser strategyUser, Cell base1, Cell base2, ReadOnlyBitSet16 basePossibilities,
        List<SingleTargetExocet> result)
    {
        if (base1.Row == base2.Row) TryOfBaseForRow(strategyUser, base1, base2, basePossibilities, result);
        else TryOfBaseForColumn(strategyUser, base1, base2, basePossibilities, result);
    }
    
    private static void TryOfBaseForColumn(ISudokuStrategyUser strategyUser, Cell base1, Cell base2,
        ReadOnlyBitSet16 basePossibilities, List<SingleTargetExocet> result)
    {
        var targetCandidates = ColumnTargetCandidates(strategyUser, base1, basePossibilities, true);

        var parallels = OtherUnitsInBox(base1.Column);
        foreach (var target in targetCandidates)
        {
            var last = OtherUnitInBox(target.Row / 3, base1.Row / 3);
            
            for (int u = 0; u < 3; u++)
            {
                var unit = last * 3 + u;
                bool notOk = false;
                foreach (var para in parallels)
                {
                    if (strategyUser.ContainsAny(unit, para, basePossibilities))
                    {
                        notOk = true;
                        break;
                    }
                }

                if (notOk) continue;

                var cross = new House(Unit.Row, unit);
                var sCellsPositions = new Dictionary<int, GridPositions>();
                foreach (var possibility in basePossibilities.EnumeratePossibilities())
                {
                    sCellsPositions.Add(possibility, new GridPositions());
                }

                FillColumnSCells(strategyUser, sCellsPositions, base1, base2, target, cross, basePossibilities);

                int wildCard = -1;
                foreach (var entry in sCellsPositions)
                {
                    if (entry.Value.CanBeCoveredByUnits(2, Unit.Column, Unit.Row)) continue;
                    
                    if (wildCard == -1) wildCard = entry.Key;
                    else
                    {
                        notOk = true;
                        break;
                    }
                }

                if (notOk || wildCard == -1) continue;

                result.Add(new SingleTargetExocet(base1, base2, target, cross,
                    wildCard, new Cell(OtherUnitInBox(base1.Row, base2.Row), base1.Column), basePossibilities, sCellsPositions));
            }
        }
    }

    private static void TryOfBaseForRow(ISudokuStrategyUser strategyUser, Cell base1, Cell base2,
        ReadOnlyBitSet16 basePossibilities, List<SingleTargetExocet> result)
    {
        var targetCandidates = RowTargetCandidates(strategyUser, base1, basePossibilities, true);

        var parallels = OtherUnitsInBox(base1.Row);
        foreach (var target in targetCandidates)
        {
            var last = OtherUnitInBox(target.Column / 3, base1.Column / 3);
            
            for (int u = 0; u < 3; u++)
            {
                var unit = last * 3 + u;
                bool notOk = false;
                foreach (var para in parallels)
                {
                    if (strategyUser.ContainsAny(para, unit, basePossibilities))
                    {
                        notOk = true;
                        break;
                    }
                }

                if (notOk) continue;

                var cross = new House(Unit.Column, unit);
                var sCellsPositions = new Dictionary<int, GridPositions>();
                foreach (var possibility in basePossibilities.EnumeratePossibilities())
                {
                    sCellsPositions.Add(possibility, new GridPositions());
                }

                FillRowSCells(strategyUser, sCellsPositions, base1, base2, target, cross, basePossibilities);

                int wildCard = -1;
                foreach (var entry in sCellsPositions)
                {
                    if (entry.Value.CanBeCoveredByUnits(2, Unit.Row, Unit.Column)) continue;
                    
                    if (wildCard == -1) wildCard = entry.Key;
                    else
                    {
                        notOk = true;
                        break;
                    }
                }

                if (notOk || wildCard == -1) continue;
                
                result.Add(new SingleTargetExocet(base1, base2, target, cross,
                    wildCard, new Cell(base1.Row, OtherUnitInBox(base1.Column, base2.Column)), basePossibilities, sCellsPositions));
            }
        }
    }

    private static void TryOfBase(ISudokuStrategyUser strategyUser, Cell base1, Cell base2, ReadOnlyBitSet16 basePossibilities,
        List<JuniorExocet> result)
    {
        var isRowJe = base1.Row == base2.Row;
        var targetCandidates = isRowJe
            ? RowTargetCandidates(strategyUser, base1, basePossibilities, false)
            : ColumnTargetCandidates(strategyUser, base1, basePossibilities, false);

        for (int i = 0; i < targetCandidates.Count; i++)
        {
            for (int j = i + 1; j < targetCandidates.Count; j++)
            {
                //Rule 4
                var t1 = targetCandidates[i];
                var t2 = targetCandidates[j];

                if ((t1.Row / 3 == t2.Row / 3 && t1.Column / 3 == t2.Column / 3) || 
                    !(strategyUser.PossibilitiesAt(t1) | strategyUser.PossibilitiesAt(t2)).ContainsAll(basePossibilities)) continue;

                Cell[] t1MirrorNodes;
                Cell[] t2MirrorNodes;

                if (isRowJe) ComputeRowMirrorNode(out t1MirrorNodes, out t2MirrorNodes, t1, t2, base1);
                else ComputeColumnMirrorNode(out t1MirrorNodes, out t2MirrorNodes, t1, t2, base1);

                bool notOk = true;

                foreach (var cell in t1MirrorNodes)
                {
                    if (strategyUser.ContainsAny(cell.Row, cell.Column, basePossibilities)) notOk = false;
                }
                
                foreach (var cell in t2MirrorNodes)
                {
                    if (strategyUser.ContainsAny(cell.Row, cell.Column, basePossibilities)) notOk = false;
                }

                if (notOk) continue;
                
                //Rule 3
                var sCellsPositions = new Dictionary<int, GridPositions>();
                foreach (var possibility in basePossibilities.EnumeratePossibilities())
                {
                    sCellsPositions.Add(possibility, new GridPositions());
                }

                if (isRowJe) FillRowSCells(strategyUser, sCellsPositions, base1, base2, t1, t2, basePossibilities);
                else FillColumnSCells(strategyUser, sCellsPositions, base1, base2, t1, t2, basePossibilities);

                foreach (var gp in sCellsPositions.Values)
                {
                    if (!gp.CanBeCoveredByUnits(2, Unit.Row, Unit.Column))
                    {
                        notOk = true;
                        break;
                    }
                }

                if (notOk) continue;

                var escapeCell = isRowJe
                    ? new Cell(base1.Row, OtherUnitInBox(base1.Column, base2.Column))
                    : new Cell(OtherUnitInBox(base1.Row, base2.Row), base1.Column);
                result.Add(new JuniorExocet(base1, base2, t1, t2, basePossibilities, escapeCell,
                    t1MirrorNodes, t2MirrorNodes, sCellsPositions));
            }
        }
    }

    private static List<Cell> RowTargetCandidates(ISudokuStrategyUser strategyUser, Cell base1,
        ReadOnlyBitSet16 basePossibilities, bool containsFull)
    {
        //Rule 2
        List<Cell> result = new();
        var rows = OtherUnitsInBox(base1.Row);
        var baseGridCol = base1.Column / 3;

        for (int gridCol = 0; gridCol < 3; gridCol++)
        {
            if (gridCol == baseGridCol) continue;

            for (int c = 0; c < 3; c++)
            {
                var col = gridCol * 3 + c;

                var poss1 = strategyUser.PossibilitiesAt(rows[0], col);
                var poss2 = strategyUser.PossibilitiesAt(rows[1], col);

                if (containsFull)
                {
                    if (poss1.ContainsAll(basePossibilities) && !strategyUser.ContainsAny(rows[1], col, basePossibilities))
                        result.Add(new Cell(rows[0], col));
                    else if (poss2.ContainsAll(basePossibilities) && !strategyUser.ContainsAny(rows[0], col, basePossibilities))
                        result.Add(new Cell(rows[1], col));
                    continue;
                }
                
                if (poss1.ContainsAny(basePossibilities) && !strategyUser.ContainsAny(rows[1], col, basePossibilities))
                    result.Add(new Cell(rows[0], col));
                else if (poss2.ContainsAny(basePossibilities) && !strategyUser.ContainsAny(rows[0], col, basePossibilities))
                    result.Add(new Cell(rows[1], col));
            }
        }

        return result;
    }
    
    private static List<Cell> ColumnTargetCandidates(ISudokuStrategyUser strategyUser, Cell base1,
        ReadOnlyBitSet16 basePossibilities, bool containsFull)
    {
        //Rule 2
        List<Cell> result = new();
        var cols = OtherUnitsInBox(base1.Column);
        var baseGridRow = base1.Row / 3;

        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            if (gridRow == baseGridRow) continue;

            for (int r = 0; r < 3; r++)
            {
                var row = gridRow * 3 + r;

                var poss1 = strategyUser.PossibilitiesAt(row, cols[0]);
                var poss2 = strategyUser.PossibilitiesAt(row, cols[1]);

                if (containsFull)
                {
                    if (poss1.ContainsAll(basePossibilities) && !strategyUser.ContainsAny(row, cols[1], basePossibilities))
                        result.Add(new Cell(row, cols[0]));
                    else if (poss2.ContainsAll(basePossibilities) && !strategyUser.ContainsAny(row, cols[0], basePossibilities))
                        result.Add(new Cell(row, cols[1]));
                    continue;
                }
                
                if (poss1.ContainsAny(basePossibilities) && !strategyUser.ContainsAny(row, cols[1], basePossibilities))
                    result.Add(new Cell(row, cols[0]));
                else if (poss2.ContainsAny(basePossibilities) && !strategyUser.ContainsAny(row, cols[0], basePossibilities))
                    result.Add(new Cell(row, cols[1]));
            }
        }

        return result;
    }

    private static void ComputeRowMirrorNode(out Cell[] t1MirrorCells, out Cell[]
        t2MirrorCells, Cell t1, Cell t2, Cell baseC)
    {
        var t1StartCol = t1.Column / 3 * 3;
        var t2StartCol = t2.Column / 3 * 3;
                
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

                if (col1 != t1.Column) t2MirrorCells[cursor2++] = new Cell(lastRow, col1);
                if (col2 != t2.Column) t1MirrorCells[cursor1++] = new Cell(lastRow, col2);
            }
        }
        else
        {
            for (int c = 0; c < 3; c++)
            {
                var col1 = t1StartCol + c;
                var col2 = t2StartCol + c;

                if (col1 != t1.Column) t2MirrorCells[cursor2++] = new Cell(t1.Row, col1);
                if (col2 != t2.Column) t1MirrorCells[cursor1++] = new Cell(t2.Row, col2);
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
                
        if (t1.Column == t2.Column)
        {
            var lastCol = OtherUnitInBox(t1.Column, baseC.Column);

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

                if (row1 != t1.Row) t2MirrorCells[cursor2++] = new Cell(row1, t1.Column);
                if (row2 != t2.Row) t1MirrorCells[cursor1++] = new Cell(row2, t2.Column);
            }
        }
    }

    private static void FillRowSCells(ISudokuStrategyUser strategyUser, Dictionary<int, GridPositions> sCellsPositions,
        Cell base1, Cell base2, Cell t1, Cell t2, ReadOnlyBitSet16 basePossibilities)
    {
        var lastBaseCol = OtherUnitInBox(base1.Column, base2.Column);
        for (int row = 0; row < 9; row++)
        {
            if (row / 3 == base1.Row / 3) continue;

            foreach (var possibility in basePossibilities.EnumeratePossibilities())
            {
                if(strategyUser.Contains(row, lastBaseCol, possibility))
                    sCellsPositions[possibility].Add(row, lastBaseCol);
                        
                if(strategyUser.Contains(row, t1.Column, possibility))
                    sCellsPositions[possibility].Add(row, t1.Column);
                        
                if(strategyUser.Contains(row, t2.Column, possibility))
                    sCellsPositions[possibility].Add(row, t2.Column);
            }
        }
    }
    
    private static void FillRowSCells(ISudokuStrategyUser strategyUser, Dictionary<int, GridPositions> sCellsPositions,
        Cell base1, Cell base2, Cell t, House cross, ReadOnlyBitSet16 basePossibilities)
    {
        var lastBaseCol = OtherUnitInBox(base1.Column, base2.Column);
        for (int row = 0; row < 9; row++)
        {
            if (row / 3 == base1.Row / 3) continue;

            foreach (var possibility in basePossibilities.EnumeratePossibilities())
            {
                if(strategyUser.Contains(row, lastBaseCol, possibility))
                    sCellsPositions[possibility].Add(row, lastBaseCol);
                        
                if(strategyUser.Contains(row, t.Column, possibility))
                    sCellsPositions[possibility].Add(row, t.Column);
                        
                if(strategyUser.Contains(row, cross.Number, possibility))
                    sCellsPositions[possibility].Add(row, cross.Number);
            }
        }
    }
    
    private static void FillColumnSCells(ISudokuStrategyUser strategyUser, Dictionary<int, GridPositions> sCellsPositions,
        Cell base1, Cell base2, Cell t1, Cell t2, ReadOnlyBitSet16 basePossibilities)
    {
        var lastBaseRow = OtherUnitInBox(base1.Row, base2.Row);
        for (int col = 0; col < 9; col++)
        {
            if (col / 3 == base1.Column / 3) continue;

            foreach (var possibility in basePossibilities.EnumeratePossibilities())
            {
                if(strategyUser.Contains(lastBaseRow, col, possibility))
                    sCellsPositions[possibility].Add(lastBaseRow, col);
                        
                if(strategyUser.Contains(t1.Row, col, possibility))
                    sCellsPositions[possibility].Add(t1.Row, col);
                        
                if(strategyUser.Contains(t2.Row, col, possibility))
                    sCellsPositions[possibility].Add(t2.Row, col);
            }
        }
    }
    
    private static void FillColumnSCells(ISudokuStrategyUser strategyUser, Dictionary<int, GridPositions> sCellsPositions,
        Cell base1, Cell base2, Cell t, House cross, ReadOnlyBitSet16 basePossibilities)
    {
        var lastBaseRow = OtherUnitInBox(base1.Row, base2.Row);
        for (int col = 0; col < 9; col++)
        {
            if (col / 3 == base1.Column / 3) continue;

            foreach (var possibility in basePossibilities.EnumeratePossibilities())
            {
                if(strategyUser.Contains(lastBaseRow, col, possibility))
                    sCellsPositions[possibility].Add(lastBaseRow, col);
                        
                if(strategyUser.Contains(t.Row, col, possibility))
                    sCellsPositions[possibility].Add(t.Row, col);
                        
                if(strategyUser.Contains(cross.Number, col, possibility))
                    sCellsPositions[possibility].Add(cross.Number, col);
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