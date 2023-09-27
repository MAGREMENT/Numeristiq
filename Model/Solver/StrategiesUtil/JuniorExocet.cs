using System;
using Model.Solver.Positions;
using Model.Solver.Possibilities;

namespace Model.Solver.StrategiesUtil;

public class JuniorExocet
{
    private Cell Base1 { get; }
    private Cell Base2 { get; }
    private Cell Target1 { get; }
    private Cell Target2 { get; }
    private IPossibilities BaseCandidates { get; }
    private Cell EscapeCell { get; }
    private SCells[] SCells { get; }
    
    private JuniorExocet(Cell base1, Cell base2, Cell target1, Cell target2, IPossibilities baseCandidates
        , Cell escapeCell, SCells[] sCells)
    {
        Base1 = base1;
        Base2 = base2;
        Target1 = target1;
        Target2 = target2;
        BaseCandidates = baseCandidates;
        EscapeCell = escapeCell;
        SCells = sCells;
    }

    public static JuniorExocet? TryCreateFromRow(IStrategyManager strategyManager, Cell base1, Cell base2, Cell target1,
        Cell target2, IPossibilities baseCandidates)
    {
        //Check if companions doesn't have base candidates
        if (strategyManager.PossibilitiesAt(target1.Row, target2.Col).PeekAny(baseCandidates)) return null;
        if (strategyManager.PossibilitiesAt(target2.Row, target1.Col).PeekAny(baseCandidates)) return null;
        
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
        foreach (var possibility in baseCandidates)
        {
            var s1 = strategyManager.ColumnPositionsAt(target1.Col, possibility).Copy();
            s1.VoidMiniGrid(miniRow);
            var solved = SolvedRowPositionsAt(strategyManager, target1.Col, possibility);
            if(solved != -1) s1.Add(solved);
            
            var s2 = strategyManager.ColumnPositionsAt(target2.Col, possibility).Copy();
            s2.VoidMiniGrid(miniRow);
            solved = SolvedRowPositionsAt(strategyManager, target2.Col, possibility);
            if(solved != -1) s2.Add(solved);
            
            var se = strategyManager.ColumnPositionsAt(eCol, possibility).Copy();
            se.VoidMiniGrid(miniRow);
            solved = SolvedRowPositionsAt(strategyManager, eCol, possibility);
            if(solved != -1) se.Add(solved);
            
            //2 cols
            if (se.Count == 0 || s1.Count == 0 || s2.Count == 0) continue;
            
            //2 rows
            if(s1.Or(s2).Or(se).Count <= 2) continue;
            
            //1 row & 1 col
            if((se.Count <= 1 && s1.Count <= 1 && se.Or(s1).Count == 1) 
               || (s1.Count <= 1 && s2.Count <= 1 && s1.Or(s2).Count == 1) 
               || (se.Count <= 1 && s2.Count <= 1 && se.Or(s2).Count == 1)) continue;

            return null;
        }

        return new JuniorExocet(base1, base2, target1, target2, baseCandidates, new Cell(base1.Row, eCol),
            Array.Empty<SCells>()); //TODO
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

    private static int SolvedRowPositionsAt(IStrategyManager strategyManager, int col, int n)
    {
        for (int row = 0; row < 9; row++)
        {
            if (strategyManager.Sudoku[row, col] == n) return row;
        }

        return -1;
    }
}

public class SCells
{
    public SCells(IReadOnlyLinePositions fromTarget1, IReadOnlyLinePositions fromTarget2,
        IReadOnlyLinePositions fromEscapeCell, int possibility)
    {
        FromTarget1 = fromTarget1;
        FromTarget2 = fromTarget2;
        FromEscapeCell = fromEscapeCell;
        Possibility = possibility;
    }

    private IReadOnlyLinePositions FromTarget1 { get; }
    private IReadOnlyLinePositions FromTarget2 { get; }
    private IReadOnlyLinePositions FromEscapeCell { get; }
    private int Possibility { get; }
}