using System.Collections.Generic;
using Model.Solver.Position;
using Model.Solver.Possibility;

namespace Model.Solver.StrategiesUtil.Exocet;

public class JuniorExocet
{
    public Cell Base1 { get; }
    public Cell Base2 { get; }
    public Cell Target1 { get; }
    public Cell Target2 { get; }
    public Possibilities BaseCandidates { get; }
    public Cell EscapeCell { get; }
    public SPossibility[] SPossibilities { get; }

    public JuniorExocet(Cell base1, Cell base2, Cell target1, Cell target2, Possibilities baseCandidates
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