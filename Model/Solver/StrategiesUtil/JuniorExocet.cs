using System.Collections.Generic;
using Model.Solver.Position;
using Model.Solver.Possibility;

namespace Model.Solver.StrategiesUtil;

public class JuniorExocet
{
    private const int Max = 4;

    public Cell Base1 { get; }
    public Cell Base2 { get; }
    public Cell Target1 { get; }
    public Cell Target2 { get; }
    public Possibilities BaseCandidates { get; }
    public Cell EscapeCell { get; }
    public SPossibility[] SPossibilities { get; }

    private JuniorExocet(Cell base1, Cell base2, Cell target1, Cell target2, Possibilities baseCandidates
        , Cell escapeCell, SPossibility[] sPossibilities)
    {
        Base1 = base1;
        Base2 = base2;
        Target1 = target1;
        Target2 = target2;
        BaseCandidates = baseCandidates;
        EscapeCell = escapeCell;
        SPossibilities = sPossibilities;
    }

    public Unit GetUnit()
    {
        return Base1.Row == Base2.Row ? Unit.Row : Unit.Column;
    }

    public List<int[]> IncompatibilityTest(IStrategyManager strategyManager) //TODO
    {
        return new List<int[]>(0);
    }

    public List<Cell> GetSCells(){
        List<Cell> sCells = new();
        if (GetUnit() == Unit.Row)
        {
            for (int row = 0; row < 9; row++)
            {
                if (row / 3 == Base1.Row / 3) continue;
                
                sCells.Add(new Cell(row, Target1.Col));
                sCells.Add(new Cell(row, Target2.Col));
                sCells.Add(new Cell(row, EscapeCell.Col));
            }
        }
        else
        {
            for (int col = 0; col < 9; col++)
            {
                if (col / 3 == Base1.Col / 3) continue;
                
                sCells.Add(new Cell(Target1.Row, col));
                sCells.Add(new Cell(Target2.Row, col));
                sCells.Add(new Cell(EscapeCell.Row, col));
            }
        }

        return sCells;
    }

    public static Cell[] GetMirrorNodes(Cell t2, Unit unit)
    {
        Cell[] result = new Cell[2];
        var cursor = 0;
        
        if (unit == Unit.Row)
        {
            var startCol = t2.Col / 3 * 3;
            for(int gridCol = 0; gridCol < 3; gridCol++)
            {
                var col = startCol + gridCol;
                if (col == t2.Col) continue;

                result[cursor++] = new Cell(t2.Row, col);
            }
        }
        else
        {
            var startRow = t2.Row / 3 * 3;
            for(int gridRow = 0; gridRow < 3; gridRow++)
            {
                var row = startRow + gridRow;
                if (row == t2.Row) continue;

                result[cursor++] = new Cell(row, t2.Col);
            }
        }

        return result;
    }

    public static List<JuniorExocet> SearchFullGrid(IStrategyManager strategyManager)
    {
        var result = new List<JuniorExocet>();
        
        //Rows
        for (int row = 0; row < 9; row++)
        {
            for (int gridCol = 0; gridCol < 3; gridCol++)
            {
                for (int m1 = 0; m1 < 3; m1++)
                {
                    int col1 = gridCol * 3 + m1;
                    var poss1 = strategyManager.PossibilitiesAt(row, col1);
                    if (poss1.Count is < 2 or > Max) continue;

                    for (int m2 = m1 + 1; m2 < 3; m2++)
                    {
                        int col2 = gridCol * 3 + m2;
                        var poss2 = strategyManager.PossibilitiesAt(row, col2);
                        if (poss2.Count is < 2 or > Max) continue;

                        var or = poss1.Or(poss2);
                        if (or.Count > Max) continue;

                        //Base cells found, search for target cells
                        var gridCols = GridUnitsExcept(gridCol);
                        var rows = UnitsInGridExcept(row);

                        for (int m3 = 0; m3 < 3; m3++)
                        {
                            var col3 = gridCols[0] * 3 + m3;
                            var pos3 = strategyManager.PossibilitiesAt(rows[0], col3);
                            if (pos3.Count == 0) continue;

                            for (int m4 = 0; m4 < 3; m4++)
                            {
                                var col4 = gridCols[1] * 3 + m4;
                                var pos4 = strategyManager.PossibilitiesAt(rows[1], col4);
                                if (pos4.Count == 0 || !pos3.Or(pos4).PeekAll(or)) continue;

                                var je = TryCreateFromRow(strategyManager, new Cell(row, col1),
                                    new Cell(row, col2), new Cell(rows[0], col3),
                                    new Cell(rows[1], col4), or);
                                if (je is not null) result.Add(je);
                            }
                        }

                        for (int m3 = 0; m3 < 3; m3++)
                        {
                            var col3 = gridCols[1] * 3 + m3;
                            var pos3 = strategyManager.PossibilitiesAt(rows[0], col3);
                            if (pos3.Count == 0) continue;

                            for (int m4 = 0; m4 < 3; m4++)
                            {
                                var col4 = gridCols[0] * 3 + m4;
                                var pos4 = strategyManager.PossibilitiesAt(rows[1], col4);
                                if (pos4.Count == 0 || !pos3.Or(pos4).PeekAll(or)) continue;

                                var je = TryCreateFromRow(strategyManager, new Cell(row, col1),
                                    new Cell(row, col2), new Cell(rows[0], col3),
                                    new Cell(rows[1], col4), or);
                                if (je is not null) result.Add(je);
                            }
                        }
                    }
                }
            }
        }
        
        //Cols
        for (int col = 0; col < 9; col++)
        {
            for (int gridRow = 0; gridRow < 3; gridRow++)
            {
                for (int m1 = 0; m1 < 3; m1++)
                {
                    int row1 = gridRow * 3 + m1;
                    var poss1 = strategyManager.PossibilitiesAt(row1, col);
                    if (poss1.Count is < 2 or > Max) continue;

                    for (int m2 = m1 + 1; m2 < 3; m2++)
                    {
                        int row2 = gridRow * 3 + m2;
                        var poss2 = strategyManager.PossibilitiesAt(row2, col);
                        if (poss2.Count is < 2 or > Max) continue;

                        var or = poss1.Or(poss2);
                        if (or.Count > Max) continue;

                        //Base cells found, search for target cells
                        var gridRows = GridUnitsExcept(gridRow);
                        var cols = UnitsInGridExcept(col);

                        for (int m3 = 0; m3 < 3; m3++)
                        {
                            var row3 = gridRows[0] * 3 + m3;
                            var pos3 = strategyManager.PossibilitiesAt(row3, cols[0]);
                            if (pos3.Count == 0) continue;

                            for (int m4 = 0; m4 < 3; m4++)
                            {
                                var row4 = gridRows[1] * 3 + m4;
                                var pos4 = strategyManager.PossibilitiesAt(row4, cols[1]);
                                if (pos4.Count == 0 || !pos3.Or(pos4).PeekAll(or)) continue;

                                var je = TryCreateFromColumn(strategyManager, new Cell(row1, col),
                                    new Cell(row2, col), new Cell(row3, cols[0]),
                                    new Cell(row4, cols[1]), or);
                                if (je is not null) result.Add(je);
                            }
                        }

                        for (int m3 = 0; m3 < 3; m3++)
                        {
                            var row3 = gridRows[1] * 3 + m3;
                            var pos3 = strategyManager.PossibilitiesAt(row3, cols[0]);
                            if (pos3.Count == 0) continue;

                            for (int m4 = 0; m4 < 3; m4++)
                            {
                                var row4 = gridRows[0] * 3 + m4;
                                var pos4 = strategyManager.PossibilitiesAt(row4, cols[1]);
                                if (pos4.Count == 0 || !pos3.Or(pos4).PeekAll(or)) continue;

                                var je = TryCreateFromColumn(strategyManager, new Cell(row1, col),
                                    new Cell(row2, col), new Cell(row3, cols[0]),
                                    new Cell(row4, cols[1]), or);
                                if (je is not null) result.Add(je);
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    private static bool GeneralCheck(IStrategyManager strategyManager, Cell target1,
        Cell target2, Possibilities baseCandidates)
    {
        //Check if companions doesn't have base candidates
        if (strategyManager.PossibilitiesAt(target1.Row, target2.Col).PeekAny(baseCandidates)) return false;
        if (strategyManager.PossibilitiesAt(target2.Row, target1.Col).PeekAny(baseCandidates)) return false;

        return true;
    }

    private static JuniorExocet? TryCreateFromRow(IStrategyManager strategyManager, Cell base1, Cell base2, Cell target1,
        Cell target2, Possibilities baseCandidates)
    {
        if (!GeneralCheck(strategyManager, target1, target2, baseCandidates)) return null;
        
        //Check if mirror cells can contain at least one base candidate
        var startCol1 = target1.Col / 3 * 3;
        var startCol2 = target2.Col / 3 * 3;
        bool ok1 = false;
        bool ok2 = false;

        for (int gridCol = 0; gridCol < 3; gridCol++)
        {
            var col1 = startCol1 + gridCol;
            var col2 = startCol2 + gridCol;

            if (col1 != target1.Col)
            {
                var solved = strategyManager.Sudoku[target1.Row, col1];
                if (solved != 0)
                {
                    if (baseCandidates.Peek(solved)) ok1 = true;
                } 
                else if(strategyManager.PossibilitiesAt(target1.Row, col1).PeekAny(baseCandidates)) ok1 = true;
            }

            if (col2 != target2.Col)
            {
                var solved = strategyManager.Sudoku[target2.Row, col2];
                if (solved != 0)
                {
                    if (baseCandidates.Peek(solved)) ok2 = true;
                }
                else if(strategyManager.PossibilitiesAt(target2.Row, col2).PeekAny(baseCandidates)) ok2 = true;
            }
        }

        if (!ok1 || !ok2) return null;
        
        //Check that each base digit is in max 2 cover houses (2 rows, 2 cols, 1 row & 1 col) in the S cells
        var miniRow = base1.Row / 3;
        var eCol = UnitInGridExcept(base1.Col, base2.Col);
        SPossibility[] sCells = new SPossibility[baseCandidates.Count];
        int cursor = 0;
        
        foreach (var possibility in baseCandidates)
        {
            var s1 = strategyManager.ColumnPositionsAt(target1.Col, possibility).Copy();
            s1.VoidMiniGrid(miniRow);
            var solved = SolvedColumnPositionsAt(strategyManager, target1.Col, possibility);
            if(solved != -1) s1.Add(solved);
            
            var s2 = strategyManager.ColumnPositionsAt(target2.Col, possibility).Copy();
            s2.VoidMiniGrid(miniRow);
            solved = SolvedColumnPositionsAt(strategyManager, target2.Col, possibility);
            if(solved != -1) s2.Add(solved);
            
            var se = strategyManager.ColumnPositionsAt(eCol, possibility).Copy();
            se.VoidMiniGrid(miniRow);
            solved = SolvedColumnPositionsAt(strategyManager, eCol, possibility);
            if(solved != -1) se.Add(solved);

            var current  = new SPossibility(s1, s2, se, possibility);
            if (!current.IsValid()) return null;

            sCells[cursor++] = current;
        }

        return new JuniorExocet(base1, base2, target1, target2, baseCandidates, new Cell(base1.Row, eCol),
            sCells);
    }
    
    private static JuniorExocet? TryCreateFromColumn(IStrategyManager strategyManager, Cell base1, Cell base2, Cell target1,
        Cell target2, Possibilities baseCandidates)
    {
        if (!GeneralCheck(strategyManager, target1, target2, baseCandidates)) return null;
        
        //Check if mirror cells can contain at least one base candidate
        var startRow1 = target1.Row / 3 * 3;
        var startRow2 = target2.Row / 3 * 3;
        bool ok1 = false;
        bool ok2 = false;

        for (int gridRow = 0; gridRow < 3; gridRow++)
        {
            var row1 = startRow1 + gridRow;
            var row2 = startRow2 + gridRow;

            if (row1 != target1.Row)
            {
                var solved = strategyManager.Sudoku[row1, target1.Col];
                if (solved != 0)
                {
                    if (baseCandidates.Peek(solved)) ok1 = true;
                } 
                else if(strategyManager.PossibilitiesAt(row1, target1.Col).PeekAny(baseCandidates)) ok1 = true;
            }

            if (row2 != target2.Row)
            {
                var solved = strategyManager.Sudoku[row2, target2.Col];
                if (solved != 0)
                {
                    if (baseCandidates.Peek(solved)) ok2 = true;
                }
                else if(strategyManager.PossibilitiesAt(row2, target2.Col).PeekAny(baseCandidates)) ok2 = true;
            }
        }

        if (!ok1 || !ok2) return null;
        
        //Check that each base digit is in max 2 cover houses (2 rows, 2 cols, 1 row & 1 col) in the S cells
        var miniCol = base1.Col / 3;
        var eRow = UnitInGridExcept(base1.Row, base2.Row);
        SPossibility[] sCells = new SPossibility[baseCandidates.Count];
        int cursor = 0;
        
        foreach (var possibility in baseCandidates)
        {
            var s1 = strategyManager.RowPositionsAt(target1.Row, possibility).Copy();
            s1.VoidMiniGrid(miniCol);
            var solved = SolvedRowPositionsAt(strategyManager, target1.Row, possibility);
            if(solved != -1) s1.Add(solved);
            
            var s2 = strategyManager.RowPositionsAt(target2.Row, possibility).Copy();
            s2.VoidMiniGrid(miniCol);
            solved = SolvedRowPositionsAt(strategyManager, target2.Row, possibility);
            if(solved != -1) s2.Add(solved);
            
            var se = strategyManager.RowPositionsAt(eRow, possibility).Copy();
            se.VoidMiniGrid(miniCol);
            solved = SolvedRowPositionsAt(strategyManager, eRow, possibility);
            if(solved != -1) se.Add(solved);
            
            var current = new SPossibility(s1, s2, se, possibility);
            if (!current.IsValid()) return null;

            sCells[cursor++] = current;
        }

        return new JuniorExocet(base1, base2, target1, target2, baseCandidates, new Cell(eRow, base1.Col),
            sCells);
    }
    
    private static int[] GridUnitsExcept(int i)
    {
        var cursor = 0;
        int[] result = new int[2];

        for (int n = 0; n < 3; n++)
        {
            if (n == i) continue;
            
            result[cursor] = n;
            cursor++;
        }

        return result;
    }
    
    private static int[] UnitsInGridExcept(int i)
    {
        var start = i / 3 * 3;
        var cursor = 0;
        int[] result = new int[2];
        
        for (int n = 0; n < 3; n++)
        {
            var u = start + n;
            if (u == i) continue;
            
            result[cursor] = u;
            cursor++;
        }

        return result;
    }

    private static int UnitInGridExcept(int i1, int i2)
    {
        int start = i1 / 3 * 3;

        for (int n = 0; n < 3; n++)
        {
            var u = start + n;
            if (u == i1 || u == i2) continue;

            return u;
        }

        return -1;
    }

    private static int SolvedColumnPositionsAt(IStrategyManager strategyManager, int col, int n)
    {
        for (int row = 0; row < 9; row++)
        {
            if (strategyManager.Sudoku[row, col] == n) return row;
        }

        return -1;
    }
    
    private static int SolvedRowPositionsAt(IStrategyManager strategyManager, int row, int n)
    {
        for (int col = 0; col < 9; col++)
        {
            if (strategyManager.Sudoku[row, col] == n) return col;
        }

        return -1;
    }
}

public class SPossibility
{
    public SPossibility(IReadOnlyLinePositions fromTarget1, IReadOnlyLinePositions fromTarget2,
        IReadOnlyLinePositions fromEscapeCell, int possibility)
    {
        FromTarget1 = fromTarget1;
        FromTarget2 = fromTarget2;
        FromEscapeCell = fromEscapeCell;
        Possibility = possibility;
    }

    public IReadOnlyLinePositions FromTarget1 { get; }
    public IReadOnlyLinePositions FromTarget2 { get; }
    public IReadOnlyLinePositions FromEscapeCell { get; }
    public int Possibility { get; }

    public bool IsDoublePerpendicular()
    {
        return FromTarget1.Count == 0 || FromTarget2.Count == 0 || FromEscapeCell.Count == 0;
    }

    public bool IsDoubleParallel()
    {
        return FromTarget1.Or(FromTarget2).Or(FromEscapeCell).Count <= 2;
    }

    public bool IsOnePerpendicularAndOneParallel()
    {
        return IsPerpendicularWithOneParallel(FromTarget1, FromTarget2, FromEscapeCell)
               || IsPerpendicularWithOneParallel(FromTarget2, FromTarget1, FromEscapeCell)
               || IsPerpendicularWithOneParallel(FromEscapeCell, FromTarget2, FromTarget1);
    }

    public LinePositions Or()
    {
        return FromTarget1.Or(FromTarget2).Or(FromEscapeCell);
    }

    public static bool IsPerpendicularWithOneParallel(IReadOnlyLinePositions perp, IReadOnlyLinePositions par1,
        IReadOnlyLinePositions par2)
    {
        if (perp.Count <= 1) return false;
        return par1.Or(par2).Count == 1;
    }

    public bool IsValid()
    {
        return IsDoublePerpendicular() || IsDoubleParallel() || IsOnePerpendicularAndOneParallel();
    }
}