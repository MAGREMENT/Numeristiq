using System.Collections.Generic;
using Model.Sudokus.Solver.Position;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Utility.Exocet;

public class SeniorExocet : Exocet
{
    public House EscapeCross { get; }
    public House Cross1 { get; }
    public House Cross2 { get; }
    
    
    public SeniorExocet(Cell base1, Cell base2, Cell target1, Cell target2,
        ReadOnlyBitSet16 baseCandidates, House escapeCross, House cross1, House cross2, Dictionary<int, GridPositions> sCells)
    : base(base1, base2, target1, target2, baseCandidates, sCells)
    {
        EscapeCross = escapeCross;
        Cross1 = cross1;
        Cross2 = cross2;
    }

    public override List<Cell> AllPossibleSCells()
    {
        var list = new List<Cell>();
        var unit = GetUnit();
        foreach (var cell in EscapeCross.EnumerateCells())
        {
            if (unit == Unit.Row)
            {
                if (cell.Row == Target1.Row || cell.Row == Target2.Row) continue;
            }
            else if (cell.Column == Target1.Column || cell.Column == Target2.Column) continue;
            if(!SudokuCellUtility.ShareAUnit(cell, Base1)) list.Add(cell);
        }
        
        foreach (var cell in Cross1.EnumerateCells())
        {
            if (unit == Unit.Row)
            {
                if (cell.Row == Target1.Row || cell.Row == Target2.Row) continue;
            }
            else if (cell.Column == Target1.Column || cell.Column == Target2.Column) continue;
            if(!SudokuCellUtility.ShareAUnit(cell, Base1)) list.Add(cell);
        }
        
        foreach (var cell in Cross2.EnumerateCells())
        {
            if (unit == Unit.Row)
            {
                if (cell.Row == Target1.Row || cell.Row == Target2.Row) continue;
            }
            else if (cell.Column == Target1.Column || cell.Column == Target2.Column) continue;
            if(!SudokuCellUtility.ShareAUnit(cell, Base1)) list.Add(cell);
        }

        return list;
    }
}